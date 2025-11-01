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
    public class INVERSIONESController : Controller
    {
        private Entities db = new Entities();
        [AllowAnonymous]
        public JsonResult ListarInversiones()
        {
            try
            {
                var lista = db.INVERSIONES
                    .Include(i => i.CLIENTES)
                    .AsNoTracking()
                    .ToList()
                    .Select(i => new
                    {
                        IDINVERSION = i.IDINVERSION,
                        CIF = i.IDCLIENTE,
                        NOMBRECLIENTE = i.CLIENTES != null ? i.CLIENTES.NOMBRECOMPLETO : "(sin cliente)",
                        MONTO = i.MONTO,
                        PLAZOMESES = i.PLAZOMESES,
                        TASA = ObtenerTasa((int)i.PLAZOMESES),
                        INTERESESGANADOS = i.INTERESESGANADOS,
                        FECHAINICIO = i.FECHAINICIO.ToString("dd/MM/yyyy"),
                        FECHAVENCIMIENTO = i.FECHAVENCIMIENTO.ToString("dd/MM/yyyy"),
                        ESTADO = i.ESTADO
                    })
                    .ToList();

                return new JsonResult
                {
                    Data = lista,
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    MaxJsonLength = int.MaxValue // 🔹 evita truncamiento o errores por tamaño
                };
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al listar inversiones: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }






        [HttpPost]
        //[ValidateAntiForgeryToken]
        public JsonResult CrearInversion(decimal monto, int plazoMeses, string tipoPago)
        {
            try
            {
                if (Session["NuevoClienteId"] == null)
                {
                    return Json(new { success = false, message = "Debe registrar un cliente antes de crear una inversión." });
                }

                decimal idCliente = Convert.ToDecimal(Session["NuevoClienteId"]);
                decimal tasa = ObtenerTasa(plazoMeses);
                decimal interesGanado = monto * (tasa / 100m) * (plazoMeses / 12m);
                var nueva = new INVERSIONES
                {
                    IDCLIENTE = idCliente,
                    MONTO = monto,
                    PLAZOMESES = (byte)plazoMeses,
                    TIPODEPAGO = tipoPago,
                    FECHAINICIO = DateTime.Now,
                    FECHAVENCIMIENTO = DateTime.Now.AddMonths(plazoMeses),
                    INTERESESGANADOS = interesGanado,
                    ESTADO = "Activa"
                };

                db.INVERSIONES.Add(nueva);
                db.SaveChanges();
                return Json(new
                {
                    success = true,
                    message = $"Inversión creada correctamente. Interés total: Q{interesGanado:N2}"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error de base de datos: " + ex.Message });
            }
        }



        [HttpPost]
        public JsonResult EliminarInversion(decimal id)
        {
            try
            {
                var inversion = db.INVERSIONES.Find(id);
                if (inversion == null)
                {
                    return Json(new { success = false, message = "La inversión no existe o ya fue eliminada." });
                }

                db.INVERSIONES.Remove(inversion);
                db.SaveChanges();

                return Json(new { success = true, message = "Inversión eliminada correctamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al eliminar inversión: " + ex.Message });
            }
        }



        [HttpPost]
        public JsonResult EditarInversion(decimal id, decimal monto, int plazoMeses, string tipoPago, string estado)
        {
            try
            {
                var inversion = db.INVERSIONES.Find(id);
                if (inversion == null)
                {
                    return Json(new { success = false, message = "La inversión no existe." });
                }

                decimal tasa = ObtenerTasa(plazoMeses);
                decimal interesGanado = monto * (tasa / 100m) * (plazoMeses / 12m);

                inversion.MONTO = monto;
                inversion.PLAZOMESES = (byte)plazoMeses;
                inversion.TIPODEPAGO = tipoPago;
                inversion.INTERESESGANADOS = interesGanado;
                inversion.FECHAVENCIMIENTO = inversion.FECHAINICIO.AddMonths(plazoMeses);
                inversion.ESTADO = estado;

                db.Entry(inversion).State = EntityState.Modified;
                db.SaveChanges();

                return Json(new
                {
                    success = true,
                    message = $"Inversión actualizada correctamente. Nuevo interés total: Q{interesGanado:N2}"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al editar inversión: " + ex.Message });
            }
        }



        private decimal ObtenerTasa(int meses)
        {
            switch (meses)
            {
                case 1:
                case 2: return 2.25m;
                case 3: return 2.50m;
                case 6:
                case 9: return 2.75m;
                case 12: return 3.00m;
                default: return 2.00m;
            }
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing && db != null)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
