using Banco.Data;
using Banco.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Banco.Controllers
{
    public class CajaDeAhorroesController : Controller
    {
        private readonly MiContexto _context;
        private Usuario? uLogeado;


        public CajaDeAhorroesController(MiContexto context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;

            var userId = httpContextAccessor.HttpContext.Session.GetInt32("UserId");
            uLogeado = _context.usuarios.Include(u => u.cajas).FirstOrDefault(u => u.id == userId);

            _context.usuarios
                .Include(u => u.tarjetas)
                .Include(u => u.pf)
                .Include(u => u.pagos)
                .Load();

            _context.cajas
                .Include(c => c.movimientos)
                .Include(c => c.titulares)
                .Load();

            _context.tarjetas.Load();
            _context.pagos.Load();
            _context.movimientos.Load();
            _context.plazosFijos.Load();
        }


        // GET: CajaDeAhorroes
        public IActionResult Index()
        {
            if (uLogeado == null)
            {
                return RedirectToAction("Index", "Login");
            }
            if (uLogeado.isAdmin)
            {
                
                ViewBag.Admin = uLogeado.isAdmin;
               // ViewBag.NombreUsuario = uLogeado.nombre;
               // ViewBag.ApellidoUsuario = uLogeado.apellido;
                return View(_context.cajas.ToList());
            }
            else
            {
                ViewBag.Admin = false;
               // ViewBag.NombreUsuario = uLogeado.nombre;
               // ViewBag.ApellidoUsuario = uLogeado.apellido;
                return View(uLogeado.cajas.ToList());
            }

        }
        [HttpGet]
        public IActionResult Index(string success)
        {
            if (uLogeado == null)
            {
                return RedirectToAction("Index", "Login");
            }
            ViewBag.success = success;
            if (uLogeado.isAdmin)
            {
                ViewBag.Admin = uLogeado.isAdmin;
              //  ViewBag.NombreUsuario = uLogeado.nombre;
               // ViewBag.ApellidoUsuario = uLogeado.apellido;
                return View(_context.cajas.ToList());
            }
            else
            {
                ViewBag.Admin = false;
               // ViewBag.NombreUsuario = uLogeado.nombre;
               // ViewBag.ApellidoUsuario = uLogeado.apellido;
                return View(uLogeado.cajas.ToList());
            }

        }

        public IActionResult Details(int id)
        {
            var caja = _context.cajas
                .Include(c => c.titulares)
                .FirstOrDefault(c => c.id == id);

            if (caja == null)
            {
                return NotFound();
            }

            bool esAdmin = uLogeado.isAdmin;

            if (esAdmin)
            {
                var titulares = caja.titulares.ToList();

                if (titulares.Count > 0)
                {
                
                    ViewBag.TitularesNombres = titulares.Select(t => t.nombre).ToList();
                    ViewBag.TitularesApellidos = titulares.Select(t => t.apellido).ToList();
                }
            }

            ViewBag.Admin = esAdmin;
            return View(caja);
        }


        // GET: CajaDeAhorroes/Create
        public IActionResult Create()
        {
            if (uLogeado == null)
            {
                return RedirectToAction("Index", "Login");
            }
            ViewBag.Admin = uLogeado.isAdmin;
            if (uLogeado.isAdmin)
            {
                ViewBag.titulares = _context.usuarios.ToList();
            }
            ViewBag.id_titular = new SelectList(_context.usuarios, "id", "apellido");
            Random random = new();
            int nuevoNumero = random.Next(100000000, 999999999);
            ViewBag.cbu = nuevoNumero;
            while (_context.cajas.Any(t => t.cbu == nuevoNumero))
            { 
                nuevoNumero = random.Next(100000000, 999999999);
                ViewBag.cbu = nuevoNumero;
            }
            ViewBag.saldo = 0;
            return View();
        }

        // POST: CajaDeAhorro/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int id_titular, [Bind("id,cbu,saldo")] CajaDeAhorro cajaDeAhorro)
        {
            if (uLogeado.isAdmin)
            {
                ViewBag.titulares = _context.usuarios.ToList();
            }
            if (cajaDeAhorro.saldo < 0)
            {
                ViewBag.error = 0;
                return View(cajaDeAhorro);
            }
            if (ModelState.IsValid)
            {
                Usuario titular;
                if (uLogeado.isAdmin)
                {
                    titular = _context.usuarios.FirstOrDefault(u => u.id == id_titular);
                }
                else
                {
                    titular = uLogeado;
                }
                _context.Add(cajaDeAhorro);
                titular.cajas.Add(cajaDeAhorro);
                cajaDeAhorro.titulares.Add(titular);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "CajaDeAhorroes", new { success = "1" });
            }
            return View(cajaDeAhorro);
        }

        // GET: CajaDeAhorro/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (uLogeado == null)
            {
                return RedirectToAction("Index", "Login");
            }
            if (id == null || _context.cajas == null)
            {
                return NotFound();
            }

            var cajaDeAhorro = await _context.cajas.FindAsync(id);
            if (cajaDeAhorro == null)
            {
                return NotFound();
            }
            return View(cajaDeAhorro);
        }

        // POST: CajaDeAhorro/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,cbu,saldo")] CajaDeAhorro cajaDeAhorro)
        {
            if (id != cajaDeAhorro.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var cajaActual = await _context.cajas.FindAsync(id);
                    if (cajaActual == null)
                    {
                        return NotFound();
                    }

                    cajaActual.cbu = cajaDeAhorro.cbu;
                    cajaActual.saldo = cajaDeAhorro.saldo;

                    _context.Update(cajaActual);
                    await _context.SaveChangesAsync();

                    return RedirectToAction("Index", "CajaDeAhorroes", new { success = "2" });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CajaDeAhorroExists(cajaDeAhorro.id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return View(cajaDeAhorro);
        }


        // GET: CajaDeAhorro/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (uLogeado == null)
            {
                return RedirectToAction("Index", "Login");
            }
            if (id == null || _context.cajas == null)
            {
                return NotFound();
            }
            var cajaDeAhorro = await _context.cajas.FirstOrDefaultAsync(m => m.id == id);
            if (cajaDeAhorro == null)
            {
                return NotFound();
            }
            return View(cajaDeAhorro);
        }

        // POST: CajaDeAhorro/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.cajas == null)
            {
                return Problem("Entity set 'MiContexto.cajas'  is null.");
            }
            var cajaDeAhorro = await _context.cajas.FindAsync(id);
            if (cajaDeAhorro == null)
            {
                return NotFound();
            }
            if (cajaDeAhorro.saldo != 0)
            {
                ViewBag.error = 0;
                return View(cajaDeAhorro);
            }
            _context.cajas.Remove(cajaDeAhorro);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "CajaDeAhorroes", new { success = "3" });
        }


        // GET: CajaDeAhorro/Depositar
        public IActionResult Depositar(int? id)
        {
            if (uLogeado == null)
            {
                return RedirectToAction("Index", "Login");
            }

            if (id == null || uLogeado.cajas == null)
            {
                return NotFound();
            }
            CajaDeAhorro caja;
            if (uLogeado.isAdmin)
            {
                caja = _context.cajas.FirstOrDefault(c => c.id == id);
            }
            else
            {
                caja = uLogeado.cajas.FirstOrDefault(c => c.id == id);
            }
            if (caja == null)
            {
                return NotFound();
            }

            return View(caja);
        }

        // POST: CajaDeAhorro/Depositar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Depositar(int? id, float Monto)
        {
            CajaDeAhorro caja;
            if (uLogeado.isAdmin)
            {
                caja = _context.cajas.FirstOrDefault(C => C.id == id);
            }
            else
            {
                caja = uLogeado.cajas.FirstOrDefault(C => C.id == id);
            }
            if (caja == null)
            {
                return NotFound();
            }
            if (Monto <= 0)
            {
                ViewBag.error = 1;
                return View(caja);
            }
            altaMovimiento(caja, "Depositar", Monto);
            caja.saldo += Monto;
            _context.Update(caja);
            _context.SaveChanges();
            return RedirectToAction("Index", "CajaDeAhorroes", new { success = "4" });

        }

        // GET: CajaDeAhorro/Retirar
        public IActionResult Retirar(int? id)
        {
            if (uLogeado == null)
            {
                return RedirectToAction("Index", "Login");
            }

            if (id == null || uLogeado.cajas == null)
            {
                return NotFound();
            }

            CajaDeAhorro caja;
            if (uLogeado.isAdmin)
            {
                caja = _context.cajas.FirstOrDefault(c => c.id == id);
            }
            else
            {
                caja = uLogeado.cajas.FirstOrDefault(c => c.id == id);
            }
            if (caja == null)
            {
                return NotFound();
            }

            return View(caja);
        }

        // POST: CajaDeAhorro/Retirar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Retirar(int? id, float Monto)
        {
            CajaDeAhorro caja;
            if (uLogeado.isAdmin)
            {
                caja = _context.cajas.FirstOrDefault(C => C.id == id);
            }
            else
            {
                caja = uLogeado.cajas.FirstOrDefault(C => C.id == id);
            }
            if (caja == null)
            {
                return NotFound();
            }
            if (Monto < 0)
            {
                ViewBag.error = 0;
                return View(caja);
            }
            if (Monto > caja.saldo)
            {
                ViewBag.error = 1;
                return View(caja);
            }
            altaMovimiento(caja, "Retirar", Monto);
            caja.saldo -= Monto;
            _context.Update(caja);
            _context.SaveChanges();
            return RedirectToAction("Index", "CajaDeAhorroes", new { success = "5" });
        }

        public IActionResult Transferir()
        {
            if (uLogeado == null)
            {
                return RedirectToAction("Index", "Login");
            }
            ViewBag.cbuOrigen = uLogeado.cajas;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Transferir(int CbuOrigen, int CbuDestino, float Monto)
        {
            CajaDeAhorro cajaOrigen = uLogeado.cajas.FirstOrDefault(c => c.cbu == CbuOrigen);
            CajaDeAhorro cajaDestino = _context.cajas.FirstOrDefault(c => c.cbu == CbuDestino);

            // Verificar si existen las cajas de origen y destino
            if (cajaOrigen == null)
            {
                ViewBag.error = 1;
                return View();
            }
            if (cajaDestino == null)
            {
                ViewBag.error = 2;
                return View();
            }

            // Verificar si el monto es válido
            if (Monto <= 0)
            {
                ViewBag.error = 3;
                return View();
            }

            // Verificar si hay saldo suficiente en la caja de origen
            if (cajaOrigen.saldo < Monto)
            {
                ViewBag.error = 4;
                return View();
            }

            // Realizar la transferencia
            cajaOrigen.saldo -= Monto;
            cajaDestino.saldo += Monto;

            // Registrar los movimientos en ambas cajas de ahorro
            altaMovimiento(cajaOrigen, "Transferencia", Monto);
            altaMovimiento(cajaDestino, "Transferencia Recibida", Monto);

            // Actualizar los cambios en la base de datos
            _context.Update(cajaOrigen);
            _context.Update(cajaDestino);
            _context.SaveChanges();

            // Redireccionar a la página de inicio con un mensaje de éxito
            return RedirectToAction("Index", "CajaDeAhorroes", new { success = "6" });
        }


        // GET: CajaDeAhorroes/AgregarTitular
        public async Task<IActionResult> AgregarTitular(int? id)
        {
            if (uLogeado == null)
            {
                return RedirectToAction("Index", "Login");
            }

            if (id == null)
            {
                return NotFound();
            }

            CajaDeAhorro caja = null;

            if (uLogeado.isAdmin)
            {
                caja = await _context.cajas.FirstOrDefaultAsync(C => C.id == id);
                if (caja == null)
                {
                    return NotFound();
                }

                ViewBag.usuarios = await _context.usuarios.ToListAsync();
            }
            else
            {
                caja = uLogeado.cajas.FirstOrDefault(C => C.id == id);
                if (caja == null)
                {
                    return NotFound();
                }
            }

            ViewBag.Titulares = caja.titulares;
            ViewBag.Admin = uLogeado.isAdmin;
            return View(caja);
        }



        // POST: CajaDeAhorro/AgregarTitular
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AgregarTitular(int id, int idUsuario)
        {
            if (uLogeado == null)
            {
                return RedirectToAction("Index", "Login");
            }

            CajaDeAhorro caja = _context.cajas.Include(c => c.titulares).FirstOrDefault(c => c.id == id);

            if (caja == null)
            {
                return NotFound();
            }

            // Verificar si el usuario seleccionado existe
            Usuario usuario = _context.usuarios.FirstOrDefault(u => u.id == idUsuario);
            if (usuario == null)
            {
                ViewBag.error = "El usuario seleccionado no existe.";
                return View(caja);
            }

            // Verificar si el usuario ya es titular de la caja
            if (caja.titulares.Contains(usuario))
            {
                ViewBag.error = "El usuario seleccionado ya es titular de esta caja.";
                return View(caja);
            }

            caja.titulares.Add(usuario);
            usuario.cajas.Add(caja);

            try
            {
                _context.Update(usuario);
                _context.Update(caja);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al agregar el titular: " + ex.Message;
                return View(caja);
            }

            return RedirectToAction("Index", "CajaDeAhorroes", new { success = "7" });
        }



        public IActionResult EliminarTitular(int? id)
        {
            if (uLogeado == null)
            {
                return RedirectToAction("Index", "Login");
            }
            if (id == null || _context.cajas == null)
            {
                return NotFound();
            }
            CajaDeAhorro caja;
            if (uLogeado.isAdmin)
            {
                caja = _context.cajas.FirstOrDefault(C => C.id == id);
            }
            else
            {
                caja = uLogeado.cajas.FirstOrDefault(C => C.id == id);
            }
            ViewBag.Titulares = caja.titulares;
            ViewBag.Admin = uLogeado.isAdmin;
            return View(caja);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarTitular(int id, int idUsuario)
        {
            if (uLogeado == null)
            {
                return RedirectToAction("Index", "Login");
            }

            CajaDeAhorro caja;
            Usuario usuario;
            if (uLogeado.isAdmin)
            {
                caja = _context.cajas.FirstOrDefault(C => C.id == id);
                usuario = _context.usuarios.FirstOrDefault(C => C.id == idUsuario);
            }
            else
            {
                caja = uLogeado.cajas.FirstOrDefault(C => C.id == id);
                usuario = _context.usuarios.FirstOrDefault(C => C.id == idUsuario);
            }
            if (caja == null)
            {
                return NotFound();
            }
            ViewBag.Admin = uLogeado.isAdmin;
            ViewBag.Titulares = caja.titulares;
            if (caja.titulares.Count() == 1)
            {
                ViewBag.error = 1;
                return View(caja);
            }
            caja.titulares.Remove(usuario);
            usuario.cajas.Remove(caja);
            _context.Update(usuario);
            _context.Update(caja);
            _context.SaveChanges();

            return RedirectToAction("Index", "CajaDeAhorroes", new { success = "8" });
        }
        private bool CajaDeAhorroExists(int id)
        {
            return _context.cajas.Any(e => e.id == id);
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
        [HttpGet]
        public async Task<IActionResult> Movimientos(int id)
        {
            if (uLogeado == null)
            {
                return RedirectToAction("Index", "Login");
            }
            @ViewBag.Id_caja = id;
            return View(await _context.movimientos.Where(m => m.id_Caja == id).ToListAsync());
        }
        [HttpPost]
        public async Task<IActionResult> Movimientos(int Id_caja, DateTime Fecha, float Monto, string Detalle)
        {
            @ViewBag.Id_caja = Id_caja;
            if (uLogeado == null)
            {
                return RedirectToAction("Index", "Login");
            }
            DateTime fechadefault = new DateTime(0001, 1, 1, 0, 0, 0);
            if (DateTime.Compare(Fecha, fechadefault) != 0 && Monto != 0 && Detalle != null) // los 3
            {
                return View(await _context.movimientos.Where(movimiento => movimiento.monto == Monto && movimiento.fecha.Date == Fecha.Date && movimiento.detalle == Detalle && movimiento.id_Caja == Id_caja).ToListAsync());
            }
            if (DateTime.Compare(Fecha, fechadefault) != 0 && Monto != 0 && Detalle == null) //Monto y Fecha
            {
                return View(await _context.movimientos.Where(m => m.monto == Monto && m.fecha.Date == Fecha.Date && m.id_Caja == Id_caja).ToListAsync());
            }
            if (DateTime.Compare(Fecha, fechadefault) != 0 && Monto == 0 && Detalle != null) //Fecha y Detalle
            {
                return View(await _context.movimientos.Where(m => m.detalle == Detalle && m.fecha.Date == Fecha.Date && m.id_Caja == Id_caja).ToListAsync());
            }
            if (DateTime.Compare(Fecha, fechadefault) == 0 && Monto != 0 && Detalle != null) //Monto y Detalle
            {
                return View(await _context.movimientos.Where(m => m.detalle == Detalle && m.monto == Monto && m.id_Caja == Id_caja).ToListAsync());
            }
            if (DateTime.Compare(Fecha, fechadefault) != 0 && Monto == 0 && Detalle == null) //Fecha
            {
                return View(await _context.movimientos.Where(movimiento => movimiento.fecha.Date == Fecha.Date && movimiento.id_Caja == Id_caja).ToListAsync());
            }
            if (DateTime.Compare(Fecha, fechadefault) == 0 && Monto != 0 && Detalle == null) //Monto
            {
                return View(await _context.movimientos.Where(movimiento => movimiento.monto == Monto && movimiento.id_Caja == Id_caja).ToListAsync());
            }
            if (DateTime.Compare(Fecha, fechadefault) == 0 && Monto == 0 && Detalle != null) //Detalle
            {
                return View(await _context.movimientos.Where(m => m.detalle == Detalle && m.id_Caja == Id_caja).ToListAsync());
            }

            return View(await _context.movimientos.Where(m => m.id_Caja == Id_caja).ToListAsync());
        }
    }
}
