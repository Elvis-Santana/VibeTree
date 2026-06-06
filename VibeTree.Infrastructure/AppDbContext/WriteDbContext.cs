using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using VibeTree.Application.Interfaces;
using VibeTree.Domain.Entity;

namespace VibeTree.Infrastructure.AppDbContext;

public class WriteDbContext(DbContextOptions<WriteDbContext> dbContextOptions) : AbstractDbContext(dbContextOptions), IWriteDbContext;

  

    

