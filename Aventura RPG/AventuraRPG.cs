/*
 Este é um jogo de RPG simples desenvolvido por
 Lucas Barbosa e Luis Felipe (FATEC ITU - 2º SEMESTRE GTI)
 O código está todo comentado, com explicações de como o código funciona.
 Esse é um projeto para a disciplina de Linguagem de Programação C# da
 Professora Angelina Melaré.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
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

            //Local local = new Local(1, "Casa", "Esta é a sua casa.", null, null, null);
           // fica assim se definido os parametros lá na classe 'Local'.

            _jogador = new Jogador(15, 15, 0, 0); // falta o ultimo parâmetro de level aqui, porém como o jogador estará subindo de level, não precisa determinar o level inicial.
            MoverPara(Mundo.LocalPorID(Mundo.LOCAL_ID_CASA));
            _jogador.Inventario.Add(new InventarioItem(Mundo.ItemPorID(Mundo.ITEM_ID_ESPADA_ENFERRUJADA), 1));

            AtualizaStatsDoJogador();
         }

        // Função do buttonNorte, Leste, Sul e Oeste

        private void buttonNorte_Click(object sender, EventArgs e)
        {
            MoverPara(_jogador.LocalAtual.LocalParaNorte);
        }

        private void buttonLeste_Click(object sender, EventArgs e)
        {
            MoverPara(_jogador.LocalAtual.LocalParaLeste);
        }

        private void buttonSul_Click(object sender, EventArgs e)
        {
            MoverPara(_jogador.LocalAtual.LocalParaSul);
        }

        private void buttonOeste_Click(object sender, EventArgs e)
        {
            MoverPara(_jogador.LocalAtual.LocalParaOeste);
        }

        private void MoverPara(Local novoLocal)
        {
            //Se esse local precisa de algum item para acessá-lo
            if (!_jogador.TemItemNecessarioParaEntrarNesteLocal(novoLocal))
            {
                richTextBoxMensagens.Text += "Você precisa ter um " + novoLocal.ItemNecessarioEntrar.Nome + " para acessar este local." + Environment.NewLine;
                return; //tinha esquecido desse return e o jogador ficava sem o que fazer.
            }

            // Atualiza a posição atual do jogador
            _jogador.LocalAtual = novoLocal;
 
            // Mostra/esconde botões de movimento disponiveis
            buttonNorte.Visible = (novoLocal.LocalParaNorte != null);
            buttonLeste.Visible = (novoLocal.LocalParaLeste != null);
            buttonSul.Visible = (novoLocal.LocalParaSul != null);
            buttonOeste.Visible = (novoLocal.LocalParaOeste != null);
 
            // Mostra o nome da localização atual e descrição
            richTextBoxLocal.Text = novoLocal.Nome + Environment.NewLine;
            richTextBoxLocal.Text += novoLocal.Descricao + Environment.NewLine;
 
            // Cura completamente a vida do jogador
            _jogador.VidaAtual = _jogador.VidaMaxima;
 
            // Atualiza a vida da tela
            lblVida.Text = _jogador.VidaAtual.ToString();
 
            // Esse local tem uma quest?
            if(novoLocal.QuestDisponivelAqui != null)
            {
                // Verifica se o jogador já possui a quest, e se já foi completada
                bool jogadorJaTemQuest = _jogador.TemEstaQuest(novoLocal.QuestDisponivelAqui);
                bool jogadorJaCompletouQuest = _jogador.QuestEstaCompletada(novoLocal.QuestDisponivelAqui);

                // Verifica se o jogador já tem quest
                if (jogadorJaTemQuest)
                {
                    // Verifica se o jogador não completou a quest ainda
                    if (!jogadorJaCompletouQuest)
                    {
                        // Checa se o jogador tem todos os itens necessários para completar a quest
                        bool JogadorTemTodosItensParaCompletarQuest = _jogador.TemTodosItensParaCompletarQuest(novoLocal.QuestDisponivelAqui);

                        // O jogador tem todos os itens necessários para completar a quest
                        if (JogadorTemTodosItensParaCompletarQuest)
                        {
                            // Mostrar mensagem
                            richTextBoxMensagens.Text += Environment.NewLine;
                            richTextBoxMensagens.Text += "Você completou '" + novoLocal.QuestDisponivelAqui.Nome + "'." + Environment.NewLine;

                            // Remove itens da quest do inventário
                            _jogador.RemovaItensDeQuestCompletada(novoLocal.QuestDisponivelAqui);
 
                            // Dá recompensas da quest
                            richTextBoxMensagens.Text += "Você recebe: " + Environment.NewLine;
                            richTextBoxMensagens.Text += novoLocal.QuestDisponivelAqui.PontosExperienciaRecompensa.ToString() + " pontos de experiência" + Environment.NewLine;
                            richTextBoxMensagens.Text += novoLocal.QuestDisponivelAqui.OuroRecompensa.ToString() + " ouro" + Environment.NewLine;
                            richTextBoxMensagens.Text += novoLocal.QuestDisponivelAqui.ItemRecompensa.Nome + Environment.NewLine;
                            richTextBoxMensagens.Text += Environment.NewLine;

                            _jogador.PontosExperiencia += novoLocal.QuestDisponivelAqui.PontosExperienciaRecompensa;
                            _jogador.Ouro += novoLocal.QuestDisponivelAqui.OuroRecompensa;
 
                            // Adiciona o item de recompensa ao inventário do jogador
                            _jogador.AdicioneItemAoInventario(novoLocal.QuestDisponivelAqui.ItemRecompensa);

                            // Marca a quest como completada
                            _jogador.MarqueQuestCompletada(novoLocal.QuestDisponivelAqui);
                        }
                    }
                }
                else
                {
                    // o jogador não tem a quest ainda
 
                    // Mostra mensagens
                    richTextBoxMensagens.Text += Environment.NewLine;
                    richTextBoxMensagens.Text += "Você recebeu a quest: " + novoLocal.QuestDisponivelAqui.Nome + "." + Environment.NewLine;
                    richTextBoxMensagens.Text += novoLocal.QuestDisponivelAqui.Descricao + Environment.NewLine;
                    richTextBoxMensagens.Text += "Para completá-la, retorne com:" + Environment.NewLine;
                    foreach (QuestCompletadaItem qci in novoLocal.QuestDisponivelAqui.QuestCompletadaItem) // VERIFICAR ESSA PARTE DO CODIGO DE NOVO, POSSIVEL ERRO DE LÓGICA "ITEM =/= ITENS"
                    {
                        if(qci.Quantidade == 1)
                        {
                            richTextBoxMensagens.Text += qci.Quantidade.ToString() + " " + qci.Detalhes.Nome + Environment.NewLine;
                        }
                        else
                        {
                            richTextBoxMensagens.Text += qci.Quantidade.ToString() + " " + qci.Detalhes.NomePlural + Environment.NewLine;
                        }
                    }
                    richTextBoxMensagens.Text += Environment.NewLine;
 
                    // Adiciona a quest para a lista de quest do jogador
                    _jogador.Quests.Add(new JogadorQuest(novoLocal.QuestDisponivelAqui));
                }
            }
 
            // Esse local tem um inimigo? (monstro)
            if(novoLocal.MonstroVivoAqui != null)
            {
                richTextBoxMensagens.Text += "Você vê um(a) " + novoLocal.MonstroVivoAqui.Nome + Environment.NewLine;
 
                // Faz um novo monstro, usando os valores de um monstro 'padrão'.
                // com todos os seus atributos (exp, ouro, danomax, nome, id, vida, vidamax)
                Monstro monstroNormal = Mundo.MonstroPorID(novoLocal.MonstroVivoAqui.ID);

                _monstroAtual = new Monstro(monstroNormal.ID, monstroNormal.Nome, monstroNormal.DanoMaximo,
                    monstroNormal.PontosExperienciaRecompensa, monstroNormal.OuroRecompensa, monstroNormal.VidaAtual, monstroNormal.VidaMaxima);
                
                // Adiciona item loot à loot table do novo monstro
                foreach (ItemLoot itemLoot in monstroNormal.LootTable)
                {
                    _monstroAtual.LootTable.Add(itemLoot);
                }
                
                //Mostra comboBox e buttons caso exista o monstro no local
                comboBoxArmas.Visible = true;
                comboBoxPoçoes.Visible = true;
                buttonUsarArma.Visible = true;
                buttonUsarPoçao.Visible = true;
            }
            else
            {
                // se não existir monstros no local, esconde comboBox e buttons
                _monstroAtual = null;

                comboBoxArmas.Visible = false;
                comboBoxPoçoes.Visible = false;
                buttonUsarArma.Visible = false;
                buttonUsarPoçao.Visible = false;
            }
            
            // Atualiza stats do Jogador
            AtualizaStatsDoJogador();

            // Atualiza a lista do inventário do jogador
            AtualizaListaInventarioNoMenu();

            // Atualiza a lista de quest do jogador
            AtualizaListaQuestNoMenu();

            // Atualiza a comboBoxArma do jogador
            AtualizaListaArmaNoMenu();

            // Atualiza a comboBoxPoçoes do jogador
            AtualizaListaPocaoNoMenu();

            // Auto Scroll da RichBox (encontrei na internet, viva o google)
            AutoScroll();
            
        }
        // Atualiza status do jogador
        private void AtualizaStatsDoJogador()
        {
            lblVida.Text = _jogador.VidaAtual.ToString();
            lblOuro.Text = _jogador.Ouro.ToString();
            lblExperiencia.Text = _jogador.PontosExperiencia.ToString();
            lblLevel.Text = _jogador.Level.ToString();
        }
        // Atualiza inventário do jogador
        private void AtualizaListaInventarioNoMenu()
        {
            dataGridViewInventario.RowHeadersVisible = false;

            dataGridViewInventario.ColumnCount = 2;
            dataGridViewInventario.Columns[0].Name = "Nome";
            dataGridViewInventario.Columns[0].Width = 197;
            dataGridViewInventario.Columns[1].Name = "Quantidade";

            dataGridViewInventario.Rows.Clear();

            foreach (InventarioItem inventarioItem in _jogador.Inventario)
            {
                if (inventarioItem.Quantidade > 0)
                {
                    dataGridViewInventario.Rows.Add(new[] { inventarioItem.Detalhes.Nome, inventarioItem.Quantidade.ToString() });
                }
            }
        }
        // Atualiza lista de quest do jogador
        private void AtualizaListaQuestNoMenu()
        {
            dataGridViewQuests.RowHeadersVisible = false;

            dataGridViewQuests.ColumnCount = 2;
            dataGridViewQuests.Columns[0].Name = "Nome";
            dataGridViewQuests.Columns[0].Width = 197;
            dataGridViewQuests.Columns[1].Name = "Completada?";

            dataGridViewQuests.Rows.Clear();

            foreach (JogadorQuest jogadorQuest in _jogador.Quests)
            {
                dataGridViewQuests.Rows.Add(new[] { jogadorQuest.Detalhes.Nome, jogadorQuest.Completado.ToString() });
            }
        }
        // Atualiza lista de arma do jogador
        private void AtualizaListaArmaNoMenu()
        {
            List<Arma> armas = new List<Arma>();

            foreach (InventarioItem inventarioItem in _jogador.Inventario)
            {
                if (inventarioItem.Detalhes is Arma)
                {
                    if (inventarioItem.Quantidade > 0)
                    {
                        armas.Add((Arma)inventarioItem.Detalhes);
                    }
                }
            }
            // * VALIDAÇÃO *
            if (armas.Count == 0)
            {
                // Se o jogador não possui nenhuma arma, então aqui vai esconder a arma da comboBoxArma e o buttonUsarArma
                comboBoxArmas.Visible = false;
                buttonUsarArma.Visible = false;
            }
            else
            {
                comboBoxArmas.DataSource = armas;
                comboBoxArmas.DisplayMember = "Nome";
                comboBoxArmas.ValueMember = "ID";

                comboBoxArmas.SelectedIndex = 0;
            }
        }
        // Atualiza lista de poções do jogador
        private void AtualizaListaPocaoNoMenu()
        {
            List<PocaoCura> pocaoCura = new List<PocaoCura>();

            foreach (InventarioItem inventarioItem in _jogador.Inventario)
            {
                if (inventarioItem.Detalhes is PocaoCura)
                {
                    if (inventarioItem.Quantidade > 0)
                    {
                        pocaoCura.Add((PocaoCura)inventarioItem.Detalhes);
                    }
                }
            }

            if (pocaoCura.Count == 0)
            {
                // Se o jogador não possui nenhuma poção, então aqui vai esconder a poção da comboBoxPoçoes e o buttonUsarPoçao
                comboBoxPoçoes.Visible = false;
                buttonUsarPoçao.Visible = false;
            }
            else
            {
                comboBoxPoçoes.DataSource = pocaoCura;
                comboBoxPoçoes.DisplayMember = "Nome";
                comboBoxPoçoes.ValueMember = "ID";

                comboBoxPoçoes.SelectedIndex = 0;
            }
        }

        private void buttonUsarArma_Click(object sender, EventArgs e) // EVENTO CLICK DO USO DA ARMA
        {
            // Escolhe a arma atual selecionada na comboBoxArma
            Arma armaAtual = (Arma)comboBoxArmas.SelectedItem;

            // Quantidade de dano que o monstro irá tomar ao ser atacado
            // Para isso eu usei um algoritmo de geração de numero SIMPLES
            // Estou usando uma classe chamada 'RANDOM' do .NET FRAMEWORK, só que a geração de numeros 'não é bem aleatória',
            // como o jogo é simples, não tem problemas. Mas se fosse um projeto maior, seria interessante usar um algoritmo melhor e mais COMPLEXO.
            int danoAoMonstro = GeradorNumeroAleatorio.NumeroEntre(armaAtual.DanoMinimo, armaAtual.DanoMaximo);

            // Aplica o dano a vida atual do monstro
            _monstroAtual.VidaAtual -= danoAoMonstro;

            // Mostrar mensagem
            richTextBoxMensagens.Text += "Você acertou o(a) " + _monstroAtual.Nome + " e causou " + danoAoMonstro.ToString() + " de ponto(s) de dano." + Environment.NewLine;

            // Checa se o monstro está morto
            if (_monstroAtual.VidaAtual <= 0)
            {
                // Se o monstro está morto, manda mensagem
                richTextBoxMensagens.Text += Environment.NewLine;
                richTextBoxMensagens.Text += "Você derrotou o(a) " + _monstroAtual.Nome + Environment.NewLine;

                // Jogador ganha pontos de experiencia ao matar o monstro
                _jogador.PontosExperiencia += _monstroAtual.PontosExperienciaRecompensa;
                richTextBoxMensagens.Text += "Você recebe " + _monstroAtual.PontosExperienciaRecompensa.ToString() + " pontos de experiência." + Environment.NewLine;

                // Jogador ganha gold ao matar o monstro
                _jogador.Ouro += _monstroAtual.OuroRecompensa;
                richTextBoxMensagens.Text += "Você recebe " + _monstroAtual.OuroRecompensa.ToString() + " de ouro." + Environment.NewLine;

                // Itens aleatórios que possam cair do monstro
                List<InventarioItem> itensSaqueados = new List<InventarioItem>();

                // Adiciona itens a lista de itens saqueados, comparando um numero aleatório com a porcentagem de drop
                foreach (ItemLoot itemLoot in _monstroAtual.LootTable)
                {
                    if (GeradorNumeroAleatorio.NumeroEntre(1, 100) <= itemLoot.PorcentagemDrop)
                    {
                        itensSaqueados.Add(new InventarioItem(itemLoot.Detalhes, 1));
                    }
                }

                // Se nenhum item foi selecionado aleatóriamente, então adicionar um item comum (caso seja 0)
                if (itensSaqueados.Count == 0)
                {
                    foreach (ItemLoot itemLoot in _monstroAtual.LootTable)
                    {
                        if (itemLoot.EItemComum) 
                        {
                            itensSaqueados.Add(new InventarioItem(itemLoot.Detalhes, 1));
                        }
                    }
                }

                // Adiciona os itens saqueados ao inventário do jogador
                foreach (InventarioItem inventarioItem in itensSaqueados)
                {
                    _jogador.AdicioneItemAoInventario(inventarioItem.Detalhes); // aqui pode dar erro de lógica, falta instrução em jogador - "_jogador.xxxxxxx(inventarioItem.Detalhes);"

                    // se for um ou mais itens, manda a devida mensagem (singular ou plural)
                    if (inventarioItem.Quantidade == 1)
                    {
                        richTextBoxMensagens.Text += "Seu saque: " + inventarioItem.Quantidade.ToString() + " " + inventarioItem.Detalhes.Nome + Environment.NewLine;
                    }
                    else
                    {
                        richTextBoxMensagens.Text += "Seu saque: " + inventarioItem.Quantidade.ToString() + " " + inventarioItem.Detalhes.NomePlural + Environment.NewLine;
                    }
                }

                AtualizaStatsDoJogador();
                AtualizaListaInventarioNoMenu();
                AtualizaListaArmaNoMenu();
                AtualizaListaPocaoNoMenu();
                
              
                // Adiciona uma linha a caixa de texto, para dar um espaço e não ficar grudado
                richTextBoxMensagens.Text += Environment.NewLine;

                // Move o jogador para o local atual (cura ele completamente e cria um novo monstro para este lutar)
                MoverPara(_jogador.LocalAtual);
            }
            else
            {
                // Monstro está ainda vivo

                // Determina a quantidade de dano que o monstro irá fazer ao jogador
                int danoAoJogador = GeradorNumeroAleatorio.NumeroEntre(0, _monstroAtual.DanoMaximo);

                // Mostra a mensagem do dano tomado
                richTextBoxMensagens.Text += "O(A) " + _monstroAtual.Nome + " causou a você " + danoAoJogador.ToString() + " pontos de dano." + Environment.NewLine;

                // Subtrai o dano da vida do jogador
                _jogador.VidaAtual -= danoAoJogador;

                // Atualiza os dados do jogador no menu
                lblVida.Text = _jogador.VidaAtual.ToString();

                // E finalmente, se o jogador morre...
                if (_jogador.VidaAtual <= 0)
                {
                    // Mostra Mensagem
                    richTextBoxMensagens.Text += "O(A) " + _monstroAtual.Nome + " matou você." + Environment.NewLine;

                    // Move jogador para o inicio ("casa")
                    MoverPara(Mundo.LocalPorID(Mundo.LOCAL_ID_CASA));
                }
            }

            AutoScroll(); //instancia do auto scroll para poder ocorrer o scroll após a mensagem.
        }

        private void buttonUsarPoçao_Click(object sender, EventArgs e) // EVENTO CLICK DO USO DA POÇÃO
        {
            // Pega a poção que está na comboBoxPoçoes
            PocaoCura pocao = (PocaoCura)comboBoxPoçoes.SelectedItem;

            // Adiciona a qtd de cura a vida atual do jogador
            _jogador.VidaAtual = (_jogador.VidaAtual + pocao.QtdCura);

            // * VALIDAÇÃO * VidaAtual não pode ser maior que a vida maxima do jogador
            if (_jogador.VidaAtual > _jogador.VidaMaxima)
            {
                _jogador.VidaAtual = _jogador.VidaMaxima;
            }

            // Remove a poção do inventário do jogador uma vez que é usada
            foreach (InventarioItem ii in _jogador.Inventario)
            {
                if (ii.Detalhes.ID == pocao.ID)
                {
                    ii.Quantidade--;
                    break;
                }
            }

            // Mostra mensagem que bebeu a poção
            richTextBoxMensagens.Text += "Você bebeu uma " + pocao.Nome + Environment.NewLine;
            
            //****\/ COLOQUE ESSE CODIGO PARA QUE O MONSTRO ATAQUE O JOGADOR APÓS ESTE TER TOMADO A POÇÃO DE CURA \/ ****
             
            // Monstro recebe a sua vez de atacar

            // Determina a quantidade de dano que o jogador irá fazer ao jogador
            int danoAoJogador = GeradorNumeroAleatorio.NumeroEntre(0, _monstroAtual.DanoMaximo);

            // Mostra mensagem do dano causado ao jogador
            richTextBoxMensagens.Text += "O(A) " + _monstroAtual.Nome + " causou a você " + danoAoJogador.ToString() + " pontos de dano." + Environment.NewLine;

            // Subtrai o dano da vida atual do jogador
            _jogador.VidaAtual -= danoAoJogador;

            // E finalmente, se o jogador morre...
            if (_jogador.VidaAtual <= 0)
            {
                // Mostra mensagem que o jogador morreu e move ele para o inicio (casa)
                richTextBoxMensagens.Text += "O(A) " + _monstroAtual.Nome + " matou você." + Environment.NewLine;

                // Move jogador para "casa"
                MoverPara(Mundo.LocalPorID(Mundo.LOCAL_ID_CASA));
            }

            //****/\ COLOQUE ESSE CODIGO PARA QUE O MONSTRO ATAQUE O JOGADOR APÓS ESTE TER TOMADO A POÇÃO DE CURA /\****

            // Atualiza os dados do jogador no menu
            lblVida.Text = _jogador.VidaAtual.ToString();

            AtualizaListaInventarioNoMenu();
            AtualizaListaPocaoNoMenu();

            AutoScroll(); // novamente, outra instancia do auto scroll para que ocorra a função scroll 
          
        }

        private void AutoScroll()
        {
            // Auto Scroll (encontrei na internet, viva o google)
            richTextBoxMensagens.SelectionStart = richTextBoxMensagens.Text.Length;
            richTextBoxMensagens.ScrollToCaret();
        }
    }
}

