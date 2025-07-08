using Microsoft.EntityFrameworkCore;
using Model;

namespace DAL
{
    public class QuestionContext : DbContext
    {
        public DbSet<QuestionSet> QuestionSets { get; set; }
        public DbSet<Questions> Questions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=ExamApp;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Questions>()
                .HasOne(q => q.QuestionSet)
                .WithMany(s => s.Questions)
                .HasForeignKey(q => q.QuestionSetId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
