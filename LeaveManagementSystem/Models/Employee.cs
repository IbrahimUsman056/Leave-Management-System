using System.ComponentModel.DataAnnotations;

namespace LeaveManagementSystem.Models
{
    public class Employee
    {
        [Key]
        public int EmployeeId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [Display(Name = "Full Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "CNIC is required")]
        [Display(Name = "CNIC")]
        [RegularExpression(@"^\d{5}-\d{7}-\d{1}$", ErrorMessage = "CNIC format: XXXXX-XXXXXXX-X")]
        public string CNIC { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department is required")]
        public string Department { get; set; } = string.Empty;

        [Required(ErrorMessage = "Designation is required")]
        public string Designation { get; set; } = string.Empty;

        [Display(Name = "Active Status")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "User ID")]
        public string IdentityUserId { get; set; } = string.Empty;
    }
}