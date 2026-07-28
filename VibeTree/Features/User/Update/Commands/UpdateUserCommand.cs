namespace VibeTree.Features.User.Update.Commands;

public record UpdateUserCommand(string Id,string? Name,  string? Email,string? Password);


