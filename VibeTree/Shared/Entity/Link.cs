using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VibeTree.Shared.Entity;

public class Link : EntityBase
{
    public string LinkUrl { get; private set; }
    public string Descricao { get; private set; }

    public Guid IdPerfil { get;private set; }

    public decimal Order { get; private set; }

    public bool Ativo { get; private set; } = true;


    private Link() : base() { }

    public Link(Guid id, DateTime createdAt, DateTime updatedAt, string linkUrl, string descricao, Guid idPerfil, decimal order, bool? ativo ) : base(id, createdAt, updatedAt)
    {
        LinkUrl = linkUrl;
        Descricao = descricao;
        IdPerfil = idPerfil;
        Order = order;
        Ativo = ativo??=true;
    }
}
