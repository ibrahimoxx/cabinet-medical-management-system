using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cabinet_Medical.Models
{
    public class Alerte
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Type { get; set; }
        // AnnulationRDV, RappelRDV

        [Required]
        public string Message { get; set; }

        [Required]
        public int UserId { get; set; }

        public int? RendezVousId { get; set; }

        public bool EstLue { get; set; } = false;

        public DateTime DateCreation { get; set; } = DateTime.Now;

        // Navigation
        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        [ForeignKey(nameof(RendezVousId))]
        public RendezVous? RendezVous { get; set; }
    }
}

