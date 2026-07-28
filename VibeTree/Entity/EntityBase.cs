using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VibeTree.Entity;

public abstract class EntityBase
{
    public Guid Id { get; }
    public DateTime CreatedAt { get;  }
    public DateTime UpdatedAt { get;  }

    protected EntityBase() { }
    public EntityBase(Guid id, DateTime createdAt, DateTime updatedAt)
    {
        Id = id;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }
}
