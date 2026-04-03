using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VibeTree.Domain.Entity;

public class Perfile : EntityBase
{
    public string Cor { get;  }
    public string Descricao { get; }
    public string ImagemUrl { get; }

    public Perfile(Guid id, DateTime createdAt, DateTime updatedAt,string cor,string descricao,string imagemUrl) : base(id, createdAt, updatedAt)
    {
        Cor = cor;
        Descricao = descricao;
        ImagemUrl = imagemUrl;
    }
}
