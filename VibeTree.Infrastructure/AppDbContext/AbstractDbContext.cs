using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VibeTree.Application.Interfaces;
using VibeTree.Domain.Entity;
using UserEntity = VibeTree.Domain.Entity.User;

namespace VibeTree.Infrastructure.AppDbContext;

public abstract class AbstractDbContext(DbContextOptions dbContextOptions) : DbContext(dbContextOptions), IAppDbContext
{
    public DbSet<UserEntity> Users { get; set; }
    public DbSet<Link> Links { get ; set ; }
    public DbSet<Perfil> perfils { get  ; set; }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
     => base.SaveChangesAsync(cancellationToken);


    
 

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Link>()
          .HasKey(l => l.Id);
       
        modelBuilder.Entity<UserEntity>(u =>
        {
            u.HasKey(x => x.Id);
            u.Property(x => x.Name);
            u.Property(x => x.PasswordHash).HasMaxLength(255);
            u.Property(x => x.Email).IsUnicode();
            u.Property(x => x.CreatedAt);
            u.Property(x => x.UpdatedAt);

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
            p.Property(x => x.CreatedAt);
            p.Property(x => x.UpdatedAt);

            p.HasOne<UserEntity>()
            .WithOne()
            .HasForeignKey<Perfil>(x => x.IdUser)
            .OnDelete(DeleteBehavior.Cascade);


        });

        base.OnModelCreating(modelBuilder);
    }
}
