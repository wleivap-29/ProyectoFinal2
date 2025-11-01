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
    public class ESTADODECUENTAsController : Controller
    {
        private Entities db = new Entities();



        public JsonResult ObtenerBalance()
        {
            try
            {
                // --- INGRESOS: Inversiones ---
                var ingresos = db.INVERSIONES
                    .Select(i => new
                    {
                        Fecha = i.FECHAINICIO,
                        Descripcion = "Apertura de inversión #" + i.IDINVERSION + " (Cliente " + i.IDCLIENTE + ")",
                        Autorizacion = (decimal)i.IDINVERSION,
                        Ingreso = (decimal?)i.MONTO,
                        Egreso = (decimal?)null
                    })
                    .ToList(); // ✅ materializamos en memoria

                // --- EGRESOS: Préstamos ---
                var egresos = db.PRESTAMOS
                    .Select(p => new
                    {
                        Fecha = p.FECHAINICIO, // ✅ usa el nombre correcto del modelo
                Descripcion = "Préstamo otorgado a " + p.ENTIDADFINANCIERA,
                        Autorizacion = (decimal)p.IDPRESTAMO,
                        Ingreso = (decimal?)null,
                        Egreso = (decimal?)p.MONTO
                    })
                    .ToList(); // ✅ también materializamos en memoria

                // --- UNIMOS Y ORDENAMOS CRONOLÓGICAMENTE ---
                var movimientos = ingresos
                    .Concat(egresos)
                    .OrderBy(m => m.Fecha)
                    .ToList();

                // --- CALCULAMOS SALDO ACUMULADO ---
                decimal saldo = 0;
                var resultado = movimientos.Select(m => new
                {
                    Fecha = m.Fecha.ToString("dd/MM/yyyy"),
                    m.Descripcion,
                    m.Autorizacion,
                    Ingreso = m.Ingreso ?? 0,
                    Egreso = m.Egreso ?? 0,
                    Saldo = (saldo += (m.Ingreso ?? 0) - (m.Egreso ?? 0))
                });

                return Json(resultado, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al obtener balance: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


















        // GET: ESTADODECUENTAs
        public ActionResult Index()
        {
            return View(db.ESTADODECUENTA.ToList());
        }

        // GET: ESTADODECUENTAs/Details/5
        public ActionResult Details(decimal id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ESTADODECUENTA eSTADODECUENTA = db.ESTADODECUENTA.Find(id);
            if (eSTADODECUENTA == null)
            {
                return HttpNotFound();
            }
            return View(eSTADODECUENTA);
        }

        // GET: ESTADODECUENTAs/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ESTADODECUENTAs/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IDMOVIMIENTO,FECHAMOVIMIENTO,TIPOMOVIMIENTO,DESCRIPCION,MONTO,REFERENCIAID,SALDOACUMULADO")] ESTADODECUENTA eSTADODECUENTA)
        {
            if (ModelState.IsValid)
            {
                db.ESTADODECUENTA.Add(eSTADODECUENTA);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(eSTADODECUENTA);
        }

        // GET: ESTADODECUENTAs/Edit/5
        public ActionResult Edit(decimal id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ESTADODECUENTA eSTADODECUENTA = db.ESTADODECUENTA.Find(id);
            if (eSTADODECUENTA == null)
            {
                return HttpNotFound();
            }
            return View(eSTADODECUENTA);
        }

        // POST: ESTADODECUENTAs/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IDMOVIMIENTO,FECHAMOVIMIENTO,TIPOMOVIMIENTO,DESCRIPCION,MONTO,REFERENCIAID,SALDOACUMULADO")] ESTADODECUENTA eSTADODECUENTA)
        {
            if (ModelState.IsValid)
            {
                db.Entry(eSTADODECUENTA).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(eSTADODECUENTA);
        }

        // GET: ESTADODECUENTAs/Delete/5
        public ActionResult Delete(decimal id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ESTADODECUENTA eSTADODECUENTA = db.ESTADODECUENTA.Find(id);
            if (eSTADODECUENTA == null)
            {
                return HttpNotFound();
            }
            return View(eSTADODECUENTA);
        }

        // POST: ESTADODECUENTAs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(decimal id)
        {
            ESTADODECUENTA eSTADODECUENTA = db.ESTADODECUENTA.Find(id);
            db.ESTADODECUENTA.Remove(eSTADODECUENTA);
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
