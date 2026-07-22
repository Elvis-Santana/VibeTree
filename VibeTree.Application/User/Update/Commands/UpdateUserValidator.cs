using FluentValidation;
using VibeTree.Application.Common;

namespace VibeTree.Application.User.Update.Commands;

public class UpdateUserValidator: AbstractValidator<UpdateUserCommand>
{
    public UpdateUserValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage(Error.IdEmpty.Message);



        RuleFor(x => x.Email)
         .EmailAddress(FluentValidation.Validators.EmailValidationMode.Net4xRegex)
         .WithMessage(Error.EmailAddress.Message)
         .When(x => !string.IsNullOrWhiteSpace(x.Email));



       RuleFor(x => x.Password)
          .MinimumLength(6)
          .WithMessage(Error.PasswordMinimumLength.Message)
          .When(x => !string.IsNullOrWhiteSpace(x.Password));

        RuleFor(x => x.Name)
         .MinimumLength(3)
         .WithMessage(Error.NameEmpty.Message)
         .When(x => !string.IsNullOrWhiteSpace(x.Name));

    }
}
