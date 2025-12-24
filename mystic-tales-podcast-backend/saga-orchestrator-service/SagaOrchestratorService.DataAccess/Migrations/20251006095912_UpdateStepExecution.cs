using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SagaOrchestratorService.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateStepExecution : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SagaId",
                table: "SagaStepExcecutions",
                newName: "SagaInstanceId");

            migrationBuilder.RenameIndex(
                name: "IX_SagaStepExcecutions_SagaId",
                table: "SagaStepExcecutions",
                newName: "IX_SagaStepExcecutions_SagaInstanceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SagaInstanceId",
                table: "SagaStepExcecutions",
                newName: "SagaId");

            migrationBuilder.RenameIndex(
                name: "IX_SagaStepExcecutions_SagaInstanceId",
                table: "SagaStepExcecutions",
                newName: "IX_SagaStepExcecutions_SagaId");
        }
    }
}
