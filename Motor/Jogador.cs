using System.Collections.Generic;
using System.Linq;

namespace Motor
{
    public class Jogador : SerVivo
    {
        public int Ouro { get; set; }
        public int PontosExperiencia { get; set; }

        // A cada 100 pontos de experiência o jogador sobe 1 level.
        // Para desativar a subida de level, substitua pela propriedade manual: public int Level { get; set; }
        public int Level => (PontosExperiencia / 100) + 1;

        public Local LocalAtual { get; set; }
        public List<InventarioItem> Inventario { get; set; }
        public List<JogadorQuest> Quests { get; set; }

        public Jogador(int vidaAtual, int vidaMaxima, int ouro, int pontosExperiencia)
            : base(vidaAtual, vidaMaxima)
        {
            Ouro = ouro;
            PontosExperiencia = pontosExperiencia;
            Inventario = new List<InventarioItem>();
            Quests = new List<JogadorQuest>();
        }

        public bool TemItemNecessarioParaEntrarNesteLocal(Local local)
        {
            // Se não há item necessário para entrar, libera acesso direto
            if (local.ItemNecessarioEntrar == null)
                return true;

            // Verifica se o jogador tem o item necessário no inventário
            return Inventario.Any(ii => ii.Detalhes.ID == local.ItemNecessarioEntrar.ID);
        }

        public bool TemEstaQuest(Quest quest)
        {
            return Quests.Any(jq => jq.Detalhes.ID == quest.ID);
        }

        public bool QuestEstaCompletada(Quest quest)
        {
            var jogadorQuest = Quests.FirstOrDefault(jq => jq.Detalhes.ID == quest.ID);
            return jogadorQuest != null && jogadorQuest.Completado;
        }

        public bool TemTodosItensParaCompletarQuest(Quest quest)
        {
            // Verifica se o jogador tem todos os itens necessários na quantidade certa
            foreach (var qci in quest.QuestCompletadaItem)
            {
                var itemNoInventario = Inventario.FirstOrDefault(ii => ii.Detalhes.ID == qci.Detalhes.ID);

                if (itemNoInventario == null || itemNoInventario.Quantidade < qci.Quantidade)
                    return false;
            }

            return true;
        }

        public void RemovaItensDeQuestCompletada(Quest quest)
        {
            foreach (var qci in quest.QuestCompletadaItem)
            {
                var itemNoInventario = Inventario.FirstOrDefault(ii => ii.Detalhes.ID == qci.Detalhes.ID);

                if (itemNoInventario != null)
                    itemNoInventario.Quantidade -= qci.Quantidade;
            }
        }

        public void AdicioneItemAoInventario(Item itemParaAdicionar)
        {
            var itemExistente = Inventario.FirstOrDefault(ii => ii.Detalhes.ID == itemParaAdicionar.ID);

            if (itemExistente != null)
            {
                // O jogador já tem o item: apenas incrementa a quantidade
                itemExistente.Quantidade++;
            }
            else
            {
                // Item novo: adiciona ao inventário com quantidade 1
                Inventario.Add(new InventarioItem(itemParaAdicionar, 1));
            }
        }

        public void MarqueQuestCompletada(Quest quest)
        {
            var jogadorQuest = Quests.FirstOrDefault(jq => jq.Detalhes.ID == quest.ID);

            if (jogadorQuest != null)
                jogadorQuest.Completado = true;
        }
    }
}
