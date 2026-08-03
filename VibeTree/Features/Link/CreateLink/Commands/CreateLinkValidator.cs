using FluentValidation;
using VibeTree.Shared.Common;

namespace VibeTree.Features.Link.CreateLink.Commands;

public class CreateLinkValidator :AbstractValidator<CreateLinkCommand>
{
    public CreateLinkValidator()
    {
        RuleFor(l => l.Descricao)
            .MaximumLength(255)
            .WithMessage("descrição não pode ter mais de 255 caracteres");

        RuleFor(l => l.IdPerfil)
          .NotEmpty()
          .WithMessage("IdPerfil não pode ser vazio");


        RuleFor(l => l.Order)
           .NotNull()
           .WithMessage("Order não  pode ser nulo");

        RuleFor(l => l.LinkUrl)
         .NotEmpty()
         .WithMessage("LinkUrl não pode ser vazio");

    }

    
}
