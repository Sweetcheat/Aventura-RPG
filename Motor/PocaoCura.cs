namespace Motor
{
    public class PocaoCura : Item
    {
        public int QtdCura { get; set; }

        public PocaoCura(int id, string nome, string nomePlural, int qtdCura)
            : base(id, nome, nomePlural)
        {
            QtdCura = qtdCura;
        }
    }
}
