using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motor
{
    public class ItemLoot
    {
        public Item Detalhes { get; set; }
        public int PorcentagemDrop { get; set; }
        public bool EItemComum { get; set; } 

        public ItemLoot(Item detalhes, int porcentagemDrop, bool eItemComum) // eItemComum é do tipo Boolean por que vai determinar se este é ou não é item comum("true" ou "false")
        {                                                                   
            Detalhes = detalhes;
            PorcentagemDrop = porcentagemDrop;
            EItemComum = eItemComum; 
        }
    }
}
