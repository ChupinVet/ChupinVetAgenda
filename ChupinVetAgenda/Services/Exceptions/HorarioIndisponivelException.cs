namespace ChupinVetAgenda.Services.Exceptions;

public class HorarioIndisponivelException : Exception
{
    public HorarioIndisponivelException()
        : base("Horario indisponivel")
    {
    } 
}