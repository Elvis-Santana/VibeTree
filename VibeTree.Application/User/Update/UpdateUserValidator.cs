using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VibeTree.User.CreateUser;
using VibeTree.Application.Result;
using System.ComponentModel.DataAnnotations;
using FluentValidation.Validators;

namespace VibeTree.Application.User.Update;

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
