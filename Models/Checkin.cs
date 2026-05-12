using System.ComponentModel.DataAnnotations;

namespace CheckinLog.Models
{
    public class Checkin
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "A matrícula é obrigatória")]
        public string Matricula { get; set; } = string.Empty;

        [Required(ErrorMessage = "O nome é obrigatório")]
        public string Nome { get; set; } = string.Empty;

        public string? Email { get; set; } = string.Empty;

        public string? Funcao { get; set; } = string.Empty;

        public string? Empresa { get; set; } = string.Empty;

        public string? CaminhoCNH { get; set; }
        public string? CaminhoCursoDefensiva { get; set; }

        public DateTime DataCheckin { get; set; } = DateTime.Now;
    }
}