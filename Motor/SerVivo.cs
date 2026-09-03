using System.Collections.Generic;

namespace Motor
{
    public class SerVivo
    {
        public int VidaAtual { get; set; }
        public int VidaMaxima { get; set; }

        public SerVivo(int vidaAtual, int vidaMaxima)
        {
            VidaAtual = vidaAtual;
            VidaMaxima = vidaMaxima;
        }
    }
}
