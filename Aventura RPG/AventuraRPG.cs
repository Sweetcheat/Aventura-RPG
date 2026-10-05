/*
 Este é um jogo de RPG simples desenvolvido por
 Lucas Barbosa (FATEC ITU - 2º SEMESTRE GTI)
 O código está todo comentado, com explicações de como o código funciona.
 Esse é um projeto para a disciplina de Linguagem de Programação C# da
 Professora Angelina Melaré.
 */

using System;
using System.Linq;
using System.Windows.Forms;
using Motor;

namespace Aventura_RPG
{
    public partial class AventuraRPG : Form
    {
        // Estado e regras do jogo vivem na Partida (Motor). O form é
        // responsável apenas pela apresentação: chama os métodos do jogo e
        // exibe as mensagens retornadas e o estado atual.
        private Partida _partida;

        // Limite de linhas mantidas no log de mensagens (o RichTextBox tem teto de 32.767 caracteres)
        private const int MAX_LINHAS_LOG = 200;

        public AventuraRPG()
        {
            InitializeComponent();

            _partida = new Partida();
            MoverPara(Mundo.LocalPorID(Mundo.LOCAL_ID_CASA));
        }

        // Botões de movimento
        private void buttonNorte_Click(object sender, EventArgs e) => MoverPara(_partida.Jogador.LocalAtual.LocalParaNorte);
        private void buttonLeste_Click(object sender, EventArgs e) => MoverPara(_partida.Jogador.LocalAtual.LocalParaLeste);
        private void buttonSul_Click(object sender, EventArgs e)   => MoverPara(_partida.Jogador.LocalAtual.LocalParaSul);
        private void buttonOeste_Click(object sender, EventArgs e) => MoverPara(_partida.Jogador.LocalAtual.LocalParaOeste);

        // Envolve Partida.MoverPara: registra as mensagens retornadas e atualiza a
        // tela. Se a entrada for bloqueada (item em falta), apenas a mensagem é
        // exibida e a tela não é atualizada.
        private void MoverPara(Local novoLocal)
        {
            var localAnterior = _partida.Jogador.LocalAtual;

            foreach (var mensagem in _partida.MoverPara(novoLocal))
                AdicionaMensagem(mensagem);

            // Entrada bloqueada: o jogador não se moveu, nada da tela é atualizado
            if (ReferenceEquals(_partida.Jogador.LocalAtual, localAnterior))
                return;

            // Mostra nome e descrição do local
            richTextBoxLocal.Text = $"{_partida.Jogador.LocalAtual.Nome}{Environment.NewLine}{_partida.Jogador.LocalAtual.Descricao}{Environment.NewLine}";

            AtualizaStatsDoJogador();
            AtualizaListaInventarioNoMenu();
            AtualizaListaQuestNoMenu();
            AtualizaListaArmaNoMenu();
            AtualizaListaPocaoNoMenu();
            AtualizaBotoesMovimento();
            AtualizaVisibilidadeCombate();
            RolarMensagensParaFim();
        }

        private void AtualizaStatsDoJogador()
        {
            lblVida.Text        = $"{_partida.Jogador.VidaAtual}/{_partida.Jogador.VidaMaximaEfetiva}";
            lblOuro.Text        = _partida.Jogador.Ouro.ToString();
            lblExperiencia.Text = _partida.Jogador.PontosExperiencia.ToString();
            lblLevel.Text       = _partida.Jogador.Level.ToString();
        }

        private void AtualizaListaInventarioNoMenu()
        {
            dataGridViewInventario.RowHeadersVisible = false;
            dataGridViewInventario.ColumnCount = 2;
            dataGridViewInventario.Columns[0].Name  = "Nome";
            dataGridViewInventario.Columns[0].Width = 197;
            dataGridViewInventario.Columns[1].Name  = "Quantidade";
            dataGridViewInventario.Rows.Clear();

            foreach (var item in _partida.Jogador.Inventario)
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

            foreach (var jq in _partida.Jogador.Quests)
                dataGridViewQuests.Rows.Add(jq.Detalhes.Nome, jq.Completado ? "✓" : "–");
        }

        private void AtualizaListaArmaNoMenu()
        {
            var armas = _partida.Jogador.Inventario
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
            var pocoes = _partida.Jogador.Inventario
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
            var armaAtual   = (Arma)comboBoxArmas.SelectedItem;
            var localAnterior = _partida.Jogador.LocalAtual;

            foreach (var mensagem in _partida.Atacar(armaAtual))
                AdicionaMensagem(mensagem);

            if (!ReferenceEquals(_partida.Jogador.LocalAtual, localAnterior))
            {
                // O jogador morreu e foi teletransportado para a Casa: atualiza a
                // tela da mesma forma que em MoverPara
                richTextBoxLocal.Text = $"{_partida.Jogador.LocalAtual.Nome}{Environment.NewLine}{_partida.Jogador.LocalAtual.Descricao}{Environment.NewLine}";

                AtualizaStatsDoJogador();
                AtualizaListaInventarioNoMenu();
                AtualizaListaQuestNoMenu();
                AtualizaListaArmaNoMenu();
                AtualizaListaPocaoNoMenu();
                AtualizaBotoesMovimento();
                AtualizaVisibilidadeCombate();
            }
            else
            {
                AtualizaStatsDoJogador();

                // Monstro derrotado: o inventário pode ter mudado (loot) — atualiza as listas
                if (_partida.MonstroAtual == null)
                {
                    AtualizaListaInventarioNoMenu();
                    AtualizaListaArmaNoMenu();
                    AtualizaListaPocaoNoMenu();
                    AtualizaVisibilidadeCombate();
                }

                AtualizaBotoesMovimento();
            }

            RolarMensagensParaFim();
        }

        private void buttonUsarPocao_Click(object sender, EventArgs e)
        {
            var pocao = (PocaoCura)comboBoxPocoes.SelectedItem;

            // Com a vida cheia a poção não é consumida: nesse caso a tela não é atualizada
            bool vidaCheia = _partida.Jogador.VidaAtual >= _partida.Jogador.VidaMaximaEfetiva;

            foreach (var mensagem in _partida.UsarPocao(pocao))
                AdicionaMensagem(mensagem);

            if (!vidaCheia)
            {
                AtualizaStatsDoJogador();
                AtualizaListaInventarioNoMenu();
                AtualizaListaPocaoNoMenu();
                AtualizaVisibilidadeCombate();
            }

            RolarMensagensParaFim();
        }

        // Mostra/esconde os botões de movimento: só estão disponíveis quando não há
        // monstro vivo no local (durante o combate o jogador precisa encerrar a luta)
        private void AtualizaBotoesMovimento()
        {
            bool semMonstro = _partida.MonstroAtual == null;
            var local = _partida.Jogador.LocalAtual;

            buttonNorte.Visible = semMonstro && local.LocalParaNorte != null;
            buttonLeste.Visible = semMonstro && local.LocalParaLeste != null;
            buttonSul.Visible   = semMonstro && local.LocalParaSul   != null;
            buttonOeste.Visible = semMonstro && local.LocalParaOeste != null;
        }

        // Única fonte de visibilidade dos controles de combate:
        // eles só aparecem quando há monstro vivo E o jogador tem o item no inventário.
        private void AtualizaVisibilidadeCombate()
        {
            bool temArma  = _partida.Jogador.Inventario.Any(ii => ii.Detalhes is Arma && ii.Quantidade > 0);
            bool temPocao = _partida.Jogador.Inventario.Any(ii => ii.Detalhes is PocaoCura && ii.Quantidade > 0);

            comboBoxArmas.Visible    = _partida.MonstroAtual != null && temArma;
            buttonUsarArma.Visible   = _partida.MonstroAtual != null && temArma;
            comboBoxPocoes.Visible   = _partida.MonstroAtual != null && temPocao;
            buttonUsarPocao.Visible  = _partida.MonstroAtual != null && temPocao;
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

        private void button1_Click(object sender, EventArgs e)
        {
            
        }
    }
}
