# Checklist Manual - Fluxo Principal Web (Fase 12)

## Objetivo
Validar manualmente o fluxo completo da camada Web MVVM, do cadastro de cliente ate o acompanhamento da OS.

## Ambiente recomendado
- API: http://localhost:8087
- Web: http://localhost:5111
- Navegador: Edge ou Chrome atualizado

## Checklist de execucao
- [ ] Abrir a tela de Clientes e cadastrar um cliente valido.
- [ ] Confirmar notificacao de sucesso no cadastro de cliente.
- [ ] Abrir a tela de Equipamentos, informar ClienteId e carregar equipamentos.
- [ ] Cadastrar um equipamento para o cliente.
- [ ] Selecionar o equipamento para o fluxo de OS.
- [ ] Abrir a tela de Nova OS e confirmar preenchimento de contexto (ClienteId/EquipamentoId).
- [ ] Criar OS em Rascunho com defeito preenchido.
- [ ] Abrir detalhe da OS criada.
- [ ] Adicionar servico, produto, taxa e desconto no detalhe.
- [ ] Adicionar anotacao no detalhe.
- [ ] Enviar OS para Orcamento.
- [ ] Confirmar bloqueio de edicao apos envio para Orcamento.
- [ ] Aprovar OS, iniciar andamento e concluir.
- [ ] Registrar pagamento cobrindo o saldo restante.
- [ ] Entregar OS.
- [ ] Abrir tela de Lista e validar exibicao da OS.
- [ ] Aplicar filtros por numero, status e cliente e confirmar resultados.
- [ ] Validar comportamento responsivo em viewport mobile (<= 768px).
- [ ] Validar foco visivel ao navegar por teclado (Tab/Shift+Tab).

## Criterio de aprovacao
A fase esta aprovada quando todos os itens acima forem executados sem falhas bloqueantes e com mensagens de erro/sucesso coerentes para o usuario.
