using CodeLab.Autenticacao;
using CodeLab.Database;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace CodeLab.Controllers
{
    public class AuthController : Controller
    {
        private readonly DatabaseConnection db = new DatabaseConnection();
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string senha, string? returnUrl = null)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(senha))
            {
                ViewBag.Error = "informe o email e a senha";
                return View();
            }

            using var conn = db.GetConnection();
            using var cmd = new MySqlCommand("sp_usuario_obter_email", conn) { CommandType = System.Data.CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("p_email_usuario", email);
            using var read = cmd.ExecuteReader();

            if (!read.Read())
            {
                ViewBag.Error = "informe o email e a senha";
                return View();
            }

            var id = read.GetInt32("id_usuario");
            var nome = read.GetString("nome_usuario");
            var role = read.GetString("nivelAcesso");
            var senhaHash = read["senha"] as string ?? "";
            // verificação da senha

            bool ok;

            try
            {
                ok = BCrypt.Net.BCrypt.Verify(senha, senhaHash);

            }
            catch { ok = false; }

            if (!ok)
            {
                ViewBag.Error = "senha inválida";
                return View();
            }

            //setar sessão

            HttpContext.Session.SetInt32(SessionKeys.UserId, id);
            HttpContext.Session.SetString(SessionKeys.UserName, nome);
            HttpContext.Session.SetString(SessionKeys.UserEmail, email);
            HttpContext.Session.SetString(SessionKeys.UserRole, role);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Curso");
        }
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult AcessoNegado() => View();
    }
}

