using Bogus;
using Bogus.Extensions;
using FluentAssertions;
using FluentValidation.TestHelper;
using VibeTree.Features.User.CreateUser.Commands;
using VibeTree.Features.User.DeleteUser.Commands;
using VibeTree.Features.User.UpdateUser.Commands;
using VibeTree.Shared.Common;

namespace VibeTree.Test.UnitTests.Features.Users.CreateUser;

public class ValidatorCreateUserAndProfileCommandTests
{
    private static readonly Faker _faker = new Faker("pt_BR");


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

    public static IEnumerable<object[]> GetInvalidNames => new List<object[]>
    {
        new object[] { string.Empty, _faker.Internet.Password(),_faker.Internet.Email(),  _faker.Internet.UserName(), Error.NameEmpty.Message },
        new object[] { "1", _faker.Internet.Password(),_faker.Internet.Email(),  _faker.Internet.UserName(),Error.NameMinimumLength.Message},
        new object[] { "13", _faker.Internet.Password(),_faker.Internet.Email(),  _faker.Internet.UserName(),Error.NameMinimumLength.Message},
    };
    public static IEnumerable<object[]> GetInvalidPasswords => new List<object[]>
    {
        new object[] { _faker.Internet.UserName(), string.Empty,_faker.Internet.Email(),  _faker.Internet.UserName(),Error.PasswordEmpty.Message },
        new object[] { _faker.Internet.UserName(), _faker.Internet.Password().ClampLength(max:5),_faker.Internet.Email(),  _faker.Internet.UserName(),Error.PasswordMinimumLength.Message},
       
    };

    public static IEnumerable<object[]> GetInvalidEmails => new List<object[]>
    {
        new object[] { _faker.Internet.UserName(), _faker.Internet.Password(),string.Empty,  _faker.Internet.UserName(),Error.EmailEmpty.Message},
        new object[] { _faker.Internet.UserName(), _faker.Internet.Password().ClampLength(max:5),"elvin@#$$$",  _faker.Internet.UserName(),Error.EmailAddress.Message},
    };

    public static IEnumerable<object[]> GetInvalidSlugs => new List<object[]>
    {
        new object[]{ _faker.Internet.UserName(), _faker.Internet.Password(),_faker.Internet.Email(),  string.Empty,Error.SlugEmpty.Message},
    };


    [Theory]
    [MemberData(nameof(GetInvalidNames))]
    [MemberData(nameof(GetInvalidPasswords))]
    [MemberData(nameof(GetInvalidEmails))]
    [MemberData(nameof(GetInvalidSlugs))]

    public async Task Validate_InvalidCreateUserCommand_ReturnsSpecificErrorMessage(
      string name,
      string password,
      string email,
      string slug,
      string expectedErrorMessage)
    {
        var command = new CreateUserAndProfileCommand(name, password, email, slug);
        var validator = new CreateUserAndProfileValidator();

        var result = await validator.TestValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Any(x => x.ErrorMessage == expectedErrorMessage)
                          .Should()
                          .BeTrue();
    }


}
