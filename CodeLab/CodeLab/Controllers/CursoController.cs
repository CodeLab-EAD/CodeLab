using CodeLab.Autenticacao;
using CodeLab.Database;
using CodeLab.Models;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Data;

namespace CodeLab.Controllers
{
    public class CursoController : Controller
    {
        private readonly DatabaseConnection db = new DatabaseConnection();

        public IActionResult Index()
        {
            var list = new List<CursoModel>();
            var conn = db.GetConnection();
            var cmd = new MySqlCommand("sp_listar_curso", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            using var rows = cmd.ExecuteReader();
            while (rows.Read())
            {
                list.Add(new CursoModel
                {
                    IdCurso = rows.GetInt32("id_curso"),
                    NomeCurso = rows.GetString("nome_curso"),
                    Duracao = rows.GetString("duracao"),
                    FotoCurso = rows["foto_curso"] as string
                });
            }
            return View(list);
        }

        // GET: CADASTRAR CURSO
        [HttpGet, SessionAuthorize(RoleAnyOf ="Gerente, Admin")]
        public IActionResult CadastrarCurso()
        {
            return View();
        }

        [HttpPost, SessionAuthorize(RoleAnyOf ="Gerente, Admin")]
        public IActionResult CadastrarCurso(CursoModel cursoModel, IFormFile? foto)
        {
            string? relPath = null;

            // Upload de imagem (opcional)
            if (foto != null && foto.Length > 0)
            {
                var ext = Path.GetExtension(foto.FileName);
                var fileName = $"{Guid.NewGuid()}{ext}";
                var saveDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "fotos");
                Directory.CreateDirectory(saveDir);
                var absPath = Path.Combine(saveDir, fileName);

                using var fs = new FileStream(absPath, FileMode.Create);
                foto.CopyTo(fs);

                relPath = Path.Combine("fotos", fileName).Replace("\\", "/");
            }

            using var conn = db.GetConnection();
            using var cmd = new MySqlCommand("sp_cadastrar_curso", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("p_nome_curso", cursoModel.NomeCurso);
            cmd.Parameters.AddWithValue("p_duracao", cursoModel.Duracao);
            cmd.Parameters.AddWithValue("p_foto_curso", (object?)relPath ?? DBNull.Value);

            cmd.ExecuteNonQuery();

            TempData["ok"] = "Curso cadastrado com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        [SessionAuthorize(RoleAnyOf ="Admin, Gerente")]
        public IActionResult ExcluirCurso(int id)
        {
            using var conn = db.GetConnection();
            try
            {
                using var cmd = new MySqlCommand("sp_excluir_curso", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("p_id_curso", id);
                cmd.ExecuteNonQuery();

                TempData["ok"] = "Curso excluído com sucesso!";
            }
            catch (MySqlException ex)
            {
                TempData["ok"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet, SessionAuthorize(RoleAnyOf ="Admin, Gerente")]
        public IActionResult EditarCurso(int id)
        {
            using var conn = db.GetConnection();
            CursoModel? cursoModel = null;

            using var cmd = new MySqlCommand("sp_curso_obter", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("p_id_curso", id);

            using var rd = cmd.ExecuteReader();
            if (rd.Read())
            {
                cursoModel = new CursoModel
                {
                    IdCurso = rd.GetInt32("id_curso"),
                    NomeCurso = rd.GetString("nome_curso"),
                    Duracao = rd.GetString("duracao"),
                    FotoCurso = rd.IsDBNull(rd.GetOrdinal("foto_curso")) ? null : rd.GetString("foto_curso")
                };
            }

            if (cursoModel == null) return NotFound();
            return View(cursoModel);
        }

        [HttpPost, SessionAuthorize(RoleAnyOf ="Admin, Gerente")]
        public IActionResult EditarCurso(CursoModel cursoModel, IFormFile? foto)
        {
            if (cursoModel.IdCurso <= 0) return NotFound();
            if (string.IsNullOrWhiteSpace(cursoModel.NomeCurso))
            {
                ModelState.AddModelError("", "Informe o nome do curso");
                return View(cursoModel);
            }

            string? relPath = null;

            // Upload da nova imagem (se enviada)
            if (foto != null && foto.Length > 0)
            {
                var ext = Path.GetExtension(foto.FileName);
                var fileName = $"{Guid.NewGuid()}{ext}";
                var saveDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "fotos");
                Directory.CreateDirectory(saveDir);
                var absPath = Path.Combine(saveDir, fileName);
                using var fs = new FileStream(absPath, FileMode.Create);
                foto.CopyTo(fs);
                relPath = Path.Combine("fotos", fileName).Replace("\\", "/");
            }

            using var conn = db.GetConnection();
            using var cmd = new MySqlCommand("sp_atualizar_curso", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("p_id_curso", cursoModel.IdCurso);
            cmd.Parameters.AddWithValue("p_nome_curso", cursoModel.NomeCurso);
            cmd.Parameters.AddWithValue("p_duracao", cursoModel.Duracao);
            cmd.Parameters.AddWithValue("p_foto_curso", (object?)relPath ?? DBNull.Value);

            cmd.ExecuteNonQuery();

            TempData["ok"] = "Curso atualizado com sucesso!";
            return RedirectToAction(nameof(Index));
        }
    }
}
