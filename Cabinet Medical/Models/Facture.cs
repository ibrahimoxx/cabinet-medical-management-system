using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cabinet_Medical.Models
{
    public class Facture
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PatientId { get; set; }

        [Required]
        public int ConsultationId { get; set; }

        [Required]
        public decimal Montant { get; set; }

        public DateTime DateFacture { get; set; } = DateTime.Now;

        [Required]
        public string Statut { get; set; }
        // Payee | NonPayee

        // Navigation
        [ForeignKey(nameof(PatientId))]
        public Patient Patient { get; set; }

        [ForeignKey(nameof(ConsultationId))]
        public Consultation Consultation { get; set; }

        public ICollection<Paiement> Paiements { get; set; }
    }
}
