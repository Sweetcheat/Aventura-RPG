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
