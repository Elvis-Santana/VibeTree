using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VibeTree.Application.Common;

namespace VibeTree.Application.User.Delete.Commands;

public class DeleteUserValidator : AbstractValidator<DeleteUserCommand>
{
    public DeleteUserValidator()
    {
        RuleFor(a => a.Id)
            .NotEmpty()
            .WithMessage(Error.IdEmpty.Message);
    }
}
