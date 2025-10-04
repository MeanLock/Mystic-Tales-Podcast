using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SagaOrchestratorService.DataAccess.Entities;

namespace SagaOrchestratorService.DataAccess.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
    // DbSets
    public DbSet<SagaInstance> SagaInstances { get; set; } = null!;
    public DbSet<SagaStepExcecution> SagaStepExcecutions { get; set; } = null!;
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Shared JSON converter for Dictionary<string, object>
        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = false
        };

        // SagaInstance
        modelBuilder.Entity<SagaInstance>(entity =>
        {
            entity.HasKey(e => e.SagaId);

            entity.Property(e => e.FlowName)
                  .IsRequired();

            entity.Property(e => e.InitialData)
                  .HasColumnType("nvarchar(max)");

            entity.Property(e => e.ResultData)
                  .HasColumnType("nvarchar(max)");

            entity.Property(e => e.FlowStatus)
                  .HasConversion<int>(); // store enum as int

            entity.Property(e => e.CreatedAt)
                  .HasColumnType("datetime2");

            entity.Property(e => e.UpdatedAt)
                  .HasColumnType("datetime2");

            entity.Property(e => e.CompletedAt)
                  .HasColumnType("datetime2");
        });

        // SagaStepExcecution
        modelBuilder.Entity<SagaStepExcecution>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.SagaId)
                  .IsRequired();

            entity.Property(e => e.StepName)
                  .IsRequired();

            entity.Property(e => e.TopicName);

            entity.Property(e => e.StepStatus)
                  .HasConversion<int>(); // store enum as int

            entity.Property(e => e.RequestData)
                  .HasColumnType("nvarchar(max)");

            entity.Property(e => e.responseData)
                  .HasColumnType("nvarchar(max)");

            entity.Property(e => e.ErrorMessage);

            entity.Property(e => e.CreatedAt)
                  .HasColumnType("datetime2");

            entity.HasIndex(e => e.SagaId); // helpful for lookups
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
