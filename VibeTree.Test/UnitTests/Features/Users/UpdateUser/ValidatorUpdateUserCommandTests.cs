using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VibeTree.Features.User.UpdateUser.Commands;

namespace VibeTree.Test.UnitTests.Features.Users.UpdateUser;

public class ValidatorUpdateUserCommandTests
{
    [Theory]
    [InlineData("", "", "")]
    [InlineData("john", "", "")]
    [InlineData("", "john@gmail.com", "")]
    [InlineData("", "", "1234567")]
    public void Validate_ValidUpdateUserCommand_ReturnsSuccess(string name, string email, string password)
    {
        // Arrange
        var command = new UpdateUserCommand(Guid.NewGuid().ToString(), name, email, password);
        var validator = new UpdateUserValidator();

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "john", "john@gmail.com", "1234567")]
    [InlineData("7c9e6679-7425-40de-944b-e07fc1f90ae7", "jo", "john@gmail.com", "1234567")]
    [InlineData("7c9e6679-7425-40de-944b-e07fc1f90ae7", "john", "john#@$gmailcom", "1234567")]
    [InlineData("7c9e6679-7425-40de-944b-e07fc1f90ae7", "jo", "john@gmail.com", "1234")]
    public void Validate_ValidUpdateUserCommand_ReturnsError(string id, string name, string email, string password)
    {
        // Arrange
        var command = new UpdateUserCommand(id, name, email, password);
        var validator = new UpdateUserValidator();

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }
}
