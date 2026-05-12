using System.ComponentModel.DataAnnotations.Schema;

namespace CheckinLog.Models
{
    public class Permissao
    {
        public int Id { get; set; }

        public string? NomeExibicao { get; set; }

        // Mapeia para a coluna correta no SQL Server, mas mantém o nome que os Controllers usam
        [Column("ChaveSistema")]
        public string? ChaveSistema { get; set; }

        public int? ParentId { get; set; }
        public string? Tipo { get; set; }
    }
}