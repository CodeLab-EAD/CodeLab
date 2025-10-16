using CodeLab.Autenticacao;
using CodeLab.Database;
using CodeLab.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MySql.Data.MySqlClient;
using System.Data;

namespace CodeLab.Controllers
{
    public class AulaController : Controller
    {
        private readonly DatabaseConnection db = new DatabaseConnection();

        private List<SelectListItem> CarregarInstrutores()
        {
            var list = new List<SelectListItem>();
            using var conn = db.GetConnection();
            using var cmd = new MySqlCommand("sp_listar_instrutor", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            using var rows = cmd.ExecuteReader();
            while (rows.Read())
            {
                list.Add(new SelectListItem
                {
                    Value = rows.GetInt32("id_instrutor").ToString(),
                    Text = rows.GetString("nome_instrutor")
                });
            }
            return list;
        }

        private List<SelectListItem> CarregarCursos()
        {
            var list = new List<SelectListItem>();
            using var conn = db.GetConnection();
            using var cmd = new MySqlCommand("sp_listar_curso", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            using var rows = cmd.ExecuteReader();
            while (rows.Read())
            {
                list.Add(new SelectListItem
                {
                    Value = rows.GetInt32("id_curso").ToString(),
                    Text = rows.GetString("nome_curso")
                });
            }
            return list;
        }

        public IActionResult Index()
        {
            var list = new List<AulaModel>();
            using var conn = db.GetConnection();
            using var cmd = new MySqlCommand("sp_listar_aula", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            using var rows = cmd.ExecuteReader();

            while (rows.Read())
            {
                list.Add(new AulaModel
                {
                    IdAula = rows.GetInt32("id_aula"),
                    Instrutor = rows["instrutor"] as string,
                    Curso = rows["curso"] as string,
                });
            }

            return View(list);
        }

        [SessionAuthorize(RoleAnyOf = "Admin, Gerente")]
        public IActionResult CadastrarAula()
        {
            var model = new AulaModel
            {
                Instrutores = CarregarInstrutores(),
                Cursos = CarregarCursos()
            };
            return View(model);
        }

        [HttpPost, SessionAuthorize(RoleAnyOf = "Admin, Gerente")]
        public IActionResult CadastrarAula(AulaModel model)
        {
            if (model.IdInstrutor <= 0)
            {
                TempData["erro"] = "Selecione um instrutor antes de cadastrar.";
                model.Instrutores = CarregarInstrutores();
                model.Cursos = CarregarCursos();
                return View(model);
            }

            if (model.IdCurso <= 0)
            {
                TempData["erro"] = "Selecione um curso antes de cadastrar.";
                model.Instrutores = CarregarInstrutores();
                model.Cursos = CarregarCursos();
                return View(model);
            }

            try
            {
                using var conn = db.GetConnection();
                using var cmd = new MySqlCommand("sp_cadastrar_aula", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("p_id_instrutor", model.IdInstrutor);
                cmd.Parameters.AddWithValue("p_id_curso", model.IdCurso);
                cmd.ExecuteNonQuery();

                TempData["ok"] = "Aula cadastrada com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            catch (MySqlException ex)
            {
                TempData["erro"] = $"Erro ao cadastrar aula: {ex.Message}";
                model.Instrutores = CarregarInstrutores();
                model.Cursos = CarregarCursos();
                return View(model);
            }
        }

        [SessionAuthorize(RoleAnyOf = "Admin, Gerente")]
        public IActionResult ExcluirAula(int id)
        {
            using var conn = db.GetConnection();
            try
            {
                using var cmd = new MySqlCommand("sp_excluir_aula", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("p_id_aula", id);
                cmd.ExecuteNonQuery();
                TempData["ok"] = "Aula excluída com sucesso!";
            }
            catch (MySqlException ex)
            {
                TempData["erro"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [SessionAuthorize(RoleAnyOf = "Admin, Gerente")]
        public IActionResult EditarAula(int id)
        {
            AulaModel? model = null;
            using var conn = db.GetConnection();
            using (var cmd = new MySqlCommand("sp_aula_obter", conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                cmd.Parameters.AddWithValue("p_id_aula", id);
                using var rd = cmd.ExecuteReader();
                if (rd.Read())
                {
                    model = new AulaModel
                    {
                        IdAula = rd.GetInt32("id_aula"),
                        Curso = rd["nome_curso"] as string,
                        Instrutor = rd["nome_instrutor"] as string
                    };
                }
            }

            if (model == null) return NotFound();

            model.Instrutores = CarregarInstrutores();
            model.Cursos = CarregarCursos();

            return View(model);
        }

        [HttpPost, SessionAuthorize(RoleAnyOf = "Admin, Gerente")]
        public IActionResult EditarAula(AulaModel model)
        {
            if (model.IdAula <= 0)
            {
                TempData["erro"] = "Aula não encontrada.";
                return RedirectToAction(nameof(Index));
            }

            if (model.IdInstrutor <= 0 || model.IdCurso <= 0)
            {
                TempData["erro"] = "Selecione um instrutor e curso válidos.";
                model.Instrutores = CarregarInstrutores();
                model.Cursos = CarregarCursos();
                return View(model);
            }

            try
            {
                using var conn = db.GetConnection();
                using var cmd = new MySqlCommand("sp_atualizar_aula", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("p_id_aula", model.IdAula);
                cmd.Parameters.AddWithValue("p_id_instrutor", model.IdInstrutor);
                cmd.Parameters.AddWithValue("p_id_curso", model.IdCurso);

                cmd.ExecuteNonQuery();

                TempData["ok"] = "Aula atualizada com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            catch (MySqlException ex)
            {
                TempData["erro"] = $"Erro ao atualizar aula: {ex.Message}";
                model.Instrutores = CarregarInstrutores();
                model.Cursos = CarregarCursos();
                return View(model);
            }
        }
    }
}
