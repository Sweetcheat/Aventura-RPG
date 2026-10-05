using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Motor;

// Testes da Motor (net8.0): rodam as regras da Partida diretamente, sem WinForms.
// Substitui o harness antigo (.NET 3.5), que não consome o Motor net8.0.
static class Program
{
    static int _falhas = 0;

    static void Verificar(bool condicao, string mensagem)
    {
        Console.WriteLine((condicao ? "  OK   : " : "  FALHA: ") + mensagem);
        if (!condicao) _falhas++;
    }

    static void Cabecalho(string texto)
    {
        Console.WriteLine();
        Console.WriteLine("Cenário: " + texto);
    }

    // Replica a inicialização da apresentação: o Form cria a Partida e faz o
    // primeiro MoverPara para a Casa (a Partida em si não define local inicial)
    static Partida NovaPartida()
    {
        var partida = new Partida();
        partida.MoverPara(Mundo.LocalPorID(Mundo.LOCAL_ID_CASA));
        return partida;
    }

    static bool TemItem(Partida p, int id, int qtd)
    {
        var ii = p.Jogador.Inventario.FirstOrDefault(x => x.Detalhes.ID == id);
        return ii != null && ii.Quantidade == qtd;
    }

    // Rotas pelo grafo do mundo (partindo da Casa)
    static void IrParaCabana(Partida p)
    {
        p.MoverPara(Mundo.LocalPorID(Mundo.LOCAL_ID_PRACA));
        p.MoverPara(Mundo.LocalPorID(Mundo.LOCAL_ID_CABANA_DOS_ALQUIMISTAS));
    }

    static void IrParaJardim(Partida p)
    {
        IrParaCabana(p);
        p.MoverPara(Mundo.LocalPorID(Mundo.LOCAL_ID_JARDIM_DOS_ALQUIMISTAS));
    }

    static void IrParaAreaCamponeses(Partida p)
    {
        p.MoverPara(Mundo.LocalPorID(Mundo.LOCAL_ID_PRACA));
        p.MoverPara(Mundo.LocalPorID(Mundo.LOCAL_ID_CASA_DA_FAZENDA));
        p.MoverPara(Mundo.LocalPorID(Mundo.LOCAL_ID_AREA_DOS_CAMPONESES));
    }

    // CasaFazenda -> Leste Praça -> Sul Casa ; Cabana -> Sul Praça -> Sul Casa
    static void IrParaCasa(Partida p)
    {
        p.MoverPara(Mundo.LocalPorID(Mundo.LOCAL_ID_PRACA));
        p.MoverPara(Mundo.LocalPorID(Mundo.LOCAL_ID_CASA));
    }

    static void CenarioPorreteOrdem1()
    {
        Cabecalho("Porrete: ordem Jardim dos Alquimistas -> Area dos Camponeses");

        var p = NovaPartida();
        var questJardim  = Mundo.QuestPorID(Mundo.QUEST_ID_LIMPAR_JARDIM_DOS_ALQUIMISTAS);
        var questCampos  = Mundo.QuestPorID(Mundo.QUEST_ID_LIMPAR_AREA_DOS_CAMPONESES);

        IrParaCabana(p);                                   // recebe a quest do Jardim
        for (int i = 0; i < 3; i++)
            p.Jogador.AdicioneItemAoInventario(Mundo.ItemPorID(Mundo.ITEM_ID_CAUDA_DE_RATO));
        IrParaCasa(p);
        var msgs = p.MoverPara(Mundo.LocalPorID(Mundo.LOCAL_ID_CABANA_DOS_ALQUIMISTAS)); // completa a quest do Jardim

        Verificar(p.Jogador.QuestEstaCompletada(questJardim), "quest do Jardim concluída");
        Verificar(msgs.Any(m => m.Contains("Você completou 'Limpe o Jardim dos Alquimistas'.")), "mensagem de conclusão da quest");
        Verificar(p.Jogador.Ouro == 10, "ouro de recompensa da quest (10)");
        Verificar(TemItem(p, Mundo.ITEM_ID_POCAO_DE_CURA, 1), "recompensa da quest: pocao de cura");
        Verificar(!TemItem(p, Mundo.ITEM_ID_CAUDA_DE_RATO, 3) && !p.Jogador.Inventario.Any(ii => ii.Detalhes.ID == Mundo.ITEM_ID_CAUDA_DE_RATO && ii.Quantidade > 0), "itens da quest removidos do inventário");
        Verificar(!p.Jogador.RecebeuRecompensaPorrete, "apenas uma quest concluída => sem Porrete");
        Verificar(!p.Jogador.Inventario.Any(ii => ii.Detalhes.ID == Mundo.ITEM_ID_PORRETE), "sem item Porrete no inventário");

        IrParaAreaCamponeses(p);                           // recebe a quest dos Camponeses
        for (int i = 0; i < 3; i++)
            p.Jogador.AdicioneItemAoInventario(Mundo.ItemPorID(Mundo.ITEM_ID_PRESA_DE_COBRA));
        IrParaCasa(p);
        msgs = p.MoverPara(Mundo.LocalPorID(Mundo.LOCAL_ID_CASA_DA_FAZENDA));             // completa a quest dos Camponeses => Porrete

        Verificar(p.Jogador.RecebeuRecompensaPorrete, "duas quests concluídas => flag de recompensa ativada");
        Verificar(TemItem(p, Mundo.ITEM_ID_PORRETE, 1), "Porrete concedido (quantidade 1)");
        Verificar(msgs.Any(m => m.Contains("Você completou as duas quests e recebeu um Porrete!")), "mensagem de concessão do Porrete");

        IrParaCasa(p);
        p.MoverPara(Mundo.LocalPorID(Mundo.LOCAL_ID_CASA_DA_FAZENDA));                    // novas entradas => sem segunda concessão
        IrParaCasa(p);
        IrParaCabana(p);
        Verificar(TemItem(p, Mundo.ITEM_ID_PORRETE, 1), "novas entradas em locais => sem segunda concessão");
    }

    static void CenarioPorreteOrdem2()
    {
        Cabecalho("Porrete: ordem Area dos Camponeses -> Jardim dos Alquimistas");

        var p = NovaPartida();
        var questJardim = Mundo.QuestPorID(Mundo.QUEST_ID_LIMPAR_JARDIM_DOS_ALQUIMISTAS);
        var questCampos = Mundo.QuestPorID(Mundo.QUEST_ID_LIMPAR_AREA_DOS_CAMPONESES);

        IrParaAreaCamponeses(p);
        for (int i = 0; i < 3; i++)
            p.Jogador.AdicioneItemAoInventario(Mundo.ItemPorID(Mundo.ITEM_ID_PRESA_DE_COBRA));
        IrParaCasa(p);
        p.MoverPara(Mundo.LocalPorID(Mundo.LOCAL_ID_CASA_DA_FAZENDA));                    // completa a 1ª quest

        Verificar(!p.Jogador.RecebeuRecompensaPorrete, "apenas uma quest concluída => sem Porrete");

        IrParaCabana(p);
        for (int i = 0; i < 3; i++)
            p.Jogador.AdicioneItemAoInventario(Mundo.ItemPorID(Mundo.ITEM_ID_CAUDA_DE_RATO));
        IrParaCasa(p);
        var msgs = p.MoverPara(Mundo.LocalPorID(Mundo.LOCAL_ID_CABANA_DOS_ALQUIMISTAS));  // completa a 2ª quest => Porrete

        Verificar(p.Jogador.QuestEstaCompletada(questCampos) && p.Jogador.QuestEstaCompletada(questJardim), "duas quests concluídas => flag de recompensa ativada");
        Verificar(TemItem(p, Mundo.ITEM_ID_PORRETE, 1), "Porrete concedido (quantidade 1)");
        Verificar(msgs.Any(m => m.Contains("recebeu um Porrete!")), "mensagem de concessão do Porrete");

        IrParaCasa(p);
        IrParaAreaCamponeses(p);
        Verificar(TemItem(p, Mundo.ITEM_ID_PORRETE, 1), "novas entradas em locais => sem segunda concessão");
    }

    static void CenarioPorreteParcial()
    {
        Cabecalho("Porrete: uma quest concluída, a outra nem recebida");

        var p = NovaPartida();

        IrParaCabana(p);
        for (int i = 0; i < 3; i++)
            p.Jogador.AdicioneItemAoInventario(Mundo.ItemPorID(Mundo.ITEM_ID_CAUDA_DE_RATO));
        IrParaCasa(p);
        p.MoverPara(Mundo.LocalPorID(Mundo.LOCAL_ID_CABANA_DOS_ALQUIMISTAS));             // só a quest do Jardim

        Verificar(!p.Jogador.RecebeuRecompensaPorrete, "quest pendente => sem Porrete");
        Verificar(!p.Jogador.Inventario.Any(ii => ii.Detalhes.ID == Mundo.ITEM_ID_PORRETE), "sem item Porrete no inventário");
    }

    static void RegrasIniciaisEGates()
    {
        Cabecalho("Estado inicial, gates e cura");

        var p = NovaPartida();
        Verificar(TemItem(p, Mundo.ITEM_ID_ESPADA_ENFERRUJADA, 1), "espada inicial no inventário");
        Verificar(p.Jogador.LocalAtual.ID == Mundo.LOCAL_ID_CASA, "jogador inicia na Casa");
        Verificar(p.Jogador.VidaAtual == p.Jogador.VidaMaximaEfetiva, "vida cheia no início");

        p.MoverPara(Mundo.LocalPorID(Mundo.LOCAL_ID_PRACA));
        var msgsBloqueio = p.MoverPara(Mundo.LocalPorID(Mundo.LOCAL_ID_POSTO_DE_GUARDA));
        Verificar(p.Jogador.LocalAtual.ID == Mundo.LOCAL_ID_PRACA, "gate: sem o passe a entrada é bloqueada (fica na Praça)");
        Verificar(msgsBloqueio.Count == 1 && msgsBloqueio[0] == "Você precisa ter um Passe de Aventureiro para acessar este local." + Environment.NewLine, "mensagem exata do gate");

        p.Jogador.AdicioneItemAoInventario(Mundo.ItemPorID(Mundo.ITEM_ID_PASSE_AVENTUREIRO));
        p.MoverPara(Mundo.LocalPorID(Mundo.LOCAL_ID_POSTO_DE_GUARDA));
        Verificar(p.Jogador.LocalAtual.ID == Mundo.LOCAL_ID_POSTO_DE_GUARDA, "gate: com o passe a entrada é liberada");

        p.Jogador.VidaAtual = 5;
        p.MoverPara(Mundo.LocalPorID(Mundo.LOCAL_ID_PRACA));
        Verificar(p.Jogador.VidaAtual == 5, "fora da Casa a vida não recupera");
        p.MoverPara(Mundo.LocalPorID(Mundo.LOCAL_ID_CASA));
        Verificar(p.Jogador.VidaAtual == p.Jogador.VidaMaximaEfetiva, "volta à Casa cura a vida (total)");
    }

    static void RegrasCombate()
    {
        Cabecalho("Combate: dano mínimo, morte do monstro, loot, XP, level-up, respawn");

        var p = NovaPartida();
        var espada = (Arma)Mundo.ItemPorID(Mundo.ITEM_ID_ESPADA_ENFERRUJADA);
        p.Jogador.PontosExperiencia = 99;   // +3 do rato => level 2
        p.Jogador.Ouro = 100;

        IrParaJardim(p);
        Verificar(p.MonstroAtual != null && p.MonstroAtual.ID == Mundo.MONSTRO_ID_RATO, "monstro aparece ao entrar no local (Rato)");

        p.MonstroAtual.VidaAtual = 1;                                             // mata em 1 golpe (dano mínimo 1)
        p.MonstroAtual.LootTable.Clear();
        p.MonstroAtual.LootTable.Add(new ItemLoot(Mundo.ItemPorID(Mundo.ITEM_ID_PELO_DE_RATO), 100, false)); // drop garantido p/ teste determinístico

        var msgs = p.Atacar(espada);
        Verificar(p.MonstroAtual == null, "monstro derrotado (nulo após o combate)");
        Verificar(msgs.Any(m => m.Contains("Você acertou o(a) Rato")), "mensagem de acerto");
        Verificar(msgs.Any(m => m.Contains("Você derrotou o(a) Rato")), "mensagem de derrota do monstro");
        Verificar(msgs.Any(m => m == "Você recebe 3 pontos de experiência." + Environment.NewLine), "XP do monstro (3)");
        Verificar(msgs.Any(m => m == "Você recebe 10 de ouro." + Environment.NewLine), "ouro do monstro (10)");
        Verificar(msgs.Any(m => m.Contains("Seu saque: 1 Pelo de rato")), "loot com drop de 100% cai");
        Verificar(msgs.Any(m => m == "Você subiu para o level 2!" + Environment.NewLine), "level-up (99 + 3 >= 100)");
        Verificar(p.Jogador.Level == 2, "level 2 após o XP");
        Verificar(p.Jogador.Ouro == 110, "ouro somado (100 + 10)");
        Verificar(TemItem(p, Mundo.ITEM_ID_PELO_DE_RATO, 1), "item do saque no inventário");

        IrParaCasa(p);
        IrParaJardim(p);
        Verificar(p.MonstroAtual != null && p.MonstroAtual.VidaAtual == 3, "monstro ressurge ao sair e voltar ao local");
    }

    static void RegrasCriticoEFalha()
    {
        Cabecalho("Combate: crítico (10%) e falha do monstro (10%) — amostra de 300 golpes");

        var p = NovaPartida();
        var espada = (Arma)Mundo.ItemPorID(Mundo.ITEM_ID_ESPADA_ENFERRUJADA);

        IrParaJardim(p);
        p.MonstroAtual.VidaAtual = 100000;   // o monstro não morre durante a amostra

        int criticos = 0, falhasMonstro = 0;
        for (int i = 0; i < 300; i++)
        {
            // O jogador pode morrer no meio da amostra (comportamento normal:
            // Morte teleporta para a Casa). Se morreu, volta ao Jardim e recria o combate.
            if (p.MonstroAtual == null)
            {
                if (p.Jogador.LocalAtual.ID != Mundo.LOCAL_ID_JARDIM_DOS_ALQUIMISTAS)
                    IrParaCasa(p);
                IrParaJardim(p);
                p.MonstroAtual.VidaAtual = 100000;
            }

            if (p.Jogador.VidaAtual < 5)
                p.Jogador.VidaAtual = 15;    // mantém o jogador vivo na amostra

            var msgs = p.Atacar(espada);
            if (msgs.Any(m => m.Contains("(CRÍTICO!)"))) criticos++;
            if (msgs.Any(m => m.Contains("errou o ataque"))) falhasMonstro++;
        }

        Verificar(criticos > 0, $"críticos ocorreram na amostra ({criticos} em 300)");
        Verificar(falhasMonstro > 0, $"falhas do monstro ocorreram na amostra ({falhasMonstro} em 300)");
    }

    static void RegrasLootVazioEPoacao()
    {
        Cabecalho("Loot vazio (sem item comum) e poção");

        var p = NovaPartida();
        var espada = (Arma)Mundo.ItemPorID(Mundo.ITEM_ID_ESPADA_ENFERRUJADA);
        var pocao = (PocaoCura)Mundo.ItemPorID(Mundo.ITEM_ID_POCAO_DE_CURA);

        IrParaJardim(p);
        p.MonstroAtual.VidaAtual = 1;
        p.MonstroAtual.LootTable.Clear();   // sem nenhum item: nada cai
        var msgs = p.Atacar(espada);
        Verificar(p.MonstroAtual == null, "monstro derrotado");
        Verificar(!msgs.Any(m => m.Contains("Seu saque")), "loot vazio => sem saque");

        // Poção com vida cheia: não é consumida e o monstro não contra-ataca
        var p2 = NovaPartida();
        p2.Jogador.AdicioneItemAoInventario(Mundo.ItemPorID(Mundo.ITEM_ID_POCAO_DE_CURA));
        var msgsCheia = p2.UsarPocao(pocao);
        Verificar(msgsCheia.Count == 1 && msgsCheia[0] == "Sua vida já está cheia; a Pocao de cura não foi utilizada." + Environment.NewLine, "vida cheia => poção não utilizada (mensagem exata)");
        Verificar(TemItem(p2, Mundo.ITEM_ID_POCAO_DE_CURA, 1), "poção não consumida com vida cheia");
        Verificar(p2.Jogador.VidaAtual == p2.Jogador.VidaMaximaEfetiva, "vida inalterada");

        // Poção com vida parcial: cura e consome, com teto na vida máxima
        p2.Jogador.VidaAtual = 5;
        p2.UsarPocao(pocao);
        Verificar(p2.Jogador.VidaAtual == 10, "cura aplicada (5 + 5 = 10)");
        Verificar(!p2.Jogador.Inventario.Any(ii => ii.Detalhes.ID == Mundo.ITEM_ID_POCAO_DE_CURA && ii.Quantidade > 0), "poção consumida");

        p2.Jogador.AdicioneItemAoInventario(Mundo.ItemPorID(Mundo.ITEM_ID_POCAO_DE_CURA));
        p2.Jogador.VidaAtual = 12;
        p2.UsarPocao(pocao);
        Verificar(p2.Jogador.VidaAtual == 15, "cura não excede a vida máxima (12 + 5 => 15)");
    }

    static void RegrasMorte()
    {
        Cabecalho("Morte: perda de 25% do ouro e respawn na Casa");

        var p = NovaPartida();
        var espada = (Arma)Mundo.ItemPorID(Mundo.ITEM_ID_ESPADA_ENFERRUJADA);

        IrParaJardim(p);
        p.MonstroAtual.VidaAtual = 100000;   // o monstro não morre
        p.MonstroAtual.DanoMaximo = 10;      // dano 0..10 contra vida 1 => morte rápida
        p.Jogador.Ouro = 100;
        p.Jogador.VidaAtual = 1;

        var msgs = null as System.Collections.Generic.List<string>;
        for (int i = 0; i < 500 && p.Jogador.LocalAtual.ID != Mundo.LOCAL_ID_CASA; i++)
            msgs = p.Atacar(espada);

        Verificar(p.Jogador.LocalAtual.ID == Mundo.LOCAL_ID_CASA, "morte => teletransporte para a Casa");
        Verificar(p.Jogador.VidaAtual == p.Jogador.VidaMaximaEfetiva, "respawn com vida cheia");
        Verificar(p.MonstroAtual == null, "monstro limpo após a morte");
        Verificar(p.Jogador.Ouro == 75, "perde 25% do ouro (100 - 25 = 75)");
        Verificar(msgs != null && msgs.Any(m => m.Contains("matou você")), "mensagem de morte");
        Verificar(msgs != null && msgs.Any(m => m.Contains("Você perdeu 25 de ouro por conta da derrota.")), "mensagem da perda de ouro");
    }

    static void RegrasAtaqueDoMonstro()
    {
        Cabecalho("Ataque do monstro: dano, teto de dano, sobrevivência, morte e respawn");

        var p = NovaPartida();
        IrParaJardim(p);
        p.MonstroAtual.DanoMaximo = 5;
        p.Jogador.VidaAtual = 10000;   // o jogador não morre durante a amostra

        int vidaInicial = p.Jogador.VidaAtual;
        int xpInicial = p.Jogador.PontosExperiencia;
        int ouroInicial = p.Jogador.Ouro;
        bool houveDano = false, houveFalha = false;
        for (int i = 0; i < 200; i++)
        {
            var msgs = p.AtaqueDoMonstro();
            if (msgs.Any(m => m.Contains("causou a você"))) houveDano = true;
            if (msgs.Any(m => m.Contains("errou o ataque"))) houveFalha = true;
        }

        Verificar(p.Jogador.VidaAtual < vidaInicial, "ataque do monstro causa dano ao jogador");
        Verificar(p.Jogador.VidaAtual >= vidaInicial - 5 * 200, "dano do monstro respeita o teto (0..DanoMaximo)");
        Verificar(houveDano, "mensagem de dano do monstro existe");
        Verificar(houveFalha, "falha do monstro (10%) ocorre na amostra");
        Verificar(p.Jogador.LocalAtual.ID == Mundo.LOCAL_ID_JARDIM_DOS_ALQUIMISTAS, "jogador sobrevive com HP > 0 (sem morte)");
        Verificar(p.Jogador.PontosExperiencia == xpInicial && p.Jogador.Ouro == ouroInicial, "ataque do monstro não altera XP/ouro");
        Verificar(p.MonstroAtual != null && p.MonstroAtual.VidaAtual == 3, "ataque do monstro não altera a vida do monstro");

        // Morte: HP 1 => o próximo acerto mata; a regra existente é aplicada
        // (Casa, perda de 25% do ouro, respawn com vida cheia)
        p.Jogador.VidaAtual = 1;
        p.Jogador.Ouro = 100;
        var msgsMorte = null as List<string>;
        for (int i = 0; i < 500 && p.Jogador.LocalAtual.ID != Mundo.LOCAL_ID_CASA; i++)
            msgsMorte = p.AtaqueDoMonstro();

        Verificar(p.Jogador.LocalAtual.ID == Mundo.LOCAL_ID_CASA, "morte pelo monstro => teletransporte para a Casa");
        Verificar(p.Jogador.VidaAtual == p.Jogador.VidaMaximaEfetiva, "respawn com vida cheia");
        Verificar(p.MonstroAtual == null, "monstro limpo após a morte");
        Verificar(p.Jogador.Ouro == 75, "perde 25% do ouro (100 - 25 = 75)");
        Verificar(msgsMorte != null && msgsMorte.Any(m => m.Contains("matou você")), "mensagem de morte");
        Verificar(msgsMorte != null && msgsMorte.Any(m => m.Contains("Você perdeu 25 de ouro por conta da derrota.")), "mensagem da perda de ouro");

        // Sem monstro em combate: não faz nada
        var msgsSemMonstro = p.AtaqueDoMonstro();
        Verificar(msgsSemMonstro.Count == 0, "sem monstro em combate o ataque do monstro não faz nada");
    }

    static int Main()
    {
        Console.OutputEncoding = Encoding.UTF8;

        CenarioPorreteOrdem1();
        CenarioPorreteOrdem2();
        CenarioPorreteParcial();
        RegrasIniciaisEGates();
        RegrasCombate();
        RegrasCriticoEFalha();
        RegrasLootVazioEPoacao();
        RegrasMorte();
        RegrasAtaqueDoMonstro();

        Console.WriteLine();
        Console.WriteLine(_falhas == 0 ? "TODOS OS TESTES PASSARAM" : _falhas + " TESTE(S) FALHARAM");
        return _falhas == 0 ? 0 : 1;
    }
}
