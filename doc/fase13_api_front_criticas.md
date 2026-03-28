# Fase 13 - Consideracoes Criticas API + Front

## W-080 - Contrato Swagger alinhado
- Endpoints de Clientes, Equipamentos e Ordens de Servico com `Produces`/`ProducesProblem` para status reais.
- Incluidos esquemas de sucesso e erro para 200/201/204 e falhas 400/404/409/422/500 conforme operacao.

## W-081 - CORS e BaseAddress por ambiente
- API:
  - appsettings.Development.json com origens locais.
  - appsettings.Homologation.json e appsettings.Production.json com origens dedicadas.
- Web:
  - appsettings.{Environment}.json com `Api:BaseUrl` especifico por ambiente.

## W-082 - Padrao de erros no front
- Parser central de erros no front com mensagens amigaveis para 400/404/422/500.
- Falhas de transporte e timeout tratadas com mensagens consistentes no ApiClient.

## W-083 - Bloqueio de acoes por status da OS
- ViewModel de detalhe com regras `Pode*` para transicoes e operacoes financeiras.
- UI bloqueia comandos invalidos conforme status corrente e saldo.

## W-084 - Estrategia de concorrencia (otimista)
- Definida concorrencia otimista via `ExpectedUpdatedAt` em operacoes criticas.
- API valida timestamp esperado contra `UpdatedAt` persistido.
- Em caso de divergencia, retorna conflito HTTP 409.

## W-085 - Idempotencia em acoes criticas
- Alteracao de status: repeticao para mesmo status e tratada como no-op.
- Pagamento: repeticao de mesmo meio/valor/data e tratada como no-op.

## W-086 - Otimizacao de listagem no front
- Paginacao e filtros mantidos.
- Debounce de 300ms em filtros para reduzir re-render excessivo durante digitacao.
- Indicadores de loading mantidos na tela de listagem.

## W-087 - CorrelationId API <-> Front
- Front envia `X-Correlation-Id` automaticamente em cada request.
- API devolve `X-Correlation-Id` e registra id no log de requests.
- ProblemDetails inclui correlationId para rastreio de falhas.

## W-088 - Autenticacao/autorizacao de consumo
- Definida protecao por API Key (`X-Api-Key`) via middleware.
- Comportamento por ambiente:
  - Development: opcional (chave vazia por padrao).
  - Homologation/Production: chave definida por configuracao.

## W-089 - Testes integrados API + Front
- Criado teste de integracao exercitando servicos da camada Web (`ApiClient` + APIs Web) contra API real em fixture de testes.
- Fluxo validado: cliente -> equipamento -> OS -> composicao -> mudanca de status.
