using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VibeTree.Application.Interfaces;
using VibeTree.Application.Result;
using VibeTree.Domain;

namespace VibeTree.Application.User.Get;

public record GetAllUserQuery(): IRequest<Userlogin>;


