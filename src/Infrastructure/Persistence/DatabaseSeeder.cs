using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class DatabaseSeeder
{
    private readonly AppDbContext _context;

    public DatabaseSeeder(AppDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await _context.Clientes.AnyAsync(cancellationToken))
        {
            return;
        }

        await ExecutarSeedAsync(cancellationToken);
    }

    public async Task ForceSeedAsync(CancellationToken cancellationToken = default)
    {
        // Limpa tudo na ordem correta (filhas primeiro)
        _context.OrdensServicoAnotacoes.RemoveRange(_context.OrdensServicoAnotacoes);
        _context.OrdensServicoFotos.RemoveRange(_context.OrdensServicoFotos);
        _context.OrdensServicoPagamentos.RemoveRange(_context.OrdensServicoPagamentos);
        _context.OrdensServicoTaxas.RemoveRange(_context.OrdensServicoTaxas);
        _context.OrdensServicoProdutos.RemoveRange(_context.OrdensServicoProdutos);
        _context.OrdensServicoServicos.RemoveRange(_context.OrdensServicoServicos);
        _context.OrdensServico.RemoveRange(_context.OrdensServico);
        _context.Equipamentos.RemoveRange(_context.Equipamentos);
        _context.Clientes.RemoveRange(_context.Clientes);
        await _context.SaveChangesAsync(cancellationToken);

        await ExecutarSeedAsync(cancellationToken);
    }

    private async Task ExecutarSeedAsync(CancellationToken cancellationToken)
    {

        // =============================================
        // CLIENTES
        // =============================================
        var maria = Cliente.Criar("Maria Silva", "123.456.789-00", "(11) 99876-5432", "maria.silva@email.com", "Rua das Flores, 123 - Centro, Sao Paulo/SP");
        var joao = Cliente.Criar("Joao Santos", "987.654.321-00", "(21) 98765-4321", "joao.santos@gmail.com", "Av. Brasil, 456 - Copacabana, Rio de Janeiro/RJ");
        var ana = Cliente.Criar("Ana Costa Oliveira", "456.789.123-00", "(31) 97654-3210", "ana.costa@hotmail.com", "Rua Minas Gerais, 789 - Savassi, Belo Horizonte/MG");
        var carlos = Cliente.Criar("Carlos Ferreira", "321.654.987-00", "(41) 96543-2109", "carlos.ferreira@empresa.com.br", "Rua XV de Novembro, 321 - Centro, Curitiba/PR");
        var lucia = Cliente.Criar("Lucia Rodrigues", "654.321.987-00", "(51) 95432-1098", null, "Av. Ipiranga, 654 - Partenon, Porto Alegre/RS");
        var pedro = Cliente.Criar("Pedro Almeida ME", "12.345.678/0001-90", "(11) 3456-7890", "contato@pedroalmeida.com.br", "Rua Augusta, 1500 - Consolacao, Sao Paulo/SP");

        await _context.Clientes.AddRangeAsync(new[] { maria, joao, ana, carlos, lucia, pedro }, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // =============================================
        // EQUIPAMENTOS
        // =============================================
        var notebookMaria = Equipamento.Criar(maria.Id, "Notebook", "Dell", "Inspiron 15 5000", "SN-DELL-2024-001");
        var celularMaria = Equipamento.Criar(maria.Id, "Celular", "Samsung", "Galaxy S24", "IMEI-SAM-2024-555");

        var notebookJoao = Equipamento.Criar(joao.Id, "Notebook", "Apple", "MacBook Pro 14\"", "SN-APPLE-C02X1234");
        var impressoraJoao = Equipamento.Criar(joao.Id, "Impressora", "HP", "LaserJet Pro M404dn", "SN-HP-LJ-78901");

        var desktopAna = Equipamento.Criar(ana.Id, "Desktop", "Lenovo", "ThinkCentre M90q", "SN-LEN-TC-45678");
        var monitorAna = Equipamento.Criar(ana.Id, "Monitor", "LG", "UltraWide 34WN80C", "SN-LG-MON-11223");

        var notebookCarlos = Equipamento.Criar(carlos.Id, "Notebook", "Asus", "VivoBook S15", "SN-ASUS-VB-99887");

        var celularLucia = Equipamento.Criar(lucia.Id, "Celular", "Apple", "iPhone 15 Pro", "IMEI-APPLE-2024-777");

        var servidorPedro = Equipamento.Criar(pedro.Id, "Servidor", "Dell", "PowerEdge T340", "SN-DELL-PE-44556");
        var notebookPedro = Equipamento.Criar(pedro.Id, "Notebook", "HP", "EliteBook 840 G9", "SN-HP-EB-33445");

        await _context.Equipamentos.AddRangeAsync(new[]
        {
            notebookMaria, celularMaria, notebookJoao, impressoraJoao,
            desktopAna, monitorAna, notebookCarlos, celularLucia,
            servidorPedro, notebookPedro
        }, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // =============================================
        // ORDENS DE SERVICO
        // =============================================

        // --- OS 1: Maria - Notebook Dell - Fluxo COMPLETO (Entregue) ---
        var os1 = OrdemServico.Criar(maria.Id, notebookMaria.Id,
            "Notebook nao liga, sem reacao ao pressionar botao power. Cliente informa que houve queda da mesa ontem.",
            "3 horas", "Notebook com marcas de impacto na lateral esquerda", null, null, null);
        os1.DefinirNumeroOS(NumeroOS.Gerar(DateTime.UtcNow.AddDays(-10), 1));
        os1.AdicionarServico("Diagnostico completo de hardware", 1, 120.00m);
        os1.AdicionarServico("Troca de placa-mae", 1, 350.00m);
        os1.AdicionarServico("Limpeza interna e troca de pasta termica", 1, 80.00m);
        os1.AdicionarProduto("Placa-mae compativel Dell Inspiron 15", 1, 890.00m);
        os1.AdicionarProduto("Pasta termica Arctic MX-4", 1, 25.00m);
        os1.AdicionarTaxa("Taxa de urgencia", 50.00m);
        os1.AplicarDesconto(new Desconto(TipoDesconto.Percentual, 5m));
        os1.AdicionarAnotacao("Cliente autorizou reparo por telefone. Informou que pode retirar a partir de sexta.", "Joao Tecnico");
        os1.AdicionarAnotacao("Placa-mae encomendada. Previsao de chegada: 2 dias uteis.", "Marcos Estoque");
        os1.MarcarComoOrcamento();
        os1.Aprovar();
        os1.IniciarAndamento();
        os1.ConcluirTrabalho();
        var totalOs1 = os1.CalcularTotalReal();
        os1.AdicionarPagamento(MeioPagamento.PIX, totalOs1.Valor, DateTime.UtcNow.AddDays(-2));
        os1.FinalizarEEntregar();

        // --- OS 2: Joao - MacBook - Em Orcamento ---
        var os2 = OrdemServico.Criar(joao.Id, notebookJoao.Id,
            "Tela com manchas escuras na parte inferior. Apareceu gradualmente nos ultimos 2 meses.",
            "5 dias uteis", "Equipamento em bom estado geral, sem danos fisicos visiveis", null,
            DateOnly.FromDateTime(DateTime.Today.AddDays(15)),
            DateOnly.FromDateTime(DateTime.Today.AddDays(20)));
        os2.DefinirNumeroOS(NumeroOS.Gerar(DateTime.UtcNow.AddDays(-5), 1));
        os2.AdicionarServico("Diagnostico de tela LCD/OLED", 1, 150.00m);
        os2.AdicionarServico("Substituicao de display", 1, 500.00m);
        os2.AdicionarProduto("Display Retina 14\" MacBook Pro (compativel)", 1, 2800.00m);
        os2.AdicionarTaxa("Frete de peca importada", 120.00m);
        os2.AdicionarAnotacao("Orcamento enviado por e-mail ao cliente. Aguardando aprovacao.", "Ana Atendente");
        os2.MarcarComoOrcamento();

        // --- OS 3: Ana - Desktop Lenovo - Em Andamento ---
        var os3 = OrdemServico.Criar(ana.Id, desktopAna.Id,
            "Computador reiniciando sozinho a cada 20 minutos. Tela azul com erro WHEA_UNCORRECTABLE_ERROR.",
            "2 dias", "Problema comecou apos atualizacao do Windows", null, null,
            DateOnly.FromDateTime(DateTime.Today.AddDays(3)));
        os3.DefinirNumeroOS(NumeroOS.Gerar(DateTime.UtcNow.AddDays(-3), 1));
        os3.AdicionarServico("Diagnostico de estabilidade e stress test", 1, 100.00m);
        os3.AdicionarServico("Troca de modulo de memoria RAM", 1, 60.00m);
        os3.AdicionarProduto("Memoria DDR4 16GB Kingston Fury", 2, 189.90m);
        os3.AdicionarAnotacao("Teste de stress com Prime95 confirmou falha em memoria slot B. Modulo com defeito fisico.", "Carlos Tecnico");
        os3.MarcarComoOrcamento();
        os3.Aprovar();
        os3.IniciarAndamento();

        // --- OS 4: Carlos - Notebook Asus - Rascunho ---
        var os4 = OrdemServico.Criar(carlos.Id, notebookCarlos.Id,
            "Teclado com teclas falhando: Enter, Espaco e setas nao respondem. Derramou cafe no teclado ha 3 dias.",
            "1 hora", "Liquido visivel sob as teclas afetadas", "Protocolo anterior: 2024-0088", null, null);
        os4.DefinirNumeroOS(NumeroOS.Gerar(DateTime.UtcNow.AddDays(-1), 1));
        os4.AdicionarServico("Limpeza e avaliacao de dano por liquido", 1, 80.00m);

        // --- OS 5: Lucia - iPhone - Concluida (aguardando pagamento/entrega) ---
        var os5 = OrdemServico.Criar(lucia.Id, celularLucia.Id,
            "Bateria nao dura mais que 2 horas. Aparelho esquenta bastante durante uso normal.",
            "40 minutos", "Saude da bateria em 62% segundo diagnostico Apple", null, null, null);
        os5.DefinirNumeroOS(NumeroOS.Gerar(DateTime.UtcNow.AddDays(-4), 1));
        os5.AdicionarServico("Troca de bateria iPhone 15 Pro", 1, 150.00m);
        os5.AdicionarProduto("Bateria original Apple iPhone 15 Pro", 1, 320.00m);
        os5.AplicarDesconto(new Desconto(TipoDesconto.ValorFixo, 20.00m));
        os5.AdicionarAnotacao("Bateria trocada com sucesso. Saude 100%. Aguardando retirada pela cliente.", "Joao Tecnico");
        os5.MarcarComoOrcamento();
        os5.Aprovar();
        os5.IniciarAndamento();
        os5.ConcluirTrabalho();

        // --- OS 6: Pedro (empresa) - Servidor Dell - Aguardando Peca ---
        var os6 = OrdemServico.Criar(pedro.Id, servidorPedro.Id,
            "Servidor com ruido excessivo no fan 2. Alerta de temperatura no iDRAC. Disco RAID 5 com 1 disco em estado Warning.",
            "1 dia", "Equipamento em producao - necessita janela de manutencao", "Contrato SLA-2024-055", null,
            DateOnly.FromDateTime(DateTime.Today.AddDays(5)));
        os6.DefinirNumeroOS(NumeroOS.Gerar(DateTime.UtcNow.AddDays(-2), 1));
        os6.AdicionarServico("Diagnostico de hardware servidor", 1, 200.00m);
        os6.AdicionarServico("Substituicao de fan redundante", 1, 100.00m);
        os6.AdicionarServico("Substituicao de disco RAID", 1, 150.00m);
        os6.AdicionarServico("Rebuild de array RAID 5", 1, 250.00m);
        os6.AdicionarProduto("Fan redundante Dell PowerEdge compativel", 1, 180.00m);
        os6.AdicionarProduto("HD SAS 1TB 7.2K Dell certificado", 1, 650.00m);
        os6.AdicionarTaxa("Atendimento fora do horario comercial", 200.00m);
        os6.AplicarDesconto(new Desconto(TipoDesconto.Percentual, 10m));
        os6.AdicionarAnotacao("Cliente solicitou manutencao para sabado. Peca encomendada.", "Marcos Estoque");
        os6.AdicionarAnotacao("Fan substituido com sucesso. Disco RAID aguardando peca (HD SAS).", "Carlos Tecnico");
        os6.MarcarComoOrcamento();
        os6.Aprovar();
        os6.IniciarAndamento();
        os6.PausarAguardandoPeca();

        // --- OS 7: Joao - Impressora HP - Rejeitada ---
        var os7 = OrdemServico.Criar(joao.Id, impressoraJoao.Id,
            "Impressora nao puxa papel. Rolos de tracao parecem gastos.",
            "30 minutos", null, null,
            DateOnly.FromDateTime(DateTime.Today.AddDays(7)), null);
        os7.DefinirNumeroOS(NumeroOS.Gerar(DateTime.UtcNow.AddDays(-6), 2));
        os7.AdicionarServico("Diagnostico de mecanismo de tracao", 1, 80.00m);
        os7.AdicionarServico("Troca de rolos de tracao", 1, 120.00m);
        os7.AdicionarProduto("Kit rolos de tracao HP LaserJet Pro", 1, 250.00m);
        os7.AdicionarAnotacao("Cliente informou que vai comprar impressora nova. Orcamento rejeitado.", "Ana Atendente");
        os7.MarcarComoOrcamento();
        os7.Rejeitar();

        // --- OS 8: Maria - Celular Samsung - Rascunho (sem itens) ---
        var os8 = OrdemServico.Criar(maria.Id, celularMaria.Id,
            "Tela trincada apos queda. Touch funciona parcialmente - lado direito nao responde.",
            "1 hora", "Pelicula protetora estilhacada junto com a tela", null, null, null);
        os8.DefinirNumeroOS(NumeroOS.Gerar(DateTime.UtcNow, 2));

        await _context.OrdensServico.AddRangeAsync(new[] { os1, os2, os3, os4, os5, os6, os7, os8 }, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
