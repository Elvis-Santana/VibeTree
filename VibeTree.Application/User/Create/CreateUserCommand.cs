using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VibeTree.Application.Interfaces;
using VibeTree.Application.Result;
using VibeTree.Application.User;

namespace VibeTree.User.CreateUser;

public record class CreateUserCommand(string Name,string Password,string Email) :IResquest<Userlogin>;



