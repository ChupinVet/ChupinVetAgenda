using ChupinVetAgenda.Models;
using ChupinVetAgenda.Services.Interfaces;
using ChupinVetAgenda.Services.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace ChupinVetAgenda.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AgendamentosController : ControllerBase
{
    private readonly IAgendamentoService _agendamentoService;
    private readonly ILogger<AgendamentosController> _logger;

    public AgendamentosController(
        IAgendamentoService agendamentoService,
        ILogger<AgendamentosController> logger)
    {
        _agendamentoService = agendamentoService;
        _logger = logger;
    }

    // GET: api/agendamentos
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Agendamento>>> GetAgendamentos()
    {
        _logger.LogInformation("Listando todos os agendamentos.");

        var agendamentos = await _agendamentoService.ListarTodosAsync();

        return Ok(agendamentos);
    }

    // GET: api/agendamentos/{id}
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Agendamento>> GetAgendamento(int id)
    {
        var agendamento = await _agendamentoService.BuscarPorIdAsync(id);

        if (agendamento == null)
        {
            _logger.LogWarning(
                "Agendamento com id {Id} nao encontrado.", id);

            return NotFound();
        }

        return Ok(agendamento);
    }

    // GET: api/agendamentos/responsavel/{email}
    [HttpGet("responsavel/{email}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Agendamento>>> GetAgendamentosPorResponsavel(
        string email)
    {
        _logger.LogInformation(
            "Buscando agendamentos do responsavel {Email}.", email);

        var agendamentos = await _agendamentoService
            .ListarPorResponsavelAsync(email);

        return Ok(agendamentos);
    }

    // GET: api/agendamentos/veterinario
    [HttpGet("veterinario")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Agendamento>>> GetAgendamentosVeterinario()
    {
        _logger.LogInformation(
            "Listando agendamentos marcados para o veterinario.");

        var agendamentos = await _agendamentoService.ListarVeterinarioAsync();

        return Ok(agendamentos);
    }

    // POST: api/agendamentos
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Agendamento>> PostAgendamento(
        Agendamento agendamento)
    {
        try
        {
            var criado = await _agendamentoService.CriarAsync(agendamento);

            _logger.LogInformation(
                "Agendamento criado com id {Id} para o pet {NomePet}.",
                criado.Id, criado.NomePet);

            return CreatedAtAction(
                nameof(GetAgendamento),
                new { id = criado.Id },
                criado);
        }
        catch (HorarioNaoEncontradoException ex)
        {
            return NotFound(ex.Message);
        }
        catch (HorarioIndisponivelException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar agendamento.");

            return StatusCode(
                StatusCodes.Status500InternalServerError);
        }
    }

    // PUT: api/agendamentos/{id}
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PutAgendamento(
        int id,
        Agendamento agendamento)
    {
        var atualizado = await _agendamentoService
            .AtualizarAsync(id, agendamento);

        if (!atualizado)
        {
            _logger.LogWarning(
                "Tentativa de atualizar agendamento inexistente. Id: {Id}.",
                id);

            return NotFound();
        }

        _logger.LogInformation(
            "Agendamento com id {Id} atualizado.", id);

        return NoContent();
    }

    // DELETE: api/agendamentos/{id}
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteAgendamento(int id)
    {
        var excluido = await _agendamentoService.ExcluirAsync(id);

        if (!excluido)
        {
            _logger.LogWarning(
                "Tentativa de remover agendamento inexistente. Id: {Id}.",
                id);

            return NotFound();
        }

        _logger.LogInformation(
            "Agendamento com id {Id} removido.", id);

        return NoContent();
    }
}
