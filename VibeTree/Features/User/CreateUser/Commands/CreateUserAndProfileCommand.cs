namespace VibeTree.Features.User.CreateUser.Commands;

public record class CreateUserAndProfileCommand(string Name,string Password,string Email, string Slug);



