using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VibeTree.Application.Interfaces;

namespace VibeTree.Application.User.Update.Commands;

public record UpdateUserCommand(string Id,string? Name,  string? Email,string? Password) :IRequest<Userlogin>;


