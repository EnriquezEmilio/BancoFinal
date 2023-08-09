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

namespace Banco.Controllers
{
    public class TarjetasController : Controller
    {
        private readonly MiContexto _context;
        private Usuario? uLogeado;


        public TarjetasController(MiContexto context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _context.usuarios
                   .Include(u => u.tarjetas)
                   .Include(u => u.cajas)
                   .Load();
            _context.cajas
                .Include(c => c.movimientos)
                .Include(c => c.titulares)
                .Load();
            _context.tarjetas.Load();
            uLogeado = _context.usuarios.Where(u => u.id == httpContextAccessor.HttpContext.Session.GetInt32("UserId")).FirstOrDefault();
        }

        // GET: Tarjetas
        public IActionResult Index()
        {
            if (uLogeado == null)
            {
                return RedirectToAction("Index", "Login");
            }
            if (uLogeado.isAdmin)
            {
                ViewData["Admin"] = "True";
                var miContexto = _context.tarjetas.Include(t => t.titular);
                return View(miContexto.ToList());
            }
            else
            {
                ViewData["Admin"] = "False";
                return View(uLogeado.tarjetas.ToList());
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
                ViewData["Admin"] = "True";
                var miContexto = _context.tarjetas.Include(t => t.titular);
                return View(miContexto.ToList());

            }
            else
            {
                ViewData["Admin"] = "False";
                return View(uLogeado.tarjetas.ToList());
            }
        }

        // GET: Tarjetas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (uLogeado == null)
            {
                return RedirectToAction("Index", "Login");
            }
            if (id == null || _context.tarjetas == null)
            {
                return NotFound();
            }

            var tarjeta = await _context.tarjetas
                .Include(t => t.titular)
                .FirstOrDefaultAsync(m => m.id == id);
            if (tarjeta == null)
            {
                return NotFound();
            }

            return View(tarjeta);
        }

        // GET: Tarjetas/Create
        public IActionResult Create()
        {
            if (uLogeado == null)
            {
                return RedirectToAction("Index", "Login");
            }
            ViewBag.Admin = uLogeado.isAdmin;
            ViewBag.id_titular = new SelectList(_context.usuarios, "id", "apellido");
            Random random = new();
            int nuevoNumero = random.Next(100000000, 999999999);
            ViewBag.numero = nuevoNumero;
            while (_context.tarjetas.Any(t => t.numero == nuevoNumero))
            {  // Mientras haya alguna tarjeta con ese numero se crea otro numero
                nuevoNumero = random.Next(100000000, 999999999);
                ViewBag.numero = nuevoNumero;
            }
            ViewBag.codigoV = random.Next(100, 999);
            ViewBag.limite = 50000;
            ViewBag.consumo = 0;
            return View();
        }

        // POST: Tarjetas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,id_titular,numero,codigoV,limite,consumo")] Tarjeta tarjeta)
        {
            if (uLogeado == null)
            {
                return RedirectToAction("Index", "Login");
            }
            ViewBag.Admin = uLogeado.isAdmin;
            if (ModelState.IsValid)
            {

                if (uLogeado.isAdmin)
                {
                    if (tarjeta.consumo > tarjeta.limite)
                    {
                        ViewBag.error = 0;
                        return View(tarjeta);
                    }
                    if (tarjeta.limite < 0)
                    {
                        ViewBag.error = 1;
                        return View(tarjeta);
                    }
                    if (tarjeta.consumo < 0)
                    {
                        ViewBag.error = 2;
                        return View(tarjeta);
                    }
                    Usuario usuario = await _context.usuarios.Where(u => u.id == tarjeta.id_titular).FirstOrDefaultAsync();
                    tarjeta.titular = usuario;
                    usuario.tarjetas.Add(tarjeta);
                    _context.Update(usuario);
                }
                else
                {
                    tarjeta.titular = uLogeado;
                    tarjeta.id_titular = uLogeado.id;
                    uLogeado.tarjetas.Add(tarjeta);
                    _context.Update(uLogeado);
                }
                _context.Add(tarjeta);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Tarjetas", new { success = 1 });
            }
            ViewBag.id_titular = new SelectList(_context.usuarios, "id", "apellido", tarjeta.id_titular);
            return View(tarjeta);
        }

        // GET: Tarjetas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.tarjetas == null)
            {
                return NotFound();
            }

            var tarjeta = await _context.tarjetas.FindAsync(id);
            if (tarjeta == null)
            {
                return NotFound();
            }
            ViewData["id_titular"] = new SelectList(_context.usuarios, "id", "apellido", tarjeta.id_titular);
            return View(tarjeta);
        }

        // POST: Tarjetas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,numero,codigoV,limite,consumo")] Tarjeta tarjeta)
        {
            if (id != tarjeta.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var tarjetaActual = await _context.tarjetas.FindAsync(id);
                    if (tarjetaActual == null)
                    {
                        return NotFound();
                    }

                    // Actualizar solo las propiedades necesarias
                    tarjetaActual.numero = tarjeta.numero;
                    tarjetaActual.codigoV = tarjeta.codigoV;
                    tarjetaActual.limite = tarjeta.limite;
                    tarjetaActual.consumo = tarjeta.consumo;

                    _context.Update(tarjetaActual);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TarjetaExists(tarjeta.id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index", "Tarjetas", new { success = 2 });
            }
            ViewData["id_titular"] = new SelectList(_context.usuarios, "id", "apellido", tarjeta.id_titular);
            return View(tarjeta);
        }





        // GET: Tarjetas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (uLogeado == null)
            {
                return RedirectToAction("Index", "Login");
            }
            if (id == null || _context.tarjetas == null)
            {
                return NotFound();
            }

            var tarjeta = await _context.tarjetas
                .Include(t => t.titular)
                .FirstOrDefaultAsync(m => m.id == id);
            if (tarjeta == null)
            {
                return NotFound();
            }

            return View(tarjeta);
        }

        // POST: Tarjetas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.tarjetas == null)
            {
                return Problem("Entity set 'MiContexto.tarjetas'  is null.");
            }

            var tarjeta = await _context.tarjetas.FindAsync(id);
            if (tarjeta == null)
            {
                return NotFound();
            }
            if (tarjeta != null)
            {
                _context.tarjetas.Remove(tarjeta);
                tarjeta.titular.tarjetas.Remove(tarjeta);
                _context.Update(tarjeta.titular);

            }
            if (tarjeta.consumo != 0)
            {
                ViewBag.error = 0;
                return View(tarjeta);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Tarjetas", new { success = 3 });
        }

        // GET: Tarjetas/pagarTarjeta
        public IActionResult PagarTarjeta(int? id)
        {
            if (uLogeado == null)
            {
                return RedirectToAction("Index", "Login");
            }
            if (id == null || _context.tarjetas == null)
            {
                return NotFound();
            }
            var tarjeta = _context.tarjetas.FirstOrDefault(t => t.id == id);
            ViewBag.cajas = uLogeado.cajas.ToList();
            return View(tarjeta);
        }

        // POST: Tarjetas/pagarTarjeta
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PagarTarjeta(int? id, int id_caja)
        {
            if (_context.tarjetas == null)
            {
                return Problem("Entity set 'MiContexto.tarjetas'  is null.");
            }
            ViewBag.cajas = uLogeado.cajas.ToList();
            var tarjeta = _context.tarjetas.FirstOrDefault(t => t.id == id);
            if (tarjeta == null)
            {
                return NotFound();
            }
            CajaDeAhorro caja = _context.cajas.Where(c => c.id == id_caja).FirstOrDefault();
            if (caja == null)
            {
                return NotFound();
            }
            if (caja.saldo < tarjeta.consumo)
            {
                ViewBag.error = 0;
                return View(tarjeta);
            }
            caja.saldo -= tarjeta.consumo;
            altaMovimiento(caja, "Pago tarjeta", (float)tarjeta.consumo);
            tarjeta.consumo = 0;
            _context.Update(tarjeta);
            _context.Update(caja);
            _context.SaveChanges();
            return RedirectToAction("Index", "Tarjetas", new { success = 4 });
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

        private bool TarjetaExists(int id)
        {
            return _context.tarjetas.Any(e => e.id == id);
        }
    }
}
