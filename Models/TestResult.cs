using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;

namespace FastFingerTest.Models
{
    public class TestResult
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string? WritingTestId { get; set; }
        public int Points { get; set; }
        public int MaxPoints { get; set; }
        public DateTime ResultTime { get; set; }
        public IdentityUser User { get; set; }
        public WritingTest? WritingTest { get; set; }
    }   
}
