using System.Collections.Generic;
using Godot;
using Motor;

/* A Area dos Camponeses: área do Local AREA_DOS_CAMPONESES do Motor — lavoura
   aberta com a Cobra (Local.MonstroVivoAqui do Motor, representada pelo
   Inimigo2D) e uma porta leste que leva à Casa de Fazenda (LocalParaLeste do
   grafo do Motor). */
public partial class AreaCamponeses : AreaLocal
{
    private static readonly Vector2 TamanhoArea = new Vector2(800, 600);

    public AreaCamponeses(Local local)
        : base(local)
    {
    }

    protected override Vector2 PosicaoSpawn => new Vector2(TamanhoArea.X / 2, TamanhoArea.Y / 2);

    // A Cobra da area (MonstrosVivosAqui do Motor) fica no meio da lavoura, longe da porta
    protected override List<Vector2> PosicoesInimigos => new List<Vector2>
    {
        new Vector2(400, 300),
    };

    protected override void CriaCenario()
    {
        // Chão (lavoura)
        var chao = new Polygon2D
        {
            Color = new Color(0.30f, 0.28f, 0.16f),
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

        // Fileiras da lavoura (apenas visual)
        for (int linha = 0; linha < 4; linha++)
        {
            var fileira = new ColorRect
            {
                Position = new Vector2(100, 120f + linha * 120f),
                Size = new Vector2(400, 24),
                Color = new Color(0.42f, 0.38f, 0.20f),
            };
            AddChild(fileira);
        }

        // Porta para a Casa de Fazenda (parede leste)
        var porta = new ColorRect
        {
            Position = new Vector2(TamanhoArea.X - 8, 270),
            Size = new Vector2(16, 60),
            Color = new Color(0.45f, 0.30f, 0.18f),
        };
        AddChild(porta);

        // Saída: porta leste -> LocalParaLeste (Casa de Fazenda); entrada na Fazenda perto da porta oeste dela
        var saida = CriaSaida(new Vector2(700, 300), Local.LocalParaLeste);
        saida.PosicaoEntradaDestino = new Vector2(100, 200);
    }

    private void AdicionaParede(Vector2 posicao, Vector2 tamanho)
    {
        var parede = new StaticBody2D { Position = posicao };
        var forma = new CollisionShape2D { Shape = new RectangleShape2D { Size = tamanho } };
        parede.AddChild(forma);
        AddChild(parede);
    }
}
