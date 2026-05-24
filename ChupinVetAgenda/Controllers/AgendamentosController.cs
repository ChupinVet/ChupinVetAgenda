using ChupinVetAgenda.Data;
using ChupinVetAgenda.Models;
using ChupinVetAgenda.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChupinVetAgenda.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AgendamentosController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly EmailService _emailService;

    public AgendamentosController(
        AppDbContext context,
        EmailService emailService
    )
    {
        _context = context;
        _emailService = emailService;
    }
    
    //GET: api/agendamentos
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Agendamento>>>
        GetAgendamentos()
    {
        return await _context.Agendamentos
            .Include(a => a.HorarioDisponivel)
            .ToListAsync();
    }
    
    //GET: api/agendamentos/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<Agendamento>>
        GetAgendamento(int id)
    {
        var agendamento = await _context.Agendamentos
            .Include(a => a.HorarioDisponivel)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (agendamento == null)
        {
            return NotFound();
        }
        
        return agendamento;
    }
    
    //GET: api/agendamentos/responsavel/email
    [HttpGet("responsavel/{email}")]
    public async Task<ActionResult<IEnumerable<Agendamento>>>
        GetAgendamentosPorResponsavel(
            string email
        )
    {
        return await _context.Agendamentos
            .Include(a => a.HorarioDisponivel)
            .Where(a => a.EmailResponsavel == email)
            .ToListAsync();
    }
    
    //GET: api/agendamentos/veterinario
    [HttpGet("veterinario")]
    public async Task<ActionResult<IEnumerable<Agendamento>>>
        GetAgendamentosVeterinario()
    {
        return await _context.Agendamentos
            .Include(a => a.HorarioDisponivel)
            .Where(a => a.Status == "Marcado")
            .ToListAsync();
    }
    
    //POST: api/agendamentos
    [HttpPost]
    public async Task<ActionResult<Agendamento>>
        PostAgendamento(
            Agendamento agendamento
        )
    {
        var horario =
            await _context.HorariosDisponiveis
                .FindAsync(
                    agendamento.HorarioDisponivelId
                );

        if (horario == null)
        {
            return NotFound(
                "Horário nao encontrado"
            );
        }

        if (horario.Status != "Disponivel")
        {
            return BadRequest(
                "Horario indisponivel"
            );
        }

        horario.Status = "Ocupado";

        _context.Agendamentos
            .Add(agendamento);

        await _context.SaveChangesAsync();

        _emailService.EnviarEmailNovoAgendamento(
            agendamento.NomePet,
            agendamento.EspeciePet,
            horario.TipoAtendimento,
            horario.DataHora
        );

        return CreatedAtAction(
            nameof(GetAgendamentos),
            new { id = agendamento.Id },
            agendamento
        );
    }
    
    //PUT: api/agendamentos/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> PutAgendamento(int id, Agendamento agendamento)
    {
        var agendamentoExistente = await _context.Agendamentos.FindAsync(id);

        if (agendamentoExistente == null)
        {
            return NotFound();
        }
        
        agendamentoExistente.NomeResponsavel = agendamento.NomeResponsavel;
        agendamentoExistente.EmailResponsavel = agendamento.EmailResponsavel;
        agendamentoExistente.NomePet = agendamento.NomePet;
        agendamentoExistente.EspeciePet = agendamento.EspeciePet;
        agendamentoExistente.Status = agendamento.Status;
        
        await _context.SaveChangesAsync();
        
        return NoContent();
    }
    
    //DELETE: api/agendamentos/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult>
        DeleteAgendamento(int id)
    {
        var agendamento =
            await _context.Agendamentos
                .FindAsync(id);

        if (agendamento == null)
        {
            return NotFound();
        }

        var horario =
            await _context.HorariosDisponiveis
                .FindAsync(
                    agendamento.HorarioDisponivelId
                );

        if (horario != null)
        {
            horario.Status = "Disponivel";
        }

        _context.Agendamentos
            .Remove(agendamento);

        await _context.SaveChangesAsync();
        
        return NoContent();
    }
}