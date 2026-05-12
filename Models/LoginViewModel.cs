namespace CheckinLog.Models
{
    public class LoginViewModel
    {
        // Inicializado com string.Empty para evitar avisos de nulabilidade (CS8618)
        public string Usuario { get; set; } = string.Empty;

        public string Senha { get; set; } = string.Empty;
    }
}