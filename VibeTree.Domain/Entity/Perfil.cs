using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VibeTree.Domain.Entity;

public class Perfil : EntityBase
{
    public string Cor { get;  }
    public string Descricao { get; }
    public string ImagemUrl { get; }

    public Guid IdUser { get; }

    private Perfil() : base() { }

    public Perfil(Guid id, DateTime createdAt, DateTime updatedAt,string cor,string descricao,string imagemUrl,Guid idUser) : base(id, createdAt, updatedAt)
    {
        Cor = cor;
        Descricao = descricao;
        ImagemUrl = imagemUrl;
        IdUser = idUser;
    }
}
