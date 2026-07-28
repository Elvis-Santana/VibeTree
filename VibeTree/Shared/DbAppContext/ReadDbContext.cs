using Microsoft.EntityFrameworkCore;

namespace VibeTree.Shared.DbAppContext;

public class ReadDbContext: AbstractDbContext
{

    public ReadDbContext(DbContextOptions<ReadDbContext> options) : base(options)
    {
    }

    protected ReadDbContext() : base()
    {
    }
}

