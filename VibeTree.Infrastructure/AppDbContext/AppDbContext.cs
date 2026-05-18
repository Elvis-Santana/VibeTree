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
    public DbSet<Perfil> perfils { get; set; }


   
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Link>()
          .HasKey(l => l.Id);
        modelBuilder.Entity<User>(u =>
        {
            u.HasKey(x => x.Id);
            u.Property(x => x.Name);
            u.Property(x => x.PasswordHash).HasMaxLength(255);
            u.Property(x => x.Email).IsUnicode();

             u.HasMany(x => x.Links)
            .WithOne()
            .HasForeignKey(l => l.IdUser)
            .OnDelete(DeleteBehavior.Cascade);

        });

          

        modelBuilder.Entity<Perfil>(p =>
        {
            p.HasKey(x => x.Id);
            p.Property(x => x.Descricao).HasMaxLength(255);
            p.Property(x => x.Slug).HasMaxLength(255);
            p.Property(x => x.ImagemUrl);
            p.Property(x => x.Cor);

            p.HasOne<User>()
            .WithOne()
            .HasForeignKey<Perfil>(x => x.IdUser)
            .OnDelete(DeleteBehavior.Cascade);


        });
            
        base.OnModelCreating(modelBuilder);
    }
}
