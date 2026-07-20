using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VibeTree.Domain.Entity;


namespace VibeTree.Application.Interfaces;

public interface IAppDbContext
{
    public DbSet<Domain.Entity.User> Users { get; set; }
    public DbSet<Link> Links { get; set; }
    public DbSet<Domain.Entity.Perfil> perfils { get; set; }

    public DatabaseFacade Database { get; }


    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

}
