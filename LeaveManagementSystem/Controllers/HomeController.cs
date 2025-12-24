using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using LeaveManagementSystem.Data;
using LeaveManagementSystem.Models;
using System.Security.Claims;

namespace LeaveManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Dashboard()
        {
            if (User.IsInRole("Admin"))
            {
                ViewBag.TotalEmployees = await _context.Employees.CountAsync();
                ViewBag.ActiveEmployees = await _context.Employees.CountAsync(e => e.IsActive);
                ViewBag.PendingLeaves = await _context.LeaveRequests.CountAsync(l => l.Status == "Pending");
                ViewBag.TodaysLeaves = await _context.LeaveRequests
                    .CountAsync(l => l.StartDate <= DateTime.Today && l.EndDate >= DateTime.Today && l.Status == "Approved");

              
                var recentLeaves = await _context.LeaveRequests
                    .Include(l => l.Employee)
                    .Where(l => l.Status == "Pending")
                    .OrderByDescending(l => l.CreatedDate)
                    .Take(10)
                    .Select(l => new
                    {
                        l.LeaveId,
                        EmployeeName = l.Employee.Name,
                        l.LeaveType,
                        l.StartDate,
                        l.EndDate,
                        l.Status
                    })
                    .ToListAsync();

                ViewBag.RecentLeaves = recentLeaves;

                
                var employees = await _context.Employees
                    .OrderBy(e => e.Name)
                    .Take(15)
                    .Select(e => new
                    {
                        e.EmployeeId,
                        e.Name,
                        e.Email,
                        e.Department,
                        e.Designation,
                        e.IsActive
                    })
                    .ToListAsync();

                ViewBag.Employees = employees;

                return View("AdminDashboard");
            }
            else
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var employee = await _context.Employees
                    .FirstOrDefaultAsync(e => e.IdentityUserId == userId);

                if (employee != null)
                {
                    var employeeId = employee.EmployeeId;

                    ViewBag.MyTotalLeaves = await _context.LeaveRequests
                        .CountAsync(l => l.EmployeeId == employeeId);

                    ViewBag.MyApprovedLeaves = await _context.LeaveRequests
                        .CountAsync(l => l.EmployeeId == employeeId && l.Status == "Approved");

                    ViewBag.MyPendingLeaves = await _context.LeaveRequests
                        .CountAsync(l => l.EmployeeId == employeeId && l.Status == "Pending");

                    ViewBag.MyRejectedLeaves = await _context.LeaveRequests
                        .CountAsync(l => l.EmployeeId == employeeId && l.Status == "Rejected");

                    var myLeaves = await _context.LeaveRequests
                        .Where(l => l.EmployeeId == employeeId)
                        .OrderByDescending(l => l.CreatedDate)
                        .Take(10)
                        .ToListAsync();

                    ViewBag.MyLeaves = myLeaves;

                    var upcomingLeaves = await _context.LeaveRequests
                        .Where(l => l.EmployeeId == employeeId &&
                                   l.Status == "Approved" &&
                                   l.StartDate > DateTime.Today)
                        .OrderBy(l => l.StartDate)
                        .Take(5)
                        .ToListAsync();

                    ViewBag.UpcomingLeaves = upcomingLeaves;

                    ViewBag.MyProfile = new
                    {
                        employee.Name,
                        employee.Email,
                        employee.CNIC,
                        employee.Department,
                        employee.Designation,
                        employee.IsActive
                    };
                }

                return View("EmployeeDashboard");
            }
        }

        [Authorize]
        public async Task<IActionResult> TestLeaves()
        {
            var context = HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();

            var leaves = await context.LeaveRequests.ToListAsync();

            var employees = await context.Employees.ToListAsync();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentEmployee = await context.Employees
                .FirstOrDefaultAsync(e => e.IdentityUserId == userId);

            ViewBag.Leaves = leaves;
            ViewBag.Employees = employees;
            ViewBag.CurrentEmployee = currentEmployee;
            ViewBag.CurrentUserId = userId;

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}