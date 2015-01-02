using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motor
{
    public class Item
    {
        public int ID { get; set; }
        public string Nome { get; set; }
        public string NomePlural { get; set; }

        public Item(int id, string nome, string nomePlural)
        {
            ID = id;
            Nome = nome;
            NomePlural = nomePlural;
        }
    }
}
