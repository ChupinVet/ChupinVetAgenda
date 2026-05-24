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
    
    public HorariosController(AppDbContext context)
    {
        _context = context;
    }
    
    // GET: api/horarios
    [HttpGet]
    public async Task<ActionResult<IEnumerable<HorarioDisponivel>>>
        GetHorarios()
    {
        return await _context.HorariosDisponiveis
            .ToListAsync();
    }
    
    // GET: api/horarios/disponiveis
    [HttpGet("disponiveis")]
    public async Task<ActionResult<IEnumerable<HorarioDisponivel>>>
        GetDisponiveis()
    {
        return await _context.HorariosDisponiveis
            .Where(h => h.Status == "Disponivel")
            .ToListAsync();
    }
    
    //GET: api/horarios/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<HorarioDisponivel>>
        GetHorario(int id)
    {
        var horario =
            await _context.HorariosDisponiveis
                .FindAsync(id);

        if (horario == null)
        {
            return NotFound();
        }

        return horario;
    }
    
    //POST: api/horarios
    [HttpPost]
    public async Task<ActionResult<HorarioDisponivel>>
        PostHorario(HorarioDisponivel horario)
    {
        _context.HorariosDisponiveis.Add(horario);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetHorario),
            new { id = horario.Id },
            horario
        );
    }
    
    // PUT: api/horarios/{id}
    [HttpPut("{id}")]
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
            return NotFound();
        }
        
        horarioExistente.DataHora =
            horario.DataHora;
        
        horarioExistente.TipoAtendimento =
            horario.TipoAtendimento;
        
        horarioExistente.Status =
            horario.Status;
        
        await _context.SaveChangesAsync();
        
        return NoContent();
    }
    
    //DELETE: api/horarios/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult>
        DeleteHorario(int id)
    {
        var horario =
            await _context.HorariosDisponiveis
                .FindAsync(id);

        if (horario == null)
        {
            return NotFound();
        }

        _context.HorariosDisponiveis
            .Remove(horario);
        
        await _context.SaveChangesAsync();
        
        return NoContent();
    }
}