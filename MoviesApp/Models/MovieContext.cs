using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoviesApp.Models
{
    public class MovieContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
    {
        public MovieContext(DbContextOptions<MovieContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<ApplicationUser>().HasKey(x => x.Id);
            builder.Entity<ApplicationUser>().HasMany(x => x.Movies).WithOne(x => x.ApplicationUser).HasForeignKey(x => x.UserId);
            builder.Entity<ApplicationUser>().HasMany(x => x.RefreshTokens).WithOne(x => x.ApplicationUser).HasForeignKey(x => x.UserId);
        }

        public DbSet<Movie> Movies { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
    }
}
