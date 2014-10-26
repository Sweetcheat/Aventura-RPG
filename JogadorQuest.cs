using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motor
{
    public class JogadorQuest
    {
        public Quest Detalhes { get; set; }
        public bool Completado { get; set; }

        public JogadorQuest(Quest detalhes)
        {
          Detalhes = detalhes;
          Completado = false;
        }
    }
}
