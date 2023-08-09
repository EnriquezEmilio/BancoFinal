using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Banco.Data;
using Banco.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.DotNet.Scaffolding.Shared.CodeModifier.CodeChange;


namespace Banco.Controllers
{
    public class PagoesController : Controller
    {
        private readonly MiContexto _context;
        private Usuario? uLogeado;


        public PagoesController(MiContexto context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _context.usuarios
                   .Include(u => u.tarjetas)
                   .Include(u => u.cajas)
                   .Include(u => u.pf)
                   .Include(u => u.pagos)
                   .Load();
            _context.cajas
                .Include(c => c.movimientos)
                .Include(c => c.titulares)
                .Load();
            _context.pagos.Load();
            _context.tarjetas.Load();
            uLogeado = _context.usuarios.Where(u => u.id == httpContextAccessor.HttpContext.Session.GetInt32("UserId")).FirstOrDefault();
        }

        // GET: Pagoes
        public IActionResult Index()
        {
            if (uLogeado == null)
            {
                return RedirectToAction("Index", "Login");
            }
            if (uLogeado.isAdmin)
            {
                ViewBag.Admin = true;
                return View(_context.pagos.ToList());
            }
            else
            {
                ViewBag.Admin = false;
                return View(uLogeado.pagos.ToList());
            }
        }
        [HttpGet]
        public IActionResult Index(int success)
        {
            if (uLogeado == null)
            {
                return RedirectToAction("Index", "Login");
            }
            ViewBag.success = success;
            if (uLogeado.isAdmin)
            {
                ViewBag.Admin = true;
                return View(_context.pagos.ToList());
            }
            else
            {
                ViewBag.Admin = false;
                return View(uLogeado.pagos.ToList());
            }

        }

        // GET: Pagoes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (uLogeado == null)
            {
                return RedirectToAction("Index", "Login");
            }
            if (id == null || _context.pagos == null)
            {
                return NotFound();
            }

            var pago = await _context.pagos
                .Include(p => p.usuario)
                .FirstOrDefaultAsync(m => m.id == id);
            if (pago == null)
            {
                return NotFound();
            }

            return View(pago);
        }

        // GET: Pagoes/Create
        public IActionResult Create()
        {
            if (uLogeado == null)
            {
                return RedirectToAction("Index", "Login");
            }
            ViewData["id_usuario"] = new SelectList(_context.usuarios, "id", "apellido");
            ViewBag.Admin = uLogeado.isAdmin;
            return View();
        }

        // POST: Pagoes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,id_usuario,nombre,monto")] Pago pago)
        {
            if (uLogeado == null)
            {
                return RedirectToAction("Index", "Login");
            }
            if (pago.monto < 0)
            {
                ViewBag.error = 1;
                return View();
            }
            ViewBag.Admin = uLogeado.isAdmin;
            ViewData["id_usuario"] = new SelectList(_context.usuarios, "id", "apellido", pago.id_usuario);
            Usuario titular;
            if (uLogeado.isAdmin)
            {
                titular = _context.usuarios.FirstOrDefault(u => u.id == pago.id_usuario);
                if (titular == null)
                {
                    ViewBag.error = 1;
                    return View();
                }
            }
            else
            {
                titular = uLogeado;
                pago.id_usuario = uLogeado.id;
            }
            if (ModelState.IsValid)
            {
                pago.metodo = "No pagado";
                pago.pagado = false;
                pago.usuario = titular;
                _context.Add(pago);
                titular.pagos.Add(pago);
                _context.Update(titular);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Pagoes", new { success = 1 });
            }
            return View(pago);
        }

        // POST: Pago/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        public async Task<IActionResult> Edit(int id)
        {
            Pago pago = await _context.pagos.FirstOrDefaultAsync(p => p.id == id);

            if (pago == null)
            {
                return NotFound();
            }

            if (pago.pagado)
            {
                return RedirectToAction("Details", "Pagoes", new { id = pago.id }); // Redirigir a la vista de detalles si ya está pagado
            }

            return View(pago); // Mostrar el formulario de edición solo si el pago no está pagado
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,nombre,monto")] Pago pago)
        {
            if (id != pago.id)
            {
                return NotFound();
            }

            Pago pagoEnDB = await _context.pagos.FirstOrDefaultAsync(p => p.id == id);

            if (pagoEnDB == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    pagoEnDB.nombre = pago.nombre;
                    pagoEnDB.monto = pago.monto;
                    pagoEnDB.pagado = true;
                    pagoEnDB.metodo = "Pagado desde el banco";

                    _context.Update(pagoEnDB);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PagoExists(pago.id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index", "Pagoes", new { success = 2 });
            }

            return View(pagoEnDB); // Si hay errores de validación, mostrar la vista de edición nuevamente
        }




        // GET: Pagoes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (uLogeado == null)
            {
                return RedirectToAction("Index", "Login");
            }
            if (id == null || _context.pagos == null)
            {
                return NotFound();
            }
            Pago pago;
            if (uLogeado.isAdmin)
            {
                pago = await _context.pagos.Include(p => p.usuario).FirstOrDefaultAsync(m => m.id == id);
            }
            else
            {
                pago = uLogeado.pagos.FirstOrDefault(p => p.id == id);
            }
            if (pago == null)
            {
                return NotFound();
            }

            return View(pago);
        }

        // POST: Pagoes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.pagos == null)
            {
                return Problem("Entity set 'MiContexto.pagos'  is null.");
            }
            var pago = await _context.pagos.FindAsync(id);
            if (pago == null)
            {
                return NotFound();
            }
            if (!pago.pagado)
            {
                ViewBag.error = 0;
                return View(pago);
            }
            if (uLogeado.isAdmin)
            {
                pago.usuario.pagos.Remove(pago);
                _context.Update(pago.usuario);
            }
            else

            {
                uLogeado.pagos.Remove(pago);
                _context.Update(uLogeado);
            }
            _context.pagos.Remove(pago);
            await _context.SaveChangesAsync();
            return RedirectToAction("index", "Pagoes", new { success = 3 });

        }

        //Get Pagar
        public async Task<IActionResult> Pagar(int? id)
        {
            if (uLogeado == null)
            {
                return RedirectToAction("Index", "Login");
            }
            if (id == null || _context.pagos == null)
            {
                return NotFound();
            }
            Pago pago;
            if (uLogeado.isAdmin)
            {
                ViewBag.cajas = _context.cajas.ToList();
                ViewBag.tarjetas = _context.tarjetas.ToList();
                pago = await _context.pagos
                   .Include(p => p.usuario)
                   .FirstOrDefaultAsync(m => m.id == id);
            }
            else
            {
                ViewBag.cajas = uLogeado.cajas.ToList();
                ViewBag.tarjetas = uLogeado.tarjetas.ToList();
                pago = uLogeado.pagos
                    .FirstOrDefault(p => p.id == id);
            }
            if (pago == null)
            {
                return NotFound();
            }

            return View(pago);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pagar(int id, int idMetodoDePago)
        {
            if (_context.pagos == null)
            {
                return Problem("Entity set 'MiContexto.pagos'  is null.");
            }
            var pago = await _context.pagos.FindAsync(id);
            if (pago == null)
            {
                return NotFound();
            }
            if (uLogeado.isAdmin)
            {
                ViewBag.cajas = _context.cajas.ToList();
                ViewBag.tarjetas = _context.tarjetas.ToList();
                pago = await _context.pagos
                   .Include(p => p.usuario)
                   .FirstOrDefaultAsync(m => m.id == id);
            }
            else
            {
                ViewBag.cajas = uLogeado.cajas.ToList();
                ViewBag.tarjetas = uLogeado.tarjetas.ToList();
                pago = uLogeado.pagos
                    .FirstOrDefault(p => p.id == id);
            }
            if (pago.pagado)
            {
                ViewBag.error = 0;
                return View(pago);
            }
            CajaDeAhorro caja = await _context.cajas.Where(c => c.id == idMetodoDePago).FirstOrDefaultAsync();
            if (caja != null)
            {
                if (caja.saldo < pago.monto)
                {
                    ViewBag.error = 1;
                    return View(pago);
                }
                pago.metodo = "Transferencia";
                altaMovimiento(caja, pago.nombre, pago.monto);
                caja.saldo -= pago.monto;
                _context.Update(caja);
            }
            else
            {
                Tarjeta tarjeta = await _context.tarjetas.Where(c => c.id == idMetodoDePago).FirstOrDefaultAsync();
                if (tarjeta == null)
                {
                    return NotFound();
                }
                float disponible = tarjeta.limite - tarjeta.consumo;
                if (disponible < pago.monto)
                {
                    ViewBag.error = 2;
                    return View(pago);
                }
                pago.metodo = "Tarjeta";
                tarjeta.consumo += pago.monto;
                _context.Update(tarjeta);
            }
            pago.pagado = true;
            _context.Update(pago);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Pagoes", new { success = 4 });
        }

       

        private bool PagoExists(int id)
        {
            return _context.pagos.Any(e => e.id == id);
        }
        public bool altaMovimiento(CajaDeAhorro Caja, string Detalle, float Monto)
        {
            try
            {
                Movimiento movimientoNuevo = new Movimiento(Caja, Detalle, Monto);
                _context.movimientos.Add(movimientoNuevo);
                Caja.movimientos.Add(movimientoNuevo);
                _context.Update(Caja);
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}


