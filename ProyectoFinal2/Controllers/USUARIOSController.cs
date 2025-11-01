using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ProyectoFinal2.Models;
using System.Data.SqlClient;

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
        public ActionResult Login(string usuario, string contrasena)
        {
            try
            {
                // CONEXIÓN DIRECTA CON ADO.NET
                string connectionString = "Server=tcp:sqlserver-mibank-umg-prg2.database.windows.net,1433;Database=sql-mibank-Proyecto2;User ID=usrproyecto;Password=a123d123*;Encrypt=true;TrustServerCertificate=false;Connection Timeout=30;";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Buscar usuario en la base de datos
                    string query = "SELECT * FROM USUARIOS WHERE USUARIO = @Usuario AND CONTRASENA = @Contrasena";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Usuario", usuario);
                        command.Parameters.AddWithValue("@Contrasena", contrasena);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Login exitoso
                                USUARIOS usuarioBD = new USUARIOS
                                {
                                    IDUSUARIO = reader.GetInt32(0),
                                    NOMBRECOMPLETO = reader.GetString(1),
                                    USUARIO = reader.GetString(2),
                                    CONTRASENA = reader.GetString(3)
                                };

                                Session["UsuarioActual"] = usuarioBD;
                                return RedirectToAction("Index", "Home");
                            }
                            else
                            {
                                // Redirigir con mensaje de error
                                return RedirectToAction("Login", new { error = "Usuario+o+contraseña+incorrectos" });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Redirigir con mensaje de error detallado
                string errorMsg = $"Error+de+conexión:+{ex.Message.Replace(" ", "+")}";
                return RedirectToAction("Login", new { error = errorMsg });
            }
        }
    }
}
