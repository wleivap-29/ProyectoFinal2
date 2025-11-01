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
    public class CLIENTESController : Controller
    {
        private Entities db = new Entities();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult CrearCliente(string nombreCompleto, string dpi, string telefono, string correo, string direccion)
        {
            System.Diagnostics.Debug.WriteLine("IDCLIENTE en sesión: " + Session["NuevoClienteId"]);

            try
            {
                var nuevo = new CLIENTES
                {
                    NOMBRECOMPLETO = nombreCompleto,
                    DPI = dpi,
                    TELEFONO = telefono,
                    CORREO = correo,
                    DIRECCION = direccion
                };

                db.CLIENTES.Add(nuevo);
                db.SaveChanges();

                // ⚠️ Recuperar el ID real desde la base, ya que Oracle no siempre devuelve el autoincremento
                var clienteInsertado = db.CLIENTES
                    .OrderByDescending(c => c.IDCLIENTE)
                    .FirstOrDefault();

                if (clienteInsertado == null)
                {
                    return Json(new { success = false, message = "No se pudo obtener el cliente recién creado." });
                }

                // ✅ Guardar ID en sesión
                Session["NuevoClienteId"] = clienteInsertado.IDCLIENTE;

                return Json(new { success = true, message = $"Cliente registrado correctamente con ID {clienteInsertado.IDCLIENTE}" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al registrar cliente: " + ex.Message });
            }
        }

    }
}
