using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cabinet_Medical.Models
{
    public class OrdonnanceDetail
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OrdonnanceId { get; set; }

        [Required]
        public string Type { get; set; }
        // Medicament | Analyse | Radiologie

        [Required]
        public string Description { get; set; }

        public string Dosage { get; set; }

        // Navigation
        [ForeignKey(nameof(OrdonnanceId))]
        public Ordonnance Ordonnance { get; set; }
    }
}
