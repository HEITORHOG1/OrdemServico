namespace Application.DTOs.OrdemServicos;

public record AdicionarProdutoRequest(
    string Descricao,
    int Quantidade,
    decimal ValorUnitario
);
