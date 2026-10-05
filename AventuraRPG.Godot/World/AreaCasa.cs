using Godot;
using Motor;

/* A Casa: área do Local CASA do Motor — sala com 4 paredes e uma porta
   que leva à Praça (LocalParaNorte do grafo do Motor). */
public partial class AreaCasa : AreaLocal
{
    private static readonly Vector2 TamanhoSala = new Vector2(600, 400);

    public AreaCasa(Local local)
        : base(local)
    {
    }

    protected override Vector2 PosicaoSpawn => new Vector2(TamanhoSala.X / 2, TamanhoSala.Y / 2);

    protected override void CriaCenario()
    {
        // Chão
        var chao = new Polygon2D
        {
            Color = new Color(0.22f, 0.25f, 0.22f),
            Polygon = new[]
            {
                Vector2.Zero,
                new Vector2(TamanhoSala.X, 0f),
                TamanhoSala,
                new Vector2(0f, TamanhoSala.Y),
            },
        };
        AddChild(chao);

        // Paredes (limites da área)
        AdicionaParede(new Vector2(TamanhoSala.X / 2, -20), new Vector2(TamanhoSala.X + 40, 40));               // topo
        AdicionaParede(new Vector2(TamanhoSala.X / 2, TamanhoSala.Y + 20), new Vector2(TamanhoSala.X + 40, 40)); // base
        AdicionaParede(new Vector2(-20, TamanhoSala.Y / 2), new Vector2(40, TamanhoSala.Y + 40));               // esquerda
        AdicionaParede(new Vector2(TamanhoSala.X + 20, TamanhoSala.Y / 2), new Vector2(40, TamanhoSala.Y + 40)); // direita

        // Porta (apenas visual — a interação é da SaidaLocal)
        var porta = new ColorRect
        {
            Position = new Vector2(430, 392),
            Size = new Vector2(60, 16),
            Color = new Color(0.45f, 0.30f, 0.18f),
        };
        AddChild(porta);

        // Saída: porta -> LocalParaNorte (Praça)
        CriaSaida(new Vector2(455, 345), Local.LocalParaNorte);
    }

    private void AdicionaParede(Vector2 posicao, Vector2 tamanho)
    {
        var parede = new StaticBody2D { Position = posicao };
        var forma = new CollisionShape2D { Shape = new RectangleShape2D { Size = tamanho } };
        parede.AddChild(forma);
        AddChild(parede);
    }
}
