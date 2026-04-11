using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VibeTree.Domain.Entity;

namespace VibeTree.Infrastructure.AppDbContext;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> dbContextOptions) : base(dbContextOptions)
    {

    }

    public DbSet<User> Users { get;  set; }
    public  DbSet<Link> Links { get;  set; }


   
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Link>()
           .HasKey(l => l.Id);


        modelBuilder.Entity<User>(u =>
        {
            u.HasKey(x => x.Id);
            u.Property(x => x.Email).IsUnicode();
             u.HasMany(x => x.Links)
            .WithOne()
            .HasForeignKey(l => l.IdUser)
            .OnDelete(DeleteBehavior.Cascade);

        });
            
        base.OnModelCreating(modelBuilder);
    }
}
