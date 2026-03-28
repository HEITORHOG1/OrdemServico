using Application.DTOs.Clientes;
using Application.DTOs.OrdemServicos;
using Bogus;
using Domain.Enums;

namespace Api.IntegrationTests.TestData;

public static class OrdemServicoTestData
{
    private static readonly Faker Faker = new("pt_BR");

    public static CriarClienteRequest NovoCliente()
    {
        return new CriarClienteRequest(
            Nome: Faker.Person.FullName,
            Documento: Faker.Random.ReplaceNumbers("###########"),
            Telefone: Faker.Phone.PhoneNumber("11#########"),
            Email: Faker.Internet.Email(),
            Endereco: Faker.Address.FullAddress());
    }

    public static CriarOrdemServicoRequest NovaOrdemServico(Guid clienteId, Guid? equipamentoId = null)
    {
        return new CriarOrdemServicoRequest(
            ClienteId: clienteId,
            EquipamentoId: equipamentoId,
            Defeito: Faker.Lorem.Sentence(3),
            Duracao: "2h",
            Observacoes: Faker.Lorem.Sentence(6),
            Referencia: $"IT-{Faker.Random.AlphaNumeric(8).ToUpperInvariant()}",
            ValidadeOrcamento: DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(7)),
            PrazoEntrega: DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(5)));
    }

    public static AdicionarServicoRequest NovoServico()
    {
        return new AdicionarServicoRequest(
            Descricao: Faker.Commerce.ProductName(),
            Quantidade: Faker.Random.Int(1, 3),
            ValorUnitario: Faker.Random.Decimal(50, 200));
    }

    public static AdicionarProdutoRequest NovoProduto()
    {
        return new AdicionarProdutoRequest(
            Descricao: Faker.Commerce.Product(),
            Quantidade: Faker.Random.Int(1, 2),
            ValorUnitario: Faker.Random.Decimal(20, 150));
    }

    public static AplicarDescontoRequest NovoDesconto()
    {
        return new AplicarDescontoRequest(
            Tipo: TipoDesconto.Percentual,
            Valor: 10m);
    }
}
