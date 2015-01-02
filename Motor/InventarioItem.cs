using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motor
{
    public class InventarioItem
    {
        public Item Detalhes { get; set; }
        public int Quantidade { get; set; }

        public InventarioItem(Item detalhes, int quantidade)
        {
            Detalhes = detalhes;
            Quantidade = quantidade;
        }
    }
}
