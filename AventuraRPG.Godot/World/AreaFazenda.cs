using Godot;
using Motor;

/* A Casa de Fazenda: área do Local CASA_DA_FAZENDA do Motor — casa com
   porta leste para a Praça (LocalParaLeste) e porta oeste para a Area dos
   Camponeses (LocalParaOeste). É aqui que o Motor entrega a quest "Limpar a
   área dos camponeses" (Local.QuestDisponivelAqui); a apresentação apenas
   exibe as mensagens do Motor. */
public partial class AreaFazenda : AreaLocal
{
    private static readonly Vector2 TamanhoSala = new Vector2(600, 400);

    public AreaFazenda(Local local)
        : base(local)
    {
    }

    protected override Vector2 PosicaoSpawn => new Vector2(TamanhoSala.X / 2, TamanhoSala.Y / 2);

    protected override void CriaCenario()
    {
        // Chão
        var chao = new Polygon2D
        {
            Color = new Color(0.26f, 0.26f, 0.20f),
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
        var portaOeste = new ColorRect
        {
            Position = new Vector2(-8, 170),
            Size = new Vector2(16, 60),
            Color = new Color(0.45f, 0.30f, 0.18f),
        };
        AddChild(portaOeste);

        var portaLeste = new ColorRect
        {
            Position = new Vector2(TamanhoSala.X - 8, 170),
            Size = new Vector2(16, 60),
            Color = new Color(0.45f, 0.30f, 0.18f),
        };
        AddChild(portaLeste);

        // Saída: porta leste -> LocalParaLeste (Praça); entrada na Praça perto da porta oeste dela
        var saidaLeste = CriaSaida(new Vector2(500, 200), Local.LocalParaLeste);
        saidaLeste.PosicaoEntradaDestino = new Vector2(100, 400);

        // Saída: porta oeste -> LocalParaOeste (Area dos Camponeses); entrada na area perto da porta leste dela
        var saidaOeste = CriaSaida(new Vector2(100, 200), Local.LocalParaOeste);
        saidaOeste.PosicaoEntradaDestino = new Vector2(700, 300);
    }

    private void AdicionaParede(Vector2 posicao, Vector2 tamanho)
    {
        var parede = new StaticBody2D { Position = posicao };
        var forma = new CollisionShape2D { Shape = new RectangleShape2D { Size = tamanho } };
        parede.AddChild(forma);
        AddChild(parede);
    }
}
