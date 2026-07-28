using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VibeTree.Shared.Entity;

public class Link : EntityBase
{
    public string LinkUrl { get; }
    public string Descricao { get; }

    public Guid IdUser { get; }

    public int Order { get; }

    public bool Ativo { get; }


    private Link() : base() { }

    public Link(Guid id, DateTime createdAt, DateTime updatedAt, string linkUrl, string descricao, Guid idUser, int order, bool ativo) : base(id, createdAt, updatedAt)
    {
        LinkUrl = linkUrl;
        Descricao = descricao;
        IdUser = idUser;
        Order = order;
        Ativo = ativo;
    }
}
