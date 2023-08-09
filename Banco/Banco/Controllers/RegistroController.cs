using Banco.Data;
using Banco.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using BCrypt.Net;

namespace Banco.Controllers
{
    public class RegistroController : Controller
    {
        private readonly MiContexto _context;
        private Usuario? uLogeado;

        public RegistroController(MiContexto context, IHttpContextAccessor httpContextAccessor)
        { //Relaciones del context
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

        // GET: RegistroController
        public ActionResult Index()
        {
            if (uLogeado != null)
            {
                return RedirectToAction("Index", "Main");
            }
            ViewBag.logeado = "no";
            return View();
        }
        [HttpPost]
        public ActionResult Index([Bind("id,dni,nombre,apellido,mail,password")] Usuario usuario)
        {
            if (uLogeado != null)
            {
                return RedirectToAction("Index", "Main");
            }
            ViewBag.logeado = "no";
            if (_context.usuarios.Any(us => us.dni == usuario.dni))
            {
                ViewBag.error = 0;
                return View();
            }
            if (ModelState.IsValid)
            {
                usuario.bloqueado = false;
                usuario.isAdmin = false;
                usuario.intentosFallidos = 0;
                usuario.password = BCrypt.Net.BCrypt.HashPassword(usuario.password);

                _context.usuarios.Add(usuario);
                _context.SaveChanges();
                return RedirectToAction("Index", "Login");
            }
          

            return View();

        }
    }
}
