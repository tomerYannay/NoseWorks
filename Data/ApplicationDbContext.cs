using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyFirstMvcApp.Models;

namespace MyFirstMvcApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        
        public DbSet<Session> Sessions { get; set; }
        public DbSet<Dog> Dogs { get; set; }
        public DbSet<TrainingProgram> TrainingPrograms { get; set; }
        public DbSet<Trial> Trials { get; set; } 

        public DbSet<RegisterModel> RegisterUsers { get; set; }
        
    }
}