using System;
using System.Collections.Generic;
using System.Linq;

namespace Motor
{
    /*  Sessão de jogo: detém o estado (jogador + monstro atual) e concentra as
        regras de movimentação, combate, poção e morte/respawn.
        A camada de apresentação (hoje o form WinForms, no futuro o Godot)
        chama os métodos e exibe as mensagens retornadas. */
    public class Partida
    {
        // Chance (em 100) de evento de combate: crítico do jogador (dano x2) ou falha do monstro (dano zero)
        private const int CHANCE_EVENTO_COMBATE = 10;

        public Jogador Jogador { get; private set; }
        public Monstro MonstroAtual { get; private set; }

        public Partida()
        {
            Jogador = new Jogador(15, 15, 0, 0);
            // A espada inicial é adicionada antes do primeiro MoverPara, para que
            // o inventário já apareça com ela ao iniciar o jogo
            Jogador.Inventario.Add(new InventarioItem(Mundo.ItemPorID(Mundo.ITEM_ID_ESPADA_ENFERRUJADA), 1));
        }

        /*  Move o jogador para um novo local: gate de entrada, cura na Casa, quests,
            recompensa especial e respawn de monstro.
            Retorna as mensagens que a camada de apresentação deve exibir. */
        public List<string> MoverPara(Local novoLocal)
        {
            var mensagens = new List<string>();

            // Se o local exige um item que o jogador não tem, bloqueia a entrada
            if (!Jogador.TemItemNecessarioParaEntrarNesteLocal(novoLocal))
            {
                mensagens.Add($"Você precisa ter um {novoLocal.ItemNecessarioEntrar.Nome} para acessar este local.{Environment.NewLine}");
                return mensagens;
            }

            // A cura e o respawn de monstro só ocorrem em mudanças reais de local.
            // Ao encerrar um combate, a apresentação não reentra em MoverPara (ver Atacar),
            // então o monstro derrotado só volta quando o jogador sair e voltar a este local.
            bool mudouDeLocal = !ReferenceEquals(novoLocal, Jogador.LocalAtual);
            Jogador.LocalAtual = novoLocal;

            // Apenas a Casa cura a vida (total): nos demais locais o jogador
            // mantém a vida que tinha (cura em campo só com poção)
            if (mudouDeLocal && novoLocal.ID == Mundo.LOCAL_ID_CASA)
                Jogador.VidaAtual = Jogador.VidaMaximaEfetiva;

            // Lógica de quest do local
            if (novoLocal.QuestDisponivelAqui != null)
            {
                bool jogadorJaTemQuest    = Jogador.TemEstaQuest(novoLocal.QuestDisponivelAqui);
                bool jogadorJaCompletouQuest = Jogador.QuestEstaCompletada(novoLocal.QuestDisponivelAqui);

                if (jogadorJaTemQuest)
                {
                    if (!jogadorJaCompletouQuest && Jogador.TemTodosItensParaCompletarQuest(novoLocal.QuestDisponivelAqui))
                    {
                        mensagens.Add(Environment.NewLine);
                        mensagens.Add($"Você completou '{novoLocal.QuestDisponivelAqui.Nome}'.{Environment.NewLine}");

                        Jogador.RemovaItensDeQuestCompletada(novoLocal.QuestDisponivelAqui);

                        mensagens.Add($"Você recebe:{Environment.NewLine}");
                        mensagens.Add($"{novoLocal.QuestDisponivelAqui.PontosExperienciaRecompensa} pontos de experiência{Environment.NewLine}");
                        mensagens.Add($"{novoLocal.QuestDisponivelAqui.OuroRecompensa} ouro{Environment.NewLine}");
                        mensagens.Add($"{novoLocal.QuestDisponivelAqui.ItemRecompensa.Nome}{Environment.NewLine}");
                        mensagens.Add(Environment.NewLine);

                        GanhaExperiencia(novoLocal.QuestDisponivelAqui.PontosExperienciaRecompensa, mensagens);
                        Jogador.Ouro              += novoLocal.QuestDisponivelAqui.OuroRecompensa;

                        Jogador.AdicioneItemAoInventario(novoLocal.QuestDisponivelAqui.ItemRecompensa);
                        Jogador.MarqueQuestCompletada(novoLocal.QuestDisponivelAqui);
                    }
                }
                else
                {
                    // Jogador ainda não tem a quest: entrega ela
                    mensagens.Add(Environment.NewLine);
                    mensagens.Add($"Você recebeu a quest: {novoLocal.QuestDisponivelAqui.Nome}.{Environment.NewLine}");
                    mensagens.Add($"{novoLocal.QuestDisponivelAqui.Descricao}{Environment.NewLine}");
                    mensagens.Add($"Para completá-la, retorne com:{Environment.NewLine}");

                    foreach (var qci in novoLocal.QuestDisponivelAqui.QuestCompletadaItem)
                    {
                        var nomeItem = qci.Quantidade == 1 ? qci.Detalhes.Nome : qci.Detalhes.NomePlural;
                        mensagens.Add($"{qci.Quantidade} {nomeItem}{Environment.NewLine}");
                    }

                    mensagens.Add(Environment.NewLine);
                    Jogador.Quests.Add(new JogadorQuest(novoLocal.QuestDisponivelAqui));
                }
            }

            // Recompensa especial: o Porrete é concedido uma única vez quando o jogador
            // completa as duas quests, independentemente da ordem em que foram feitas
            var questJardimAlquimistas = Mundo.QuestPorID(Mundo.QUEST_ID_LIMPAR_JARDIM_DOS_ALQUIMISTAS);
            var questAreaCamponeses    = Mundo.QuestPorID(Mundo.QUEST_ID_LIMPAR_AREA_DOS_CAMPONESES);

            if (!Jogador.RecebeuRecompensaPorrete
                && Jogador.QuestEstaCompletada(questJardimAlquimistas)
                && Jogador.QuestEstaCompletada(questAreaCamponeses))
            {
                mensagens.Add(Environment.NewLine);
                mensagens.Add($"Você completou as duas quests e recebeu um Porrete!{Environment.NewLine}");

                Jogador.AdicioneItemAoInventario(Mundo.ItemPorID(Mundo.ITEM_ID_PORRETE));
                Jogador.RecebeuRecompensaPorrete = true;
            }

            // Lógica de monstro do local: o monstro é recriado apenas quando o jogador
            // realmente muda de local (sai e volta), evitando o respawn instantâneo
            // logo após a derrota.
            if (mudouDeLocal)
            {
                if (novoLocal.MonstroVivoAqui != null)
                {
                    mensagens.Add($"Você vê um(a) {novoLocal.MonstroVivoAqui.Nome}{Environment.NewLine}");

                    // Instancia um novo monstro a partir dos dados padrão do Mundo
                    var monstroNormal = Mundo.MonstroPorID(novoLocal.MonstroVivoAqui.ID);
                    MonstroAtual = new Monstro(
                        monstroNormal.ID, monstroNormal.Nome, monstroNormal.DanoMaximo,
                        monstroNormal.PontosExperienciaRecompensa, monstroNormal.OuroRecompensa,
                        monstroNormal.VidaAtual, monstroNormal.VidaMaxima);

                    foreach (var itemLoot in monstroNormal.LootTable)
                        MonstroAtual.LootTable.Add(itemLoot);
                }
                else
                {
                    MonstroAtual = null;
                }
            }

            return mensagens;
        }

        /*  Ataque do jogador: dano (mínimo 1 + bônus de level), crítico e morte
            do monstro (XP, ouro e loot com garantia do item comum).
            O monstro não contra-ataca: esta é a parte do turno que pertence ao
            jogador (também usada pelo combate espacial).
            Sem monstro em combate, não faz nada. */
        public List<string> AtaqueDoJogador(Arma armaAtual)
        {
            var mensagens = new List<string>();

            if (MonstroAtual == null)
                return mensagens;

            // Bônus de level (a partir do level 2): +1 de dano, aplicado antes do crítico
            // Dano mínimo de 1: a arma sempre acerta (o bônus de level é somado depois)
            int danoAoMonstro = Math.Max(1, GeradorNumeroAleatorio.NumeroEntre(armaAtual.DanoMinimo, armaAtual.DanoMaximo)) + (Jogador.Level - 1);

            // Crítico (10%): dobra o dano final, desde que seja maior que zero
            bool critico = danoAoMonstro > 0 && GeradorNumeroAleatorio.NumeroEntre(1, 100) <= CHANCE_EVENTO_COMBATE;
            if (critico)
                danoAoMonstro *= 2;

            MonstroAtual.VidaAtual -= danoAoMonstro;
            mensagens.Add($"Você acertou o(a) {MonstroAtual.Nome} e causou {danoAoMonstro} ponto(s) de dano{(critico ? " (CRÍTICO!)" : "")}.{Environment.NewLine}");

            if (MonstroAtual.VidaAtual <= 0)
            {
                mensagens.Add(Environment.NewLine);
                mensagens.Add($"Você derrotou o(a) {MonstroAtual.Nome}{Environment.NewLine}");

                GanhaExperiencia(MonstroAtual.PontosExperienciaRecompensa, mensagens);
                mensagens.Add($"Você recebe {MonstroAtual.PontosExperienciaRecompensa} pontos de experiência.{Environment.NewLine}");

                Jogador.Ouro             += MonstroAtual.OuroRecompensa;
                mensagens.Add($"Você recebe {MonstroAtual.OuroRecompensa} de ouro.{Environment.NewLine}");

                // Sorteia o loot do monstro
                var itensSaqueados = MonstroAtual.LootTable
                    .Where(il => GeradorNumeroAleatorio.NumeroEntre(1, 100) <= il.PorcentagemDrop)
                    .Select(il => new InventarioItem(il.Detalhes, 1))
                    .ToList();

                // Garante ao menos o item comum se nada caiu
                if (itensSaqueados.Count == 0)
                {
                    itensSaqueados = MonstroAtual.LootTable
                        .Where(il => il.EItemComum)
                        .Select(il => new InventarioItem(il.Detalhes, 1))
                        .ToList();
                }

                foreach (var item in itensSaqueados)
                {
                    Jogador.AdicioneItemAoInventario(item.Detalhes);
                    var nomeItem = item.Quantidade == 1 ? item.Detalhes.Nome : item.Detalhes.NomePlural;
                    mensagens.Add($"Seu saque: {item.Quantidade} {nomeItem}{Environment.NewLine}");
                }

                // Encerra o combate sem reentrar em MoverPara: o monstro não ressuscita no local
                // nem o jogador ganha cura gratuita. Ele só volta se o jogador
                // sair e voltar a este local.
                MonstroAtual = null;

                mensagens.Add(Environment.NewLine);
            }
            return mensagens;
        }

        /*  Turno completo de combate por turnos: ataque do jogador seguido,
            se o monstro sobreviver, do contra-ataque (com chance de falha).
            Se o jogador morre, é teletransportado para a Casa. */
        public List<string> Atacar(Arma armaAtual)
        {
            var mensagens = AtaqueDoJogador(armaAtual);

            // Monstro ainda vivo: contra-ataca (10% de chance de falha)
            if (MonstroAtual != null)
                mensagens.AddRange(AtaqueDoMonstro());

            return mensagens;
        }

        /*  Ataque do monstro contra o jogador (10% de falha, dano 0..DanoMaximo).
            Usado pelo contra-ataque do combate por turnos (Atacar) e pelo
            combate espacial (quando a apresentação avisa que o inimigo está em
            posição de atacar). Se o jogador morre, a regra de morte é aplicada
            (Casa, ouro, respawn). Sem monstro em combate, não faz nada. */
        public List<string> AtaqueDoMonstro()
        {
            var mensagens = new List<string>();

            if (MonstroAtual == null)
                return mensagens;

            bool monstroFalhou = GeradorNumeroAleatorio.NumeroEntre(1, 100) <= CHANCE_EVENTO_COMBATE;
            int danoAoJogador = monstroFalhou ? 0 : GeradorNumeroAleatorio.NumeroEntre(0, MonstroAtual.DanoMaximo);

            if (monstroFalhou)
                mensagens.Add($"O(A) {MonstroAtual.Nome} errou o ataque.{Environment.NewLine}");
            else
                mensagens.Add($"O(A) {MonstroAtual.Nome} causou a você {danoAoJogador} pontos de dano.{Environment.NewLine}");

            Jogador.VidaAtual -= danoAoJogador;

            if (Jogador.VidaAtual <= 0)
            {
                MorreuNaMaoDo(MonstroAtual, mensagens);
            }

            return mensagens;
        }

        /*  Usa uma poção de cura: com a vida cheia a poção não é consumida.
            Usar a poção não consome o turno: o monstro não contra-ataca. */
        public List<string> UsarPocao(PocaoCura pocao)
        {
            var mensagens = new List<string>();

            // Com a vida cheia a poção não é consumida e o turno não é gasto
            // (o monstro não contra-ataca)
            if (Jogador.VidaAtual >= Jogador.VidaMaximaEfetiva)
            {
                mensagens.Add($"Sua vida já está cheia; a {pocao.Nome} não foi utilizada.{Environment.NewLine}");
                return mensagens;
            }

            // Aplica cura sem exceder a vida máxima
            Jogador.VidaAtual = Math.Min(Jogador.VidaAtual + pocao.QtdCura, Jogador.VidaMaximaEfetiva);

            // Remove a poção do inventário
            var itemPocao = Jogador.Inventario.FirstOrDefault(ii => ii.Detalhes.ID == pocao.ID);
            if (itemPocao != null)
                itemPocao.Quantidade--;

            mensagens.Add($"Você bebeu uma {pocao.Nome}{Environment.NewLine}");

            return mensagens;
        }

        // Única rotina de morte: teletransporta o jogador para a casa e cobra a
        // penalidade de ouro (perde 25% do ouro, arredondado para baixo)
        private void MorreuNaMaoDo(Monstro monstro, List<string> mensagens)
        {
            int ouroPerdido = Jogador.Ouro / 4;

            mensagens.Add($"O(A) {monstro.Nome} matou você.{Environment.NewLine}");
            if (ouroPerdido > 0)
                mensagens.Add($"Você perdeu {ouroPerdido} de ouro por conta da derrota.{Environment.NewLine}");

            Jogador.Ouro -= ouroPerdido;
            MonstroAtual = null;

            mensagens.AddRange(MoverPara(Mundo.LocalPorID(Mundo.LOCAL_ID_CASA)));
        }

        // Concentra o ganho de XP e avisa quando o jogador sobe de level
        private void GanhaExperiencia(int quantidade, List<string> mensagens)
        {
            int levelAntes = Jogador.Level;
            Jogador.PontosExperiencia += quantidade;

            if (Jogador.Level > levelAntes)
                mensagens.Add($"Você subiu para o level {Jogador.Level}!{Environment.NewLine}");
        }
    }
}
