using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motor
{
    public class Monstro : SerVivo
    {
        public int ID { get; set; }
        public string Nome { get; set; }
        public int DanoMaximo { get; set; }
        public int PontosExperienciaRecompensa { get; set; }
        public int OuroRecompensa { get; set; }
        public List<ItemLoot> LootTable { get; set; }

        public Monstro(int id, string nome, int danoMaximo, int pontosExperienciaRecompensa, int ouroRecompensa, int vidaAtual, int vidaMaxima) : base(vidaAtual, vidaMaxima)
        {
            ID = id;
            Nome = nome;
            DanoMaximo = danoMaximo;
            PontosExperienciaRecompensa = pontosExperienciaRecompensa;
            OuroRecompensa = ouroRecompensa;
            LootTable = new List<ItemLoot>();
        }

    }
}
