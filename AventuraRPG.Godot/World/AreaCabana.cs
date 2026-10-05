using Godot;
using Motor;

/* A Cabana: área do Local CABANA_DOS_ALQUIMISTAS do Motor — sala com 4 paredes,
   porta sul para a Praça (LocalParaSul) e porta norte para o Jardim
   (LocalParaNorte). É aqui que o Motor entrega a quest "Limpe o Jardim dos
   Alquimistas" (Local.QuestDisponivelAqui) e onde ela é completada. */
public partial class AreaCabana : AreaLocal
{
    private static readonly Vector2 TamanhoSala = new Vector2(600, 400);

    public AreaCabana(Local local)
        : base(local)
    {
    }

    protected override Vector2 PosicaoSpawn => new Vector2(TamanhoSala.X / 2, TamanhoSala.Y / 2);

    protected override void CriaCenario()
    {
        // Chão
        var chao = new Polygon2D
        {
            Color = new Color(0.24f, 0.22f, 0.19f),
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

        // Portas (apenas visual — a interação é da SaidaLocal)
        var portaNorte = new ColorRect
        {
            Position = new Vector2(270, -8),
            Size = new Vector2(60, 16),
            Color = new Color(0.45f, 0.30f, 0.18f),
        };
        AddChild(portaNorte);

        var portaSul = new ColorRect
        {
            Position = new Vector2(270, 392),
            Size = new Vector2(60, 16),
            Color = new Color(0.45f, 0.30f, 0.18f),
        };
        AddChild(portaSul);

        // Saída: porta sul -> LocalParaSul (Praça); entrada na Praça perto da porta norte dela
        var saidaSul = CriaSaida(new Vector2(300, 345), Local.LocalParaSul);
        saidaSul.PosicaoEntradaDestino = new Vector2(550, 80);

        // Saída: porta norte -> LocalParaNorte (Jardim); entrada no Jardim perto da porta sul dele
        var saidaNorte = CriaSaida(new Vector2(300, 60), Local.LocalParaNorte);
        saidaNorte.PosicaoEntradaDestino = new Vector2(400, 520);
    }

    private void AdicionaParede(Vector2 posicao, Vector2 tamanho)
    {
        var parede = new StaticBody2D { Position = posicao };
        var forma = new CollisionShape2D { Shape = new RectangleShape2D { Size = tamanho } };
        parede.AddChild(forma);
        AddChild(parede);
    }
}
