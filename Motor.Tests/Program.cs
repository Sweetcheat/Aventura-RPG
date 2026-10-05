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
        Verificar(p.MonstrosAtuais.Count == 3 && p.MonstrosAtuais[0].ID == Mundo.MONSTRO_ID_RATO,
            "monstros aparecem ao entrar no local (3 Ratos no Jardim)");

        var alvo = p.MonstrosAtuais[0];
        alvo.VidaAtual = 1;                                             // mata em 1 golpe (dano mínimo 1)
        alvo.LootTable.Clear();
        alvo.LootTable.Add(new ItemLoot(Mundo.ItemPorID(Mundo.ITEM_ID_PELO_DE_RATO), 100, false)); // drop garantido p/ teste determinístico

        var msgs = p.Atacar(espada);
        Verificar(!p.MonstrosAtuais.Contains(alvo), "monstro derrotado sai da luta");
        Verificar(p.MonstrosAtuais.Count == 2, "os demais monstros do local continuam vivos");
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
        Verificar(p.MonstrosAtuais.Count == 3 && p.MonstrosAtuais.TrueForAll(m => m.VidaAtual == 3),
            "monstros ressurgem ao sair e voltar ao local");
    }

    static void RegrasCriticoEFalha()
    {
        Cabecalho("Combate: crítico (10%) e falha do monstro (10%) — amostra de 300 golpes");

        var p = NovaPartida();
        var espada = (Arma)Mundo.ItemPorID(Mundo.ITEM_ID_ESPADA_ENFERRUJADA);

        IrParaJardim(p);
        p.MonstrosAtuais[0].VidaAtual = 100000;   // o monstro atacado não morre durante a amostra

        int criticos = 0, falhasMonstro = 0;
        for (int i = 0; i < 300; i++)
        {
            // O jogador pode morrer no meio da amostra (comportamento normal:
            // Morte teleporta para a Casa). Se morreu, volta ao Jardim e recria o combate.
            if (p.MonstrosAtuais.Count == 0)
            {
                if (p.Jogador.LocalAtual.ID != Mundo.LOCAL_ID_JARDIM_DOS_ALQUIMISTAS)
                    IrParaCasa(p);
                IrParaJardim(p);
                p.MonstrosAtuais[0].VidaAtual = 100000;
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
        var alvo = p.MonstrosAtuais[0];
        alvo.VidaAtual = 1;
        alvo.LootTable.Clear();   // sem nenhum item: nada cai
        var msgs = p.Atacar(espada);
        Verificar(!p.MonstrosAtuais.Contains(alvo), "monstro derrotado");
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
        p.MonstrosAtuais[0].VidaAtual = 100000;   // o monstro não morre
        p.MonstrosAtuais[0].DanoMaximo = 10;      // dano 0..10 contra vida 1 => morte rápida
        p.Jogador.Ouro = 100;
        p.Jogador.VidaAtual = 1;

        var msgs = null as System.Collections.Generic.List<string>;
        for (int i = 0; i < 500 && p.Jogador.LocalAtual.ID != Mundo.LOCAL_ID_CASA; i++)
            msgs = p.Atacar(espada);

        Verificar(p.Jogador.LocalAtual.ID == Mundo.LOCAL_ID_CASA, "morte => teletransporte para a Casa");
        Verificar(p.Jogador.VidaAtual == p.Jogador.VidaMaximaEfetiva, "respawn com vida cheia");
        Verificar(p.MonstrosAtuais.Count == 0, "monstros limpos após a morte");
        Verificar(p.Jogador.Ouro == 75, "perde 25% do ouro (100 - 25 = 75)");
        Verificar(msgs != null && msgs.Any(m => m.Contains("matou você")), "mensagem de morte");
        Verificar(msgs != null && msgs.Any(m => m.Contains("Você perdeu 25 de ouro por conta da derrota.")), "mensagem da perda de ouro");
    }

    static void RegrasAtaqueDoMonstro()
    {
        Cabecalho("Ataque do monstro: dano, teto de dano, sobrevivência, morte e respawn");

        var p = NovaPartida();
        IrParaJardim(p);
        p.MonstrosAtuais[0].DanoMaximo = 5;
        p.Jogador.VidaAtual = 10000;   // o jogador não morre durante a amostra

        int vidaInicial = p.Jogador.VidaAtual;
        int xpInicial = p.Jogador.PontosExperiencia;
        int ouroInicial = p.Jogador.Ouro;
        bool houveDano = false, houveFalha = false;
        for (int i = 0; i < 200; i++)
        {
            var msgs = p.AtaqueDoMonstro(p.MonstrosAtuais.Count > 0 ? p.MonstrosAtuais[0] : null);
            if (msgs.Any(m => m.Contains("causou a você"))) houveDano = true;
            if (msgs.Any(m => m.Contains("errou o ataque"))) houveFalha = true;
        }

        Verificar(p.Jogador.VidaAtual < vidaInicial, "ataque do monstro causa dano ao jogador");
        Verificar(p.Jogador.VidaAtual >= vidaInicial - 5 * 200, "dano do monstro respeita o teto (0..DanoMaximo)");
        Verificar(houveDano, "mensagem de dano do monstro existe");
        Verificar(houveFalha, "falha do monstro (10%) ocorre na amostra");
        Verificar(p.Jogador.LocalAtual.ID == Mundo.LOCAL_ID_JARDIM_DOS_ALQUIMISTAS, "jogador sobrevive com HP > 0 (sem morte)");
        Verificar(p.Jogador.PontosExperiencia == xpInicial && p.Jogador.Ouro == ouroInicial, "ataque do monstro não altera XP/ouro");
        Verificar(p.MonstrosAtuais.Count == 3 && p.MonstrosAtuais.TrueForAll(m => m.VidaAtual == 3),
            "ataque do monstro não altera a vida dos monstros");

        // Morte: HP 1 => o próximo acerto mata; a regra existente é aplicada
        // (Casa, perda de 25% do ouro, respawn com vida cheia)
        p.Jogador.VidaAtual = 1;
        p.Jogador.Ouro = 100;
        var msgsMorte = null as List<string>;
        for (int i = 0; i < 500 && p.Jogador.LocalAtual.ID != Mundo.LOCAL_ID_CASA; i++)
            msgsMorte = p.AtaqueDoMonstro(p.MonstrosAtuais.Count > 0 ? p.MonstrosAtuais[0] : null);

        Verificar(p.Jogador.LocalAtual.ID == Mundo.LOCAL_ID_CASA, "morte pelo monstro => teletransporte para a Casa");
        Verificar(p.Jogador.VidaAtual == p.Jogador.VidaMaximaEfetiva, "respawn com vida cheia");
        Verificar(p.MonstrosAtuais.Count == 0, "monstros limpos após a morte");
        Verificar(p.Jogador.Ouro == 75, "perde 25% do ouro (100 - 25 = 75)");
        Verificar(msgsMorte != null && msgsMorte.Any(m => m.Contains("matou você")), "mensagem de morte");
        Verificar(msgsMorte != null && msgsMorte.Any(m => m.Contains("Você perdeu 25 de ouro por conta da derrota.")), "mensagem da perda de ouro");

        // Sem monstro em combate: não faz nada
        var msgsSemMonstro = p.AtaqueDoMonstro(null);
        Verificar(msgsSemMonstro.Count == 0, "sem monstro em combate o ataque do monstro não faz nada");
    }

    static void RegrasArmaSelecionada()
    {
        Cabecalho("Arma selecionada: estado da Partida, validação e uso no ataque");

        var p = NovaPartida();
        var espada  = (Arma)Mundo.ItemPorID(Mundo.ITEM_ID_ESPADA_ENFERRUJADA);
        var porrete = (Arma)Mundo.ItemPorID(Mundo.ITEM_ID_PORRETE);

        Verificar(p.ArmaSelecionada != null && p.ArmaSelecionada.ID == Mundo.ITEM_ID_ESPADA_ENFERRUJADA,
            "arma inicial selecionada = Espada Enferrujada");

        // Arma fora do inventário não pode ser selecionada
        var msgsInvalida = p.SelecionarArma(porrete);
        Verificar(p.ArmaSelecionada.ID == Mundo.ITEM_ID_ESPADA_ENFERRUJADA, "arma inválida (fora do inventário) não é selecionada");
        Verificar(msgsInvalida.Count == 1 && msgsInvalida[0] == "Você não tem Porrete no inventário." + Environment.NewLine,
            "mensagem exata do Motor para arma inválida");

        // Seleção válida: o Porrete entra no inventário e é selecionado
        p.Jogador.AdicioneItemAoInventario(Mundo.ItemPorID(Mundo.ITEM_ID_PORRETE));
        var msgs = p.SelecionarArma(porrete);
        Verificar(p.ArmaSelecionada.ID == Mundo.ITEM_ID_PORRETE, "seleção válida troca a arma selecionada");
        Verificar(msgs.Count == 1 && msgs[0] == "Você equipou o(a) Porrete." + Environment.NewLine,
            "mensagem exata de seleção");
        Verificar(TemItem(p, Mundo.ITEM_ID_PORRETE, 1) && TemItem(p, Mundo.ITEM_ID_ESPADA_ENFERRUJADA, 1),
            "seleção não altera o inventário (sem duplicar armas)");

        // O ataque usa a arma selecionada: o Porrete (dano mínimo 3) em vez da
        // Espada (dano mínimo 1) — amostra de 50 golpes
        IrParaJardim(p);
        p.MonstrosAtuais[0].VidaAtual = 100000;

        bool danoSempreNoMinimo = true;
        for (int i = 0; i < 50; i++)
        {
            // O jogador pode morrer no meio da amostra (comportamento normal:
            // Morte teleporta para a Casa). Se morreu, volta ao Jardim e recria o combate.
            if (p.MonstrosAtuais.Count == 0)
            {
                if (p.Jogador.LocalAtual.ID != Mundo.LOCAL_ID_JARDIM_DOS_ALQUIMISTAS)
                    IrParaCasa(p);
                IrParaJardim(p);
                p.MonstrosAtuais[0].VidaAtual = 100000;
            }

            if (p.Jogador.VidaAtual < 5)
                p.Jogador.VidaAtual = 15;    // mantém o jogador vivo na amostra

            int vidaAntes = p.MonstrosAtuais[0].VidaAtual;
            p.Atacar(p.ArmaSelecionada);
            if (p.MonstrosAtuais.Count > 0 && vidaAntes - p.MonstrosAtuais[0].VidaAtual < 3)
                danoSempreNoMinimo = false;
        }
        Verificar(danoSempreNoMinimo, "50 ataques com o Porrete: dano sempre >= 3 (a arma selecionada é usada)");
    }

    static void RegrasMultiplosMonstros()
    {
        Cabecalho("Múltiplos monstros: local com 3 Ratos (Jardim)");

        var p = NovaPartida();
        var espada = (Arma)Mundo.ItemPorID(Mundo.ITEM_ID_ESPADA_ENFERRUJADA);

        IrParaJardim(p);
        Verificar(p.MonstrosAtuais.Count == 3, "Jardim tem 3 monstros vivos");
        Verificar(p.MonstrosAtuais.TrueForAll(m => m.ID == Mundo.MONSTRO_ID_RATO), "os 3 monstros são Ratos");
        Verificar(p.MonstrosAtuais.Distinct().Count() == 3, "instâncias independentes (sem compartilhamento)");

        // Derrota um monstro específico: os demais continuam vivos
        var alvo = p.MonstrosAtuais[0];
        alvo.VidaAtual = 1;
        alvo.LootTable.Clear();
        alvo.LootTable.Add(new ItemLoot(Mundo.ItemPorID(Mundo.ITEM_ID_PELO_DE_RATO), 100, true));
        int xpAntes = p.Jogador.PontosExperiencia;
        int ouroAntes = p.Jogador.Ouro;
        var msgs = p.AtaqueDoJogador(espada, alvo);

        Verificar(!p.MonstrosAtuais.Contains(alvo), "monstro derrotado sai da luta");
        Verificar(p.MonstrosAtuais.Count == 2, "matar um monstro não remove os demais");
        Verificar(p.MonstrosAtuais.TrueForAll(m => m.VidaAtual == 3), "monstros restantes não são afetados pelo golpe");
        Verificar(msgs.Any(m => m.Contains("Você derrotou o(a) Rato")), "mensagem de derrota do monstro");
        Verificar(p.Jogador.PontosExperiencia == xpAntes + 3 && p.Jogador.Ouro == ouroAntes + 10,
            "XP (3) e ouro (10) do monstro morto (e não dos demais)");
        Verificar(p.Jogador.Inventario.Any(ii => ii.Detalhes.ID == Mundo.ITEM_ID_PELO_DE_RATO && ii.Quantidade > 0),
            "loot do monstro morto no inventário");

        // Ataque de um monstro específico: só o monstro indicado age
        var monstro2 = p.MonstrosAtuais[0];
        monstro2.DanoMaximo = 5;
        int vidaMonstro2 = monstro2.VidaAtual;
        int vidaJogador = p.Jogador.VidaAtual;
        p.AtaqueDoMonstro(monstro2);
        Verificar(monstro2.VidaAtual == vidaMonstro2, "ataque do monstro não altera a vida dele");
        Verificar(p.MonstrosAtuais[1].VidaAtual == 3, "outro monstro não é afetado");
        Verificar(p.Jogador.VidaAtual <= vidaJogador, "o monstro escolhido causa dano ao jogador");

        // Alvo fora da luta não pode ser atacado
        var msgsAlvoFora = p.AtaqueDoJogador(espada, alvo);
        Verificar(msgsAlvoFora.Count == 0, "atacar um monstro fora da luta não faz nada");

        // Morte do jogador: todos os monstros são limpos e o respawn segue a
        // regra existente (Casa, 25% do ouro, vida cheia)
        p.Jogador.VidaAtual = 1;
        p.Jogador.Ouro = 100;
        var agressor = p.MonstrosAtuais[0];
        agressor.DanoMaximo = 10;
        var msgsMorte = null as List<string>;
        for (int i = 0; i < 500 && p.Jogador.LocalAtual.ID != Mundo.LOCAL_ID_CASA; i++)
            msgsMorte = p.AtaqueDoMonstro(agressor);

        Verificar(p.Jogador.LocalAtual.ID == Mundo.LOCAL_ID_CASA, "morte => teletransporte para a Casa");
        Verificar(p.MonstrosAtuais.Count == 0, "morte do jogador limpa todos os monstros");
        Verificar(p.Jogador.VidaAtual == p.Jogador.VidaMaximaEfetiva, "respawn com vida cheia");
        Verificar(p.Jogador.Ouro == 75, "perde 25% do ouro (100 - 25 = 75)");
        Verificar(msgsMorte != null && msgsMorte.Any(m => m.Contains("matou você")), "mensagem de morte");

        // Sair e voltar: o local recria todos os monstros
        IrParaJardim(p);
        Verificar(p.MonstrosAtuais.Count == 3, "sair e voltar recria os 3 monstros do local");
    }

    static void RegrasPocaoSemPoção()
    {
        Cabecalho("Poção: sem poção no inventário");

        var p = NovaPartida();
        var pocao = (PocaoCura)Mundo.ItemPorID(Mundo.ITEM_ID_POCAO_DE_CURA);
        p.Jogador.VidaAtual = 5;

        var msgs = p.UsarPocao(pocao);
        Verificar(p.Jogador.VidaAtual == 5, "sem poção: vida inalterada");
        Verificar(msgs.Count == 1 && msgs[0] == "Você não tem uma Pocao de cura para usar." + Environment.NewLine,
            "mensagem exata do Motor para uso sem poção");
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
        RegrasArmaSelecionada();
        RegrasPocaoSemPoção();
        RegrasMultiplosMonstros();

        Console.WriteLine();
        Console.WriteLine(_falhas == 0 ? "TODOS OS TESTES PASSARAM" : _falhas + " TESTE(S) FALHARAM");
        return _falhas == 0 ? 0 : 1;
    }
}
