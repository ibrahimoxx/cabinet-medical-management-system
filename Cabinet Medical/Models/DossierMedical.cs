using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cabinet_Medical.Models
{
    public class DossierMedical
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PatientId { get; set; }

        public DateTime DateCreation { get; set; } = DateTime.Now;

        public string Remarques { get; set; }

        // Navigation
        [ForeignKey(nameof(PatientId))]
        public Patient Patient { get; set; }
    }
}
