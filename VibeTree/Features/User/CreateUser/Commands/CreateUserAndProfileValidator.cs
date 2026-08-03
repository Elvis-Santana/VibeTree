using FluentValidation;
using VibeTree.Shared.Common;

namespace VibeTree.Features.User.CreateUser.Commands;

public class CreateUserAndProfileValidator : AbstractValidator<CreateUserAndProfileCommand>
{

    public CreateUserAndProfileValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(Error.NameEmpty.Message)
            .MinimumLength(3).WithMessage(Error.NameMinimumLength.Message);


        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(Error.PasswordEmpty.Message)
            .MinimumLength(6).WithMessage(Error.PasswordMinimumLength.Message);

        RuleFor(x => x.Email)
           .NotEmpty().WithMessage(Error.EmailEmpty.Message)
           .EmailAddress(FluentValidation.Validators.EmailValidationMode.Net4xRegex)
           .WithMessage(Error.EmailAddress.Message);

        RuleFor(p => p.Slug)
        .NotEmpty()
        .WithMessage(Error.SlugEmpty.Message);
    }

}
