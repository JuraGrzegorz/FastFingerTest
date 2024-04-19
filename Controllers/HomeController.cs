using System.Diagnostics;
using System.Linq;
using FastFingerTest.Data;
using FastFingerTest.Dto;
using FastFingerTest.Models;
using FastFingerTest.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NuGet.Packaging.Signing;

namespace FastFingerTest.Controllers
{

    public class HomeController : Controller
    {
        private readonly FastFingerTestDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private LanguageService _localization;
        public HomeController(FastFingerTestDbContext context, UserManager<IdentityUser> userManager, LanguageService localization)
        {
            _context = context;
            _userManager = userManager;
            _localization = localization;
        }

        public IActionResult Index()
        {
            var Words = _context.Words.Select(x => x.Name).OrderBy(x => Guid.NewGuid()).Take(200).ToList();
            var data = new startPageDto()
            {
                Id="",
                Words = Words,
                MaxPoints = 200,
                Length = 60
            };
            return View(data);
        }

        
        public async Task<IActionResult> UserTest(string id)
        {
            string? TestId = _context.WritingTests
                .Where(wt => wt.Id == id)
                .Select(wt => wt.Id)
                .FirstOrDefault();

            if (TestId == null)
            {
                return NotFound();
            }

            var Words = _context.WordSequences
                .Where(ws => ws.WritingTestId == TestId)
                .OrderBy(ws => ws.Position)
                .Select(ws => ws.Word.Name)
                .ToList();

            var testInfo = await _context.WritingTests
                .Where(wt => wt.Id == id)
                .Select(wt => new
                {
                    wt.Length,
                    wt.MaxPoints
                })
                .FirstOrDefaultAsync();

            var data= new startPageDto()
            {
                Id=id,
                Words = Words,
                Length= testInfo.Length,
                MaxPoints=testInfo.MaxPoints
            };

            return View("Index", data);
        }

        public IActionResult UsersTests()
        {
            var UserTests = _context.WritingTests
                        .Include(w => w.User)
                        .Select(w => new ShowUserTestDto
                        {
                            TestId = w.Id,
                            UserName = w.User.UserName,
                            TestName = w.Name,
                            TestLength = w.Length,
                            MaxPoints = w.MaxPoints,
                            TestCreationDate = w.CreationTime
                        })
                        .ToList();

            return View(UserTests);
        }

        [Authorize]
        public IActionResult CreateTest()
        {

            ViewBag.SuccessModal = JsonConvert.SerializeObject(false);
            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateTest(CreateTestDto val)
        {

            if (!ModelState.IsValid)
            {
                ViewBag.SuccessModal = JsonConvert.SerializeObject(false);
                return View(val);
            }


            using (var reader = new StreamReader(val.File.OpenReadStream()))
            {
                var fileContent = await reader.ReadToEndAsync();
                var splittedText = fileContent.Split((char)val.Separator, StringSplitOptions.RemoveEmptyEntries);

                var writingTest = new WritingTest
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = val.Name,
                    Length = val.Length,
                    MaxPoints = splittedText.Length,
                    CreationTime = DateTime.Now,
                    UserId = (await _userManager.GetUserAsync(User))?.Id
                };
                _context.WritingTests.Add(writingTest);
                _context.SaveChanges();

                foreach (var item in splittedText)
                {

                    var existingWord = _context.UserWorlds.FirstOrDefault(w => w.Name == item);
                    if (existingWord == null)
                    {
                        var newWord = new UserWorld
                        {
                            Name = item
                        };
                        _context.UserWorlds.Add(newWord);
                        _context.SaveChanges();

                        var newWordSequence = new WordSequence
                        {
                            WritingTestId = writingTest.Id,
                            Position = Array.IndexOf(splittedText, item) + 1,
                            UserWordId = newWord.Id
                        };
                        _context.WordSequences.Add(newWordSequence);
                    }
                    else
                    {
                        var newWordSequence = new WordSequence
                        {
                            WritingTestId = writingTest.Id,
                            Position = Array.IndexOf(splittedText, item) + 1,
                            UserWordId = existingWord.Id
                        };
                        _context.WordSequences.Add(newWordSequence);
                    }
                }
                _context.SaveChanges();
            }

            ViewBag.SuccessModal = JsonConvert.SerializeObject(true);
            return View(new CreateTestDto());

        }
        public IActionResult BestPlayers()
        {
            var Results = _context.WritingTests
                .Select(wt => new SelectWritingTestDto { Id = wt.Id, Name = wt.Name })
                .ToList();
            return View(Results);
        }

        public IActionResult TestResults(string writingTestId)
        {
            var bestResults =  _context.TestsResults
            .Where(tr => tr.WritingTestId == writingTestId)
            .GroupBy(tr => tr.UserId)
            .Select(group => new
            {
                Id = group.OrderByDescending(tr => tr.Points).Select(tr => tr.Id).FirstOrDefault(),
                UserId = group.Key,
                MaxPoints = group.Max(tr => tr.Points)
            })
            .ToList();

            var allRecordsForIds = _context.TestsResults
                .Where(tr => bestResults.Select(br => br.Id).Contains(tr.Id))
                .Select(tr => new SelectTestResultDto
                {
                    UserNick = tr.User.UserName,
                    Score = (float)tr.Points / tr.MaxPoints,
                    ResultTime = tr.ResultTime
                })
                .OrderByDescending(tr => tr.Score)
                .ToList();


            return PartialView("_TestResults", allRecordsForIds);
        }


        [Authorize]
        public IActionResult UserTestManage()
        {
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                var loginUrl = Url.Page("/Account/Login");
                return View("Identity", loginUrl);
            }
            var data=_context.WritingTests
                .Where(wt => wt.UserId == userId)
                .Select(wt=>new UserTestManageDto
                {
                    Id=wt.Id,
                    Name= wt.Name,
                    Lenght=wt.Length,
                    MaxPoints=wt.MaxPoints,
                    CreationDate=wt.CreationTime
                })
                .ToList();
            return View(data);
        }

        public IActionResult TestDel(string id)
        {
            
            var testToDelete = _context.WritingTests.FirstOrDefault(t => t.Id == id);

            if (testToDelete == null)
            {
                return NotFound();
            }
            var resultsToDelete = _context.TestsResults.Where(tr => tr.WritingTestId == id);
            _context.TestsResults.RemoveRange(resultsToDelete);
            _context.WritingTests.Remove(testToDelete);
            _context.SaveChanges();

            return RedirectToAction("UserTestManage");
        }

        public IActionResult ChangeLanguage(string culture)
        {
            Response.Cookies.Append(CookieRequestCultureProvider.DefaultCookieName, CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)), new CookieOptions()
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1)
            });
            return Redirect(Request.Headers["Referer"].ToString());
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
