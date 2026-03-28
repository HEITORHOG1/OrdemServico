---
name: corrigir-bug
description: Diagnostica e corrige bugs no projeto seguindo a abordagem de um Senior .NET Developer - analisa root cause, corrige e adiciona teste de regressao.
disable-model-invocation: true
user-invocable: true
argument-hint: "[descricao-do-bug]"
---

# Corrigir Bug — $ARGUMENTS

Diagnostique e corrija o bug descrito: **$ARGUMENTS**

## Processo de Investigacao

### 1. Entender o Problema
- Identificar a camada afetada (Domain, Application, Infrastructure, Api, Web)
- Reproduzir mentalmente o fluxo que causa o bug
- Verificar se ha logs de erro relevantes

### 2. Localizar a Causa Raiz
- Rastrear o fluxo da requisicao: Endpoint -> Service -> Entity -> Repository
- Verificar se e um problema de:
  - **Regra de negocio** (Domain): guard clause faltando, transicao invalida, calculo errado
  - **Orquestracao** (Application): ordem de operacoes, commit faltando, mapping errado
  - **Persistencia** (Infrastructure): Include faltando, configuracao EF errada, tracking issue
  - **API** (Api): rota errada, validacao faltando, status code incorreto
  - **UI** (Web): ViewModel state errado, deserializacao falha, binding incorreto

### 3. Corrigir
- Aplicar a correcao minima necessaria — nao refatorar codigo adjacente
- Manter todos os padroes do projeto (sealed, naming, etc.)
- Garantir que `dotnet build OrdemServico.sln` compila sem warnings

### 4. Teste de Regressao
- Criar teste unitario que falha sem a correcao e passa com ela
- Seguir o padrao da camada correspondente:
  - Domain: teste puro sem mock
  - Application: mock com Moq
  - Web: mock de API services
- Nomenclatura: `MetodoAfetado_CenarioDoBug_DeveComportarCorretamente`

### 5. Verificacao Final
- `dotnet build OrdemServico.sln` — sem warnings
- `dotnet test OrdemServico.sln` — todos os testes passam
- Verificar se a correcao nao introduziu regressao em outros fluxos

## Tipos Comuns de Bugs neste Projeto

| Sintoma | Causa Provavel |
|---------|---------------|
| 422 Unprocessable Entity | DomainException - regra de negocio violada |
| 409 Conflict | ConcurrencyConflictException - objeto alterado por outro processo |
| NullReferenceException | Include faltando no repository, ou nullable nao tratado |
| Valor financeiro errado | Faltou `Math.Round(value, 2)` ou calculo de desconto incorreto |
| Status nao muda | Transicao invalida no workflow (verificar `EfetuarTransicaoWorkflow`) |
| Filho nao salva | `MarkIfDetachedAsAdded` faltando ou backing field nao configurado |
| ViewModel nao atualiza | `SetProperty()` nao chamado ou `PropertyChanged` nao disparado |
