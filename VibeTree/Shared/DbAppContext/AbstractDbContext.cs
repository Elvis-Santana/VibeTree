using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VibeTree.Shared.Entity;


namespace VibeTree.Shared.DbAppContext;

public abstract class AbstractDbContext : DbContext
{

    public AbstractDbContext(DbContextOptions dbContextOptions) : base(dbContextOptions) { }

    protected AbstractDbContext() { }

    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<Link> Links { get ; set ; }
    public virtual DbSet<Perfil> perfils { get  ; set; }


  
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Link>(l =>
        {
            l.HasKey(l => l.Id);

            l.HasOne<Perfil>()
            .WithMany(p => p.Links)
            .HasForeignKey(l => l.IdPerfil)
            .OnDelete(DeleteBehavior.Cascade);

        });
       
        modelBuilder.Entity<User>(u =>
        {
            u.HasKey(x => x.Id);

            u.HasIndex(a => a.Email);

            u.Property(x => x.Name);
            u.Property(x => x.PasswordHash).HasMaxLength(255);
            u.Property(x => x.Email)
                .IsUnicode();
            u.Property(x => x.CreatedAt);
            u.Property(x => x.UpdatedAt);


            u.Property(x => x.Id)
            .ValueGeneratedNever();
        });



        modelBuilder.Entity<Perfil>(p =>
        {
            p.HasKey(x => x.Id);

            p.Property(x => x.Id)
                .ValueGeneratedNever();
            p.Property(x => x.Descricao)
                .HasMaxLength(255);
            p.HasIndex(x => x.Slug);
            p.Property(x => x.Slug)
                .HasMaxLength(255)
                .IsUnicode();
            p.Property(x => x.ImagemUrl);
            p.Property(x => x.Cor);
            p.Property(x => x.CreatedAt);
            p.Property(x => x.UpdatedAt);

            p.HasOne<User>()                    
            .WithOne(u => u.Perfil)                   
            .HasForeignKey<Perfil>(x => x.IdUser)     
            .OnDelete(DeleteBehavior.Cascade)       
            .IsRequired(false);


        
        });

        base.OnModelCreating(modelBuilder);
    }
}
