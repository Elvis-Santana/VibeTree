using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VibeTree.Application.Result;

namespace VibeTree.Application.Perfil.Create;

public class CreatePerfilValidator : AbstractValidator<CreatePerfilCommand>
{
    public CreatePerfilValidator()
    {
        RuleFor(p => p.IdUser)
            .NotEmpty()
            .WithMessage(Error.IdUserEmpty.Message);

        RuleFor(p => p.Slug)
            .NotEmpty()
            .WithMessage(Error.SlugEmpty.Message);

        RuleFor(p => p.Cor)
          .NotEmpty()
          .WithMessage(Error.CorEmpty.Message);

      

    }
}
