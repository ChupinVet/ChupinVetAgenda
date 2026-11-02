using ChupinVetAgenda.Models;
using ChupinVetAgenda.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ChupinVetAgenda.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HorariosController : ControllerBase
{
    private readonly IHorarioService _horarioService;
    private readonly ILogger<HorariosController> _logger;

    public HorariosController(
        IHorarioService horarioService,
        ILogger<HorariosController> logger)
    {
        _horarioService = horarioService;
        _logger = logger;
    }

    // GET: api/horarios
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<HorarioDisponivel>>> GetHorarios()
    {
        _logger.LogInformation("Listando todos os horarios.");

        var horarios = await _horarioService.ListarTodosAsync();

        return Ok(horarios);
    }

    // GET: api/horarios/disponiveis
    [HttpGet("disponiveis")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<HorarioDisponivel>>> GetDisponiveis()
    {
        _logger.LogInformation("Listando horarios disponiveis.");

        var horarios = await _horarioService.ListarDisponiveisAsync();

        return Ok(horarios);
    }

    // GET: api/horarios/{id}
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HorarioDisponivel>> GetHorario(int id)
    {
        var horario = await _horarioService.BuscarPorIdAsync(id);

        if (horario == null)
        {
            _logger.LogWarning(
                "Horario com id {Id} nao encontrado.", id);

            return NotFound();
        }

        return Ok(horario);
    }

    // POST: api/horarios
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<HorarioDisponivel>> PostHorario(
        HorarioDisponivel horario)
    {
        try
        {
            var criado = await _horarioService.CriarAsync(horario);

            _logger.LogInformation(
                "Horario criado com id {Id}.", criado.Id);

            return CreatedAtAction(
                nameof(GetHorario),
                new { id = criado.Id },
                criado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar horario.");

            return StatusCode(
                StatusCodes.Status500InternalServerError);
        }
    }

    // PUT: api/horarios/{id}
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PutHorario(
        int id,
        HorarioDisponivel horario)
    {
        var atualizado = await _horarioService.AtualizarAsync(id, horario);

        if (!atualizado)
        {
            _logger.LogWarning(
                "Tentativa de atualizar horario inexistente. Id: {Id}.",
                id);

            return NotFound();
        }

        _logger.LogInformation(
            "Horario com id {Id} atualizado.", id);

        return NoContent();
    }

    // DELETE: api/horarios/{id}
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteHorario(int id)
    {
        try
        {
            var excluido = await _horarioService.ExcluirAsync(id);

            if (!excluido)
            {
                _logger.LogWarning(
                    "Tentativa de remover horario inexistente. Id: {Id}.",
                    id);

                return NotFound();
            }

            _logger.LogInformation(
                "Horario com id {Id} removido.", id);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(
                "Nao foi possivel remover o horario {Id}: {Motivo}",
                id, ex.Message);

            return BadRequest(ex.Message);
        }
    }
}