namespace ChupinVetAgenda.Services.Exceptions;

public class HorarioNaoEncontradoException : Exception
{
    public HorarioNaoEncontradoException()
        : base("Horario nao encontrado")
    {
    }
}