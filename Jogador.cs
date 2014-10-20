using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motor
{
    public class Jogador : SerVivo
    {
        public int Ouro { get; set; }
        public int PontosExperiencia { get; set; }
        // public int Level { get; set; } somente coloque isso aqui, se não quiser que o jogador suba de level
        public int Level
        {
            get { return ((PontosExperiencia / 100)+ 1); } // a cada 100 pontos de experiencia, o jogador sobe 1 level.

        }
        public Local LocalAtual { get; set; }
        public List<InventarioItem> Inventario { get; set; }
        public List<JogadorQuest> Quests { get; set; }

        public Jogador(int vidaAtual, int vidaMaxima, int ouro, int pontosExperiencia) : base(vidaAtual, vidaMaxima)
        {
            Ouro = ouro;
            PontosExperiencia = pontosExperiencia;
            // Level = level;  somente coloque isso aqui, se não quiser que o jogador suba de level
            Inventario = new List<InventarioItem>();
            Quests = new List<JogadorQuest>();
             
        }

        public bool TemItemNecessarioParaEntrarNesteLocal(Local local) 
        {
            if(local.ItemNecessarioEntrar == null)
            {
                // Não há item necessário para entrar nesse local, então retorne "true"
                return true;
            }
 
            // Verifica se o jogador tem o item necessário no inventário
            foreach(InventarioItem ii in Inventario)
            {
                if(ii.Detalhes.ID == local.ItemNecessarioEntrar.ID)
                {
                    // Item foi encontrado, então retorne "true"
                    return true;
                }
            }

            // Item necessário não foi encontrado, então retorne "false"
            return false;
        }
 
        public bool TemEstaQuest(Quest quest)
        {
            foreach(JogadorQuest jogadorQuest in Quests)
            {
                if(jogadorQuest.Detalhes.ID == quest.ID)
                {
                    return true;
                }
            }
 
            return false;
        }
 
        public bool QuestEstaCompletada(Quest quest)
        {
            foreach(JogadorQuest jogadorQuest in Quests)
            {
                if(jogadorQuest.Detalhes.ID == quest.ID)
                {
                    return jogadorQuest.Completado;
                }
            }
 
            return false;
        }
 
        public bool TemTodosItensParaCompletarQuest(Quest quest)
        {
            // Verifica se o jogador tem todos os itens necessários para completar a quest aqui
            foreach(QuestCompletadaItem qci in quest.QuestCompletadaItem) // qci = quest completada item
            {
                bool encontradoNoInventarioDoJogador = false;
 
               //Checa cada item no inventario do jogador, e checa se existe, e se tem o suficiente
                foreach (InventarioItem ii in Inventario) // ii = inventário item
                {
                    if(ii.Detalhes.ID == qci.Detalhes.ID) // O jogador tem o item em seu inventário
                    {
                        encontradoNoInventarioDoJogador = true;
 
                        if(ii.Quantidade < qci.Quantidade) // O jogador não tem a quantidade de itens necessária para completar a quest
                        {
                            return false;
                        }
                    }
                }
 
                // O jogador não tem nenhum item para completar a quest
                if (!encontradoNoInventarioDoJogador)
                {
                    return false;
                }
            }
 
            // Se chegou aqui, então o jogador deve ter todos os itens necessários, a quantidade necessária, para completar a quest.
            return true;
        }
 
        public void RemovaItensDeQuestCompletada(Quest quest)
        {
            foreach(QuestCompletadaItem qci in quest.QuestCompletadaItem) // pode dar erro de lógica aqui
            {
                foreach (InventarioItem ii in Inventario)
                {
                    if(ii.Detalhes.ID == qci.Detalhes.ID)
                    {
                        // Subtrai a quantidade de itens necessários para completar a quest
                        ii.Quantidade -= qci.Quantidade;
                        break;
                    }
                }
            }
        }
 
        public void AdicioneItemAoInventario(Item itemParaAdicionar)
        {
            foreach (InventarioItem ii in Inventario)
            {
                if(ii.Detalhes.ID == itemParaAdicionar.ID)
                {
                    // O jogador tem o item em seu inventário, logo aumente a quantidade em +1
                    ii.Quantidade++;
 
                    return; // Item adicionado, pronto. Só sair da função.
                }
            }
 
            //O jogador não possuia o item, ai adicione na quantidade de 1
            Inventario.Add(new InventarioItem(itemParaAdicionar, 1));
        }

        public void MarqueQuestCompletada(Quest quest)
        {
            // Encontre quest na lista de quest do jogador
            foreach (JogadorQuest jq in Quests) // jq = jogador quest
            {
                if (jq.Detalhes.ID == quest.ID)
                {
                    // Marque quest como completado
                    jq.Completado = true;

                    return; // Quest foi encontrada, e marcada como completa, então saia dessa função
                }
            }
        }
    }
}
