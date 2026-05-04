using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VibeTree.Application.Interfaces;
using VibeTree.Application.Result;

namespace VibeTree.Application.User.Login;

public record LoginQuery(string password, string email):IResquest<Result<Userlogin>>;

