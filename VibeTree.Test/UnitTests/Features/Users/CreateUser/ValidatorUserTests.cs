using Bogus;
using FluentAssertions;
using VibeTree.Features.User.CreateUser.Commands;
using VibeTree.Features.User.DeleteUser.Commands;
using VibeTree.Features.User.UpdateUser.Commands;

namespace VibeTree.Test.UnitTests.Features.Users.CreateUser;

public class ValidatorUserTest
{

    [Theory]
    [InlineData("", "","")]
    [InlineData("john", "", "")]
    [InlineData("", "john@gmail.com", "")]
    [InlineData("", "", "1234567")]
    public void Validate_ValidUpdateUserCommand_ReturnsSuccess(string name, string email,string password)
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
    [InlineData("", "john", "john@gmail.com","1234567")]
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

    [Fact]
    public void Validate_ValidCreateUserCommand_ReturnsSuccess()
    {
        // Arrange

        Faker faker = new Faker("pt_BR");

        var command = new CreateUserAndProfileCommand(
            faker.Name.FullName(), 
            faker.Internet.Password(),
            faker.Internet.Email(),
             faker.Name.FullName()
         );

        var validator = new CreateUserAndProfileValidator();

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();

    }


    [Theory]
    [InlineData("", "","","")]
    [InlineData("asd", "elvis@gmailcom", "123456","")]
    public void Validate_ValidCreateUserCommand_ReturnsError(string name, string email ,string password,string slug)
    {
        // Arrange
        var command = new CreateUserAndProfileCommand(name,  email,password, slug);
        var validator = new CreateUserAndProfileValidator();

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

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
