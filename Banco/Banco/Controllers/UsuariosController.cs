
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
    public class UsuariosController : Controller
    {
        private readonly MiContexto _context;
        private Usuario? uLogeado;

        public UsuariosController(MiContexto context, IHttpContextAccessor httpContextAccessor)
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
            _context.tarjetas.Load();
            _context.pagos.Load();
            _context.movimientos.Load();
            _context.plazosFijos.Load();
            uLogeado = _context.usuarios.Where(u => u.id == httpContextAccessor.HttpContext.Session.GetInt32("UserId")).FirstOrDefault();

        }
        public Usuario usuarioLogeado() //tomar sesion del usuario
        {
            if (HttpContext != null)
            {
                return _context.usuarios.Where(u => u.id == HttpContext.Session.GetInt32("UserId")).FirstOrDefault();
            }
            return null;
        }

        // GET: Usuarios
        public async Task<IActionResult> Index()
        {
            if (uLogeado == null)
            {
                return RedirectToAction("Index", "Login");
            }
            if (!uLogeado.isAdmin)
            {
                return RedirectToAction("Index", "Main");
            }
            ViewBag.Admin = uLogeado.isAdmin;
            return View(await _context.usuarios.ToListAsync());
        }
        [HttpGet]
        public async Task<IActionResult> Index(int success)
        {
            if (uLogeado == null)
            {
                return RedirectToAction("Index", "Login");
            }
            if (!uLogeado.isAdmin)
            {
                return RedirectToAction("Index", "Main");
            }
            ViewBag.success = success;
            ViewBag.Admin = uLogeado.isAdmin;
            return View(await _context.usuarios.ToListAsync());
        }
        // GET: Usuarios/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Usuarios/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,dni,nombre,apellido,mail,intentosFallidos,bloqueado,password,isAdmin")] Usuario usuario)
        {
            if (ModelState.IsValid)
            {
                usuario.password = BCrypt.Net.BCrypt.HashPassword(usuario.password);
                _context.Add(usuario);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage2"] = GetFieldNameCreate();


                return RedirectToAction(nameof(Index));
            }

            return View(usuario);
        }
        private string GetFieldNameCreate()
        {
            return "El usuario se creó correctamente";
        }
        // GET: Usuarios/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (uLogeado == null)
            {
                return RedirectToAction("Index", "Login");
            }
            if (!uLogeado.isAdmin)
            {
                return RedirectToAction("Index", "Main");
            }
            if (id == null || _context.usuarios == null)
            {
                return NotFound();
            }

            var usuario = await _context.usuarios
                .FirstOrDefaultAsync(m => m.id == id);
            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }
        // GET: Usuarios/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (uLogeado == null)
            {
                return RedirectToAction("Index", "Login");
            }
            if (!uLogeado.isAdmin)
            {
                return RedirectToAction("Index", "Main");
            }
            if (id == null || _context.usuarios == null)
            {
                return NotFound();
            }
            var usuario = await _context.usuarios
                .FirstOrDefaultAsync(m => m.id == id);
            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        // POST: Usuarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.usuarios == null)
            {
                return Problem("Entity set 'MiContexto.usuarios' is null.");
            }

            if (!uLogeado.isAdmin)
            {
                return RedirectToAction("Index", "Main");
            }

            var usuario = await _context.usuarios.FindAsync(id);
            if (usuario != null)
            {
                _context.usuarios.Remove(usuario);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage1"] = GetFieldNameDelete();

            }

            return RedirectToAction(nameof(Index));
        }
        private string GetFieldNameDelete()
        {
            return "El usuario se eliminó correctamente";
        }


        // GET: Usuarios/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (uLogeado == null)
            {
                return RedirectToAction("Index", "Login");
            }
            if (!uLogeado.isAdmin)
            {
                return RedirectToAction("Index", "Main");
            }
            if (id == null || _context.usuarios == null)
            {
                return NotFound();
            }

            var usuario = await _context.usuarios.FirstOrDefaultAsync(u => u.id == id);
            if (usuario == null)
            {
                return NotFound();
            }
            return View(usuario);
        }

        // POST: Usuarios/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,dni,nombre,apellido,mail,intentosFallidos,bloqueado,password,isAdmin")] Usuario usuario)
        {
            if (id != usuario.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingUsuario = await _context.usuarios.FindAsync(id);
                    if (existingUsuario == null)
                    {
                        return NotFound();
                    }

                    string campoModificado = GetFieldNameEdited(usuario, existingUsuario);

                    // Actualizar solo las propiedades necesarias
                    existingUsuario.dni = usuario.dni;
                    existingUsuario.nombre = usuario.nombre;
                    existingUsuario.apellido = usuario.apellido;
                    existingUsuario.mail = usuario.mail;
                    existingUsuario.intentosFallidos = usuario.intentosFallidos;
                    existingUsuario.bloqueado = usuario.bloqueado;
                    existingUsuario.password = BCrypt.Net.BCrypt.HashPassword(existingUsuario.password);
                    existingUsuario.isAdmin = usuario.isAdmin;

                    _context.Entry(existingUsuario).State = EntityState.Modified; // Marcar la entidad como modificada
                    await _context.SaveChangesAsync();

                    if (!string.IsNullOrEmpty(campoModificado))
                    {
                        TempData["SuccessMessage"] = campoModificado;
                    }

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UsuarioExists(usuario.id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(usuario);
        }
        private string GetFieldNameEdited(Usuario usuario, Usuario existingUsuario)
        {
            if (!string.IsNullOrEmpty(usuario.password) && usuario.password != existingUsuario.password) return "Password";
            if (!string.IsNullOrEmpty(usuario.nombre) && usuario.nombre != existingUsuario.nombre) return "Nombre";
            if (!string.IsNullOrEmpty(usuario.apellido) && usuario.apellido != existingUsuario.apellido) return "Apellido";
            if (!string.IsNullOrEmpty(usuario.mail) && usuario.mail != existingUsuario.mail) return "Mail";
            if (usuario.bloqueado != null && usuario.bloqueado != existingUsuario.bloqueado) return "Bloqueado";
            if (usuario.isAdmin != null && usuario.isAdmin != existingUsuario.isAdmin) return "Admin";
            return string.Empty;
        }

        public IActionResult Bloquear(int id)
        {
            if (uLogeado == null)
            {
                return RedirectToAction("Index", "Login");
            }
            if (!uLogeado.isAdmin)
            {
                return RedirectToAction("Index", "Main");
            }
            Usuario usuario = _context.usuarios.FirstOrDefault(u => u.id == id);

            usuario.bloqueado = !usuario.bloqueado;
            _context.Update(usuario);
            _context.SaveChanges();

            return RedirectToAction("Index", "Usuarios", new { success = 1 });
        }
        public IActionResult Admin(int id)
        {
            if (uLogeado == null)
            {
                return RedirectToAction("Index", "Login");
            }
            if (!uLogeado.isAdmin)
            {
                return RedirectToAction("Index", "Main");
            }
            Usuario usuario = _context.usuarios.FirstOrDefault(u => u.id == id);

            usuario.isAdmin = !usuario.isAdmin;
            _context.Update(usuario);
            _context.SaveChanges();

            return RedirectToAction("Index", "Usuarios", new { success = 2 });
        }


        private bool UsuarioExists(int id)
        {
          return (_context.usuarios?.Any(e => e.id == id)).GetValueOrDefault();
        }
    }
}







