using Application.DTOs.OrdemServicos;
using Application.DTOs.OrdemServicos.Validators;

namespace Application.UnitTests.Validators;

public class CriarOrdemServicoValidatorTests
{
    private readonly CriarOrdemServicoValidator _validator = new();

    [Fact]
    public void ValidateComRequestValidoNaoDeveTerErros()
    {
        var request = new CriarOrdemServicoRequest(Guid.NewGuid(), null, "Nao liga", "2h", null, null, null, null);

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidateComClienteIdVazioDeveTerErro()
    {
        var request = new CriarOrdemServicoRequest(Guid.Empty, null, "Nao liga", "2h", null, null, null, null);

        var result = _validator.Validate(request);

        Assert.Contains(result.Errors, e => e.PropertyName == "ClienteId");
    }

    [Fact]
    public void ValidateComDefeitoVazioDeveTerErro()
    {
        var request = new CriarOrdemServicoRequest(Guid.NewGuid(), null, "", "2h", null, null, null, null);

        var result = _validator.Validate(request);

        Assert.Contains(result.Errors, e => e.PropertyName == "Defeito");
    }
}
