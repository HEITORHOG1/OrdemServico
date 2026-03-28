# ✅ Tarefas do Projeto Web Blazor (MVVM) - Ordem de Servico

> Objetivo: acompanhar a implementacao completa da camada Web Blazor consumindo a API existente.
> UI library obrigatoria: `Radzen.Blazor` v9.0.7.
> Contrato de API: http://localhost:8080/swagger/v1/swagger.json

---

## Fase 1 - Foundation do Projeto Web

- [x] **W-001** Criar projeto Blazor Web App em `src/Web`
- [x] **W-002** Adicionar referencia `Radzen.Blazor` (9.0.7)
- [x] **W-003** Configurar `HttpClient` com BaseAddress `http://localhost:8080`
- [x] **W-004** Configurar servicos Radzen (`AddRadzenComponents`)
- [x] **W-005** Definir layout base (menu, header, area de notificacoes)
- [x] **W-006** Criar convencao de pastas MVVM (`Pages`, `ViewModels`, `Services`, `Models`, `State`)
- [x] **W-007** Criar classes base (`ViewModelBase`, resultado padrao de operacao, estado de carregamento)

### Gate - Fase 1
- [x] **W-008** Build local do projeto Web sem erros
- [x] **W-009** App Web sobe local e consegue chamar endpoint health/swagger da API

---

## Fase 2 - Camada de Integracao com API

- [x] **W-010** Criar `IApiClient` + `ApiClient` com metodos GET/POST/PUT/PATCH padronizados
- [x] **W-011** Criar `IClientesApi` e implementacao (`POST /api/clientes`, `GET /api/clientes/{id}`)
- [x] **W-012** Criar `IEquipamentosApi` e implementacao (`POST /api/equipamentos`, `GET /api/equipamentos/cliente/{clienteId}`)
- [x] **W-013** Criar `IOrdensServicoApi` e implementacao de todos os endpoints de OS
- [x] **W-014** Criar parser padrao de erros (`400` validation, `422` problem details, `404`)
- [x] **W-015** Adicionar politicas de resiliencia (timeout/retry basico para chamadas)
- [x] **W-016** Registrar todos os services de API no DI

### Gate - Fase 2
- [x] **W-017** Teste manual: chamadas de cliente/equipamento/OS funcionando via services

---

## Fase 3 - Models e Contratos da Web

- [x] **W-018** Criar `Models/Requests` espelhando requests da API
- [x] **W-019** Criar `Models/Responses` espelhando responses da API
- [x] **W-020** Criar modelos de apoio de UI (filtros, paginacao, itens de dropdown)
- [x] **W-021** Criar mapeamentos simples para converter resposta API em estado de tela

### Gate - Fase 3
- [x] **W-022** Garantir compatibilidade de serializacao com Swagger atual

---

## Fase 4 - Fluxo Cliente (Cadastro)

- [x] **W-023** Criar `ClientesPage.razor` (listagem + acao novo)
- [x] **W-024** Criar `ClienteCadastroViewModel` com estado e submit
- [x] **W-025** Criar formulario Radzen para cliente (nome, documento, telefone, email, endereco)
- [x] **W-026** Exibir mensagens de validacao por campo
- [x] **W-027** Exibir feedback de sucesso/erro com notificacoes
- [x] **W-028** Persistir `ClienteId` criado para proximo passo do fluxo

### Gate - Fase 4
- [x] **W-029** Fluxo completo de cadastro de cliente funcionando

---

## Fase 5 - Fluxo Equipamento (Cadastro por Cliente)

- [x] **W-030** Criar `EquipamentosPage.razor`
- [x] **W-031** Criar `EquipamentosViewModel` (carregar por cliente + criar)
- [x] **W-032** Implementar listagem de equipamentos do cliente selecionado
- [x] **W-033** Implementar formulario de novo equipamento
- [x] **W-034** Permitir selecionar `EquipamentoId` para uso na OS

### Gate - Fase 5
- [x] **W-035** Fluxo cliente -> equipamento funcionando sem recarregar pagina

---

## Fase 6 - Fluxo OS (Criacao em Rascunho)

- [x] **W-036** Criar `OrdemServicoNovaPage.razor`
- [x] **W-037** Criar `OrdemServicoCadastroViewModel`
- [x] **W-038** Implementar formulario de criacao de OS (`ClienteId`, `EquipamentoId`, `Defeito`, demais campos)
- [x] **W-039** Ao criar OS, redirecionar para detalhe da OS com `Id`
- [x] **W-040** Exibir `Numero` e `Status` retornados pela API

### Gate - Fase 6
- [x] **W-041** Criacao de OS em rascunho validada ponta a ponta

---

## Fase 7 - Fluxo OS (Composicao de Orcamento)

- [x] **W-042** Criar `OrdemServicoDetalhePage.razor` com abas Radzen
- [x] **W-043** Criar `OrdemServicoDetalheViewModel`
- [x] **W-044** Implementar aba Servicos (`POST /{id}/servicos`)
- [x] **W-045** Implementar aba Produtos (`POST /{id}/produtos`)
- [x] **W-046** Implementar aba Taxas (`POST /{id}/taxas`)
- [x] **W-047** Implementar aba Desconto (`POST /{id}/desconto`)
- [x] **W-048** Implementar aba Anotacoes (`POST /{id}/anotacoes`)
- [x] **W-049** Recarregar detalhe apos cada operacao (`GET /{id}`)
- [x] **W-050** Exibir resumo financeiro (subtotal, desconto, total)

### Gate - Fase 7
- [x] **W-051** Composicao de orcamento completa e consistente no detalhe da OS

---

## Fase 8 - Fluxo de Envio da OS/Orcamento

- [x] **W-052** Implementar acao `Enviar Orcamento`
- [x] **W-053** Integrar `PATCH /api/ordens-servico/{id}/status` com `novoStatus = Orcamento`
- [x] **W-054** Bloquear campos/acoes que nao devem ser editados apos envio
- [x] **W-055** Exibir historico visual do status atual

### Gate - Fase 8
- [x] **W-056** Fluxo cadastro -> envio concluido com feedback para usuario

---

## Fase 9 - Listagem, Busca e Acompanhamento de OS

- [x] **W-057** Criar `OrdemServicoListaPage.razor`
- [x] **W-058** Criar `OrdemServicoListaViewModel`
- [x] **W-059** Integrar paginacao (`GET /api/ordens-servico?page=&pageSize=`)
- [x] **W-060** Implementar filtros basicos (numero/status/cliente)
- [x] **W-061** Adicionar atalhos para abrir detalhe da OS

### Gate - Fase 9
- [x] **W-062** Lista paginada funcional e navegacao para detalhe ok

---

## Fase 10 - Pagamentos e Fechamento de OS

- [x] **W-063** Implementar aba Pagamentos (`POST /{id}/pagamentos`)
- [x] **W-064** Exibir saldo/restante com base no total da OS
- [x] **W-065** Implementar transicoes de status de pos-envio (Aprovada, Rejeitada, EmAndamento, Concluida, Entregue)
- [x] **W-066** Criar travas visuais conforme regras de negocio do status

### Gate - Fase 10
- [x] **W-067** Fluxo de fechamento com pagamento e entrega validado

---

## Fase 11 - Qualidade, UX e Observabilidade da Web

- [x] **W-068** Padronizar notificacoes de sucesso/erro em toda a aplicacao
- [x] **W-069** Tratar loading e submit em todas as telas e comandos
- [x] **W-070** Implementar tratamento de falhas de rede com mensagem amigavel
- [x] **W-071** Validar responsividade (desktop/tablet/mobile)
- [x] **W-072** Revisao de acessibilidade basica (labels, contraste, foco)

### Gate - Fase 11
- [x] **W-073** Smoke test funcional de UX concluido

---

## Fase 12 - Testes e Entrega

- [x] **W-074** Criar testes de ViewModel (unitarios)
- [x] **W-075** Criar testes de services de API com mocks
- [x] **W-076** Criar checklist de teste manual do fluxo completo (cadastro ate envio)
- [x] **W-077** Executar regressao do fluxo principal
- [x] **W-078** Atualizar documentacao final da camada Web

### Gate - Fase 12
- [x] **W-079** Publicacao interna aprovada para uso

---

## Fase 13 - Consideracoes API + Front (Criticas)

- [x] **W-080** Alinhar contrato da API com Swagger (status codes e schemas reais)
- [x] **W-081** Configurar CORS e BaseAddress por ambiente (dev/hml/prod)
- [x] **W-082** Padronizar tratamento de erro no front (`400`, `422`, `404`, `500`)
- [x] **W-083** Implementar bloqueios de acoes na UI conforme status da OS
- [x] **W-084** Definir estrategia de concorrencia de edicao (ex.: versao/etag)
- [x] **W-085** Garantir idempotencia em acoes criticas (status e pagamentos)
- [x] **W-086** Otimizar listagem no front (paginacao, filtros, debounce, loading)
- [x] **W-087** Propagar `CorrelationId` API <-> Front para rastreabilidade
- [x] **W-088** Definir autenticacao/autorizacao para consumo dos endpoints
- [x] **W-089** Criar testes integrados API + Front para o fluxo principal

---

## Checklist Macro do Fluxo Principal

- [x] Cliente cadastrado
- [x] Equipamento cadastrado e vinculado
- [x] OS criada em rascunho
- [x] Servicos/produtos/taxas/desconto aplicados
- [x] OS enviada como Orcamento
- [x] Lista de OS com acompanhamento ativo

---

## Controle de Execucao

- Data de inicio:
- Responsavel:
- Ultima atualizacao:
- Observacoes de bloqueio:
