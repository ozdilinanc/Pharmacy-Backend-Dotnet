using PharmacyProject.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace PharmacyProject.Application.DTOs.Admin
{
    public class ChangeRoleDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public UserRole NewRole { get; set; }
    }
}
