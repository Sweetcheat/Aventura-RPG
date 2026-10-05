using Godot;
using Motor;

/* A Ponte: área do Local PONTE do Motor — ponte de pedra sobre um rio
   (faixas de rio apenas visual), com a extremidade oeste ligada ao Posto de
   Guarda (LocalParaOeste) e a extremidade leste ligada à Floresta
   (LocalParaLeste do grafo do Motor). */
public partial class AreaPonte : AreaLocal
{
    private static readonly Vector2 TamanhoArea = new Vector2(800, 300);

    public AreaPonte(Local local)
        : base(local)
    {
    }

    protected override Vector2 PosicaoSpawn => new Vector2(TamanhoArea.X / 2, TamanhoArea.Y / 2);

    protected override void CriaCenario()
    {
        // Tabuleiro da ponte (pedra)
        var chao = new Polygon2D
        {
            Color = new Color(0.33f, 0.32f, 0.30f),
            Polygon = new[]
            {
                Vector2.Zero,
                new Vector2(TamanhoArea.X, 0f),
                TamanhoArea,
                new Vector2(0f, TamanhoArea.Y),
            },
        };
        AddChild(chao);

        // Rio nas margens (apenas visual)
        var rioTopo = new ColorRect
        {
            Position = Vector2.Zero,
            Size = new Vector2(TamanhoArea.X, 40),
            Color = new Color(0.20f, 0.35f, 0.50f),
        };
        AddChild(rioTopo);

        var rioBase = new ColorRect
        {
            Position = new Vector2(0f, TamanhoArea.Y - 40),
            Size = new Vector2(TamanhoArea.X, 40),
            Color = new Color(0.20f, 0.35f, 0.50f),
        };
        AddChild(rioBase);

        // Paredes (limites da área)
        AdicionaParede(new Vector2(TamanhoArea.X / 2, -20), new Vector2(TamanhoArea.X + 40, 40));               // topo
        AdicionaParede(new Vector2(TamanhoArea.X / 2, TamanhoArea.Y + 20), new Vector2(TamanhoArea.X + 40, 40)); // base
        AdicionaParede(new Vector2(-20, TamanhoArea.Y / 2), new Vector2(40, TamanhoArea.Y + 40));               // esquerda
        AdicionaParede(new Vector2(TamanhoArea.X + 20, TamanhoArea.Y / 2), new Vector2(40, TamanhoArea.Y + 40)); // direita

        // Extremidades (apenas visual — a interação é da SaidaLocal)
        var extremidadeOeste = new ColorRect
        {
            Position = new Vector2(-8, 120),
            Size = new Vector2(16, 60),
            Color = new Color(0.45f, 0.30f, 0.18f),
        };
        AddChild(extremidadeOeste);

        var extremidadeLeste = new ColorRect
        {
            Position = new Vector2(TamanhoArea.X - 8, 120),
            Size = new Vector2(16, 60),
            Color = new Color(0.45f, 0.30f, 0.18f),
        };
        AddChild(extremidadeLeste);

        // Saída: extremidade oeste -> LocalParaOeste (Posto de Guarda); entrada no Posto perto da porta leste dele
        var saidaOeste = CriaSaida(new Vector2(100, 150), Local.LocalParaOeste);
        saidaOeste.PosicaoEntradaDestino = new Vector2(500, 200);

        // Saída: extremidade leste -> LocalParaLeste (Floresta); entrada na Floresta perto da porta oeste dela
        var saidaLeste = CriaSaida(new Vector2(700, 150), Local.LocalParaLeste);
        saidaLeste.PosicaoEntradaDestino = new Vector2(100, 300);
    }

    private void AdicionaParede(Vector2 posicao, Vector2 tamanho)
    {
        var parede = new StaticBody2D { Position = posicao };
        var forma = new CollisionShape2D { Shape = new RectangleShape2D { Size = tamanho } };
        parede.AddChild(forma);
        AddChild(parede);
    }
}
