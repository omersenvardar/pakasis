using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DBGoreWebApp.Models.Entities
{
    public class Role
    {
        [Key]
        public int RoleId { get; set; }

        [Required]
        [MaxLength(50)]
        public string? Name { get; set; }

        public ICollection<UserRole>? UserRoles { get; set; }
    }
}
