namespace CheckinLog.Models
{
    public class PerfilPermissao
    {
        public int Id { get; set; }
        public int PerfilId { get; set; }
        public int PermissaoId { get; set; }

        // ESTA É A LINHA QUE FALTA PARA CORRIGIR OS ERROS DO CONTROLLER:
        // Como no seu banco ela é 'True'/'False', usamos bool.
        public bool Ativo { get; set; }

        // O '?' avisa que este campo pode ser nulo, removendo o alerta amarelo
        public virtual Permissao? Permissao { get; set; }
    }
}