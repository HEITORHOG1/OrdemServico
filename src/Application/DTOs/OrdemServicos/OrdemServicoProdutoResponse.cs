namespace Application.DTOs.OrdemServicos;

public record OrdemServicoProdutoResponse(Guid Id, string Descricao, int Quantidade, decimal ValorUnitario, decimal Subtotal);
