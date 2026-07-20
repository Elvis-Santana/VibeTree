using VibeTree.Application.Interfaces;
using VibeTree.Application.User;

namespace VibeTree.User.CreateUser;

public record class CreateUserAndProfileCommand(string Name,string Password,string Email, string Slug) :IRequest<Userlogin>;



