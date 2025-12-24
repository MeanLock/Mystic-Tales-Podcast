using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SagaOrchestratorService.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSagaInstance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SagaId",
                table: "SagaInstances",
                newName: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "SagaInstances",
                newName: "SagaId");
        }
    }
}
