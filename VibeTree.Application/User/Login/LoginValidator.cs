using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VibeTree.Application.Common;
using VibeTree.Domain.Entity;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace VibeTree.Application.User.Login;

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
