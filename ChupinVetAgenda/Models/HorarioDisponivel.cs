namespace ChupinVetAgenda.Models;

public class HorarioDisponivel
{
    public int Id { get; set; }
    
    public DateTime DataHora  { get; set; }

    public string TipoAtendimento { get; set; } = string.Empty;

    public string Status { get; set; } = "Dsiponível";
}