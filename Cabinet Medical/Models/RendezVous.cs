using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cabinet_Medical.Models
{
    public class RendezVous
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PatientId { get; set; }

        [Required]
        public int MedecinId { get; set; }

        [Required]
        public DateTime DateRdv { get; set; }

        [Required]
        public TimeSpan HeureRdv { get; set; }

        [Required]
        public string Statut { get; set; }
        // Planifie | Annule | Termine

        public string Motif { get; set; }

        // Navigation
        [ForeignKey(nameof(PatientId))]
        public Patient Patient { get; set; }

        [ForeignKey(nameof(MedecinId))]
        public Medecin Medecin { get; set; }
    }
}
