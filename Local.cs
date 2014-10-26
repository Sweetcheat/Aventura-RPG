using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Motor
{
    public class Local
    {
        public int ID { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public Item ItemNecessarioEntrar { get; set; }
        public Quest QuestDisponivelAqui { get; set; }
        public Monstro MonstroVivoAqui { get; set; }
        public Local LocalParaNorte { get; set; }
        public Local LocalParaLeste { get; set; }
        public Local LocalParaSul { get; set; }
        public Local LocalParaOeste { get; set; }

        
        public Local(int id, string nome, string descricao, 
            Item itemNecessarioEntrar, Quest questDisponivelAqui, Monstro monstroVivoAqui)
         {
            ID = id;
            Nome = nome;
            Descricao = descricao;
            ItemNecessarioEntrar = itemNecessarioEntrar;
            QuestDisponivelAqui = questDisponivelAqui;
            MonstroVivoAqui = monstroVivoAqui;
          }
    }
}
