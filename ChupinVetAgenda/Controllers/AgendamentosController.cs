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
    private readonly ILogger<AgendamentosController> _logger;

    public AgendamentosController(
        AppDbContext context,
        EmailService emailService,
        ILogger<AgendamentosController> logger
    )
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
    }
    
    //GET: api/agendamentos
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Agendamento>>>
        GetAgendamentos()
    {
        _logger.LogInformation(
            "Listando todos os agendamentos."
        );

        return await _context.Agendamentos
            .Include(a => a.HorarioDisponivel)
            .ToListAsync();
    }
    
    //GET: api/agendamentos/{id}
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Agendamento>>
        GetAgendamento(int id)
    {
        var agendamento = await _context.Agendamentos
            .Include(a => a.HorarioDisponivel)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (agendamento == null)
        {
            _logger.LogWarning(
                "Agendamento com id {Id} nao encontrado.",
                id
            );

            return NotFound();
        }
        
        return agendamento;
    }
    
    //GET: api/agendamentos/responsavel/email
    [HttpGet("responsavel/{email}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Agendamento>>>
        GetAgendamentosPorResponsavel(
            string email
        )
    {
        _logger.LogInformation(
            "Buscando agendamentos do responsavel {Email}.",
            email
        );

        return await _context.Agendamentos
            .Include(a => a.HorarioDisponivel)
            .Where(a => a.EmailResponsavel == email)
            .ToListAsync();
    }
    
    //GET: api/agendamentos/veterinario
    [HttpGet("veterinario")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Agendamento>>>
        GetAgendamentosVeterinario()
    {
        _logger.LogInformation(
            "Listando agendamentos marcados para o veterinario."
        );

        return await _context.Agendamentos
            .Include(a => a.HorarioDisponivel)
            .Where(a => a.Status == "Marcado")
            .ToListAsync();
    }
    
    //POST: api/agendamentos
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Agendamento>>
        PostAgendamento(
            Agendamento agendamento
        )
    {
        try
        {
            var horario =
                await _context.HorariosDisponiveis
                    .FindAsync(
                        agendamento.HorarioDisponivelId
                    );

            if (horario == null)
            {
                _logger.LogWarning(
                    "Tentativa de agendar horario inexistente. Id: {HorarioId}.",
                    agendamento.HorarioDisponivelId
                );

                return NotFound(
                    "Horario nao encontrado"
                );
            }

            if (horario.Status != "Disponivel")
            {
                _logger.LogWarning(
                    "Tentativa de agendar horario indisponivel. Id: {HorarioId}.",
                    agendamento.HorarioDisponivelId
                );

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

            _logger.LogInformation(
                "Agendamento criado com id {Id} para o pet {NomePet}.",
                agendamento.Id,
                agendamento.NomePet
            );

            return CreatedAtAction(
                nameof(GetAgendamento),
                new { id = agendamento.Id },
                agendamento
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Erro ao criar agendamento para o pet {NomePet}.",
                agendamento.NomePet
            );

            return StatusCode(
                StatusCodes.Status500InternalServerError
            );
        }
    }
    
    //PUT: api/agendamentos/{id}
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult>
        PutAgendamento(
            int id,
            Agendamento agendamento
        )
    {
        var agendamentoExistente =
            await _context.Agendamentos
                .FindAsync(id);

        if (agendamentoExistente == null)
        {
            _logger.LogWarning(
                "Tentativa de atualizar agendamento inexistente. Id: {Id}.",
                id
            );

            return NotFound();
        }
        
        agendamentoExistente.NomeResponsavel =
            agendamento.NomeResponsavel;

        agendamentoExistente.EmailResponsavel =
            agendamento.EmailResponsavel;

        agendamentoExistente.NomePet =
            agendamento.NomePet;

        agendamentoExistente.EspeciePet =
            agendamento.EspeciePet;

        agendamentoExistente.Status =
            agendamento.Status;
        
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Agendamento com id {Id} atualizado.",
            id
        );
        
        return NoContent();
    }
    
    //DELETE: api/agendamentos/{id}
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult>
        DeleteAgendamento(int id)
    {
        var agendamento =
            await _context.Agendamentos
                .FindAsync(id);

        if (agendamento == null)
        {
            _logger.LogWarning(
                "Tentativa de remover agendamento inexistente. Id: {Id}.",
                id
            );

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

        _logger.LogInformation(
            "Agendamento com id {Id} removido.",
            id
        );
        
        return NoContent();
    }
}