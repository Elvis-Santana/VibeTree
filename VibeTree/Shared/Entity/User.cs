using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VibeTree.Shared.Entity;

public class User : EntityBase
{
    public string Name { get; private set ; } 
    public string PasswordHash { get; private set; }
    public string Email { get; private set; }
    public virtual ICollection<Link> Links { get; private set; }

    public virtual Perfil Perfil { get; private  set; }

    private User() : base() { }
    
    
    public User
        (Guid id, DateTime createdAt, DateTime updatedAt,string name,string passwordHash,string email) 
        : base(id, createdAt, updatedAt)
    {
        Name = name;
        PasswordHash = passwordHash;
        Email = email;
    }

    public void SetName(string ?Name)=>
        this.Name = string.IsNullOrEmpty(Name) ? this.Name : Name;
    

    public void SetEmail(string ?Email) =>
       this.Email = string.IsNullOrEmpty(Email) ? this.Email : Email;

    

    public void setPasswordHash(string? PasswordHash )=>
        this.PasswordHash = string.IsNullOrWhiteSpace(PasswordHash) ?this.PasswordHash : BCrypt.Net.BCrypt.HashPassword(PasswordHash);

    
}
