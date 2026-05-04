using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VibeTree.Domain.Entity;

public class User : EntityBase
{
    public string Name { get; } 
    public string PasswordHash { get; }
    public string Email { get; }
    public virtual ICollection<Link> Links { get; }

    private User() : base() { }
    
    
    public User
        (Guid id, DateTime createdAt, DateTime updatedAt,string name,string passwordHash,string email) 
        : base(id, createdAt, updatedAt)
    {
        Name = name;
        PasswordHash = passwordHash;
        Email = email;
    }
}
