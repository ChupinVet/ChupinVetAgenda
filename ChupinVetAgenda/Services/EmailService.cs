namespace ChupinVetAgenda.Services;

public class EmailService
{
    public void EnviarEmailNovoAgendamento(string nomePet, string especiePet, string tipoAtendimento, DateTime dataHora)
    {
        Console.WriteLine("===== EMAIL SIMULADO =====");
        Console.WriteLine("Para: veterinario@chupinvet.com");
        Console.WriteLine("Assunto: Novo agendamento marcado");
        Console.WriteLine($"Pet: {nomePet}");
        Console.WriteLine($"Espécie: {especiePet}");
        Console.WriteLine($"Tipo: {tipoAtendimento}");
        Console.WriteLine($"Data/Hora: {dataHora}");
        Console.WriteLine("==========================");
    }
}