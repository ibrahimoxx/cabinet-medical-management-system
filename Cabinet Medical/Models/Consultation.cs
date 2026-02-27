using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cabinet_Medical.Models
{
    public class Consultation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int DossierMedicalId { get; set; }

        [Required]
        public int MedecinId { get; set; }

        public DateTime DateConsultation { get; set; } = DateTime.Now;

        public string Diagnostic { get; set; }

        public string Notes { get; set; }

        // Navigation
        [ForeignKey(nameof(DossierMedicalId))]
        public DossierMedical DossierMedical { get; set; }

        [ForeignKey(nameof(MedecinId))]
        public Medecin Medecin { get; set; }
    }
}
