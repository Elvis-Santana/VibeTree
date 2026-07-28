using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VibeTree.Shared.Entity;

public class Perfil : EntityBase
{
    public string Cor { get; private set; }
    public string Descricao { get; private set; }
    public string ImagemUrl { get; private set; }

    public string Slug { get; private set; }

    public Guid IdUser { get; private set; }

    private Perfil() : base() { }

    public Perfil(Guid id, DateTime createdAt, DateTime updatedAt,string cor,string descricao,string imagemUrl,string slug,Guid idUser) : base(id, createdAt, updatedAt)
    {
        Cor = cor;
        Descricao = descricao;
        ImagemUrl = imagemUrl;
        Slug = slug;
        IdUser = idUser;
    }
}
