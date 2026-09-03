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

        public AventuraRPG()
        {
            InitializeComponent();

            _jogador = new Jogador(15, 15, 0, 0);
            MoverPara(Mundo.LocalPorID(Mundo.LOCAL_ID_CASA));
            _jogador.Inventario.Add(new InventarioItem(Mundo.ItemPorID(Mundo.ITEM_ID_ESPADA_ENFERRUJADA), 1));

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
                richTextBoxMensagens.Text += $"Você precisa ter um {novoLocal.ItemNecessarioEntrar.Nome} para acessar este local.{Environment.NewLine}";
                return;
            }

            _jogador.LocalAtual = novoLocal;

            // Mostra/esconde botões de movimento disponíveis
            buttonNorte.Visible = novoLocal.LocalParaNorte != null;
            buttonLeste.Visible = novoLocal.LocalParaLeste != null;
            buttonSul.Visible   = novoLocal.LocalParaSul   != null;
            buttonOeste.Visible = novoLocal.LocalParaOeste != null;

            // Mostra nome e descrição do local
            richTextBoxLocal.Text = $"{novoLocal.Nome}{Environment.NewLine}{novoLocal.Descricao}{Environment.NewLine}";

            // Cura completamente o jogador ao mudar de local
            _jogador.VidaAtual = _jogador.VidaMaxima;
            lblVida.Text = _jogador.VidaAtual.ToString();

            // Lógica de quest do local
            if (novoLocal.QuestDisponivelAqui != null)
            {
                bool jogadorJaTemQuest    = _jogador.TemEstaQuest(novoLocal.QuestDisponivelAqui);
                bool jogadorJaCompletouQuest = _jogador.QuestEstaCompletada(novoLocal.QuestDisponivelAqui);

                if (jogadorJaTemQuest)
                {
                    if (!jogadorJaCompletouQuest && _jogador.TemTodosItensParaCompletarQuest(novoLocal.QuestDisponivelAqui))
                    {
                        richTextBoxMensagens.Text += Environment.NewLine;
                        richTextBoxMensagens.Text += $"Você completou '{novoLocal.QuestDisponivelAqui.Nome}'.{Environment.NewLine}";

                        _jogador.RemovaItensDeQuestCompletada(novoLocal.QuestDisponivelAqui);

                        richTextBoxMensagens.Text += $"Você recebe:{Environment.NewLine}";
                        richTextBoxMensagens.Text += $"{novoLocal.QuestDisponivelAqui.PontosExperienciaRecompensa} pontos de experiência{Environment.NewLine}";
                        richTextBoxMensagens.Text += $"{novoLocal.QuestDisponivelAqui.OuroRecompensa} ouro{Environment.NewLine}";
                        richTextBoxMensagens.Text += $"{novoLocal.QuestDisponivelAqui.ItemRecompensa.Nome}{Environment.NewLine}";
                        richTextBoxMensagens.Text += Environment.NewLine;

                        _jogador.PontosExperiencia += novoLocal.QuestDisponivelAqui.PontosExperienciaRecompensa;
                        _jogador.Ouro              += novoLocal.QuestDisponivelAqui.OuroRecompensa;

                        _jogador.AdicioneItemAoInventario(novoLocal.QuestDisponivelAqui.ItemRecompensa);
                        _jogador.MarqueQuestCompletada(novoLocal.QuestDisponivelAqui);
                    }
                }
                else
                {
                    // Jogador ainda não tem a quest: entrega ela
                    richTextBoxMensagens.Text += Environment.NewLine;
                    richTextBoxMensagens.Text += $"Você recebeu a quest: {novoLocal.QuestDisponivelAqui.Nome}.{Environment.NewLine}";
                    richTextBoxMensagens.Text += $"{novoLocal.QuestDisponivelAqui.Descricao}{Environment.NewLine}";
                    richTextBoxMensagens.Text += $"Para completá-la, retorne com:{Environment.NewLine}";

                    foreach (var qci in novoLocal.QuestDisponivelAqui.QuestCompletadaItem)
                    {
                        var nomeItem = qci.Quantidade == 1 ? qci.Detalhes.Nome : qci.Detalhes.NomePlural;
                        richTextBoxMensagens.Text += $"{qci.Quantidade} {nomeItem}{Environment.NewLine}";
                    }

                    richTextBoxMensagens.Text += Environment.NewLine;
                    _jogador.Quests.Add(new JogadorQuest(novoLocal.QuestDisponivelAqui));
                }
            }

            // Lógica de monstro do local
            if (novoLocal.MonstroVivoAqui != null)
            {
                richTextBoxMensagens.Text += $"Você vê um(a) {novoLocal.MonstroVivoAqui.Nome}{Environment.NewLine}";

                // Instancia um novo monstro a partir dos dados padrão do Mundo
                var monstroNormal = Mundo.MonstroPorID(novoLocal.MonstroVivoAqui.ID);
                _monstroAtual = new Monstro(
                    monstroNormal.ID, monstroNormal.Nome, monstroNormal.DanoMaximo,
                    monstroNormal.PontosExperienciaRecompensa, monstroNormal.OuroRecompensa,
                    monstroNormal.VidaAtual, monstroNormal.VidaMaxima);

                foreach (var itemLoot in monstroNormal.LootTable)
                    _monstroAtual.LootTable.Add(itemLoot);

                comboBoxArmas.Visible   = true;
                comboBoxPoçoes.Visible  = true;
                buttonUsarArma.Visible  = true;
                buttonUsarPoçao.Visible = true;
            }
            else
            {
                _monstroAtual = null;

                comboBoxArmas.Visible   = false;
                comboBoxPoçoes.Visible  = false;
                buttonUsarArma.Visible  = false;
                buttonUsarPoçao.Visible = false;
            }

            AtualizaStatsDoJogador();
            AtualizaListaInventarioNoMenu();
            AtualizaListaQuestNoMenu();
            AtualizaListaArmaNoMenu();
            AtualizaListaPocaoNoMenu();
            AutoScroll();
        }

        private void AtualizaStatsDoJogador()
        {
            lblVida.Text        = _jogador.VidaAtual.ToString();
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
                dataGridViewQuests.Rows.Add(jq.Detalhes.Nome, jq.Completado.ToString());
        }

        private void AtualizaListaArmaNoMenu()
        {
            var armas = _jogador.Inventario
                .Where(ii => ii.Detalhes is Arma && ii.Quantidade > 0)
                .Select(ii => (Arma)ii.Detalhes)
                .ToList();

            if (armas.Count == 0)
            {
                comboBoxArmas.Visible  = false;
                buttonUsarArma.Visible = false;
            }
            else
            {
                comboBoxArmas.DataSource    = armas;
                comboBoxArmas.DisplayMember = "Nome";
                comboBoxArmas.ValueMember   = "ID";
                comboBoxArmas.SelectedIndex = 0;
            }
        }

        private void AtualizaListaPocaoNoMenu()
        {
            var pocoes = _jogador.Inventario
                .Where(ii => ii.Detalhes is PocaoCura && ii.Quantidade > 0)
                .Select(ii => (PocaoCura)ii.Detalhes)
                .ToList();

            if (pocoes.Count == 0)
            {
                comboBoxPoçoes.Visible  = false;
                buttonUsarPoçao.Visible = false;
            }
            else
            {
                comboBoxPoçoes.DataSource    = pocoes;
                comboBoxPoçoes.DisplayMember = "Nome";
                comboBoxPoçoes.ValueMember   = "ID";
                comboBoxPoçoes.SelectedIndex = 0;
            }
        }

        private void buttonUsarArma_Click(object sender, EventArgs e)
        {
            var armaAtual    = (Arma)comboBoxArmas.SelectedItem;
            int danoAoMonstro = GeradorNumeroAleatorio.NumeroEntre(armaAtual.DanoMinimo, armaAtual.DanoMaximo);

            _monstroAtual.VidaAtual -= danoAoMonstro;
            richTextBoxMensagens.Text += $"Você acertou o(a) {_monstroAtual.Nome} e causou {danoAoMonstro} ponto(s) de dano.{Environment.NewLine}";

            if (_monstroAtual.VidaAtual <= 0)
            {
                richTextBoxMensagens.Text += Environment.NewLine;
                richTextBoxMensagens.Text += $"Você derrotou o(a) {_monstroAtual.Nome}{Environment.NewLine}";

                _jogador.PontosExperiencia += _monstroAtual.PontosExperienciaRecompensa;
                richTextBoxMensagens.Text  += $"Você recebe {_monstroAtual.PontosExperienciaRecompensa} pontos de experiência.{Environment.NewLine}";

                _jogador.Ouro             += _monstroAtual.OuroRecompensa;
                richTextBoxMensagens.Text  += $"Você recebe {_monstroAtual.OuroRecompensa} de ouro.{Environment.NewLine}";

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
                    richTextBoxMensagens.Text += $"Seu saque: {item.Quantidade} {nomeItem}{Environment.NewLine}";
                }

                AtualizaStatsDoJogador();
                AtualizaListaInventarioNoMenu();
                AtualizaListaArmaNoMenu();
                AtualizaListaPocaoNoMenu();

                richTextBoxMensagens.Text += Environment.NewLine;
                MoverPara(_jogador.LocalAtual);
            }
            else
            {
                // Monstro ainda vivo: contra-ataca
                int danoAoJogador = GeradorNumeroAleatorio.NumeroEntre(0, _monstroAtual.DanoMaximo);
                richTextBoxMensagens.Text += $"O(A) {_monstroAtual.Nome} causou a você {danoAoJogador} pontos de dano.{Environment.NewLine}";

                _jogador.VidaAtual -= danoAoJogador;
                lblVida.Text = _jogador.VidaAtual.ToString();

                if (_jogador.VidaAtual <= 0)
                {
                    richTextBoxMensagens.Text += $"O(A) {_monstroAtual.Nome} matou você.{Environment.NewLine}";
                    MoverPara(Mundo.LocalPorID(Mundo.LOCAL_ID_CASA));
                }
            }

            AutoScroll();
        }

        private void buttonUsarPoçao_Click(object sender, EventArgs e)
        {
            var pocao = (PocaoCura)comboBoxPoçoes.SelectedItem;

            // Aplica cura sem exceder a vida máxima
            _jogador.VidaAtual = Math.Min(_jogador.VidaAtual + pocao.QtdCura, _jogador.VidaMaxima);

            // Remove a poção do inventário
            var itemPocao = _jogador.Inventario.FirstOrDefault(ii => ii.Detalhes.ID == pocao.ID);
            if (itemPocao != null)
                itemPocao.Quantidade--;

            richTextBoxMensagens.Text += $"Você bebeu uma {pocao.Nome}{Environment.NewLine}";

            // Monstro contra-ataca após o uso da poção
            int danoAoJogador = GeradorNumeroAleatorio.NumeroEntre(0, _monstroAtual.DanoMaximo);
            richTextBoxMensagens.Text += $"O(A) {_monstroAtual.Nome} causou a você {danoAoJogador} pontos de dano.{Environment.NewLine}";

            _jogador.VidaAtual -= danoAoJogador;

            if (_jogador.VidaAtual <= 0)
            {
                richTextBoxMensagens.Text += $"O(A) {_monstroAtual.Nome} matou você.{Environment.NewLine}";
                MoverPara(Mundo.LocalPorID(Mundo.LOCAL_ID_CASA));
            }

            lblVida.Text = _jogador.VidaAtual.ToString();
            AtualizaListaInventarioNoMenu();
            AtualizaListaPocaoNoMenu();
            AutoScroll();
        }

        private void AutoScroll()
        {
            richTextBoxMensagens.SelectionStart = richTextBoxMensagens.Text.Length;
            richTextBoxMensagens.ScrollToCaret();
        }
    }
}
