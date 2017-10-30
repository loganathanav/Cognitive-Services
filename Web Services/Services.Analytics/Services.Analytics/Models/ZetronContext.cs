using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Services.Analytics.Models
{
    public partial class ZetronContext : DbContext
    {
        public virtual DbSet<ZetronMstIncidents> ZetronMstIncidents { get; set; }
        public virtual DbSet<ZetronTrnFrames> ZetronTrnFrames { get; set; }
        public virtual DbSet<ZetronTrnFrameTags> ZetronTrnFrameTags { get; set; }
        public virtual DbSet<ZetronTrnMediaDetails> ZetronTrnMediaDetails { get; set; }
        public virtual DbSet<Tags> TagSummary { get; set; }
        public virtual DbSet<ZetronTrnDroneLocations> ZetronTrnDroneLocations { get; set; }
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

            modelBuilder.Entity<ZetronTrnFrames>(entity =>
            {
                entity.HasKey(e => e.FrameId);

                entity.Property(e => e.FrameId).HasColumnName("FrameID");

                entity.Property(e => e.FrameTime).HasColumnType("datetime");

                entity.Property(e => e.MediaId).HasColumnName("MediaID");
                
                entity.HasOne(d => d.Media)
                    .WithMany(p => p.ZetronTrnFrames)
                    .HasForeignKey(d => d.MediaId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ZetronTrnFrames_ZetronTrnMediaDetails");
            });

            modelBuilder.Entity<ZetronTrnFrameTags>(entity =>
            {
                entity.HasKey(e => e.TagId);

                entity.Property(e => e.TagId).HasColumnName("TagID");

                entity.Property(e => e.FrameId).HasColumnName("FrameID");

                entity.Property(e => e.Tag)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.Frame)
                    .WithMany(p => p.ZetronTrnFrameTags)
                    .HasForeignKey(d => d.FrameId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ZetronTrnFrameTags_ZetronTrnFrames");
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

                entity.Property(e => e.MediaSummaryUrl)
                    .HasColumnName("MediaSummaryURL")
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .HasColumnName("Name")
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

            modelBuilder.Entity<ZetronTrnDroneLocations>(entity =>
            {
                entity.HasKey(e => e.LocationID);

                entity.Property(e => e.LocationID).HasColumnName("LocationID");

                entity.Property(e => e.MediaID).HasColumnName("MediaID");

                entity.Property(e => e.Temperature)
                    .HasColumnName("Temperature")
                    .HasColumnType("decimal");

                entity.Property(e => e.Humidity)
                    .HasColumnName("Humidity")
                    .HasColumnType("decimal");

                entity.Property(e => e.WindSpeed)
                    .HasColumnName("WindSpeed")
                    .HasColumnType("decimal");

                entity.Property(e => e.DewPoint)
                    .HasColumnName("DewPoint")
                    .HasColumnType("decimal");

                entity.Property(e => e.Summary)
                    .HasColumnName("Summary")
                    .IsUnicode(false);

                entity.Property(e => e.WindDirection)
                    .HasColumnName("WindDirection")
                    .IsUnicode(false);

                entity.Property(e => e.Longitude)
                    .HasColumnName("Longitude")
                    .HasColumnType("decimal");

                entity.Property(e => e.Latitude)
                    .HasColumnName("Latitude")
                    .HasColumnType("decimal");

                entity.HasOne(d => d.Media)
                    .WithMany(p => p.ZetronTrnDroneLocations)
                    .HasForeignKey(d => d.MediaID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ZetronTrnDroneLocations_ZetronTrnMediaDetails");
            });
        }
    }
}
