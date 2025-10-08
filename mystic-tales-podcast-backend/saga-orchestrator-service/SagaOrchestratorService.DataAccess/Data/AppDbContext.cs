using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using SagaOrchestratorService.DataAccess.Entities.sqlserver;

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

    public virtual DbSet<SagaInstance> SagaInstances { get; set; }

    public virtual DbSet<SagaStepExecution> SagaStepExecutions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SagaInstance>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SagaInst__3213E83F9E1EB6A4");

            entity.ToTable("SagaInstance", tb => tb.HasTrigger("TR_SagaInstance_UpdatedAt"));

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CompletedAt)
                .HasColumnType("datetime")
                .HasColumnName("completedAt");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.CurrentStepName)
                .HasMaxLength(250)
                .HasColumnName("currentStepName");
            entity.Property(e => e.ErrorMessage).HasColumnName("errorMessage");
            entity.Property(e => e.ErrorStepName)
                .HasMaxLength(250)
                .HasColumnName("errorStepName");
            entity.Property(e => e.FlowName)
                .IsRequired()
                .HasMaxLength(250)
                .HasColumnName("flowName");
            entity.Property(e => e.FlowStatus)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("flowStatus");
            entity.Property(e => e.InitialData).HasColumnName("initialData");
            entity.Property(e => e.ResultData).HasColumnName("resultData");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("updatedAt");
        });

        modelBuilder.Entity<SagaStepExecution>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SagaStep__3213E83F6B01D2C5");

            entity.ToTable("SagaStepExecution");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.ErrorMessage).HasColumnName("errorMessage");
            entity.Property(e => e.RequestData).HasColumnName("requestData");
            entity.Property(e => e.ResponseData).HasColumnName("responseData");
            entity.Property(e => e.SagaInstanceId).HasColumnName("sagaInstanceId");
            entity.Property(e => e.StepName)
                .IsRequired()
                .HasMaxLength(250)
                .HasColumnName("stepName");
            entity.Property(e => e.StepStatus)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("stepStatus");
            entity.Property(e => e.TopicName)
                .IsRequired()
                .HasMaxLength(250)
                .HasColumnName("topicName");

            entity.HasOne(d => d.SagaInstance).WithMany(p => p.SagaStepExecutions)
                .HasForeignKey(d => d.SagaInstanceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SagaStepE__sagaI__3C69FB99");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
