using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ExpressionTrees.Stream.DB
{
    public partial class FullTextGamesContext : DbContext
    {
        public FullTextGamesContext()
        {
        }

        public FullTextGamesContext(DbContextOptions<FullTextGamesContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Document> Documents { get; set; } = null!;
        public virtual DbSet<IndexedDataset> IndexedDatasets { get; set; } = null!;
        public virtual DbSet<Word> Words { get; set; } = null!;
        public virtual DbSet<WordDocument> WordDocuments { get; set; } = null!;
        public virtual DbSet<WordDocumentsPosition> WordDocumentsPositions { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseNpgsql("Host=localhost;User Id=postgres;Password=postgres;Database=FullTextGames");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Document>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();
            });

            modelBuilder.Entity<IndexedDataset>(entity =>
            {
                entity.ToTable("IndexedDataset");

                entity.HasIndex(e => e.Ts, "FIX_IndexedDataset")
                    .HasMethod("gin");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Ts)
                    .HasColumnName("ts")
                    .HasComputedColumnSql("to_tsvector('english'::regconfig, \"Content\")", true);
            });

            modelBuilder.Entity<Word>(entity =>
            {
                entity.HasIndex(e => e.Word1, "IX_Words_Word")
                    .IsUnique();

                entity.HasIndex(e => e.Word1, "Words_Word_key")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Word1)
                    .HasMaxLength(200)
                    .HasColumnName("Word");
            });

            modelBuilder.Entity<WordDocument>(entity =>
            {
                entity.HasIndex(e => new { e.WordId, e.DocumentId }, "IX_WordDocumentsPositions_WordId_DocumentId");

                entity.HasIndex(e => e.WordId, "IX_WordDocuments_WordId");

                entity.HasIndex(e => new { e.WordId, e.DocumentId }, "IX_WordDocuments_WordId_DocumentId")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.HasOne(d => d.Document)
                    .WithMany(p => p.WordDocuments)
                    .HasForeignKey(d => d.DocumentId)
                    .HasConstraintName("WordDocuments_DocumentId_fkey");

                entity.HasOne(d => d.Word)
                    .WithMany(p => p.WordDocuments)
                    .HasForeignKey(d => d.WordId)
                    .HasConstraintName("WordDocuments_WordId_fkey");
            });

            modelBuilder.Entity<WordDocumentsPosition>(entity =>
            {
                entity.HasIndex(e => e.WordId, "IX_WordDocumentsPositions_WordId");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.HasOne(d => d.Document)
                    .WithMany(p => p.WordDocumentsPositions)
                    .HasForeignKey(d => d.DocumentId)
                    .HasConstraintName("WordDocumentsPositions_DocumentId_fkey");

                entity.HasOne(d => d.Word)
                    .WithMany(p => p.WordDocumentsPositions)
                    .HasForeignKey(d => d.WordId)
                    .HasConstraintName("WordDocumentsPositions_WordId_fkey");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
