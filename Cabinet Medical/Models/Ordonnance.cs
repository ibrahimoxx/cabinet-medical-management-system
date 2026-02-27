using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cabinet_Medical.Models
{
    public class Ordonnance
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ConsultationId { get; set; }

        public DateTime DateOrdonnance { get; set; } = DateTime.Now;

        // Navigation
        [ForeignKey(nameof(ConsultationId))]
        public Consultation Consultation { get; set; }

        public ICollection<OrdonnanceDetail> OrdonnanceDetails { get; set; }
    }
}
