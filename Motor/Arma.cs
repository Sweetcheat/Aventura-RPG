namespace Motor
{
    public class Arma : Item
    {
        public int DanoMinimo { get; set; }
        public int DanoMaximo { get; set; }

        public Arma(int id, string nome, string nomePlural, int danoMinimo, int danoMaximo)
            : base(id, nome, nomePlural)
        {
            DanoMinimo = danoMinimo;
            DanoMaximo = danoMaximo;
        }
    }
}
