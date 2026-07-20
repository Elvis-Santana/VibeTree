using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VibeTree.Application.Auth;
using VibeTree.Application.Interfaces;

namespace VibeTree.Application.User;

public record Userlogin(Guid Id,string Name,string Email,string Token);


