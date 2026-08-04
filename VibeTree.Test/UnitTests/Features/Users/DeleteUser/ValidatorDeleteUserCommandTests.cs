using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VibeTree.Features.User.DeleteUser.Commands;

namespace VibeTree.Test.UnitTests.Features.Users.DeleteUser;

public class ValidatorDeleteUserCommandTests
{
    [Fact]
    public void Validate_ValidDeleteUserCommand_ReturnsError()
    {
        // Arrange
        var command = new DeleteUserCommand(Guid.NewGuid().ToString());
        var validator = new DeleteUserValidator();

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
    [Fact]
    public void Validate_ValidDeleteUserCommand_ReturnsSuccess()
    {
        // Arrange
        var command = new DeleteUserCommand(string.Empty);
        var validator = new DeleteUserValidator();

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }
}
