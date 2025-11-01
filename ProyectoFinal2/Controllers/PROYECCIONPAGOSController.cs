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
    public class PROYECCIONPAGOSController : Controller
    {
        private Entities db = new Entities();

        // GET: PROYECCIONPAGOS
        public ActionResult Index()
        {
            var pROYECCIONPAGOS = db.PROYECCIONPAGOS.Include(p => p.PRESTAMOS);
            return View(pROYECCIONPAGOS.ToList());
        }

        // GET: PROYECCIONPAGOS/Details/5
        public ActionResult Details(decimal id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PROYECCIONPAGOS pROYECCIONPAGOS = db.PROYECCIONPAGOS.Find(id);
            if (pROYECCIONPAGOS == null)
            {
                return HttpNotFound();
            }
            return View(pROYECCIONPAGOS);
        }

        // GET: PROYECCIONPAGOS/Create
        public ActionResult Create()
        {
            ViewBag.IDPRESTAMO = new SelectList(db.PRESTAMOS, "IDPRESTAMO", "ENTIDADFINANCIERA");
            return View();
        }

        // POST: PROYECCIONPAGOS/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IDPROYECCION,IDPRESTAMO,NUMEROCUOTA,FECHAPAGO,MONTOPAGO,ESTADOPAGO")] PROYECCIONPAGOS pROYECCIONPAGOS)
        {
            if (ModelState.IsValid)
            {
                db.PROYECCIONPAGOS.Add(pROYECCIONPAGOS);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.IDPRESTAMO = new SelectList(db.PRESTAMOS, "IDPRESTAMO", "ENTIDADFINANCIERA", pROYECCIONPAGOS.IDPRESTAMO);
            return View(pROYECCIONPAGOS);
        }

        // GET: PROYECCIONPAGOS/Edit/5
        public ActionResult Edit(decimal id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PROYECCIONPAGOS pROYECCIONPAGOS = db.PROYECCIONPAGOS.Find(id);
            if (pROYECCIONPAGOS == null)
            {
                return HttpNotFound();
            }
            ViewBag.IDPRESTAMO = new SelectList(db.PRESTAMOS, "IDPRESTAMO", "ENTIDADFINANCIERA", pROYECCIONPAGOS.IDPRESTAMO);
            return View(pROYECCIONPAGOS);
        }

        // POST: PROYECCIONPAGOS/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IDPROYECCION,IDPRESTAMO,NUMEROCUOTA,FECHAPAGO,MONTOPAGO,ESTADOPAGO")] PROYECCIONPAGOS pROYECCIONPAGOS)
        {
            if (ModelState.IsValid)
            {
                db.Entry(pROYECCIONPAGOS).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.IDPRESTAMO = new SelectList(db.PRESTAMOS, "IDPRESTAMO", "ENTIDADFINANCIERA", pROYECCIONPAGOS.IDPRESTAMO);
            return View(pROYECCIONPAGOS);
        }

        // GET: PROYECCIONPAGOS/Delete/5
        public ActionResult Delete(decimal id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PROYECCIONPAGOS pROYECCIONPAGOS = db.PROYECCIONPAGOS.Find(id);
            if (pROYECCIONPAGOS == null)
            {
                return HttpNotFound();
            }
            return View(pROYECCIONPAGOS);
        }

        // POST: PROYECCIONPAGOS/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(decimal id)
        {
            PROYECCIONPAGOS pROYECCIONPAGOS = db.PROYECCIONPAGOS.Find(id);
            db.PROYECCIONPAGOS.Remove(pROYECCIONPAGOS);
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
