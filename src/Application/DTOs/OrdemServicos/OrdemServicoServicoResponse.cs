namespace Application.DTOs.OrdemServicos;

public record OrdemServicoServicoResponse(Guid Id, string Descricao, int Quantidade, decimal ValorUnitario, decimal Subtotal);
