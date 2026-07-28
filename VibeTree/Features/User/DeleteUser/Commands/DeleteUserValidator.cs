using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VibeTree.Shared.Common;

namespace VibeTree.Features.User.DeleteUser.Commands;

public class DeleteUserValidator : AbstractValidator<DeleteUserCommand>
{
    public DeleteUserValidator()
    {
        RuleFor(a => a.Id)
            .NotEmpty()
            .WithMessage(Error.IdEmpty.Message);
    }
}
