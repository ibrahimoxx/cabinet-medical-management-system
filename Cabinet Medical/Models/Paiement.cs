using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cabinet_Medical.Models
{
    public class Paiement
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int FactureId { get; set; }

        [Required]
        public string ModePaiement { get; set; }
        // EnLigne | Espece

        [Required]
        public decimal Montant { get; set; }

        public DateTime DatePaiement { get; set; } = DateTime.Now;

        // Navigation
        [ForeignKey(nameof(FactureId))]
        public Facture Facture { get; set; }
    }
}
