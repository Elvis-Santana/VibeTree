using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VibeTree.Domain.Entity;

public class User : EntityBase
{
    public string Name { get; private set ; } 
    public string PasswordHash { get; private set; }
    public string Email { get; private set; }
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

    public void SetName(string ?Name)
    {
        this.Name = Name ?? this.Name;
    }

    public void SetEmail(string ?Email) { 
     this.Email = Email??this.Email;
    }

    public void setPasswordHash(string? PasswordHash )
    {
        this.PasswordHash = PasswordHash??this.PasswordHash;
    }
}
