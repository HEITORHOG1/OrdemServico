# Blazor Web (MVVM) - Fluxo de Cadastro ate Envio

## Objetivo
Este documento define a arquitetura da camada Web em Blazor usando MVVM, consumindo a API de Ordem de Servico ja existente.

Escopo principal: fluxo funcional do sistema de cadastro ate o envio de orcamento/OS.

Dependencia UI obrigatoria:

```xml
<PackageReference Include="Radzen.Blazor" Version="9.0.7" />
```

Fonte de contrato da API (Swagger):
- http://localhost:8080/swagger/v1/swagger.json

## Diretrizes de Arquitetura (MVVM)

## Estrutura sugerida
```text
src/Web/
  Components/
    Shared/
  Pages/
    Clientes/
    Equipamentos/
    OrdensServico/
  ViewModels/
    Clientes/
    Equipamentos/
    OrdensServico/
    Shared/
  Services/
    Api/
      IApiClient.cs
      ApiClient.cs
      ClientesApi.cs
      EquipamentosApi.cs
      OrdensServicoApi.cs
  Models/
    Requests/
    Responses/
  State/
  Validators/
```

## Responsabilidades
- View (Pages/Components): renderizacao, eventos de UI e data binding com o ViewModel.
- ViewModel: estado da tela, comandos (acoes), regras de orquestracao de fluxo e tratamento de erro para exibicao.
- Model: DTOs de request/response vindos da API e modelos de apoio da UI.
- Services/Api: encapsulam HttpClient e endpoints.

## Regras MVVM para este projeto
- A View nunca chama HttpClient diretamente.
- Todo submit passa por metodo do ViewModel.
- Toda navegacao relevante acontece via ViewModel (ou coordenada por ele).
- ViewModel expoe propriedades de estado:
  - IsLoading
  - IsSubmitting
  - ErrorMessage
  - SuccessMessage
  - ValidationErrors

## Configuracao Base no Blazor

## Program.cs (web)
- Registrar Radzen:
  - builder.Services.AddRadzenComponents();
- Registrar HttpClient com BaseAddress da API:
  - http://localhost:8080
- Registrar services de API e ViewModels.

## Layout UI com Radzen
- Grid/Listagem: RadzenDataGrid
- Formularios: RadzenTemplateForm + RadzenTextBox + RadzenDropDown + RadzenDatePicker
- Feedback: RadzenNotificationService + RadzenDialogService
- Acao principal: RadzenButton

## Endpoints Utilizados (API OS)

## Clientes
- POST /api/clientes
- GET /api/clientes/{id}

## Equipamentos
- POST /api/equipamentos
- GET /api/equipamentos/cliente/{clienteId}

## Ordens de Servico
- POST /api/ordens-servico
- GET /api/ordens-servico/{id}
- GET /api/ordens-servico?page={Page}&pageSize={PageSize}
- PUT /api/ordens-servico/{id}
- POST /api/ordens-servico/{id}/servicos
- POST /api/ordens-servico/{id}/produtos
- POST /api/ordens-servico/{id}/desconto
- POST /api/ordens-servico/{id}/taxas
- POST /api/ordens-servico/{id}/pagamentos
- POST /api/ordens-servico/{id}/anotacoes
- PATCH /api/ordens-servico/{id}/status

## Fluxo do Sistema: Cadastro ate Envio

## Fase 1 - Cadastro de Cliente
1. Usuario abre tela Clientes > Novo.
2. Preenche dados (nome obrigatorio; email valido quando informado).
3. View chama ClienteCadastroViewModel.SubmitAsync().
4. ViewModel chama ClientesApi.CriarAsync() -> POST /api/clientes.
5. Em sucesso:
   - exibe notificacao
   - guarda ClienteId
   - oferece proximo passo: cadastrar equipamento

Saida da fase: ClienteId criado.

## Fase 2 - Cadastro de Equipamento (opcional, mas recomendado)
1. Usuario abre Equipamentos > Novo (contexto do cliente).
2. Preenche tipo/marca/modelo/numero serie.
3. ViewModel chama EquipamentosApi.CriarAsync() -> POST /api/equipamentos.
4. Em sucesso:
   - atualiza lista de equipamentos do cliente
   - seleciona equipamento para vincular na OS

Saida da fase: EquipamentoId (opcional na criacao de OS).

## Fase 3 - Cadastro da OS (rascunho)
1. Usuario abre Ordens de Servico > Nova.
2. Informa:
   - ClienteId (obrigatorio)
   - EquipamentoId (opcional)
   - Defeito (obrigatorio)
   - demais campos opcionais (duracao, observacoes, referencia, validade, prazo)
3. ViewModel chama OrdensServicoApi.CriarAsync() -> POST /api/ordens-servico.
4. API retorna OS com Id, Numero e Status inicial (Rascunho).

Saida da fase: OrdemServicoId + NumeroOS.

## Fase 4 - Composicao do Orcamento/Servico
Com OrdemServicoId em maos, a tela de detalhe da OS habilita secoes:

1. Servicos:
   - POST /api/ordens-servico/{id}/servicos
2. Produtos:
   - POST /api/ordens-servico/{id}/produtos
3. Taxas:
   - POST /api/ordens-servico/{id}/taxas
4. Desconto:
   - POST /api/ordens-servico/{id}/desconto
5. Anotacoes internas:
   - POST /api/ordens-servico/{id}/anotacoes

A cada operacao, recarregar detalhe com:
- GET /api/ordens-servico/{id}

Objetivo da fase: fechar total e condicoes da OS antes do envio.

## Fase 5 - Envio da OS/Orcamento
Neste contexto, envio significa marcar que a OS saiu de rascunho para orcamento enviado ao cliente.

1. Usuario aciona Enviar Orcamento.
2. ViewModel executa:
   - PATCH /api/ordens-servico/{id}/status
   - body: { "novoStatus": 1 }  // Orcamento
3. Em sucesso:
   - bloquear campos que nao devem mais ser editados livremente
   - registrar feedback visual: status atualizado para Orcamento

Observacao:
- Se houver canal externo (email/WhatsApp), ele pode ser acoplado depois via servico de notificacao.
- O marco oficial de envio no dominio atual e a transicao de status para Orcamento.

## Fase 6 - Pos-envio (continuidade natural)
- Aprovacao/Rejeicao:
  - PATCH status para Aprovada (2) ou Rejeitada (3)
- Execucao:
  - EmAndamento (4) -> AguardandoPeca (5) -> Concluida (6)
- Fechamento:
  - Lancar pagamentos: POST /pagamentos
  - Entregar: PATCH status para Entregue (7)

## Mapa de Telas (Blazor)

## 1) ClientesPage.razor
- Lista + botao Novo Cliente
- Acoes: criar, consultar por id
- ViewModel: ClientesListViewModel

## 2) EquipamentosPage.razor
- Lista por cliente + cadastro de equipamento
- ViewModel: EquipamentosViewModel

## 3) OrdemServicoNovaPage.razor
- Form de criacao da OS
- ViewModel: OrdemServicoCadastroViewModel

## 4) OrdemServicoDetalhePage.razor
- Cabecalho (numero, status, cliente)
- Abas Radzen:
  - Dados basicos
  - Servicos
  - Produtos
  - Taxas
  - Desconto
  - Pagamentos
  - Anotacoes
- Acao principal: Enviar Orcamento
- ViewModel: OrdemServicoDetalheViewModel

## 5) OrdemServicoListaPage.razor
- Grid paginado com filtros basicos
- GET /api/ordens-servico
- ViewModel: OrdemServicoListaViewModel

## Contratos Recomendados de Service

## IClientesApi
- Task<ClienteResponse> CriarAsync(CriarClienteRequest request)
- Task<ClienteResponse?> ObterPorIdAsync(Guid id)

## IEquipamentosApi
- Task<EquipamentoResponse> CriarAsync(CriarEquipamentoRequest request)
- Task<IReadOnlyList<EquipamentoResponse>> ListarPorClienteAsync(Guid clienteId)

## IOrdensServicoApi
- Task<OrdemServicoResponse> CriarAsync(CriarOrdemServicoRequest request)
- Task<OrdemServicoResponse?> ObterPorIdAsync(Guid id)
- Task<PagedResponse<OrdemServicoResumoResponse>> ListarAsync(int page, int pageSize)
- Task AtualizarBaseAsync(Guid id, AtualizarOrdemServicoRequest request)
- Task AdicionarServicoAsync(Guid id, AdicionarServicoRequest request)
- Task AdicionarProdutoAsync(Guid id, AdicionarProdutoRequest request)
- Task AplicarDescontoAsync(Guid id, AplicarDescontoRequest request)
- Task AdicionarTaxaAsync(Guid id, AdicionarTaxaRequest request)
- Task RegistrarPagamentoAsync(Guid id, RegistrarPagamentoRequest request)
- Task AdicionarAnotacaoAsync(Guid id, AdicionarAnotacaoRequest request)
- Task AlterarStatusAsync(Guid id, AlterarStatusRequest request)

## Validacoes e Tratamento de Erros
- Erro 400 (HttpValidationProblemDetails): mostrar erros por campo no formulario.
- Erro 422 (ProblemDetails): mostrar mensagem de regra de negocio (toast + bloco de alerta).
- Erro 404: exibir tela de recurso nao encontrado.
- Em toda chamada:
  - setar IsSubmitting/IsLoading
  - usar try/catch centralizado no ViewModel

## Fluxo Tecnico (resumo sequencial)
1. Criar cliente.
2. Criar equipamento (opcional).
3. Criar OS em Rascunho.
4. Compor servicos/produtos/taxas/desconto.
5. Consultar detalhe e validar total.
6. Enviar orcamento via alteracao de status para Orcamento.

## Criterios de Pronto da camada Web
- Fluxo cadastro -> envio funcional sem chamadas diretas de HttpClient na View.
- Todas as operacoes com feedback de sucesso/erro.
- Estado de carregamento e submit em todas as telas.
- Listagem de OS paginada funcionando.
- Radzen aplicado em formularios, grids e notificacoes.
