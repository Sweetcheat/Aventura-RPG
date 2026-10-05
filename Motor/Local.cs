using System.Collections.Generic;

namespace Motor
{
    public class Local
    {
        public int ID { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public Item ItemNecessarioEntrar { get; set; }
        public Quest QuestDisponivelAqui { get; set; }
        // Monstros que vivem neste local (templates do Mundo; a Partida cria as
        // instâncias vivas). Pode haver mais de um (ex.: 3 Ratos no Jardim)
        public List<Monstro> MonstrosVivosAqui { get; set; }
        public Local LocalParaNorte { get; set; }
        public Local LocalParaLeste { get; set; }
        public Local LocalParaSul { get; set; }
        public Local LocalParaOeste { get; set; }

        public Local(int id, string nome, string descricao,
            Item itemNecessarioEntrar, Quest questDisponivelAqui, List<Monstro> monstrosVivosAqui)
        {
            ID = id;
            Nome = nome;
            Descricao = descricao;
            ItemNecessarioEntrar = itemNecessarioEntrar;
            QuestDisponivelAqui = questDisponivelAqui;
            MonstrosVivosAqui = monstrosVivosAqui ?? new List<Monstro>();
        }
    }
}
