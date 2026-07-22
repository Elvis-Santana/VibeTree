using VibeTree.Application.Interfaces;
using VibeTree.Application.User;

namespace VibeTree.Application.User.Create.Commands;

public record class CreateUserAndProfileCommand(string Name,string Password,string Email, string Slug);



