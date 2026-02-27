using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cabinet_Medical.Models
{
    public class Patient
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required, MaxLength(50)]
        public string Nom { get; set; }

        [Required, MaxLength(50)]
        public string Prenom { get; set; }

        public DateTime? DateNaissance { get; set; }

        public string Adresse { get; set; }

        public string Telephone { get; set; }

        public string AntecedentsMedicaux { get; set; }

        // Navigation
        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        public DossierMedical DossierMedical { get; set; }
    }
}
