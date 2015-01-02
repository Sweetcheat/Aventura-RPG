using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motor
{
    public class Quest
    {
        public int ID { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public int PontosExperienciaRecompensa { get; set; }
        public int OuroRecompensa { get; set; }
        public Item ItemRecompensa { get; set; }
        public List<QuestCompletadaItem> QuestCompletadaItem { get; set; }

        public Quest(int id, string nome, string descricao, int pontosExperienciaRecompensa, int ouroRecompensa)
        {
            ID = id;
            Nome = nome;
            Descricao = descricao;
            PontosExperienciaRecompensa = pontosExperienciaRecompensa;
            OuroRecompensa = ouroRecompensa;
            QuestCompletadaItem = new List<QuestCompletadaItem>();
        }
    }
}
