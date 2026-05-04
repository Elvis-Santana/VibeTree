using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VibeTree.Domain.Entity;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace VibeTree.Application.User.Login;

public class LoginValidator :AbstractValidator<LoginQuery>
{
    public LoginValidator()
    {
        RuleFor(x => x.email)
            .NotEmpty().WithMessage("email não pode ser vazio")
            .EmailAddress().WithMessage("Formato de email inválido.");

        RuleFor(x => x.password).NotEmpty().WithMessage("password não pode ser vazio")
            .MinimumLength(6).WithMessage("password deve conter no mínimo 6 caracteres.");
    }

  
}
