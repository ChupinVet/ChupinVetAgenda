using ChupinVetAgenda.Data;
using ChupinVetAgenda.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChupinVetAgenda.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HorariosController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<HorariosController> _logger;
    
    public HorariosController(AppDbContext context, ILogger<HorariosController> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    // GET: api/horarios
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<HorarioDisponivel>>>
        GetHorarios()
    {
        _logger.LogInformation("Listando todos os horarios.");
        
        return await _context.HorariosDisponiveis
            .ToListAsync();
    }
    
    // GET: api/horarios/disponiveis
    [HttpGet("disponiveis")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<HorarioDisponivel>>>
        GetDisponiveis()
    {
        _logger.LogInformation("Listando horarios disponiveis.");
        
        return await _context.HorariosDisponiveis
            .Where(h => h.Status == "Disponivel")
            .ToListAsync();
    }
    
    //GET: api/horarios/{id}
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HorarioDisponivel>>
        GetHorario(int id)
    {
        var horario =
            await _context.HorariosDisponiveis
                .FindAsync(id);

        if (horario == null)
        {
            _logger.LogWarning(
                "Horario com id {Id} nao encontrado.",
                id
            );

            return NotFound();
        }

        return horario;
    }
    
    //POST: api/horarios
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<HorarioDisponivel>>
        PostHorario(HorarioDisponivel horario)
    {
        try
        {
            _context.HorariosDisponiveis.Add(horario);

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Horario criado com id {Id}.",
                horario.Id
            );

            return CreatedAtAction(
                nameof(GetHorario),
                new { id = horario.Id },
                horario
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Erro ao criar horario."
            );

            return StatusCode(
                StatusCodes.Status500InternalServerError
            );
        }
    }
    
    // PUT: api/horarios/{id}
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult>
            PutHorario(
                int id,
                HorarioDisponivel horario
            )
        {
            var horarioExistente =
                await _context.HorariosDisponiveis
                    .FindAsync(id);
    
            if (horarioExistente == null)
            {
                _logger.LogWarning(
                    "Tentativa de atualizar horario inexistente. Id: {Id}.",
                    id
                );
    
                return NotFound();
            }
            
            horarioExistente.DataHora =
                horario.DataHora;
            
            horarioExistente.TipoAtendimento =
                horario.TipoAtendimento;
            
            horarioExistente.Status =
                horario.Status;
            
            await _context.SaveChangesAsync();
    
            _logger.LogInformation(
                "Horario com id {Id} atualizado.",
                id
            );
            
            return NoContent();
        }
    
    //DELETE: api/horarios/{id}
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult>
        DeleteHorario(int id)
    {
        var horario =
            await _context.HorariosDisponiveis
                .FindAsync(id);

        if (horario == null)
        {
            _logger.LogWarning(
                "Tentativa de remover horario inexistente. Id: {Id}.",
                id
            );

            return NotFound();
        }

        var possuiAgendamento =
            await _context.Agendamentos
                .AnyAsync(a =>
                    a.HorarioDisponivelId == id
                );

        if (possuiAgendamento)
        {
            _logger.LogWarning(
                "Tentativa de remover horario ocupado. Id: {Id}.",
                id
            );

            return BadRequest(
                "Nao e possivel remover um horario ocupado."
            );
        }

        _context.HorariosDisponiveis
            .Remove(horario);
        
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Horario com id {Id} removido.",
            id
        );
        
        return NoContent();
    }
}