using FluentValidation;
using VibeTree.Shared.Common;

namespace VibeTree.Features.User.Login;

public class LoginValidator :AbstractValidator<LoginQuery>
{
    public LoginValidator()
    {


        RuleFor(x => x.email)
            .NotEmpty().WithMessage(Error.EmailEmpty.Message)
            .EmailAddress().WithMessage(Error.EmailAddress.Message);

        RuleFor(x => x.password)
            .NotEmpty().WithMessage(Error.PasswordEmpty.Message)
            .MinimumLength(6).WithMessage(Error.PasswordMinimumLength.Message);
    }

  
}
