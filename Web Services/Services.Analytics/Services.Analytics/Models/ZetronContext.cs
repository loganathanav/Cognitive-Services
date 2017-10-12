using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Services.Analytics.Models
{
    public partial class ZetronContext : DbContext
    {
        public virtual DbSet<ZetronMstIncidents> ZetronMstIncidents { get; set; }
        public virtual DbSet<ZetronTrnFrameTags> ZetronTrnFrameTags { get; set; }
        public virtual DbSet<ZetronTrnMediaDetails> ZetronTrnMediaDetails { get; set; }

        public ZetronContext(DbContextOptions<ZetronContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ZetronMstIncidents>(entity =>
            {
                entity.HasKey(e => e.IncidentId);

                entity.Property(e => e.IncidentId).HasColumnName("IncidentID");

                entity.Property(e => e.Description)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Location)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ReportedOn).HasColumnType("datetime");

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<ZetronTrnFrameTags>(entity =>
            {
                entity.HasKey(e => e.FrameId);

                entity.Property(e => e.FrameId).HasColumnName("FrameID");

                entity.Property(e => e.FrameTime).HasColumnType("datetime");

                entity.Property(e => e.MediaId).HasColumnName("MediaID");

                entity.Property(e => e.Tag)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.Media)
                    .WithMany(p => p.ZetronTrnFrameTags)
                    .HasForeignKey(d => d.MediaId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ZetronTrnFrameTags_ZetronTrnMediaDetails");
            });

            modelBuilder.Entity<ZetronTrnMediaDetails>(entity =>
            {
                entity.HasKey(e => e.MediaId);

                entity.Property(e => e.MediaId).HasColumnName("MediaID");

                entity.Property(e => e.IncidentId).HasColumnName("IncidentID");

                entity.Property(e => e.MediaUrl)
                    .IsRequired()
                    .HasColumnName("MediaURL")
                    .IsUnicode(false);

                entity.Property(e => e.PostedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PostedIon)
                    .HasColumnName("PostedIOn")
                    .HasColumnType("datetime");

                entity.HasOne(d => d.Incident)
                    .WithMany(p => p.ZetronTrnMediaDetails)
                    .HasForeignKey(d => d.IncidentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ZetronTrnMediaDetails_ZetronMstIncidents");
            });
        }
    }
}
