using ChupinVetAgenda.Data;
using ChupinVetAgenda.Models;
using ChupinVetAgenda.Services;
using ChupinVetAgenda.Services.Exceptions;
using ChupinVetAgenda.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using ChupinVetAgenda.Repositories.Implementations;
using ChupinVetAgenda.Repositories.Interfaces;

namespace ChupinVetAgenda.Tests.Unit.Services;

public class AgendamentoServiceTests
{
    private static AppDbContext CriarContexto()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task CriarAsync_HorarioOcupado_LancaExcecao()
    {
        // Arrange
        using var context = CriarContexto();

        var horario = new HorarioDisponivel
        {
            Id = 1,
            DataHora = new DateTime(2026, 9, 20, 10, 0, 0),
            TipoAtendimento = "Consulta",
            Status = "Ocupado"
        };

        context.HorariosDisponiveis.Add(horario);
        await context.SaveChangesAsync();
        
        var agendamentoRepository = new AgendamentoRepository(context);
        var horarioRepository = new HorarioRepository(context);

        var service = new AgendamentoService(
            agendamentoRepository,
            horarioRepository,
            new EmailService()
        );

        var agendamento = new Agendamento
        {
            HorarioDisponivelId = 1,
            NomeResponsavel = "Teste",
            EmailResponsavel = "teste@email.com",
            NomePet = "Thor",
            EspeciePet = "Cachorro",
            Status = "Marcado"
        };

        // Act e Assert
        await Assert.ThrowsAsync<HorarioIndisponivelException>(
            () => service.CriarAsync(agendamento)
        );

        Assert.Empty(context.Agendamentos);
        Assert.Equal("Ocupado", horario.Status);
    }
    
    
    [Fact]
    public async Task ConsultarAsync_ComPaginacao_RetornaItensCorretos()
    {
        // Arrange
        using var context = CriarContexto();
        
        for (int i = 1; i <= 5; i++)
        {
            var horario = new HorarioDisponivel
            {
                Id = i,
                DataHora = DateTime.Now.AddDays(i),
                TipoAtendimento = "Consulta",
                Status = "Ocupado"
            };

            context.HorariosDisponiveis.Add(horario);

            context.Agendamentos.Add(new Agendamento
            {
                Id = i,
                HorarioDisponivelId = i,
                HorarioDisponivel = horario,
                NomeResponsavel = "Responsavel " + i,
                EmailResponsavel = $"teste{i}@email.com",
                NomePet = "Pet " + i,
                EspeciePet = "Cachorro",
                Status = "Marcado"
            });
        }

        await context.SaveChangesAsync();
        
        var agendamentoRepository = new AgendamentoRepository(context);
        var horarioRepository = new HorarioRepository(context);

        var service = new AgendamentoService(
            agendamentoRepository,
            horarioRepository,
            new EmailService()
        );

        // Act
        var (itens, total) = await service.ConsultarAsync(
            pagina: 2,
            tamanhoPagina: 2,
            status: null,
            especie: null,
            ordenarPor: null,
            descendente: false
        );

        // Assert
        Assert.Equal(5, total);
        Assert.Equal(2, itens.Count);
        Assert.Equal(3, itens[0].Id);
        Assert.Equal(4, itens[1].Id);
    }
    
    [Fact]
    public async Task ConsultarAsync_FiltrarPorEspecie_RetornaApenasCachorros()
    {
        // Arrange
        using var context = CriarContexto();

        var especies = new[] { "Cachorro", "Gato", "Cachorro" };

        for (int i = 0; i < especies.Length; i++)
        {
            var horario = new HorarioDisponivel
            {
                Id = i + 1,
                DataHora = DateTime.Now.AddDays(i + 1),
                TipoAtendimento = "Consulta",
                Status = "Ocupado"
            };

            context.HorariosDisponiveis.Add(horario);

            context.Agendamentos.Add(new Agendamento
            {
                Id = i + 1,
                HorarioDisponivelId = horario.Id,
                HorarioDisponivel = horario,
                NomeResponsavel = "Responsavel",
                EmailResponsavel = "teste@email.com",
                NomePet = $"Pet {i + 1}",
                EspeciePet = especies[i],
                Status = "Marcado"
            });
        }

        await context.SaveChangesAsync();
        
        var agendamentoRepository = new AgendamentoRepository(context);
        var horarioRepository = new HorarioRepository(context);

        var service = new AgendamentoService(
            agendamentoRepository,
            horarioRepository,
            new EmailService()
        );

        // Act
        var (itens, total) = await service.ConsultarAsync(
            pagina: 1,
            tamanhoPagina: 10,
            status: null,
            especie: "Cachorro",
            ordenarPor: null,
            descendente: false
        );

        // Assert
        Assert.Equal(2, total);
        Assert.Equal(2, itens.Count);
        Assert.All(itens, a => Assert.Equal("Cachorro", a.EspeciePet));
    }
    
    [Fact]
    public async Task ConsultarAsync_OrdenarPorNomePet_RetornaOrdemAlfabetica()
    {
        // Arrange
        using var context = CriarContexto();

        var nomes = new[] { "Thor", "Bob", "Mel" };

        for (int i = 0; i < nomes.Length; i++)
        {
            var horario = new HorarioDisponivel
            {
                Id = i + 1,
                DataHora = DateTime.Now.AddDays(i + 1),
                TipoAtendimento = "Consulta",
                Status = "Ocupado"
            };

            context.HorariosDisponiveis.Add(horario);

            context.Agendamentos.Add(new Agendamento
            {
                Id = i + 1,
                HorarioDisponivelId = horario.Id,
                HorarioDisponivel = horario,
                NomeResponsavel = "Responsavel",
                EmailResponsavel = "teste@email.com",
                NomePet = nomes[i],
                EspeciePet = "Cachorro",
                Status = "Marcado"
            });
        }

        await context.SaveChangesAsync();
        
        var agendamentoRepository = new AgendamentoRepository(context);
        var horarioRepository = new HorarioRepository(context);

        var service = new AgendamentoService(
            agendamentoRepository,
            horarioRepository,
            new EmailService()
        );

        // Act
        var (itens, total) = await service.ConsultarAsync(
            pagina: 1,
            tamanhoPagina: 10,
            status: null,
            especie: null,
            ordenarPor: "nomepet",
            descendente: false
        );

        // Assert
        Assert.Equal(3, total);
        Assert.Equal(
            new[] { "Bob", "Mel", "Thor" },
            itens.Select(a => a.NomePet).ToArray()
        );
    }
    
    [Fact]
    public async Task AtualizarAsync_AgendamentoInexistente_RetornaFalse()
    {
        // Arrange
        using var context = CriarContexto();

        var agendamentoRepository = new AgendamentoRepository(context);
        var horarioRepository = new HorarioRepository(context);

        var service = new AgendamentoService(
            agendamentoRepository,
            horarioRepository,
            new EmailService()
        );

        var agendamento = new Agendamento
        {
            NomeResponsavel = "Maria",
            EmailResponsavel = "maria@email.com",
            NomePet = "Mel",
            EspeciePet = "Gato",
            Status = "Marcado"
        };

        // Act
        var resultado = await service.AtualizarAsync(999999, agendamento);

        // Assert
        Assert.False(resultado);
        Assert.Empty(context.Agendamentos);
    }
    
    [Fact]
    public async Task AtualizarAsync_AgendamentoExistente_AtualizaDadosERetornaTrue()
    {
        // Arrange
        using var context = CriarContexto();

        var horario = new HorarioDisponivel
        {
            Id = 1,
            DataHora = new DateTime(2026, 12, 10, 10, 0, 0),
            TipoAtendimento = "Consulta",
            Status = "Ocupado"
        };

        context.HorariosDisponiveis.Add(horario);

        var agendamentoExistente = new Agendamento
        {
            Id = 1,
            HorarioDisponivelId = 1,
            NomeResponsavel = "Maria",
            EmailResponsavel = "maria@email.com",
            NomePet = "Thor",
            EspeciePet = "Cachorro",
            Status = "Marcado"
        };

        context.Agendamentos.Add(agendamentoExistente);
        await context.SaveChangesAsync();

        var service = new AgendamentoService(
            new AgendamentoRepository(context),
            new HorarioRepository(context),
            new EmailService()
        );

        var novosDados = new Agendamento
        {
            NomeResponsavel = "Ana",
            EmailResponsavel = "ana@email.com",
            NomePet = "Mel",
            EspeciePet = "Gato",
            Status = "Confirmado"
        };

        // Act
        var resultado = await service.AtualizarAsync(1, novosDados);

        // Assert
        Assert.True(resultado);

        context.ChangeTracker.Clear();

        var atualizado = await context.Agendamentos.FindAsync(1);

        Assert.NotNull(atualizado);
        Assert.Equal("Ana", atualizado.NomeResponsavel);
        Assert.Equal("ana@email.com", atualizado.EmailResponsavel);
        Assert.Equal("Mel", atualizado.NomePet);
        Assert.Equal("Gato", atualizado.EspeciePet);
        Assert.Equal("Confirmado", atualizado.Status);
    }
    
    [Fact]
    public async Task ExcluirAsync_AgendamentoInexistente_RetornaFalse()
    {
        // Arrange
        using var context = CriarContexto();

        var service = new AgendamentoService(
            new AgendamentoRepository(context),
            new HorarioRepository(context),
            new EmailService()
        );

        // Act
        var resultado = await service.ExcluirAsync(999999);

        // Assert
        Assert.False(resultado);
        Assert.Empty(context.Agendamentos);
    }
    
    [Fact]
    public async Task ExcluirAsync_AgendamentoExistente_ExcluiELiberaHorario()
    {
        // Arrange
        using var context = CriarContexto();

        var horario = new HorarioDisponivel
        {
            Id = 1,
            DataHora = new DateTime(2026, 12, 10, 10, 0, 0),
            TipoAtendimento = "Consulta",
            Status = "Ocupado"
        };

        context.HorariosDisponiveis.Add(horario);

        context.Agendamentos.Add(new Agendamento
        {
            Id = 1,
            HorarioDisponivelId = 1,
            NomeResponsavel = "Maria",
            EmailResponsavel = "maria@email.com",
            NomePet = "Thor",
            EspeciePet = "Cachorro",
            Status = "Marcado"
        });

        await context.SaveChangesAsync();

        var service = new AgendamentoService(
            new AgendamentoRepository(context),
            new HorarioRepository(context),
            new EmailService()
        );

        // Act
        var resultado = await service.ExcluirAsync(1);

        // Assert
        Assert.True(resultado);

        context.ChangeTracker.Clear();

        var agendamentoExcluido = await context.Agendamentos.FindAsync(1);
        var horarioAtualizado = await context.HorariosDisponiveis.FindAsync(1);

        Assert.Null(agendamentoExcluido);
        Assert.NotNull(horarioAtualizado);
        Assert.Equal("Disponivel", horarioAtualizado.Status);
    }
}