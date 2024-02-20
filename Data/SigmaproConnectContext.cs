using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SigmaProConnectFileValidation.Data
{
    public partial class SigmaproConnectContext : DbContext
    {
        public SigmaproConnectContext()
        {
        }

        public SigmaproConnectContext(DbContextOptions<SigmaproConnectContext> options)
            : base(options)
        {
        }

        public virtual DbSet<patient_stage> patient_stage { get; set; }

   

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseNpgsql("Host=sigmaprodb.postgres.database.azure.com,5432;Database=sigmapro_iis;Username=sigmaprodb_user;Password=Rules@23$$11;TrustServerCertificate=False");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<patient_stage>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("patient_stage_pkey");
                entity.Property(e => e.Id)
             .HasDefaultValueSql("gen_random_uuid()")
             .HasColumnName("id");
                entity.Property(e => e.PatientName)
                    .HasColumnType("character varying")
                    .HasColumnName("patient_name");
                entity.Property(e => e.PatientId)
                    .HasColumnType("character varying")
                    .HasColumnName("patient_id");
                entity.Property(e => e.CreatedBy)
            .HasColumnType("character varying")
            .HasColumnName("created_by");
                entity.Property(e => e.CreatedDate).HasColumnName("created_date");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
