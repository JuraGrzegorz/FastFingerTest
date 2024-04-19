using FastFingerTest.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FastFingerTest.Data;

public class FastFingerTestDbContext : IdentityDbContext
{
    public FastFingerTestDbContext(DbContextOptions options): base(options)
    {
    }

    public DbSet<Word> Words { get; set; }
    public DbSet<WritingTest> WritingTests { get; set; }
    public DbSet<UserWorld> UserWorlds { get; set; }
    public DbSet<WordSequence> WordSequences { get; set; }
    public DbSet<TestResult> TestsResults {  get; set; }
    public DbSet<TestAttend> TestsAttends { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<TestResult>()
                .HasOne(tr => tr.User)
                .WithMany()
                .HasForeignKey(tr => tr.UserId)
                .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TestResult>()
            .HasOne(tr => tr.WritingTest)
            .WithMany()
            .HasForeignKey(tr => tr.WritingTestId)
            .OnDelete(DeleteBehavior.ClientCascade);
    }

}
