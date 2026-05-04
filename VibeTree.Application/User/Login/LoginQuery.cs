using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VibeTree.Application.Interfaces;

namespace VibeTree.Application.User.Login;

public record LoginQuery(string password, string email):IResquest<Userlogin>;

