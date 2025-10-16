using CodeLab.Autenticacao;
using CodeLab.Database;
using CodeLab.Models;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Data;

namespace CodeLab.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly DatabaseConnection db = new DatabaseConnection();

        public IActionResult Index()
        {
            var list = new List<UsuarioModel>();
            var conn = db.GetConnection();
            var cmd = new MySqlCommand("sp_listar_usuario", conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            using var rows = cmd.ExecuteReader();

            while (rows.Read())
            {
                list.Add(new UsuarioModel
                {
                    NomeUsuario = rows.GetString("nome_usuario"),
                    IdUsuario = rows.GetInt32("id_usuario"),
                    NivelAcesso = rows.GetString("nivelAcesso"),
                });
            }
            return View(list);
        }

       [HttpGet, SessionAuthorize(RoleAnyOf = "Admin, Comum")]
        public IActionResult CadastrarUsuario()
        {
            return View();
        }
         [HttpPost, SessionAuthorize(RoleAnyOf = "Admin, Comum")]
        public IActionResult CadastrarUsuario(UsuarioModel usuarioModel)
        { 
            using var conn = db.GetConnection();
            using var cmd = new MySqlCommand("sp_cadastrar_usuario", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            var senhaHash = BCrypt.Net.BCrypt.HashPassword(usuarioModel.Senha, workFactor: 12);

            cmd.Parameters.AddWithValue("p_nome", usuarioModel.NomeUsuario);
            cmd.Parameters.AddWithValue("p_email", (object?)usuarioModel.Email);
            cmd.Parameters.AddWithValue("p_senha", senhaHash);
            cmd.Parameters.AddWithValue("p_role", usuarioModel.NivelAcesso);
            cmd.ExecuteNonQuery();
            TempData["ok"] = "Usuário cadastrado!";
            return RedirectToAction(nameof(Index));
        }

    [SessionAuthorize(RoleAnyOf = "Admin, Gerente")]
        public IActionResult ExcluirUsuario(int id)
        {
            using var conn = db.GetConnection();
            try
            {
                using var cmd = new MySqlCommand("sp_excluir_usuario", conn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("p_id_usuario", id);
                cmd.ExecuteNonQuery();
                TempData["ok"] = "Usuário excluído!";
            }
            catch (MySqlException ex)
            {
                TempData["ok"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
       [SessionAuthorize(RoleAnyOf = "Admin, Gerente")]
        public IActionResult EditarUsuario(int id)
        {
            using var conn = db.GetConnection();
            UsuarioModel? usuarioModel = null;

            using (var cmd = new MySqlCommand("sp_usuario_obter", conn) { CommandType = CommandType.StoredProcedure })
            {
                cmd.Parameters.AddWithValue("p_id_usuario", id);
                using var rd = cmd.ExecuteReader();
                if (rd.Read())
                {
                    usuarioModel = new UsuarioModel
                    {
                        IdUsuario = rd.GetInt32("id_usuario"),
                        NomeUsuario = rd.IsDBNull(rd.GetOrdinal("nome_usuario")) ? null : rd.GetString("nome_usuario"),
                        Email = rd.IsDBNull(rd.GetOrdinal("email")) ? null : rd.GetString("email"),
                        Senha = rd.IsDBNull(rd.GetOrdinal("senha")) ? null : rd.GetString("senha"),
                        NivelAcesso = rd.IsDBNull(rd.GetOrdinal("NivelAcesso")) ? null : rd.GetString("NivelAcesso"),
                    };
                }
            }
            if (usuarioModel == null) return NotFound();
            return View(usuarioModel);
        }

          [HttpPost, SessionAuthorize(RoleAnyOf = "Admin, Gerente")]
        public IActionResult EditarUsuario(UsuarioModel usuarioModel)
        {
            if (usuarioModel.IdUsuario <= 0) return NotFound();
            if (string.IsNullOrWhiteSpace(usuarioModel.NomeUsuario))
            {
                ModelState.AddModelError("", "Informe o nome do gênero");
                return View(usuarioModel);
            }

            
            using var conn = db.GetConnection();
            using var cmd = new MySqlCommand("sp_atualizar_usuario", conn) { CommandType = CommandType.StoredProcedure };


            cmd.Parameters.AddWithValue("p_id_usuario", usuarioModel.IdUsuario);
            cmd.Parameters.AddWithValue("p_nome", usuarioModel.NomeUsuario ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("p_email", usuarioModel.Email ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("p_role", usuarioModel.NivelAcesso ?? (object)DBNull.Value);
            if (!string.IsNullOrWhiteSpace(usuarioModel.Senha))
            {
                var senhaHash = BCrypt.Net.BCrypt.HashPassword(usuarioModel.Senha, workFactor: 12);
                cmd.Parameters.AddWithValue("p_senha", senhaHash);
            }
            else
            {
                cmd.Parameters.AddWithValue("p_senha", DBNull.Value);
            }
            cmd.ExecuteNonQuery();

            TempData["ok"] = "Usuário atualizado!";
            return RedirectToAction(nameof(Index));

        }
    }
}
