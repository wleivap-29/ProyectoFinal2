using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ProyectoFinal2.Models;

namespace ProyectoFinal2.Controllers
{
    public class USUARIOSController : Controller
    {
        private Entities db = new Entities();

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "NOMBRECOMPLETO,USUARIO,CONTRASENA")] USUARIOS uSUARIOS)
        {
            if (ModelState.IsValid)
            {
                if (db.USUARIOS.Any(u => u.USUARIO == uSUARIOS.USUARIO))
                {
                    ModelState.AddModelError("", "El nombre de usuario ya está en uso.");
                    return View("Login", uSUARIOS);
                }

                db.USUARIOS.Add(uSUARIOS);
                db.SaveChanges();

                ViewBag.RegistroExitoso = true;
                return View("Login");
            }
            return View(uSUARIOS);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(string usuario, string nuevaContrasena)
        {
            if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(nuevaContrasena))
            {
                ViewBag.ErrorReset = "Debes completar ambos campos.";
                return View("Login");
            }
            var user = db.USUARIOS.SingleOrDefault(u => u.USUARIO == usuario);
            if (user == null)
            {
                ViewBag.ErrorReset = "El usuario no existe.";
                return View("Login");
            }
            user.CONTRASENA = nuevaContrasena; 
            db.Entry(user).State = EntityState.Modified;
            db.SaveChanges();
            ViewBag.ResetOk = true;
            return View("Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string usuario, string contrasena)
        {
            var usuarioBD = db.USUARIOS.FirstOrDefault(u => u.USUARIO == usuario && u.CONTRASENA == contrasena);
            if (usuarioBD != null)
            {
                Session["UsuarioActual"] = usuarioBD;
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.Error = "Usuario o contraseña incorrectos";
                return View();
            }
        }

        public ActionResult Index()
        {
            return View(db.USUARIOS.ToList());
        }

        public ActionResult Details(decimal id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            USUARIOS uSUARIOS = db.USUARIOS.Find(id);
            if (uSUARIOS == null)
            {
                return HttpNotFound();
            }
            return View(uSUARIOS);
        }
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registrar([Bind(Include = "NombreCompleto,Usuario,Contrasena")] USUARIOS uSUARIOS)
        {
            if (ModelState.IsValid)
            {
                db.USUARIOS.Add(uSUARIOS);
                db.SaveChanges();
                ViewBag.RegistroExitoso = true;
                return View("Login");
            }
            return View(uSUARIOS);
        }

        public ActionResult Edit(decimal id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            USUARIOS uSUARIOS = db.USUARIOS.Find(id);
            if (uSUARIOS == null)
            {
                return HttpNotFound();
            }
            return View(uSUARIOS);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IDUSUARIO,NOMBRECOMPLETO,USUARIO,CONTRASENA")] USUARIOS uSUARIOS)
        {
            if (ModelState.IsValid)
            {
                db.Entry(uSUARIOS).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(uSUARIOS);
        }

        public ActionResult Delete(decimal id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            USUARIOS uSUARIOS = db.USUARIOS.Find(id);
            if (uSUARIOS == null)
            {
                return HttpNotFound();
            }
            return View(uSUARIOS);
        }

        public ActionResult CerrarSesion()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Login", "USUARIOS");
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(decimal id)
        {
            USUARIOS uSUARIOS = db.USUARIOS.Find(id);
            db.USUARIOS.Remove(uSUARIOS);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
