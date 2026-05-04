using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VibeTree.Application.Result;
using VibeTree.Application.User.Login;

namespace VibeTree.User.CreateUser;

public class CreateUserValidator : AbstractValidator<CreateUserCommand>
{

    public CreateUserValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(Error.NameEmpty.Message)
            .MinimumLength(3).WithMessage(Error.NameMinimumLength.Message);

        RuleFor(x => x.Email)
           .NotEmpty().WithMessage(Error.EmailEmpty.Message)
           .EmailAddress().WithMessage(Error.EmailAddress.Message);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(Error.PasswordEmpty.Message)
            .MinimumLength(6).WithMessage(Error.PasswordMinimumLength.Message);
    }

}
