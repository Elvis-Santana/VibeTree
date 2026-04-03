using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VibeTree.Domain.Entity;

public class LinkClick : EntityBase
{
    public Guid IdLink { get; }
    public long ClickCount { get; }

    public LinkClick(Guid id, DateTime createdAt, DateTime updatedAt, Guid idLink, long clickCount) : base(id, createdAt, updatedAt)
    {
        IdLink = idLink;
        ClickCount = clickCount;
    }
}
