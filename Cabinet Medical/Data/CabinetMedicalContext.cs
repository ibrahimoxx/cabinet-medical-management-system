using Microsoft.EntityFrameworkCore;
using Cabinet_Medical.Models;

namespace Cabinet_Medical.Data
{
    public class CabinetMedicalContext : DbContext
    {
        public CabinetMedicalContext(DbContextOptions<CabinetMedicalContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Medecin> Medecins { get; set; }
        public DbSet<Secretaire> Secretaires { get; set; }
        public DbSet<DossierMedical> DossierMedicals { get; set; }
        public DbSet<RendezVous> RendezVous { get; set; }
        public DbSet<Consultation> Consultations { get; set; }
        public DbSet<Ordonnance> Ordonnances { get; set; }
        public DbSet<OrdonnanceDetail> OrdonnanceDetails { get; set; }
        public DbSet<Facture> Factures { get; set; }
        public DbSet<Paiement> Paiements { get; set; }
        public DbSet<Alerte> Alertes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =========================
            // USER relations (1–1)
            // =========================

            modelBuilder.Entity<User>()
                .HasOne(u => u.Patient)
                .WithOne(p => p.User)
                .HasForeignKey<Patient>(p => p.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Medecin)
                .WithOne(m => m.User)
                .HasForeignKey<Medecin>(m => m.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Secretaire)
                .WithOne(s => s.User)
                .HasForeignKey<Secretaire>(s => s.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // =========================
            // RendezVous (NO CASCADE)
            // =========================

            modelBuilder.Entity<RendezVous>()
                .HasOne(r => r.Patient)
                .WithMany()
                .HasForeignKey(r => r.PatientId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<RendezVous>()
                .HasOne(r => r.Medecin)
                .WithMany()
                .HasForeignKey(r => r.MedecinId)
                .OnDelete(DeleteBehavior.NoAction);

            // =========================
            // Dossier médical
            // =========================

            modelBuilder.Entity<DossierMedical>()
                .HasOne(d => d.Patient)
                .WithOne(p => p.DossierMedical)
                .HasForeignKey<DossierMedical>(d => d.PatientId)
                .OnDelete(DeleteBehavior.NoAction);

            // =========================
            // Consultation
            // =========================

            modelBuilder.Entity<Consultation>()
                .HasOne(c => c.DossierMedical)
                .WithMany()
                .HasForeignKey(c => c.DossierMedicalId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Consultation>()
                .HasOne(c => c.Medecin)
                .WithMany()
                .HasForeignKey(c => c.MedecinId)
                .OnDelete(DeleteBehavior.NoAction);

            // =========================
            // Ordonnance
            // =========================

            modelBuilder.Entity<Ordonnance>()
                .HasOne(o => o.Consultation)
                .WithMany()
                .HasForeignKey(o => o.ConsultationId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<OrdonnanceDetail>()
                .HasOne(d => d.Ordonnance)
                .WithMany(o => o.OrdonnanceDetails)
                .HasForeignKey(d => d.OrdonnanceId)
                .OnDelete(DeleteBehavior.NoAction);

            // =========================
            // Facture / Paiement
            // =========================

            modelBuilder.Entity<Facture>()
                .HasOne(f => f.Patient)
                .WithMany()
                .HasForeignKey(f => f.PatientId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Facture>()
                .HasOne(f => f.Consultation)
                .WithMany()
                .HasForeignKey(f => f.ConsultationId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Paiement>()
                .HasOne(p => p.Facture)
                .WithMany(f => f.Paiements)
                .HasForeignKey(p => p.FactureId)
                .OnDelete(DeleteBehavior.NoAction);

            // =========================
            // Alerte
            // =========================

            modelBuilder.Entity<Alerte>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Alerte>()
                .HasOne(a => a.RendezVous)
                .WithMany()
                .HasForeignKey(a => a.RendezVousId)
                .OnDelete(DeleteBehavior.NoAction);

            // =========================
            // Decimal precision (PRO)
            // =========================

            modelBuilder.Entity<Facture>()
                .Property(f => f.Montant)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Paiement>()
                .Property(p => p.Montant)
                .HasPrecision(10, 2);
        }

    }
}
