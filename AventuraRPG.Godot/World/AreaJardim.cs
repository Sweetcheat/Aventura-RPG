using System.Collections.Generic;
using Godot;
using Motor;

/* O Jardim: área do Local JARDIM_DOS_ALQUIMISTAS do Motor — jardim aberto com
   o Rato (Local.MonstroVivoAqui do Motor, representado pelo Inimigo2D) e uma
   porta sul que leva de volta à Cabana (LocalParaSul do grafo do Motor). */
public partial class AreaJardim : AreaLocal
{
    private static readonly Vector2 TamanhoArea = new Vector2(800, 600);

    public AreaJardim(Local local)
        : base(local)
    {
    }

    protected override Vector2 PosicaoSpawn => new Vector2(TamanhoArea.X / 2, TamanhoArea.Y / 2);

    // Os 3 Ratos do Jardim (MonstrosVivosAqui do Motor) ficam no meio do
    // jardim, longe da porta
    protected override List<Vector2> PosicoesInimigos => new List<Vector2>
    {
        new Vector2(550, 300),
        new Vector2(620, 340),
        new Vector2(480, 340),
    };

    protected override void CriaCenario()
    {
        // Chão (gramado)
        var chao = new Polygon2D
        {
            Color = new Color(0.20f, 0.30f, 0.20f),
            Polygon = new[]
            {
                Vector2.Zero,
                new Vector2(TamanhoArea.X, 0f),
                TamanhoArea,
                new Vector2(0f, TamanhoArea.Y),
            },
        };
        AddChild(chao);

        // Paredes (limites da área)
        AdicionaParede(new Vector2(TamanhoArea.X / 2, -20), new Vector2(TamanhoArea.X + 40, 40));               // topo
        AdicionaParede(new Vector2(TamanhoArea.X / 2, TamanhoArea.Y + 20), new Vector2(TamanhoArea.X + 40, 40)); // base
        AdicionaParede(new Vector2(-20, TamanhoArea.Y / 2), new Vector2(40, TamanhoArea.Y + 40));               // esquerda
        AdicionaParede(new Vector2(TamanhoArea.X + 20, TamanhoArea.Y / 2), new Vector2(40, TamanhoArea.Y + 40)); // direita

        // Porta para a Cabana (parede sul)
        var porta = new ColorRect
        {
            Position = new Vector2(370, TamanhoArea.Y - 8),
            Size = new Vector2(60, 16),
            Color = new Color(0.45f, 0.30f, 0.18f),
        };
        AddChild(porta);

        // Saída: porta sul -> LocalParaSul (Cabana); entrada na Cabana perto da porta norte dela
        var saida = CriaSaida(new Vector2(400, 545), Local.LocalParaSul);
        saida.PosicaoEntradaDestino = new Vector2(300, 100);
    }

    private void AdicionaParede(Vector2 posicao, Vector2 tamanho)
    {
        var parede = new StaticBody2D { Position = posicao };
        var forma = new CollisionShape2D { Shape = new RectangleShape2D { Size = tamanho } };
        parede.AddChild(forma);
        AddChild(parede);
    }
}
