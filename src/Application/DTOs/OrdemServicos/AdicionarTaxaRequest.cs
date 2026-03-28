namespace Application.DTOs.OrdemServicos;

public record AdicionarTaxaRequest(
    string Descricao,
    decimal Valor
);
