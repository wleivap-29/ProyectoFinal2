using System;
using System.Linq;
using System.Web.Mvc;
using ProyectoFinal2.Models;
using System.Globalization;
using System.Collections.Generic;


namespace ProyectoFinal2.Controllers
{
    public class PRESTAMOSController : Controller
    {
        private Entities db = new Entities();

        // API: devolver lista en JSON
        public JsonResult Listar()
        {
            var data = db.PRESTAMOS
                .ToList()
                .Select(p => new {
                    p.IDPRESTAMO,
                    p.ENTIDADFINANCIERA,
                    p.MONTO,
                    p.PLAZOMESES,
                    p.TASAINTERES,
                    p.TIPODEPAGO,
                    FECHAINICIO = p.FECHAINICIO.ToString("yyyy-MM-dd"),
                    FECHAVENCIMIENTO = p.FECHAVENCIMIENTO.ToString("yyyy-MM-dd"),
                    p.INTERESCALCULADO,
                    p.ESTADO
                });

            return Json(data, JsonRequestBehavior.AllowGet);
        }


        public JsonResult Obtener(int id)
        {
            var p = db.PRESTAMOS.Find(id);
            if (p == null) return Json(null, JsonRequestBehavior.AllowGet);

            return Json(new
            {
                p.IDPRESTAMO,
                p.ENTIDADFINANCIERA,
                MONTO = p.MONTO.ToString("0.##", CultureInfo.InvariantCulture),   // ⬅️ clave
                p.PLAZOMESES,
                p.TIPODEPAGO,
                TASAINTERES = p.TASAINTERES.ToString("0.##", CultureInfo.InvariantCulture),
                FECHAINICIO = p.FECHAINICIO.ToString("yyyy-MM-dd")
            }, JsonRequestBehavior.AllowGet);
        }



        [HttpPost]
        public JsonResult Editar(
            int IDPRESTAMO,
            string ENTIDADFINANCIERA,
            string MONTO,
            byte PLAZOMESES,
            string TIPODEPAGO,
            string TASAINTERES,
            string FECHAINICIO
        )
        {
            try
            {
                var p = db.PRESTAMOS.Find(IDPRESTAMO);
                if (p == null) return Json(new { success = false, message = "No existe" });

                var monto = decimal.Parse(MONTO, CultureInfo.InvariantCulture);
                var tasa = decimal.Parse(TASAINTERES, CultureInfo.InvariantCulture);
                var inicio = DateTime.ParseExact(FECHAINICIO, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                p.ENTIDADFINANCIERA = ENTIDADFINANCIERA;
                p.MONTO = monto;
                p.PLAZOMESES = PLAZOMESES;
                p.TIPODEPAGO = TIPODEPAGO;
                p.TASAINTERES = tasa;
                p.FECHAINICIO = inicio;

                p.FECHAVENCIMIENTO = inicio.AddMonths(PLAZOMESES);
                p.INTERESCALCULADO = monto * (tasa / 100m);

                db.SaveChanges();
                return Json(new { success = true, message = "Préstamo actualizado" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult Eliminar(int id)
        {
            try
            {
                var p = db.PRESTAMOS.Find(id);
                if (p == null)
                    return Json(new { success = false, message = "No existe" });

                // ✅ 1) Buscar cuotas asociadas
                var cuotas = db.PROYECCIONPAGOS
                    .Where(x => x.IDPRESTAMO == id)
                    .ToList();

                // ✅ 2) Borrarlas si existen
                if (cuotas.Any())
                    db.PROYECCIONPAGOS.RemoveRange(cuotas);

                // ✅ 3) Borrar préstamo
                db.PRESTAMOS.Remove(p);

                db.SaveChanges();

                return Json(new { success = true, message = "Préstamo eliminado" });
            }
            catch (Exception ex)
            {
                var msg = ex.InnerException?.InnerException?.Message
                       ?? ex.InnerException?.Message
                       ?? ex.Message;

                return Json(new { success = false, message = msg });
            }
        }

        [HttpPost]
        public JsonResult GenerarProyeccion(int idPrestamo)
        {
            try
            {
                var p = db.PRESTAMOS.Find(idPrestamo);
                if (p == null)
                    return Json(new { success = false, message = "Préstamo no encontrado." });

                decimal montoPrestamo = p.MONTO;
                decimal tasa = p.TASAINTERES;
                byte plazo = p.PLAZOMESES;

                decimal interesTotal = p.INTERESCALCULADO ?? 0m;

                decimal capitalMensual = montoPrestamo / plazo;
                decimal interesMensual = (montoPrestamo * (tasa / 100m)) / 12m;


                /* =====================================================
                   ✅     SALDO PENDIENTE
                ===================================================== */

                // Total a cobrar = capital + intereses
                decimal totalPrestamo = montoPrestamo + interesTotal;

                // Total ya pagado (suma de MONTOPAGO)
                decimal saldoPagado = db.PROYECCIONPAGOS
                    .Where(x => x.IDPRESTAMO == idPrestamo && x.ESTADOPAGO == "Pagado")
                    .Select(x => (decimal?)x.MONTOPAGO ?? 0m)
                    .DefaultIfEmpty(0m)
                    .Sum();

                decimal saldoPendiente = totalPrestamo - saldoPagado;


                /* =====================================================
                   ✅     SI YA EXISTE PROYECCIÓN
                ===================================================== */

                var cuotasDB = db.PROYECCIONPAGOS
                    .Where(x => x.IDPRESTAMO == idPrestamo)
                    .OrderBy(x => x.NUMEROCUOTA)
                    .ToList();

                if (cuotasDB.Any())
                {
                    var listaExistente = cuotasDB.Select(c =>
                    {
                        decimal capitalCalc;
                        decimal interesCalc;

                        if (p.TIPODEPAGO == "Mensual")
                        {
                            capitalCalc = capitalMensual;
                            interesCalc = interesMensual;
                        }
                        else   // ✅ FINAL
                        {
                            capitalCalc = capitalMensual;
                            interesCalc = (c.NUMEROCUOTA == plazo ? interesTotal : 0m);
                        }

                        decimal total = Math.Round(capitalCalc + interesCalc, 2);

                        return new
                        {
                            cuota = c.NUMEROCUOTA,
                            fecha = c.FECHAPAGO.ToString("yyyy-MM-dd"),
                            capital = Math.Round(capitalCalc, 2),
                            interes = Math.Round(interesCalc, 2),
                            total = total,
                            estado = c.ESTADOPAGO ?? "Pendiente"
                        };
                    });

                    return Json(new
                    {
                        success = true,
                        info = new
                        {
                            monto = montoPrestamo,
                            tasa = tasa,
                            plazo = plazo,
                            interesTotal = interesTotal,
                            saldoPendiente = saldoPendiente
                        },
                        cuotas = listaExistente
                    });
                }


                /* =====================================================
                   ✅     SI NO EXISTE → GENERAR
                ===================================================== */

                var listaNueva = new List<object>();

                for (int i = 1; i <= plazo; i++)
                {
                    decimal capital = capitalMensual;
                    decimal interes;

                    if (p.TIPODEPAGO == "Mensual")
                        interes = interesMensual;
                    else
                        interes = (i == plazo) ? interesTotal : 0m;   // ✅ FINAL

                    decimal total = Math.Round(capital + interes, 2);
                    DateTime fecha = p.FECHAINICIO.AddMonths(i - 1);

                    var proy = new PROYECCIONPAGOS
                    {
                        IDPRESTAMO = p.IDPRESTAMO,
                        NUMEROCUOTA = (byte)i,
                        FECHAPAGO = fecha,
                        MONTOPAGO = total,
                        ESTADOPAGO = "Pendiente"
                    };

                    db.PROYECCIONPAGOS.Add(proy);

                    listaNueva.Add(new
                    {
                        cuota = i,
                        fecha = fecha.ToString("yyyy-MM-dd"),
                        capital = Math.Round(capital, 2),
                        interes = Math.Round(interes, 2),
                        total = total,
                        estado = "Pendiente"
                    });
                }

                db.SaveChanges();

                return Json(new
                {
                    success = true,
                    info = new
                    {
                        monto = montoPrestamo,
                        tasa = tasa,
                        plazo = plazo,
                        interesTotal = interesTotal,
                        saldoPendiente = saldoPendiente
                    },
                    cuotas = listaNueva
                });
            }
            catch (Exception ex)
            {
                var msg = ex.InnerException?.InnerException?.Message
                       ?? ex.InnerException?.Message
                       ?? ex.Message;

                return Json(new { success = false, message = msg });
            }
        }


        [HttpPost]
        public JsonResult MarcarCuotaPagada(int idPrestamo, int numeroCuota, string estado)
        {
            try
            {
                var cuota = db.PROYECCIONPAGOS
                    .FirstOrDefault(x => x.IDPRESTAMO == idPrestamo && x.NUMEROCUOTA == numeroCuota);

                if (cuota == null)
                    return Json(new { success = false, message = "No se encontró la cuota" });

                cuota.ESTADOPAGO = estado;
                db.SaveChanges();

                return Json(new { success = true, message = "Actualizado" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }



        [HttpPost]
        public JsonResult Crear(PRESTAMOS prestamo)
        {
            try
            {
                if (!ModelState.IsValid)
                    return Json(new { success = false, message = "Modelo inválido" });

                if (prestamo.FECHAINICIO == null)
                    return Json(new { success = false, message = "FECHAINICIO vacío" });

                prestamo.FECHAVENCIMIENTO = prestamo.FECHAINICIO.AddMonths(prestamo.PLAZOMESES);
                prestamo.INTERESCALCULADO = prestamo.MONTO * (prestamo.TASAINTERES / 100);
                prestamo.ESTADO = "ACTIVO";

                db.PRESTAMOS.Add(prestamo);
                db.SaveChanges();

                return Json(new { success = true, message = "Préstamo registrado correctamente" });
            }
            catch (Exception ex)
            {
                var msg = ex.InnerException?.InnerException?.Message
                          ?? ex.InnerException?.Message
                          ?? ex.Message;

                return Json(new { success = false, message = msg });
            }

        }



    }
}
