using Application.DTOs.Equipamentos;
using Application.Interfaces;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Interfaces;

namespace Application.Services;

public sealed class EquipamentoService : IEquipamentoService
{
    private readonly IEquipamentoRepository _equipamentoRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public EquipamentoService(IEquipamentoRepository equipamentoRepository, IClienteRepository clienteRepository, IUnitOfWork unitOfWork)
    {
        _equipamentoRepository = equipamentoRepository;
        _clienteRepository = clienteRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<EquipamentoResponse> CriarAsync(CriarEquipamentoRequest request, CancellationToken cancellationToken = default)
    {
        var cliente = await _clienteRepository.ObterPorIdAsync(request.ClienteId, cancellationToken);
        if (cliente is null) throw new DomainException("Cliente não encontrado.");

        var eqp = Equipamento.Criar(request.ClienteId, request.Tipo, request.Marca, request.Modelo, request.NumeroSerie);
        
        await _equipamentoRepository.AdicionarAsync(eqp, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return new EquipamentoResponse(eqp.Id, eqp.ClienteId, eqp.Tipo, eqp.Marca, eqp.Modelo, eqp.NumeroSerie, eqp.CreatedAt);
    }

    public async Task<IEnumerable<EquipamentoResponse>> ListarPorClienteIdAsync(Guid clienteId, CancellationToken cancellationToken = default)
    {
        var lista = await _equipamentoRepository.ListarPorClienteIdAsync(clienteId, cancellationToken);
        return lista.Select(eqp => new EquipamentoResponse(eqp.Id, eqp.ClienteId, eqp.Tipo, eqp.Marca, eqp.Modelo, eqp.NumeroSerie, eqp.CreatedAt));
    }
}
