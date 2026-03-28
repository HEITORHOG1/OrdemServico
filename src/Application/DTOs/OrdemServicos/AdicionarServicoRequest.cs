namespace Application.DTOs.OrdemServicos;

public record AdicionarServicoRequest(
    string Descricao,
    int Quantidade,
    decimal ValorUnitario
);
