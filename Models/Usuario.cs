using System.ComponentModel.DataAnnotations;

namespace CheckinLog.Models
{
    public class Usuario
    {
        // Chave primária para o banco de dados
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome de usuário é obrigatório")]
        public string NomeUsuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "A senha é obrigatória")]
        [DataType(DataType.Password)]
        public string Senha { get; set; } = string.Empty;

        [Required(ErrorMessage = "O nível de acesso é obrigatório")]
        public string NivelAcesso { get; set; } = string.Empty;

        // Status do usuário (Ativo ou Inativo)
        public bool Ativo { get; set; } = true;
    }
}