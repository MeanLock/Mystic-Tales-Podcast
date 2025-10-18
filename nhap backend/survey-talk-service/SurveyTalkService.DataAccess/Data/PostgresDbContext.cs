using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using SurveyTalkService.DataAccess.Entities.postgres;

namespace SurveyTalkService.DataAccess.Data;

public partial class PostgresDbContext : DbContext
{
    public PostgresDbContext()
    {
    }

    public PostgresDbContext(DbContextOptions<PostgresDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<SurveyEmbeddingVectorTagFilter> SurveyEmbeddingVectorTagFilters { get; set; }

    public virtual DbSet<SurveyEmbeddingVectorTakenResultTagFilter> SurveyEmbeddingVectorTakenResultTagFilters { get; set; }

    public virtual DbSet<TakerEmbeddingVectorTagFilter> TakerEmbeddingVectorTagFilters { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("vector");

        modelBuilder.Entity<SurveyEmbeddingVectorTagFilter>(entity =>
        {
            entity.HasKey(e => new { e.SurveyId, e.FilterTagId }).HasName("SurveyTagFilter_pkey");

            entity.ToTable("SurveyEmbeddingVectorTagFilter");

            entity.Property(e => e.SurveyId).HasColumnName("surveyId");
            entity.Property(e => e.FilterTagId).HasColumnName("filterTagId");
            entity.Property(e => e.EmbeddingVector)
                .HasMaxLength(768)
                .HasColumnName("embeddingVector");
        });

        modelBuilder.Entity<SurveyEmbeddingVectorTakenResultTagFilter>(entity =>
        {
            entity.HasKey(e => new { e.SurveyTakenResultId, e.AdditionalFilterTagId }).HasName("SurveyTakenResultTagFilter_pkey");

            entity.ToTable("SurveyEmbeddingVectorTakenResultTagFilter");

            entity.Property(e => e.SurveyTakenResultId).HasColumnName("surveyTakenResultId");
            entity.Property(e => e.AdditionalFilterTagId).HasColumnName("additionalFilterTagId");
            entity.Property(e => e.EmbeddingVector)
                .HasMaxLength(768)
                .HasColumnName("embeddingVector");
        });

        modelBuilder.Entity<TakerEmbeddingVectorTagFilter>(entity =>
        {
            entity.HasKey(e => new { e.TakerId, e.FilterTagId }).HasName("TakerTagFilter_pkey");

            entity.ToTable("TakerEmbeddingVectorTagFilter");

            entity.Property(e => e.TakerId).HasColumnName("takerId");
            entity.Property(e => e.FilterTagId).HasColumnName("filterTagId");
            entity.Property(e => e.EmbeddingVector)
                .HasMaxLength(768)
                .HasColumnName("embeddingVector");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
