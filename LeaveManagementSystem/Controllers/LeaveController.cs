using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LeaveManagementSystem.Data;
using LeaveManagementSystem.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace LeaveManagementSystem.Controllers
{
    [Authorize]
    public class LeaveController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public LeaveController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

       
        public async Task<IActionResult> Index()
        {
            List<LeaveRequest> leaves;

            if (User.IsInRole("Admin"))
            {
                
                leaves = await _context.LeaveRequests
                    .OrderByDescending(l => l.CreatedDate)
                    .ToListAsync();

                ViewBag.Title = "All Leave Requests";
            }
            else
            {
               
                var employeeId = await GetCurrentEmployeeId();

                if (employeeId == 0)
                {
                    TempData["ErrorMessage"] = "Employee profile not found. Please contact admin.";
                    leaves = new List<LeaveRequest>();
                }
                else
                {
                    leaves = await _context.LeaveRequests
                        .Where(l => l.EmployeeId == employeeId)
                        .OrderByDescending(l => l.CreatedDate)
                        .ToListAsync();
                }

                ViewBag.Title = "My Leave Requests";
            }

            return View(leaves);
        }

     
        public IActionResult Create()
        {
            return View();
        }

      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LeaveRequest leaveRequest)
        {
            
            if (leaveRequest.StartDate < DateTime.Today)
            {
                ModelState.AddModelError("StartDate", "Start date cannot be in the past.");
            }

            if (leaveRequest.EndDate < leaveRequest.StartDate)
            {
                ModelState.AddModelError("EndDate", "End date must be after start date.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var employeeId = await GetCurrentEmployeeId();

                    if (employeeId == 0)
                    {
                        TempData["ErrorMessage"] = "Cannot submit leave. Employee profile not found.";
                        return View(leaveRequest);
                    }

                    
                    leaveRequest.EmployeeId = employeeId;
                    leaveRequest.Status = "Pending";
                    leaveRequest.CreatedDate = DateTime.Now;

                   
                    _context.LeaveRequests.Add(leaveRequest);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Leave request submitted successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Error: {ex.Message}";
                }
            }

            return View(leaveRequest);
        }
        
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var leaveRequest = await _context.LeaveRequests.FindAsync(id);
            if (leaveRequest == null)
            {
                return NotFound();
            }

            if (User.IsInRole("Employee"))
            {
                var employeeId = await GetCurrentEmployeeId();
                if (leaveRequest.EmployeeId != employeeId)
                {
                    TempData["ErrorMessage"] = "You can only edit your own leave requests.";
                    return RedirectToAction(nameof(Index));
                }

                if (leaveRequest.Status != "Pending")
                {
                    TempData["ErrorMessage"] = "You can only edit pending leave requests.";
                    return RedirectToAction(nameof(Index));
                }
            }

            return View(leaveRequest);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LeaveRequest leaveRequest)
        {
            if (id != leaveRequest.LeaveId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(leaveRequest);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Leave request updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LeaveRequestExists(leaveRequest.LeaveId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(leaveRequest);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var leaveRequest = await _context.LeaveRequests.FindAsync(id);
            if (leaveRequest == null)
            {
                return NotFound();
            }

            return View(leaveRequest);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var leaveRequest = await _context.LeaveRequests.FindAsync(id);
            if (leaveRequest != null)
            {
                _context.LeaveRequests.Remove(leaveRequest);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Leave request cancelled successfully!";
            }
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var leaveRequest = await _context.LeaveRequests.FindAsync(id);
            if (leaveRequest != null)
            {
                leaveRequest.Status = "Approved";
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Leave request approved!";
            }
            return RedirectToAction(nameof(Index));
        }

        
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var leaveRequest = await _context.LeaveRequests.FindAsync(id);
            if (leaveRequest != null)
            {
                leaveRequest.Status = "Rejected";
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Leave request rejected!";
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task<int> GetCurrentEmployeeId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return 0;
            }

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.IdentityUserId == userId);

            return employee?.EmployeeId ?? 0;
        }

        private bool LeaveRequestExists(int id)
        {
            return _context.LeaveRequests.Any(e => e.LeaveId == id);
        }
    }
}