using Domain.Enums;

namespace Application.DTOs.OrdemServicos;

public record AplicarDescontoRequest(
    TipoDesconto Tipo,
    decimal Valor
);
