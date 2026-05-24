using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChupinVetAgenda.Migrations
{
    /// <inheritdoc />
    public partial class OracleInitial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HorariosDisponiveis",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    DataHora = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    TipoAtendimento = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Status = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HorariosDisponiveis", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Agendamentos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    HorarioDisponivelId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    NomeResponsavel = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    EmailResponsavel = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    NomePet = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    EspeciePet = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Status = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agendamentos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Agendamentos_HorariosDisponiveis_HorarioDisponivelId",
                        column: x => x.HorarioDisponivelId,
                        principalTable: "HorariosDisponiveis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Agendamentos_HorarioDisponivelId",
                table: "Agendamentos",
                column: "HorarioDisponivelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Agendamentos");

            migrationBuilder.DropTable(
                name: "HorariosDisponiveis");
        }
    }
}
