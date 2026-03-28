---
name: criar-teste
description: Cria testes unitarios ou de integracao seguindo os padroes do projeto - xUnit, Moq, Bogus, com nomenclatura e organizacao corretas.
disable-model-invocation: true
user-invocable: true
argument-hint: "[camada] [classe-alvo]"
---

# Criar Testes — $ARGUMENTS

Crie testes para **$ARGUMENTS** seguindo os padroes do projeto.

## Identificar a Camada

- **Domain**: testar em `tests/Domain.UnitTests/` — SEM mocking, testar entidades puras.
- **Application**: testar em `tests/Application.UnitTests/` — COM Moq para repositorios e UnitOfWork.
- **Web**: testar em `tests/Web.UnitTests/` — COM Moq para API services, testar ViewModels.
- **Api**: testar em `tests/Api.IntegrationTests/` — COM Bogus para dados fake.

## Padrao para Testes de Domain

```csharp
namespace Domain.UnitTests.Entities;

public sealed class XxxTests
{
    [Fact]
    public void Criar_ComDadosValidos_DeveCriarComSucesso()
    {
        // Arrange & Act
        var entidade = Xxx.Criar(/* parametros validos */);

        // Assert
        Assert.NotEqual(Guid.Empty, entidade.Id);
        Assert.Equal(/* valor esperado */, entidade.Propriedade);
    }

    [Fact]
    public void Criar_ComCampoObrigatorioVazio_DeveLancarArgumentException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            Xxx.Criar(/* parametro invalido */));

        Assert.Contains("obrigatorio", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void MetodoNegocio_ComEstadoInvalido_DeveLancarDomainException()
    {
        // Arrange
        var entidade = Xxx.Criar(/* ... */);

        // Act & Assert
        Assert.Throws<DomainException>(() =>
            entidade.MetodoNegocio(/* ... */));
    }
}
```

## Padrao para Testes de Application

```csharp
using Moq;

namespace Application.UnitTests.Services;

public sealed class XxxServiceTests
{
    private readonly Mock<IXxxRepository> _repositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly XxxService _sut;

    public XxxServiceTests()
    {
        _sut = new XxxService(_repositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task CriarAsync_ComRequestValido_DeveSalvarERetornarResponse()
    {
        // Arrange
        var request = new CriarXxxRequest(/* ... */);

        // Act
        var response = await _sut.CriarAsync(request);

        // Assert
        _repositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<Xxx>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.NotNull(response);
    }

    [Fact]
    public async Task ObterPorIdAsync_QuandoNaoExiste_DeveRetornarNull()
    {
        // Arrange
        _repositoryMock.Setup(r => r.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Xxx?)null);

        // Act
        var result = await _sut.ObterPorIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }
}
```

## Padrao para Testes de Web (ViewModel)

```csharp
using Moq;

namespace Web.UnitTests.ViewModels;

public sealed class XxxViewModelTests
{
    private readonly Mock<IXxxApi> _apiMock = new();
    private readonly XxxViewModel _sut;

    public XxxViewModelTests()
    {
        _sut = new XxxViewModel(_apiMock.Object);
    }

    [Fact]
    public async Task SubmitAsync_ComSucesso_DeveSetarSuccessState()
    {
        // Arrange
        _apiMock.Setup(a => a.CriarAsync(It.IsAny<JsonObject>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResult<JsonObject>(true, HttpStatusCode.Created, new JsonObject(), null));

        // Act
        var result = await _sut.SubmitAsync();

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(ViewState.Success, _sut.State);
    }
}
```

## Regras

- **Nomenclatura**: `MetodoSobTeste_Cenario_ResultadoEsperado`
- **AAA pattern**: Arrange, Act, Assert — com comentarios separando
- **Uma asserção principal por teste** (assercoes relacionadas podem ser agrupadas)
- **Sealed** em todas as classes de teste
- **Sem dependencia entre testes** — cada teste deve ser independente
- Rodar com: `dotnet test tests/{Projeto}` ou `dotnet test --filter "FullyQualifiedName~NomeDaClasse"`
