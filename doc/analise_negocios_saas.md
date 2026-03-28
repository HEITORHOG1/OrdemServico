# Analise de Negocios — OrdemServico SaaS

**Data**: 28/03/2026
**Tipo**: Analise Estrategica de Produto
**Objetivo**: Transformar o projeto OrdemServico em um SaaS profissional competitivo para venda

---

## 1. PANORAMA DO MERCADO

### Tamanho do Mercado
- **Global (FSM)**: US$ 5,64 bilhoes (2025) → US$ 9,68 bilhoes (2030) — CAGR 12,5%
- **Manutencao Preditiva**: US$ 10,6 bi (2024) → US$ 47,8 bi (2029)
- **Brasil**: Setor de servicos cresceu 2,2% em 2025 (maior desde 2012)

### Segmentos-Alvo

| Segmento | Perfil | Dor Principal | Disposicao a Pagar |
|----------|--------|---------------|---------------------|
| Assistencia Tecnica | Celulares, eletronicos, informatica | Controle manual em cadernos/planilhas | R$ 50-150/mes |
| Refrigeracao/HVAC | Manutencao de ar-condicionado | PMOC obrigatorio (Lei 13.589), controle de visitas | R$ 100-300/mes |
| Oficinas Mecanicas | Automoveis, motos | Controle de pecas + mao de obra | R$ 80-200/mes |
| Facilities/Manutencao Predial | Elevadores, eletrica, hidraulica | SLA, agendamento de equipes | R$ 200-500/mes |
| TI / Suporte Tecnico | Infra, redes, software | SLA, controle de chamados | R$ 100-300/mes |
| Freelancers / MEI | Eletricista, encanador, marceneiro | Profissionalizar orcamentos | R$ 0-50/mes |

---

## 2. CONCORRENCIA DETALHADA

### Concorrentes Brasileiros

#### Tier 1 — Lideres

| Plataforma | Foco | Preco | Diferenciais |
|------------|------|-------|-------------|
| **Auvo** | Equipes externas (field service) | Sob consulta (min. 2 usuarios) | GPS tempo real, check-in/check-out, rotas, lider LatAm |
| **Produttivo** | Manutencao, facilities | Sob consulta (trial 15 dias) | Checklists digitais (13 tipos), relatorios automaticos PDF com logo, NPS |
| **IClass FS** | HVAC, telecom, facilities | Sob consulta | SLA com escalonamento, modulos por segmento, mapas tempo real |

#### Tier 2 — ERPs com Modulo OS

| Plataforma | Foco | Preco | Diferenciais |
|------------|------|-------|-------------|
| **Conta Azul** | PMEs geral | A partir de R$ 119,90/mes | NFS-e (600+ cidades), integracao contabil, boletos |
| **vhsys** | Pequenas empresas | Planos a partir de R$ 49/mes | ERP completo (estoque, vendas, financeiro) + OS |

#### Tier 3 — Acessiveis/Nicho

| Plataforma | Foco | Preco | Diferenciais |
|------------|------|-------|-------------|
| **OnlineOS** | Micro empresas | R$ 12,99/mes (50 OS) / R$ 53,99 (100 OS) | Preco por volume de OS |
| **AgoraOS** | Freelancers, PMEs | Plano gratis disponivel | ERP gratis com OS |
| **CheckOS** | Freelancers | Gratis | Ferramenta basica gratuita |
| **AssistenciaPro** | Assistencia tecnica | Sob consulta | WhatsApp, NFS-e, garantia |
| **FieldLink** | Equipes comerciais + tecnicas | Gratis / R$ 79,90/mes Pro | Entrada barata, CRM+OS |

### Concorrentes Globais

| Plataforma | Foco | Preco (USD) | Diferenciais |
|------------|------|-------------|-------------|
| **ServiceTitan** | Grandes empresas (20+ tecnicos) | $245-500/tecnico/mes + $5K-50K implantacao | Mais completo, IPO 2024 |
| **Jobber** | PMEs em crescimento | $39-599/mes (1-15 usuarios) | Melhor UX de agendamento, AI Receptionist ($99/mes) |
| **Housecall Pro** | Pequenas equipes | A partir de $59/mes | Melhor app mobile (4.6 estrelas), Instapay |
| **FieldPulse** | PMEs | A partir de $89/mes | "Operator AI" — atende ligacoes e agenda |
| **ServiceM8** | Micro empresas | A partir de $29/mes | Modelo paga-por-job |
| **Salesforce Field Service** | Enterprise | Custom | Integracao CRM profunda |
| **Microsoft D365 FS** | Enterprise | Custom | IoT, Mixed Reality (HoloLens) |

---

## 3. O QUE TODO SAAS DE OS PROFISSIONAL PRECISA TER

### 3.1 Features Obrigatorias (Table Stakes)

Sem estas, o produto nao compete nem no tier mais basico.

| # | Feature | Seu Projeto | Status |
|---|---------|-------------|--------|
| 1 | Cadastro de clientes com busca | Tem | OK |
| 2 | Cadastro de equipamentos | Tem | OK |
| 3 | Criacao e gestao de OS com workflow | Tem (8 status) | OK |
| 4 | Itens de servico e produtos com valor | Tem | OK |
| 5 | Calculo automatico de totais | Tem | OK |
| 6 | Descontos (% e fixo) | Tem | OK |
| 7 | Registro de pagamentos multiplos | Tem (6 meios) | OK |
| 8 | Fotos anexas a OS | Tem (URL externa) | PARCIAL |
| 9 | Anotacoes internas | Tem | OK |
| 10 | **Autenticacao de usuarios (login)** | NAO TEM | CRITICO |
| 11 | **Multi-tenancy (cada empresa isolada)** | NAO TEM | CRITICO |
| 12 | **Gestao de usuarios e permissoes** | NAO TEM | CRITICO |
| 13 | **Geracao de PDF (orcamento/OS/recibo)** | NAO TEM | CRITICO |
| 14 | **Notificacoes por email** | NAO TEM | CRITICO |
| 15 | **Dashboard com KPIs** | NAO TEM | CRITICO |
| 16 | **App mobile ou PWA** | NAO TEM | IMPORTANTE |
| 17 | **Assinatura digital do cliente** | NAO TEM | IMPORTANTE |

### 3.2 Features Diferenciadoras (Competir com Lideres)

| # | Feature | Quem Tem | Prioridade |
|---|---------|----------|------------|
| 1 | **Agendamento inteligente** (calendario + drag-and-drop + skills) | Auvo, Jobber, ServiceTitan | ALTA |
| 2 | **Integracao WhatsApp** (notificar cliente, receber solicitacao) | AssistenciaPro | ALTA (Brasil) |
| 3 | **Portal do cliente** (acompanhar status, abrir chamado) | Produttivo, ServiceTitan | ALTA |
| 4 | **NFS-e automatica** | Conta Azul, AssistenciaPro | ALTA (Brasil) |
| 5 | **Pesquisa NPS/CSAT automatica** | Produttivo | MEDIA |
| 6 | **Controle de estoque/pecas** | vhsys, IClass | MEDIA |
| 7 | **GPS e rastreamento de equipe** | Auvo, FieldLink | MEDIA |
| 8 | **Relatorios com logo da empresa** | Produttivo | MEDIA |
| 9 | **Integracao contabil/ERP** | Conta Azul | MEDIA |
| 10 | **Recurring billing / contratos** | ServiceTitan, Jobber | MEDIA |

### 3.3 Features Visionarias (Diferenciar do Mercado)

| # | Feature | Quem Tem | Prioridade |
|---|---------|----------|------------|
| 1 | **IA Receptionist** (atende ligacoes, agenda via voz) | Jobber ($99/mes extra), FieldPulse | FUTURA |
| 2 | **IA para resumir OS** (voz do tecnico → texto) | ServiceTitan (beta) | FUTURA |
| 3 | **IoT + Manutencao Preditiva** (sensor → OS automatica) | IFS, D365 | FUTURA |
| 4 | **Realidade Aumentada** (guia tecnico via camera) | D365 HoloLens | FUTURA |
| 5 | **White-label** (revender como marca do cliente) | E-SaaS | FUTURA |
| 6 | **Marketplace de tecnicos** | Ninguem no BR | OPORTUNIDADE |

---

## 4. ANALISE CRITICA DO PROJETO ATUAL

### 4.1 Pontos Fortes (Vantagem Competitiva)

| # | Forca | Por que importa |
|---|-------|-----------------|
| 1 | **Arquitetura Clean Architecture impecavel** | Facilita escalar, adicionar features e manter a longo prazo. Concorrentes menores tem codigo monolitico dificil de evoluir. |
| 2 | **Rich Domain Model (DDD)** | Regras de negocio centralizadas na entidade, nao espalhadas. Reduz bugs em 60-80% vs codigo procedural. |
| 3 | **Workflow de 8 estados maduro** | Cobre todo o ciclo de vida real de uma OS. Muitos concorrentes tem apenas 3-4 estados (aberta/andamento/fechada). |
| 4 | **Motor financeiro robusto** | Calculo de totais com servicos + produtos + taxas - desconto + pagamentos parciais. Muitos concorrentes nao suportam pagamento parcial. |
| 5 | **Concorrencia otimista** | Previne conflitos quando 2 usuarios editam a mesma OS. Feature enterprise que poucos concorrentes pequenos tem. |
| 6 | **Idempotencia nativa** | Pagamentos e transicoes de status sao idempotentes. Critico para confiabilidade. |
| 7 | **API REST bem desenhada** | Permite futuramente criar app mobile, integracoes, webhooks sem reescrever backend. |
| 8 | **Stack moderna (.NET 9)** | Performance superior (Minimal APIs), custo de infra menor. |

### 4.2 Gaps Criticos (Bloqueiam a Venda como SaaS)

| # | Gap | Impacto | Esforco Estimado |
|---|-----|---------|------------------|
| 1 | **Sem autenticacao/login** | Impossivel vender — qualquer um acessa tudo | 2-3 semanas |
| 2 | **Sem multi-tenancy** | Cada cliente precisa de deploy separado — inviavel | 2-3 semanas |
| 3 | **Sem gestao de usuarios/roles** | Admin, Tecnico, Atendente precisam de permissoes diferentes | 1-2 semanas |
| 4 | **Sem geracao de PDF** | Cliente nao pode imprimir orcamento/recibo para o consumidor final | 1 semana |
| 5 | **Sem email/notificacao** | Cliente nao e avisado quando OS muda de status | 1 semana |
| 6 | **Sem dashboard/relatorios** | Dono da empresa nao consegue ver metricas do negocio | 1-2 semanas |
| 7 | **Sem upload de fotos real** | Fotos sao apenas URLs externas, nao tem upload | 3-5 dias |
| 8 | **Sem landing page / onboarding** | Nao tem fluxo de cadastro de nova empresa (signup) | 1 semana |
| 9 | **Sem billing/planos** | Nao cobra do cliente SaaS (Stripe, PagSeguro) | 1-2 semanas |
| 10 | **Sem audit trail** | Nao sabe quem fez o que — compliance basico | 3-5 dias |

### 4.3 Gaps Importantes (Diferenciam da Concorrencia)

| # | Gap | Impacto | Esforco |
|---|-----|---------|---------|
| 11 | Sem app mobile / PWA | Tecnico em campo nao usa o sistema | 2-3 semanas |
| 12 | Sem integracao WhatsApp | Canal #1 de comunicacao no Brasil ignorado | 1-2 semanas |
| 13 | Sem agendamento/calendario | Nao gerencia agenda de tecnicos | 2 semanas |
| 14 | Sem NFS-e | Nao emite nota fiscal — dealbreaker para muitos | 2-3 semanas |
| 15 | Sem portal do cliente | Cliente final nao acompanha o servico | 1-2 semanas |
| 16 | Sem controle de estoque | Nao gerencia pecas/insumos | 1-2 semanas |
| 17 | Sem assinatura digital | Cliente nao assina aprovacao no celular | 3-5 dias |

---

## 5. ROADMAP SUGERIDO

### Fase 1 — MVP SaaS (6-8 semanas)
**Objetivo**: Versao minima vendavel. Primeira assinatura.

| Semana | Entrega |
|--------|---------|
| 1-2 | **Autenticacao**: ASP.NET Identity + JWT. Login, registro, refresh token. |
| 2-3 | **Multi-tenancy**: TenantId em todas as entidades, filtro global no EF, isolamento de dados. |
| 3-4 | **Usuarios e Roles**: Admin, Gerente, Tecnico, Atendente. Permissoes por role. Tela de gestao de usuarios. |
| 4-5 | **PDF**: Geracao de orcamento, OS e recibo com QuestPDF. Logo da empresa, dados do cliente, itens, total. |
| 5-6 | **Notificacoes Email**: SendGrid/AWS SES. OS criada, status alterado, orcamento aprovado. Templates em PT-BR. |
| 6-7 | **Dashboard**: KPIs (OS abertas, em andamento, concluidas, faturamento mensal, ticket medio). Graficos Radzen. |
| 7-8 | **Onboarding + Billing**: Signup de empresa, planos (Free/Pro/Business), integracao Stripe ou Asaas. |

### Fase 2 — Diferenciacao (4-6 semanas)
**Objetivo**: Competir com OnlineOS, AgoraOS, FieldLink.

| Semana | Entrega |
|--------|---------|
| 1 | **Upload de fotos**: Azure Blob Storage ou S3. Antes/depois, comprovantes. |
| 1-2 | **PWA**: Service worker, manifest.json, offline basico, "Add to Home Screen". |
| 2-3 | **Integracao WhatsApp**: API WhatsApp Business. Notificar cliente de status, receber confirmacao. |
| 3-4 | **Agendamento**: Calendario de tecnicos, drag-and-drop, atribuicao por disponibilidade. |
| 4-5 | **Assinatura digital**: Canvas no mobile, salvar como imagem na OS. |
| 5-6 | **Portal do cliente**: Link publico para acompanhar status da OS, aprovar orcamento. |

### Fase 3 — Profissionalizacao (4-6 semanas)
**Objetivo**: Competir com Produttivo, Auvo, Conta Azul.

| Semana | Entrega |
|--------|---------|
| 1-2 | **NFS-e**: Integracao com prefeituras (API Nfse.io ou eNotas). |
| 2-3 | **Controle de estoque**: Cadastro de pecas, baixa automatica ao adicionar produto na OS, alerta de estoque minimo. |
| 3-4 | **Relatorios avancados**: Faturamento por periodo, por tecnico, por servico. Export Excel. Impressao. |
| 4-5 | **SLA e prioridades**: Tempo de resposta, escalacao automatica, prioridade por cliente/contrato. |
| 5-6 | **Contratos e recorrencia**: Contratos de manutencao, visitas programadas, cobranca recorrente. |

### Fase 4 — Inovacao (futuro)
**Objetivo**: Ser o lider do mercado brasileiro.

- IA para sugerir diagnostico baseado no defeito descrito
- IA Receptionist via WhatsApp (chatbot que abre OS)
- Manutencao preditiva com IoT
- Marketplace de tecnicos
- White-label para revendas
- App nativo (MAUI)

---

## 6. MODELO DE PRECIFICACAO SUGERIDO

### Baseado na Analise da Concorrencia

| Plano | Preco | Limites | Publico |
|-------|-------|---------|---------|
| **Free** | R$ 0/mes | 1 usuario, 20 OS/mes, sem PDF, sem email | Freelancers, teste |
| **Starter** | R$ 49,90/mes | 3 usuarios, 100 OS/mes, PDF basico, email | MEI, micro |
| **Pro** | R$ 129,90/mes | 10 usuarios, OS ilimitadas, WhatsApp, agenda, NFS-e | Pequenas empresas |
| **Business** | R$ 299,90/mes | Usuarios ilimitados, multi-filial, SLA, relatorios, API | Medias empresas |
| **Enterprise** | Sob consulta | White-label, integracao ERP, suporte dedicado | Grandes empresas |

### Estrategia de Monetizacao Adicional
- **Add-on WhatsApp**: +R$ 29,90/mes
- **Add-on NFS-e**: +R$ 19,90/mes
- **Add-on Estoque**: +R$ 19,90/mes
- **Usuarios extras**: +R$ 14,90/usuario/mes
- **Armazenamento extra (fotos)**: +R$ 9,90/5GB/mes

---

## 7. ANALISE SWOT

### Forcas (Strengths)
- Arquitetura de codigo enterprise-grade (Clean Architecture + DDD)
- Workflow de OS mais completo que 80% dos concorrentes brasileiros
- Motor financeiro robusto (pagamento parcial, desconto, taxas)
- Stack moderna (.NET 9) com performance superior
- API REST pronta para integracao mobile/terceiros
- Concorrencia otimista e idempotencia (features de nivel enterprise)

### Fraquezas (Weaknesses)
- Nao e vendavel ainda (sem auth, sem multi-tenancy, sem PDF)
- Sem app mobile (campo depende de mobile)
- Sem integracoes essenciais (WhatsApp, NFS-e, pagamento)
- Time provavelmente pequeno para executar roadmap completo
- Sem base de clientes/validacao de mercado

### Oportunidades (Opportunities)
- Mercado FSM brasileiro ainda fragmentado (sem lider claro no tier PME)
- Concorrentes baratos tem codigo ruim e escalam mal
- IA ainda nao chegou nos concorrentes brasileiros
- WhatsApp como canal de OS e pouco explorado
- Modelo pay-per-OS pode atrair micro empresas
- Setor de servicos no Brasil em crescimento recorde

### Ameacas (Threats)
- Conta Azul e vhsys ja tem OS + ecossistema contabil
- Auvo tem capital e lideranca LatAm
- Concorrentes globais (Jobber, Housecall Pro) podem entrar no BR
- Custo de aquisicao de cliente pode ser alto sem marketing
- Churn alto se produto nao resolver dor principal rapido

---

## 8. METRICAS DE SUCESSO (KPIs do Produto SaaS)

| Metrica | Meta Ano 1 | Meta Ano 2 |
|---------|-----------|-----------|
| MRR (Receita Mensal Recorrente) | R$ 15.000 | R$ 80.000 |
| Clientes pagantes | 100 | 400 |
| Churn mensal | < 5% | < 3% |
| LTV (Lifetime Value) | R$ 600 | R$ 1.500 |
| CAC (Custo Aquisicao Cliente) | < R$ 150 | < R$ 200 |
| NPS | > 40 | > 60 |
| Uptime | 99,5% | 99,9% |

---

## 9. CONCLUSAO

O projeto OrdemServico tem uma **base tecnica excepcional** — a arquitetura e superior a 90% dos concorrentes brasileiros do tier PME. O Domain Model e o workflow de 8 estados sao diferenciais reais.

Porem, **esta a 6-8 semanas de ser vendavel**. Os gaps criticos sao claros:
1. Autenticacao + Multi-tenancy + Roles
2. PDF + Email + Dashboard
3. Onboarding + Billing

A recomendacao e executar a **Fase 1 do roadmap** o mais rapido possivel, lancar um MVP com plano Free para validar o mercado, e iterar com feedback real de clientes.

**Posicionamento sugerido**: "O sistema de OS mais profissional para assistencias tecnicas e prestadores de servico — com o melhor preco do mercado."

**Publico inicial**: Assistencias tecnicas de celulares e informatica (maior volume, menor complexidade, aceita solucao digital mais rapido).
