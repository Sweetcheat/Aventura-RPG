/*
 Este é um jogo de RPG simples desenvolvido por
 Lucas Barbosa (FATEC ITU - 2º SEMESTRE GTI)
 O código está todo comentado, com explicações de como o código funciona.
 Esse é um projeto para a disciplina de Linguagem de Programação C# da
 Professora Angelina Melaré.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Motor;

namespace Aventura_RPG
{
    public partial class AventuraRPG : Form
    {
        private Jogador _jogador;
        private Monstro _monstroAtual;

        // Chance (em 100) de evento de combate: crítico do jogador (dano x2) ou falha do monstro (dano zero)
        private const int CHANCE_EVENTO_COMBATE = 10;

        // Limite de linhas mantidas no log de mensagens (o RichTextBox tem teto de 32.767 caracteres)
        private const int MAX_LINHAS_LOG = 200;

        public AventuraRPG()
        {
            InitializeComponent();

            _jogador = new Jogador(15, 15, 0, 0);
            // A espada inicial é adicionada antes do primeiro MoverPara, para que
            // o inventário já apareça com ela ao iniciar o jogo
            _jogador.Inventario.Add(new InventarioItem(Mundo.ItemPorID(Mundo.ITEM_ID_ESPADA_ENFERRUJADA), 1));
            MoverPara(Mundo.LocalPorID(Mundo.LOCAL_ID_CASA));

            AtualizaStatsDoJogador();
        }

        // Botões de movimento
        private void buttonNorte_Click(object sender, EventArgs e) => MoverPara(_jogador.LocalAtual.LocalParaNorte);
        private void buttonLeste_Click(object sender, EventArgs e) => MoverPara(_jogador.LocalAtual.LocalParaLeste);
        private void buttonSul_Click(object sender, EventArgs e)   => MoverPara(_jogador.LocalAtual.LocalParaSul);
        private void buttonOeste_Click(object sender, EventArgs e) => MoverPara(_jogador.LocalAtual.LocalParaOeste);

        private void MoverPara(Local novoLocal)
        {
            // Se o local exige um item que o jogador não tem, bloqueia a entrada
            if (!_jogador.TemItemNecessarioParaEntrarNesteLocal(novoLocal))
            {
                AdicionaMensagem($"Você precisa ter um {novoLocal.ItemNecessarioEntrar.Nome} para acessar este local.{Environment.NewLine}");
                return;
            }

            // A cura e o respawn de monstro só ocorrem em mudanças reais de local.
            // Ao encerrar um combate, o form não reentra em MoverPara (ver buttonUsarArma_Click),
            // então o monstro derrotado só volta quando o jogador sair e voltar a este local.
            bool mudouDeLocal = !ReferenceEquals(novoLocal, _jogador.LocalAtual);
            _jogador.LocalAtual = novoLocal;

            // Mostra/esconde botões de movimento disponíveis (bloqueados durante o combate)
            AtualizaBotoesMovimento();

            // Mostra nome e descrição do local
            richTextBoxLocal.Text = $"{novoLocal.Nome}{Environment.NewLine}{novoLocal.Descricao}{Environment.NewLine}";

            // Apenas a Casa cura a vida (total): nos demais locais o jogador
            // mantém a vida que tinha (cura em campo só com poção)
            if (mudouDeLocal && novoLocal.ID == Mundo.LOCAL_ID_CASA)
            {
                _jogador.VidaAtual = _jogador.VidaMaximaEfetiva;
                lblVida.Text = $"{_jogador.VidaAtual}/{_jogador.VidaMaximaEfetiva}";
            }

            // Lógica de quest do local
            if (novoLocal.QuestDisponivelAqui != null)
            {
                bool jogadorJaTemQuest    = _jogador.TemEstaQuest(novoLocal.QuestDisponivelAqui);
                bool jogadorJaCompletouQuest = _jogador.QuestEstaCompletada(novoLocal.QuestDisponivelAqui);

                if (jogadorJaTemQuest)
                {
                    if (!jogadorJaCompletouQuest && _jogador.TemTodosItensParaCompletarQuest(novoLocal.QuestDisponivelAqui))
                    {
                        AdicionaMensagem(Environment.NewLine);
                        AdicionaMensagem($"Você completou '{novoLocal.QuestDisponivelAqui.Nome}'.{Environment.NewLine}");

                        _jogador.RemovaItensDeQuestCompletada(novoLocal.QuestDisponivelAqui);

                        AdicionaMensagem($"Você recebe:{Environment.NewLine}");
                        AdicionaMensagem($"{novoLocal.QuestDisponivelAqui.PontosExperienciaRecompensa} pontos de experiência{Environment.NewLine}");
                        AdicionaMensagem($"{novoLocal.QuestDisponivelAqui.OuroRecompensa} ouro{Environment.NewLine}");
                        AdicionaMensagem($"{novoLocal.QuestDisponivelAqui.ItemRecompensa.Nome}{Environment.NewLine}");
                        AdicionaMensagem(Environment.NewLine);

                        GanhaExperiencia(novoLocal.QuestDisponivelAqui.PontosExperienciaRecompensa);
                        _jogador.Ouro              += novoLocal.QuestDisponivelAqui.OuroRecompensa;

                        _jogador.AdicioneItemAoInventario(novoLocal.QuestDisponivelAqui.ItemRecompensa);
                        _jogador.MarqueQuestCompletada(novoLocal.QuestDisponivelAqui);
                    }
                }
                else
                {
                    // Jogador ainda não tem a quest: entrega ela
                    AdicionaMensagem(Environment.NewLine);
                    AdicionaMensagem($"Você recebeu a quest: {novoLocal.QuestDisponivelAqui.Nome}.{Environment.NewLine}");
                    AdicionaMensagem($"{novoLocal.QuestDisponivelAqui.Descricao}{Environment.NewLine}");
                    AdicionaMensagem($"Para completá-la, retorne com:{Environment.NewLine}");

                    foreach (var qci in novoLocal.QuestDisponivelAqui.QuestCompletadaItem)
                    {
                        var nomeItem = qci.Quantidade == 1 ? qci.Detalhes.Nome : qci.Detalhes.NomePlural;
                        AdicionaMensagem($"{qci.Quantidade} {nomeItem}{Environment.NewLine}");
                    }

                    AdicionaMensagem(Environment.NewLine);
                    _jogador.Quests.Add(new JogadorQuest(novoLocal.QuestDisponivelAqui));
                }
            }

            // Recompensa especial: o Porrete é concedido uma única vez quando o jogador
            // completa as duas quests, independentemente da ordem em que foram feitas
            var questJardimAlquimistas = Mundo.QuestPorID(Mundo.QUEST_ID_LIMPAR_JARDIM_DOS_ALQUIMISTAS);
            var questAreaCamponeses    = Mundo.QuestPorID(Mundo.QUEST_ID_LIMPAR_AREA_DOS_CAMPONESES);

            if (!_jogador.RecebeuRecompensaPorrete
                && _jogador.QuestEstaCompletada(questJardimAlquimistas)
                && _jogador.QuestEstaCompletada(questAreaCamponeses))
            {
                AdicionaMensagem(Environment.NewLine);
                AdicionaMensagem($"Você completou as duas quests e recebeu um Porrete!{Environment.NewLine}");

                _jogador.AdicioneItemAoInventario(Mundo.ItemPorID(Mundo.ITEM_ID_PORRETE));
                _jogador.RecebeuRecompensaPorrete = true;
            }

            // Lógica de monstro do local: o monstro é recriado apenas quando o jogador
            // realmente muda de local (sai e volta), evitando o respawn instantâneo
            // logo após a derrota.
            if (mudouDeLocal)
            {
                if (novoLocal.MonstroVivoAqui != null)
                {
                    AdicionaMensagem($"Você vê um(a) {novoLocal.MonstroVivoAqui.Nome}{Environment.NewLine}");

                    // Instancia um novo monstro a partir dos dados padrão do Mundo
                    var monstroNormal = Mundo.MonstroPorID(novoLocal.MonstroVivoAqui.ID);
                    _monstroAtual = new Monstro(
                        monstroNormal.ID, monstroNormal.Nome, monstroNormal.DanoMaximo,
                        monstroNormal.PontosExperienciaRecompensa, monstroNormal.OuroRecompensa,
                        monstroNormal.VidaAtual, monstroNormal.VidaMaxima);

                    foreach (var itemLoot in monstroNormal.LootTable)
                        _monstroAtual.LootTable.Add(itemLoot);
                }
                else
                {
                    _monstroAtual = null;
                }
            }

            AtualizaStatsDoJogador();
            AtualizaListaInventarioNoMenu();
            AtualizaListaQuestNoMenu();
            AtualizaListaArmaNoMenu();
            AtualizaListaPocaoNoMenu();
            AtualizaVisibilidadeCombate();
            RolarMensagensParaFim();
        }

        private void AtualizaStatsDoJogador()
        {
            lblVida.Text        = $"{_jogador.VidaAtual}/{_jogador.VidaMaximaEfetiva}";
            lblOuro.Text        = _jogador.Ouro.ToString();
            lblExperiencia.Text = _jogador.PontosExperiencia.ToString();
            lblLevel.Text       = _jogador.Level.ToString();
        }

        private void AtualizaListaInventarioNoMenu()
        {
            dataGridViewInventario.RowHeadersVisible = false;
            dataGridViewInventario.ColumnCount = 2;
            dataGridViewInventario.Columns[0].Name  = "Nome";
            dataGridViewInventario.Columns[0].Width = 197;
            dataGridViewInventario.Columns[1].Name  = "Quantidade";
            dataGridViewInventario.Rows.Clear();

            foreach (var item in _jogador.Inventario)
            {
                if (item.Quantidade > 0)
                    dataGridViewInventario.Rows.Add(item.Detalhes.Nome, item.Quantidade.ToString());
            }
        }

        private void AtualizaListaQuestNoMenu()
        {
            dataGridViewQuests.RowHeadersVisible = false;
            dataGridViewQuests.ColumnCount = 2;
            dataGridViewQuests.Columns[0].Name  = "Nome";
            dataGridViewQuests.Columns[0].Width = 197;
            dataGridViewQuests.Columns[1].Name  = "Completada?";
            dataGridViewQuests.Rows.Clear();

            foreach (var jq in _jogador.Quests)
                dataGridViewQuests.Rows.Add(jq.Detalhes.Nome, jq.Completado ? "✓" : "–");
        }

        private void AtualizaListaArmaNoMenu()
        {
            var armas = _jogador.Inventario
                .Where(ii => ii.Detalhes is Arma && ii.Quantidade > 0)
                .Select(ii => (Arma)ii.Detalhes)
                .ToList();

            if (armas.Count > 0)
            {
                comboBoxArmas.DataSource    = armas;
                comboBoxArmas.DisplayMember = "Nome";
                comboBoxArmas.ValueMember   = "ID";
                comboBoxArmas.SelectedIndex = 0;
            }
            else
            {
                comboBoxArmas.DataSource = null;
            }
            // A visibilidade dos controles de combate é decidida por AtualizaVisibilidadeCombate()
        }

        private void AtualizaListaPocaoNoMenu()
        {
            var pocoes = _jogador.Inventario
                .Where(ii => ii.Detalhes is PocaoCura && ii.Quantidade > 0)
                .Select(ii => (PocaoCura)ii.Detalhes)
                .ToList();

            if (pocoes.Count > 0)
            {
                comboBoxPocoes.DataSource    = pocoes;
                comboBoxPocoes.DisplayMember = "Nome";
                comboBoxPocoes.ValueMember   = "ID";
                comboBoxPocoes.SelectedIndex = 0;
            }
            else
            {
                comboBoxPocoes.DataSource = null;
            }
            // A visibilidade dos controles de combate é decidida por AtualizaVisibilidadeCombate()
        }

        private void buttonUsarArma_Click(object sender, EventArgs e)
        {
            var armaAtual    = (Arma)comboBoxArmas.SelectedItem;
            // Bônus de level (a partir do level 2): +1 de dano, aplicado antes do crítico
            // Dano mínimo de 1: a arma sempre acerta (o bônus de level é somado depois)
            int danoAoMonstro = Math.Max(1, GeradorNumeroAleatorio.NumeroEntre(armaAtual.DanoMinimo, armaAtual.DanoMaximo)) + (_jogador.Level - 1);

            // Crítico (10%): dobra o dano final, desde que seja maior que zero
            bool critico = danoAoMonstro > 0 && GeradorNumeroAleatorio.NumeroEntre(1, 100) <= CHANCE_EVENTO_COMBATE;
            if (critico)
                danoAoMonstro *= 2;

            _monstroAtual.VidaAtual -= danoAoMonstro;
            AdicionaMensagem($"Você acertou o(a) {_monstroAtual.Nome} e causou {danoAoMonstro} ponto(s) de dano{(critico ? " (CRÍTICO!)" : "")}.{Environment.NewLine}");

            if (_monstroAtual.VidaAtual <= 0)
            {
                AdicionaMensagem(Environment.NewLine);
                AdicionaMensagem($"Você derrotou o(a) {_monstroAtual.Nome}{Environment.NewLine}");

                GanhaExperiencia(_monstroAtual.PontosExperienciaRecompensa);
                AdicionaMensagem($"Você recebe {_monstroAtual.PontosExperienciaRecompensa} pontos de experiência.{Environment.NewLine}");

                _jogador.Ouro             += _monstroAtual.OuroRecompensa;
                AdicionaMensagem($"Você recebe {_monstroAtual.OuroRecompensa} de ouro.{Environment.NewLine}");

                // Sorteia o loot do monstro
                var itensSaqueados = _monstroAtual.LootTable
                    .Where(il => GeradorNumeroAleatorio.NumeroEntre(1, 100) <= il.PorcentagemDrop)
                    .Select(il => new InventarioItem(il.Detalhes, 1))
                    .ToList();

                // Garante ao menos o item comum se nada caiu
                if (itensSaqueados.Count == 0)
                {
                    itensSaqueados = _monstroAtual.LootTable
                        .Where(il => il.EItemComum)
                        .Select(il => new InventarioItem(il.Detalhes, 1))
                        .ToList();
                }

                foreach (var item in itensSaqueados)
                {
                    _jogador.AdicioneItemAoInventario(item.Detalhes);
                    var nomeItem = item.Quantidade == 1 ? item.Detalhes.Nome : item.Detalhes.NomePlural;
                    AdicionaMensagem($"Seu saque: {item.Quantidade} {nomeItem}{Environment.NewLine}");
                }

                // Encerra o combate sem reentrar em MoverPara: o monstro não ressuscita no local
                // nem o jogador ganha cura gratuita. Ele só volta se o jogador
                // sair e voltar a este local.
                _monstroAtual = null;

                AtualizaStatsDoJogador();
                AtualizaListaInventarioNoMenu();
                AtualizaListaArmaNoMenu();
                AtualizaListaPocaoNoMenu();
                AtualizaVisibilidadeCombate();

                AdicionaMensagem(Environment.NewLine);
            }
            else
            {
                // Monstro ainda vivo: contra-ataca (10% de chance de falhar)
                bool monstroFalhou = GeradorNumeroAleatorio.NumeroEntre(1, 100) <= CHANCE_EVENTO_COMBATE;
                int danoAoJogador = monstroFalhou ? 0 : GeradorNumeroAleatorio.NumeroEntre(0, _monstroAtual.DanoMaximo);

                if (monstroFalhou)
                    AdicionaMensagem($"O(A) {_monstroAtual.Nome} errou o ataque.{Environment.NewLine}");
                else
                    AdicionaMensagem($"O(A) {_monstroAtual.Nome} causou a você {danoAoJogador} pontos de dano.{Environment.NewLine}");

                _jogador.VidaAtual -= danoAoJogador;
                lblVida.Text = $"{_jogador.VidaAtual}/{_jogador.VidaMaximaEfetiva}";

                if (_jogador.VidaAtual <= 0)
                {
                    MorreuNaMaoDo(_monstroAtual);
                }
            }

            AtualizaBotoesMovimento();
            RolarMensagensParaFim();
        }

        private void buttonUsarPocao_Click(object sender, EventArgs e)
        {
            var pocao = (PocaoCura)comboBoxPocoes.SelectedItem;

            // Com a vida cheia a poção não é consumida e o turno não é gasto
            // (o monstro não contra-ataca)
            if (_jogador.VidaAtual >= _jogador.VidaMaximaEfetiva)
            {
                AdicionaMensagem($"Sua vida já está cheia; a {pocao.Nome} não foi utilizada.{Environment.NewLine}");
                RolarMensagensParaFim();
                return;
            }

            // Aplica cura sem exceder a vida máxima
            _jogador.VidaAtual = Math.Min(_jogador.VidaAtual + pocao.QtdCura, _jogador.VidaMaximaEfetiva);

            // Remove a poção do inventário
            var itemPocao = _jogador.Inventario.FirstOrDefault(ii => ii.Detalhes.ID == pocao.ID);
            if (itemPocao != null)
                itemPocao.Quantidade--;

            AdicionaMensagem($"Você bebeu uma {pocao.Nome}{Environment.NewLine}");

            // Usar a poção não consome o turno: o monstro não contra-ataca e o
            // jogador continua com o turno disponível (pode beber outra poção ou atacar)

            lblVida.Text = $"{_jogador.VidaAtual}/{_jogador.VidaMaximaEfetiva}";
            AtualizaListaInventarioNoMenu();
            AtualizaListaPocaoNoMenu();
            AtualizaVisibilidadeCombate();
            RolarMensagensParaFim();
        }

        // Única rotina de morte: teletransporta o jogador para a casa e cobra a
        // penalidade de ouro (perde 25% do ouro, arredondado para baixo)
        private void MorreuNaMaoDo(Monstro monstro)
        {
            int ouroPerdido = _jogador.Ouro / 4;

            AdicionaMensagem($"O(A) {monstro.Nome} matou você.{Environment.NewLine}");
            if (ouroPerdido > 0)
                AdicionaMensagem($"Você perdeu {ouroPerdido} de ouro por conta da derrota.{Environment.NewLine}");

            _jogador.Ouro -= ouroPerdido;
            _monstroAtual = null;

            MoverPara(Mundo.LocalPorID(Mundo.LOCAL_ID_CASA));
        }

        // Concentra o ganho de XP e avisa quando o jogador sobe de level
        private void GanhaExperiencia(int quantidade)
        {
            int levelAntes = _jogador.Level;
            _jogador.PontosExperiencia += quantidade;

            if (_jogador.Level > levelAntes)
                AdicionaMensagem($"Você subiu para o level {_jogador.Level}!{Environment.NewLine}");
        }

        // Mostra/esconde os botões de movimento: só estão disponíveis quando não há
        // monstro vivo no local (durante o combate o jogador precisa encerrar a luta)
        private void AtualizaBotoesMovimento()
        {
            bool semMonstro = _monstroAtual == null;
            var local = _jogador.LocalAtual;

            buttonNorte.Visible = semMonstro && local.LocalParaNorte != null;
            buttonLeste.Visible = semMonstro && local.LocalParaLeste != null;
            buttonSul.Visible   = semMonstro && local.LocalParaSul   != null;
            buttonOeste.Visible = semMonstro && local.LocalParaOeste != null;
        }

        // Única fonte de visibilidade dos controles de combate:
        // eles só aparecem quando há monstro vivo E o jogador tem o item no inventário.
        private void AtualizaVisibilidadeCombate()
        {
            bool temArma  = _jogador.Inventario.Any(ii => ii.Detalhes is Arma && ii.Quantidade > 0);
            bool temPocao = _jogador.Inventario.Any(ii => ii.Detalhes is PocaoCura && ii.Quantidade > 0);

            comboBoxArmas.Visible    = _monstroAtual != null && temArma;
            buttonUsarArma.Visible   = _monstroAtual != null && temArma;
            comboBoxPocoes.Visible   = _monstroAtual != null && temPocao;
            buttonUsarPocao.Visible  = _monstroAtual != null && temPocao;
        }

        // Mantém só as últimas MAX_LINHAS_LOG linhas do log: o RichTextBox tem limite
        // de 32.767 caracteres (crash ao exceder) e a performance degrada com o texto crescendo
        private void AdicionaMensagem(string mensagem)
        {
            richTextBoxMensagens.Text += mensagem;

            var linhas = richTextBoxMensagens.Text.Split('\n');
            if (linhas.Length > MAX_LINHAS_LOG)
                richTextBoxMensagens.Text = string.Join("\n", linhas.Skip(linhas.Length - MAX_LINHAS_LOG).ToArray());
        }

        private void RolarMensagensParaFim()
        {
            richTextBoxMensagens.SelectionStart = richTextBoxMensagens.Text.Length;
            richTextBoxMensagens.ScrollToCaret();
        }
    }
}
