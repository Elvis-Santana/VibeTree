using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VibeTree.Infrastructure.AppDbContext;

public class AppDbContext : DbContext
{

    public AppDbContext(DbContextOptions<AppDbContext> dbContextOptions) :base(dbContextOptions)
    {

    }
}
