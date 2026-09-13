# ChupinVet Agenda API

## Integrantes

**Nome:** Agatha Yie Won Yun  
**RM:** 561507  
**Turma:** 2TDSA

**Nome:** Ana Claudia Fernandes Martins  
**RM:** 561190  
**Turma:** 2TDSR

**Nome:** Samantha Faruolo Galdi  
**RM:** 554794  
**Turma:** 2TDSA

**Nome:** Vitor Fria Dalmagro  
**RM:** 566052  
**Turma:** 2TDSA

---

# Sobre

API REST desenvolvida em ASP.NET Core para gerenciamento de horários e agendamentos veterinários, utilizando Oracle Database com Entity Framework Core.

O sistema permite que veterinários disponibilizem horários para atendimento e que responsáveis realizem agendamentos de consultas para seus pets.

---

# Como Instalar e Executar

1. Clonar o Repositório
   ```bash
    git clone https://github.com/ChupinVet/ChupinVetAgenda.git
   ```
2. Entrar na pasta do projeto
   - cd ChupinVetAgenda
3. Restaurar dependências
   - dotnet restore
4. Executar o projeto
   - Abrir o projeto no Rider e executar a aplicação utilizando o botão Run
5. Acessar o Swagger
   - Após iniciar a API, acessar:
   ```Bash
    http://localhost:5139/swagger
   ```
   - A porta pode variar
6. Verificar o Health Check
   - Com a aplicação em execução, acessar:
   ```http
   http://localhost:5139/health
   ```
   - O endpoint retorna o estado de saúde da aplicação e de suas dependências.

---

# Tecnologias Utilizadas

- ASP.NET Core (.NET 10)
- Entity Framework Core
- Oracle Database
- Swagger / OpenAPI
- C#
- Rider
- Serilog
- OpenTelemetry
- xUnit
- Moq
- Entity Framework Core InMemory
- WebApplicationFactory

---

# Funcionalidades

## Horários

- Cadastrar horários disponíveis
- Listar horários
- Listar horários disponíveis
- Atualizar horários
- Remover horários

## Agendamentos

- Realizar agendamentos
- Listar agendamentos
- Buscar agendamentos por responsável
- Visualização para veterinário
- Atualizar agendamentos
- Cancelar agendamentos
- Simulação de envio de e-mail para novos agendamentos

---

# Regras de Negócio

- Apenas horários disponíveis podem ser agendados
- Ao criar um agendamento, o horário fica com status "Ocupado"
- Ao cancelar um agendamento, o horário volta para "Disponivel"
- Não é permitido remover horários que possuam agendamentos vinculados.

---

# Banco de Dados

O projeto utiliza Oracle Database integrado com Entity Framework Core.

---

# OpenAPI / Swagger

A documentação da API está disponível via Swagger.

Exemplo:

```bash
http://localhost:5139/swagger
```

---

# Documentação das Rotas

## Horários

### Listar todos os horários
```http
GET /api/Horarios
```
- Retorna todos os horários cadastrados.

### Listar horários disponíveis
```http
GET /api/Horarios/disponiveis
```
- Retorna apenas horários com status "Disponivel".

### Buscar horário por ID
```http
GET /api/Horarios/{id}
```
- Retorna apenas o horário com id compatível.

### Criar horário
```http
POST /api/Horarios
```
### Exemplo:
```JSON
{
  "dataHora": "2026-06-25T10:30:00",
  "tipoAtendimento": "Retorno",
  "status": "Disponivel"
}
```
- Cria um horário.

### Atualizar horário
```http
PUT /api/Horarios/{id}
```
### Exemplo:
```JSON
{
  "dataHora": "2026-06-25T10:30:00",
  "tipoAtendimento": "Vacinação",
  "status": "Ocupado"
}
```
- Atualiza um horário.

### Remover horário
```http
DELETE /api/Horarios/{id}
```
- Remove um horário caso ele não possua agendamentos vinculados.

## Agendamentos

### Listar todos os agendamentos
```http
GET /api/Agendamentos
```
- Retorna todos os agendamentos cadastrados.

### Buscar agendamento por ID
```http
GET /api/Agendamentos/{id}
```
- Retorna o agendamento compatível com o id informado.

### Buscar agendamentos por responsável
```http
GET /api/Agendamentos/responsavel/{email}
```
- Retorna todos os agendamentos feitos pelo responsável do email informado.

### Visualização do veterinário
```http
GET /api/Agendamentos/veterinario
```
- Retorna todos os agendamentos para o veterinário.

### Criar agendamento
```http
POST /api/Agendamentos
```
### Exemplo:
```JSON
{
  "horarioDisponivelId": 1,
  "nomeResponsavel": "Samantha Faruolo",
  "emailResponsavel": "samantha@email.com",
  "nomePet": "Thor",
  "especiePet": "Cachorro",
  "status": "Marcado"
}
```
- Cria um agendamento.

### Atualizar agendamento
```http
PUT /api/Agendamentos/{id}
```
### Exemplo:
```JSON
{
  "nomeResponsavel": "Samantha Faruolo",
  "emailResponsavel": "samantha@email.com",
  "nomePet": "Thor Atualizado",
  "especiePet": "Cachorro",
  "status": "Marcado"
}
```
- Atualiza um agendamento.

### Remover agendamento
```http
DELETE /api/Agendamentos/{id}
```
- Ao remover um agendamento, o horário relacionado volta para "Disponivel".

---

# Monitoramento e Observabilidade

A aplicação possui recursos de monitoramento e observabilidade para acompanhar sua disponibilidade e comportamento durante a execução.

## Health Checks

Foi implementado um endpoint de Health Check:

```http
GET /health
```

O Health Check verifica:
- Disponibilidade da aplicação;
- Conectividade com o banco de dados Oracle;
- Disponibilidade de serviço externo.

## Logging

A aplicação utiliza Serilog para geração de logs estruturados.

Os logs são registrados no console e em arquivos locais e utilizam diferentes níveis de severidade, como:

- Information;
- Warning;
- Error.

Também é utilizado um Correlation ID para facilitar o rastreamento das requisições.

## OpenTelemetry

O OpenTelemetry é utilizado para coleta de traces e métricas da aplicação.

A instrumentação permite acompanhar informações como:

- Requisições HTTP;
- Tempo de resposta;
- Status das requisições;
- Chamadas HTTP externas;
- Métricas da aplicação.

---

# Testes Automatizados

O projeto possui testes unitários e testes de integração separados da aplicação principal.

## Testes Unitários

Os testes unitários utilizam:

- xUnit;
- Moq;
- Entity Framework Core InMemory;
- Padrão AAA (Arrange, Act, Assert).

Foram implementados testes para cenários como:

- Busca de horário existente;
- Busca de horário inexistente;
- Tentativa de remoção de horário ocupado;
- Tentativa de agendamento em horário indisponível.

## Testes de Integração

Os testes de integração utilizam `WebApplicationFactory` para executar a aplicação em ambiente de teste.

Foram implementados testes para:

- Health Check da aplicação;
- Requisição válida para listagem de horários;
- Tratamento de horário inexistente com retorno HTTP 404.

## Executando os testes

Na raiz da solução, execute:

```bash
dotnet test
```

Atualmente, a solução possui 7 testes automatizados, sendo:

- 4 testes unitários;
- 3 testes de integração.

Todos os testes devem ser executados com sucesso.

---

# Melhorias Futuras

- Integração com nosso app mobile (ChupinVet);
- Sistema real de envio de e-mails;
- Notificações automáticas para consultas;
- Histórico de atendimentos dos pets.

---

# Observações

O sistema de envio de e-mails foi implementado apenas de forma simulada para demonstração da lógica de negócio da aplicação.
