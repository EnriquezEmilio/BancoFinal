using Banco.Data;
using Banco.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Banco.Controllers
{
    public class MainController : Controller
    {
        private readonly MiContexto _context;
        private Usuario? uLogeado;

        public MainController(MiContexto context, IHttpContextAccessor httpContextAccessor)
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

        // GET: MainController
        public ActionResult Index()
        {
            if (uLogeado == null)
            {
                return RedirectToAction("Index", "Login");
            }
           
                ViewBag.Admin = uLogeado.isAdmin;
                ViewBag.NombreUsuario = uLogeado.nombre;
                ViewBag.ApellidoUsuario = uLogeado.apellido;
            
            return View();
        }
      

    }
}
