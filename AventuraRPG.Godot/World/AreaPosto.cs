using Godot;
using Motor;

/* O Posto de Guarda: área do Local POSTO_DE_GUARDA do Motor — quartel com
   porta oeste para a Praça (LocalParaOeste) e porta leste para a Ponte
   (LocalParaLeste). A regra de entrada (Passe de Aventureiro,
   Local.ItemNecessarioEntrar) é do Motor: o Godot apenas chama a Partida e
   exibe o resultado; se o Motor bloquear a entrada, o jogador fica na área
   atual e a mensagem de gate vem do Motor. */
public partial class AreaPosto : AreaLocal
{
    private static readonly Vector2 TamanhoSala = new Vector2(600, 400);

    public AreaPosto(Local local)
        : base(local)
    {
    }

    protected override Vector2 PosicaoSpawn => new Vector2(TamanhoSala.X / 2, TamanhoSala.Y / 2);

    protected override void CriaCenario()
    {
        // Chão
        var chao = new Polygon2D
        {
            Color = new Color(0.25f, 0.24f, 0.21f),
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

        // Saída: porta oeste -> LocalParaOeste (Praça); entrada na Praça perto da porta leste dela
        var saidaOeste = CriaSaida(new Vector2(100, 200), Local.LocalParaOeste);
        saidaOeste.PosicaoEntradaDestino = new Vector2(700, 400);

        // Saída: porta leste -> LocalParaLeste (Ponte); entrada na Ponte na extremidade oeste
        var saidaLeste = CriaSaida(new Vector2(500, 200), Local.LocalParaLeste);
        saidaLeste.PosicaoEntradaDestino = new Vector2(100, 150);
    }

    private void AdicionaParede(Vector2 posicao, Vector2 tamanho)
    {
        var parede = new StaticBody2D { Position = posicao };
        var forma = new CollisionShape2D { Shape = new RectangleShape2D { Size = tamanho } };
        parede.AddChild(forma);
        AddChild(parede);
    }
}
