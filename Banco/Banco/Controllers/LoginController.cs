using Banco.Data;
using Banco.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Net;

namespace Banco.Controllers
{
    public class LoginController : Controller
    {
        private readonly MiContexto _context;
        private Usuario? uLogeado;

        public LoginController(MiContexto contexto)
        {
            _context = contexto;
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
            
        }
        public IActionResult Index()
        {
            if (uLogeado != null)
            {
                return RedirectToAction("Index", "Main");
            }
            ViewBag.logeado = "no";
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Index(int Dni, string password)
        {
            try
            {
                var usuario = _context.usuarios.Where(u => u.dni == Dni).FirstOrDefault();
                if (usuario == null)
                {
                    ViewBag.errorLogin = 1;
                    return View();
                }

                if (usuario.bloqueado)
                {
                    ViewBag.errorLogin = 4;
                    return View();
                }

                //if (usuario.password != password)

                if (!BCrypt.Net.BCrypt.Verify(password, usuario.password))
                {
                    usuario.intentosFallidos++;
                    if (usuario.intentosFallidos >= 3)
                    {
                        usuario.bloqueado = true;
                        ViewBag.errorLogin = 3;
                    }
                    else
                    {
                        ViewBag.errorLogin = 2;
                    }

                    _context.Update(usuario);
                    _context.SaveChanges();
                    return View();
                }

                // El usuario proporcionó la contraseña correcta, reiniciamos los intentos fallidos
                usuario.intentosFallidos = 0;
                _context.Update(usuario);
                _context.SaveChanges();

                uLogeado = usuario;

                HttpContext.Session.SetInt32("UserId", usuario.id);

                return RedirectToAction("Index", "Main");
            }
            catch
            {
                return View();
            }
        }


        public IActionResult Logout()
        {
            uLogeado = null;
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Login");
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}