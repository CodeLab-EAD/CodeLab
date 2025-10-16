using Microsoft.AspNetCore.Mvc.Rendering;

namespace CodeLab.Models
{
    public class AulaModel
    {
        public int IdCurso { get; set; }
        public int IdAula { get; set; }
        public int IdInstrutor { get; set; }

        public string? Instrutor { get; set; }
        public string? Curso { get; set; }

        public List<SelectListItem> Instrutores { get; set; } = new();
        public List<SelectListItem> Cursos { get; set; } = new();

    }
}
