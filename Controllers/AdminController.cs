using FastFingerTest.Data;
using FastFingerTest.Dto;
using FastFingerTest.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastFingerTest.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly FastFingerTestDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        public AdminController(FastFingerTestDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var data=new AdminDataDto();
            data.SumRegisterUsers = _context.Users.Count();
            data.SumTestsResult=_context.TestsResults.Count();
            data.TodayCreatedTests = _context.WritingTests.Where(x=>x.CreationTime.Date==DateTime.Today).Count();
            data.TodayFinishTests=_context.TestsResults.Where(x => x.ResultTime.Date == DateTime.Today).Count();

            DateTime firstDayOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            data.TestsAttends = _context.TestsAttends
                .Where(t => t.AttendDate >= firstDayOfMonth && t.AttendDate <= lastDayOfMonth)
                .GroupBy(t => t.AttendDate.Date)
                .Select(g => new AdminAttendsInMonth 
                {
                    Date = g.Key,
                    attends = g.Count() 
                })
                .ToList();

            return View(data);
        }
        public IActionResult UserTests()
        {
            var popularTests = _context.WritingTests
               .GroupBy(t => new { t.Id, t.Name, t.CreationTime })
               .Select(g => new AdminUserTestsDto
               {
                   Name = g.Key.Name,
                   CreationDate = g.Key.CreationTime,
                   SumOfTests = g.Count()
               })
               .OrderByDescending(t => t.SumOfTests)
               .Take(10)
               .ToList();
            return View(popularTests);
        }

        
        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length <= 0)
            {
                ModelState.AddModelError("File", "Please select a file.");
                return View(file);
            }

            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                while (reader.Peek() >= 0)
                {
                    var line = await reader.ReadLineAsync();
                    _context.Words.Add(new Word { Name = line });
                   
                }
                _context.SaveChanges();
            }

            return View();
        }
        public async Task<IActionResult> Users(string searchString)
        {
            var users = _context.Users
                .Select(x => new AdminUsersDto
                {
                    Id = x.Id,
                    Name = x.UserName,
                });

            if (!String.IsNullOrEmpty(searchString))
            {
                users = users.Where(u => u.Name.Contains(searchString));
            }

            var data = await users.ToListAsync();

            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                return RedirectToAction("Users");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View("Error");
            }
        }
    }
}
