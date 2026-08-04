using FluentAssertions;
using FluentValidation.TestHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VibeTree.Features.Link.CreateLink.Commands;

namespace VibeTree.Test.UnitTests.Features.Links.CreateLink;

public class CreateLinkCommandValidatorTests
{
    private readonly CreateLinkValidator _validator = new();

    [Theory]
    [InlineData( "LinkUrl", "LinkUrl não pode ser vazio")]
    [InlineData( "Descricao", "descrição não pode ter mais de 255 caracteres")]
    [InlineData( "IdPerfil", "IdPerfil não pode ser vazio")]
    public async Task Should_TerErroDeValidacao_When_DadosForemInvalidos(string nomeDoCampo, string mensagemEsperada)
    {
        var command = Utils.GenerateCreateLinkCommandInvalids(nomeDoCampo);

        var resultado = await _validator.TestValidateAsync(command);

        resultado.ShouldHaveValidationErrorFor(nomeDoCampo)
            .WithErrorMessage(mensagemEsperada);
    }

    [Fact]
    public async Task Should_PassarNaValidacao_When_ComandoForValido()
    {
        var command = new CreateLinkCommand("https://vibetree.com", "Descrição Válida", Guid.NewGuid().ToString(), 1, true);

        var resultado = await _validator.TestValidateAsync(command);

        resultado.ShouldNotHaveAnyValidationErrors();
        resultado.IsValid.Should().BeTrue();
    }
}
