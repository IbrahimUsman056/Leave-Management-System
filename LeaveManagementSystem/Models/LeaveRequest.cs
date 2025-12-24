using System.ComponentModel.DataAnnotations;

namespace LeaveManagementSystem.Models
{
    public class LeaveRequest
    {
        [Key]
        public int LeaveId { get; set; }

        [Required]
        [Display(Name = "Employee")]
        public int EmployeeId { get; set; }

        public virtual Employee? Employee { get; set; }

        [Required(ErrorMessage = "Leave type is required")]
        [Display(Name = "Leave Type")]
        public string LeaveType { get; set; } = "Casual";

        [Required(ErrorMessage = "Start date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "End date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; } = DateTime.Today.AddDays(1);

        [Required(ErrorMessage = "Reason is required")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Reason must be 10-500 characters")]
        public string Reason { get; set; } = string.Empty;

        [Required]
        public string Status { get; set; } = "Pending";

        [DataType(DataType.DateTime)]
        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}