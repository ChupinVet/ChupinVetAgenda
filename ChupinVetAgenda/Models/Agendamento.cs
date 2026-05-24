namespace ChupinVetAgenda.Models;

public class Agendamento
{
    public int Id { get; set; }

    public int HorarioDisponivelId { get; set; }

    public HorarioDisponivel? HorarioDisponivel { get; set; }

    public string NomeResponsavel { get; set; } = string.Empty;

    public string EmailResponsavel { get; set; } = string.Empty;

    public string NomePet { get; set; } = string.Empty;

    public string EspeciePet { get; set; } = string.Empty;

    public string Status { get; set; } = "Marcado";
}