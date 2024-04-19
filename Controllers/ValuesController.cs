using System.Security.Claims;
using FastFingerTest.Data;
using FastFingerTest.Dto;
using FastFingerTest.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace FastFingerTest.Controllers
{
    [Route("api")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly FastFingerTestDbContext _context;

        public ValuesController(FastFingerTestDbContext context)
        {
            _context = context;
        }

        [HttpPost("SaveScore")]
        public IActionResult SaveScore([FromBody] ScoreData scoreData)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return StatusCode(401, "Authorization Error");
            }

            Console.WriteLine(scoreData.Id+" : :"+scoreData.Points);
            var testRes = new TestResult
            {
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                WritingTestId= scoreData.Id is "" ? null : scoreData.Id,
                Points =scoreData.Points,
                MaxPoints=scoreData.MaxPoints,
                ResultTime=DateTime.Now
             };
            _context.TestsResults.Add(testRes);
            _context.SaveChanges();
            return Ok();
        }
       
        [HttpPost("SaveAttend")]
        public IActionResult SaveAttend()
        {
            var result = new TestAttend();
            result.AttendDate=DateTime.Now;
            _context.TestsAttends.Add(result);
            _context.SaveChanges();
            return Ok();
        }

    }
}
