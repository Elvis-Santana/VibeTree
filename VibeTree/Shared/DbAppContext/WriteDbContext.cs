using Microsoft.EntityFrameworkCore;

namespace VibeTree.Shared.DbAppContext;

public class WriteDbContext : AbstractDbContext
{

    public WriteDbContext(DbContextOptions<WriteDbContext> options) : base(options)
    {
    }

    protected WriteDbContext() : base()
    {
    }
}





