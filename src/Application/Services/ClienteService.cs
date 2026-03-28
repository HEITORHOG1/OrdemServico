using Application.DTOs.Clientes;
using Application.DTOs.Clientes.Mappings;
using Application.Interfaces;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Interfaces;

namespace Application.Services;

public sealed class ClienteService : IClienteService
{
    private readonly IClienteRepository _clienteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ClienteService(IClienteRepository clienteRepository, IUnitOfWork unitOfWork)
    {
        _clienteRepository = clienteRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ClienteResponse> CriarAsync(CriarClienteRequest request, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(request.Documento))
        {
            var existe = await _clienteRepository.ExistePorDocumentoAsync(request.Documento, cancellationToken);
            if (existe) throw new DomainException("Já existe um cliente com este documento.");
        }

        var cliente = Cliente.Criar(request.Nome, request.Documento, request.Telefone, request.Email, request.Endereco);
        
        await _clienteRepository.AdicionarAsync(cliente, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return cliente.ToResponse();
    }

    public async Task<ClienteResponse?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cliente = await _clienteRepository.ObterPorIdAsync(id, cancellationToken);
        return cliente?.ToResponse();
    }

    public async Task<IEnumerable<ClienteResponse>> BuscarPorNomeAsync(string nome, CancellationToken cancellationToken = default)
    {
        var clientes = await _clienteRepository.BuscarPorNomeAsync(nome, cancellationToken);
        return clientes.Select(c => c.ToResponse());
    }
}
