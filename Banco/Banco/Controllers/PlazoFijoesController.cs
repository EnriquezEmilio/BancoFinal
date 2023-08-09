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
    public class PlazoFijoesController : Controller
    {
        private readonly MiContexto _context;
        private Usuario? uLogeado;
        private bool acreditacionesRealizadas = false;


        public PlazoFijoesController(MiContexto context, IHttpContextAccessor httpContextAccessor)
{
    _context = context;
    uLogeado = _context.usuarios
        .Include(u => u.tarjetas)
        .Include(u => u.cajas)
        .Include(u => u.pf)
        .Include(u => u.pagos)
        .FirstOrDefault(u => u.id == httpContextAccessor.HttpContext.Session.GetInt32("UserId"));

    // Cargar las entidades relacionadas solo si son necesarias en esta acción específica.
    // Evita el uso de Load() para cargarlas todas de una vez, ya que puede causar problemas de concurrencia.
    if (uLogeado != null)
    {
        _context.Entry(uLogeado)
            .Collection(u => u.cajas)
            .Load();

        _context.Entry(uLogeado)
            .Collection(u => u.pf)
            .Load();
    }
         
}
        // GET: PlazoFijo
        public IActionResult Index()
        {
            if (uLogeado == null)
            {
                return RedirectToAction("Index", "Login");
            }

            foreach (var plazoFijo in _context.plazosFijos)
            {
                Pagar(plazoFijo);
            }

            if (uLogeado.isAdmin)
            {
                ViewBag.Admin = true;
                ViewBag.Nombre = "Administrador: " + uLogeado.nombre + " " + uLogeado.apellido;
                return View(_context.plazosFijos.ToList());
            }
            else
            {
                ViewBag.Admin = false;
                ViewBag.Nombre = uLogeado.nombre + " " + uLogeado.apellido;
                return View(uLogeado.pf.ToList());
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
                  //  VerificarYAcreditarPlazosFijos();

            if (uLogeado.isAdmin)
            {
                ViewBag.Admin = true;
                return View(_context.plazosFijos.ToList());
            }
            else
            {
                ViewBag.Admin = false;
                return View(uLogeado.pf.ToList());
            }
        }
        // GET: PlazoFijoes/Create
        public IActionResult Create()
        {
            if (uLogeado == null)
            {
                return RedirectToAction("Index", "Login");
            }

            ViewBag.Admin = uLogeado.isAdmin;

            if (uLogeado.isAdmin)
            {
                ViewBag.cajas = _context.cajas.ToList();
            }
            else
            {
                ViewBag.cajas = uLogeado.cajas.ToList();
            }

            ViewBag.fechaIn = DateTime.Now;
            ViewBag.fechaFin = DateTime.Now.AddMonths(1);
            ViewBag.tasa = 0.78m;

            PlazoFijo plazoFijo = new PlazoFijo();
            plazoFijo.titular = uLogeado.isAdmin ? _context.usuarios.FirstOrDefault() : uLogeado;

            return View(plazoFijo);
        }

        // POST: PlazoFijoes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,monto,fechaIni,fechaFin,tasa,pagado,id_titular,cbu")] PlazoFijo plazoFijo)
        {
            if (uLogeado.isAdmin)
            {
                ViewBag.cajas = _context.cajas.ToList();
            }
            else
            {
                ViewBag.cajas = uLogeado.cajas.ToList();
            }

            ViewBag.fechaIn = DateTime.Now;
            ViewBag.fechaFin = DateTime.Now.AddMonths(1);
            ViewBag.tasa = 0.78m;

            if (plazoFijo == null)
            {
                return NotFound();
            }

            if (plazoFijo.monto < 1000)
            {
                ViewBag.error = 0;
                return View();
            }

            CajaDeAhorro caja = null;

            if (uLogeado.isAdmin)
            {
                caja = _context.cajas.FirstOrDefault(c => c.cbu == plazoFijo.cbu);
            }
            else
            {
                caja = uLogeado.cajas.FirstOrDefault(c => c.cbu == plazoFijo.cbu);
            }

            if (caja == null)
            {
                return NotFound();
            }

            if (caja.saldo < plazoFijo.monto)
            {
                ViewBag.error = 1;
                return View();
            }

            if (ModelState.IsValid)
            {
                plazoFijo.tasa = 0.78m;

                _context.Add(plazoFijo);

                if (!uLogeado.isAdmin)
                {
                    plazoFijo.id_titular = uLogeado.id;
                    uLogeado.pf.Add(plazoFijo);
                    _context.Update(uLogeado);
                }
                else
                {
                    Usuario titular = caja.titulares.FirstOrDefault();
                    if (titular != null)
                    {
                        titular.pf.Add(plazoFijo);
                        _context.Update(titular);
                    }
                }

                caja.saldo -= plazoFijo.monto;
                altaMovimiento(caja, "Alta plazo fijo", (float)plazoFijo.monto);
                _context.Update(caja);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "PlazoFijoes", new { success = 1 });
            }

            ViewData["id_titular"] = new SelectList(_context.usuarios, "id", "apellido", plazoFijo.id_titular);
            return View(plazoFijo);
        }




        // GET: PlazoFijoes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (uLogeado == null)
            {
                return RedirectToAction("Index", "Login");
            }
            if (id == null || _context.plazosFijos == null)
            {
                return NotFound();
            }

            var plazoFijo = await _context.plazosFijos
                .Include(p => p.titular)
                .FirstOrDefaultAsync(m => m.id == id);
            if (plazoFijo == null)
            {
                return NotFound();
            }

            bool editable = !plazoFijo.pagado;

            ViewData["Editable"] = editable;

            return View(plazoFijo);
        }



        // GET: PlazoFijoes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (uLogeado == null)
            {
                return RedirectToAction("Index", "Login");
            }

            if (id == null || _context.plazosFijos == null)
            {
                return NotFound();
            }

            var plazoFijo = await _context.plazosFijos
                .Include(p => p.titular)
                .FirstOrDefaultAsync(m => m.id == id);

            if (plazoFijo == null)
            {
                return NotFound();
            }

            return View(plazoFijo);
        }

        // POST: PlazoFijoes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var plazoFijo = await _context.plazosFijos.FindAsync(id);

            if (plazoFijo == null)
            {
                return NotFound();
            }
            CajaDeAhorro caja = _context.cajas.FirstOrDefault(c => c.cbu == plazoFijo.cbu);
            if (caja != null)
            {
                caja.saldo += plazoFijo.monto;
                _context.Update(caja);
            }

            _context.plazosFijos.Remove(plazoFijo);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "PlazoFijoes", new { success = 2 });
        }


        // GET: PlazoFijoes/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            PlazoFijo plazoFijo = await _context.plazosFijos.FirstOrDefaultAsync(pf => pf.id == id);

            if (plazoFijo == null)
            {
                return NotFound();
            }

            if (plazoFijo.pagado)
            {
                return RedirectToAction("Details", new { id = plazoFijo.id });
            }

            return View(plazoFijo); 
        }

        // POST: PlazoFijoes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,monto,fechaIni,fechaFin,pagado,id_titular")] PlazoFijo plazoFijo)
        {
            if (id != plazoFijo.id)
            {
                return NotFound();
            }

            PlazoFijo plazoFijoEnDB = await _context.plazosFijos.FirstOrDefaultAsync(pf => pf.id == id);

            if (plazoFijoEnDB == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    plazoFijoEnDB.monto = plazoFijo.monto;
                    plazoFijoEnDB.fechaIni = plazoFijo.fechaIni;
                    plazoFijoEnDB.fechaFin = plazoFijo.fechaFin;

                    _context.Update(plazoFijoEnDB);
                    await _context.SaveChangesAsync();

                    await Pagar(plazoFijoEnDB);

                    return RedirectToAction("Index", new { success = 3 });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PlazoFijoExists(plazoFijo.id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return View(plazoFijoEnDB); 
        }



        private async Task Pagar(PlazoFijo plazoFijo)
        {
            if (!plazoFijo.pagado && DateTime.Today >= plazoFijo.fechaFin.Date)
            {
                CajaDeAhorro caja = _context.cajas.FirstOrDefault(c => c.cbu == plazoFijo.cbu);
                if (caja != null)
                {
                    caja.saldo += plazoFijo.monto;

                    plazoFijo.pagado = true;

                    _context.Update(caja);
                    _context.Update(plazoFijo);

                    string detalleMovimiento = "Pago Plazo Fijo #" + plazoFijo.id;
                    float montoMovimiento = (float)plazoFijo.monto;
                    altaMovimiento(caja, detalleMovimiento, montoMovimiento);

                    await _context.SaveChangesAsync();
                     RedirectToAction("Index", "PlazoFijoes", new { success = 2 });

                }
            }
        }




        // GET: PlazoFijoes/Rescatar/5
        public async Task<IActionResult> Rescatar(int? id)
        {
            var plazoFijo = await _context.plazosFijos.FindAsync(id);

            if (plazoFijo == null)
            {
                return NotFound();
            }

            CajaDeAhorro caja = _context.cajas.FirstOrDefault(c => c.cbu == plazoFijo.cbu);
            if (caja != null && !plazoFijo.pagado)
            {
                caja.saldo += plazoFijo.monto;
                plazoFijo.pagado = true;
                _context.Update(caja);
                _context.Update(plazoFijo);
                await _context.SaveChangesAsync();

                _context.plazosFijos.Remove(plazoFijo);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", new { success = 5});
            }

            return RedirectToAction("Index", new { success = 6 });
        }
/*
        private async Task<IActionResult> VerificarYAcreditarPlazosFijos()
        {
            if (!acreditacionesRealizadas)
            {
                foreach (var plazoFijo in _context.plazosFijos)
                {
                    await Pagar(plazoFijo);
                }

                acreditacionesRealizadas = true;

                return RedirectToAction("Index", new { success = 6 });
            }

            return RedirectToAction("Index", new { success = 7 });
        }
*/

       public bool altaMovimiento(CajaDeAhorro caja, string detalle, float monto)
{
    try
    {
        Movimiento movimientoNuevo = new Movimiento(caja, detalle, monto); 

        _context.movimientos.Add(movimientoNuevo);
        caja.movimientos.Add(movimientoNuevo);
        _context.Update(caja);
        _context.SaveChanges();
        return true;
    }
    catch
    {
        return false;
    }
}

        private bool PlazoFijoExists(int id)
        {
            return _context.plazosFijos.Any(e => e.id == id);
        }


       
    }
}
