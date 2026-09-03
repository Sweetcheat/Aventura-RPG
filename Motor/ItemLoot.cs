namespace Motor
{
    public class ItemLoot
    {
        public Item Detalhes { get; set; }
        public int PorcentagemDrop { get; set; }
        // true = item comum: sempre cai se nenhum item aleatório for selecionado
        public bool EItemComum { get; set; }

        public ItemLoot(Item detalhes, int porcentagemDrop, bool eItemComum)
        {
            Detalhes = detalhes;
            PorcentagemDrop = porcentagemDrop;
            EItemComum = eItemComum;
        }
    }
}
