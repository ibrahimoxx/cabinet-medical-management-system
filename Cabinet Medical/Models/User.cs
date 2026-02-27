using System;
using System.ComponentModel.DataAnnotations;

namespace Cabinet_Medical.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Username { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Role { get; set; }
        // Admin | Medecin | Secretaire | Patient

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // 🔗 Navigation properties (OBLIGATOIRES)
        public Patient Patient { get; set; }
        public Medecin Medecin { get; set; }
        public Secretaire Secretaire { get; set; }
    }
}
