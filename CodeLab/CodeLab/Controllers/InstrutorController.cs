using CodeLab.Autenticacao;
using CodeLab.Database;
using CodeLab.Models;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Data;

namespace CodeLab.Controllers
{
    public class InstrutorController : Controller
    {
        private readonly DatabaseConnection db = new DatabaseConnection();

        // LISTAR TODOS OS INSTRUTORES
        public IActionResult Index()
        {
            var list = new List<InstrutorModel>();
            using var conn = db.GetConnection();
            using var cmd = new MySqlCommand("sp_listar_instrutor", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            using var rows = cmd.ExecuteReader();
            while (rows.Read())
            {
                list.Add(new InstrutorModel
                {
                    IdInstrutor = rows.GetInt32("id_instrutor"),
                    NomeInstrutor = rows.GetString("nome_instrutor"),
                    Especialidade = rows.IsDBNull(rows.GetOrdinal("formacao")) ? null : rows.GetString("formacao")
                });
            }

            return View(list);
        }

        // VIEW CADASTRAR
        [SessionAuthorize(RoleAnyOf = "Admin, Gerente")]
        public IActionResult CadastrarInstrutor()
        {
            return View();
        }

        // INSERIR NO BANCO
        [HttpPost, SessionAuthorize(RoleAnyOf = "Admin, Gerente")]
        public IActionResult CadastrarInstrutor(InstrutorModel instrutorModel)
        {
            using var conn = db.GetConnection();
            using var cmd = new MySqlCommand("sp_cadastrar_instrutor", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("p_nome_instrutor", instrutorModel.NomeInstrutor);
            cmd.Parameters.AddWithValue("p_formacao", instrutorModel.Especialidade ?? (object)DBNull.Value);

            cmd.ExecuteNonQuery();
            TempData["ok"] = "Instrutor cadastrado com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        // EXCLUIR INSTRUTOR
        [SessionAuthorize(RoleAnyOf = "Admin, Gerente")]
        public IActionResult ExcluirInstrutor(int id)
        {
            using var conn = db.GetConnection();
            try
            {
                using var cmd = new MySqlCommand("sp_excluir_instrutor", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("p_id_instrutor", id);
                cmd.ExecuteNonQuery();
                TempData["ok"] = "Instrutor excluído com sucesso!";
            }
            catch (MySqlException ex)
            {
                TempData["ok"] = $"Erro ao excluir: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // VIEW EDITAR
        [SessionAuthorize(RoleAnyOf = "Admin, Gerente")]
        public IActionResult EditarInstrutor(int id)
        {
            using var conn = db.GetConnection();
            InstrutorModel? instrutorModel = null;

            using (var cmd = new MySqlCommand("sp_instrutor_obter", conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                cmd.Parameters.AddWithValue("p_id_instrutor", id);
                using var rd = cmd.ExecuteReader();
                if (rd.Read())
                {
                    instrutorModel = new InstrutorModel
                    {
                        IdInstrutor = rd.GetInt32("id_instrutor"),
                        NomeInstrutor = rd.GetString("nome_instrutor"),
                        Especialidade = rd.IsDBNull(rd.GetOrdinal("formacao")) ? null : rd.GetString("formacao")
                    };
                }
            }

            if (instrutorModel == null)
                return NotFound();

            return View(instrutorModel);
        }

        // ATUALIZAR INSTRUTOR
        [HttpPost, SessionAuthorize(RoleAnyOf = "Admin, Gerente")]
        public IActionResult EditarInstrutor(InstrutorModel instrutorModel)
        {
            if (instrutorModel.IdInstrutor <= 0)
                return NotFound();

            if (string.IsNullOrWhiteSpace(instrutorModel.NomeInstrutor))
            {
                ModelState.AddModelError("", "Informe o nome do instrutor");
                return View(instrutorModel);
            }

            using var conn = db.GetConnection();
            using var cmd = new MySqlCommand("sp_atualizar_instrutor", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("p_id_instrutor", instrutorModel.IdInstrutor);
            cmd.Parameters.AddWithValue("p_nome_instrutor", instrutorModel.NomeInstrutor);
            cmd.Parameters.AddWithValue("p_formacao", instrutorModel.Especialidade ?? (object)DBNull.Value);

            cmd.ExecuteNonQuery();
            TempData["ok"] = "Instrutor atualizado com sucesso!";
            return RedirectToAction(nameof(Index));
        }
    }
}
