using System;
using System.Collections.Generic;
using Godot;
using Motor;

/* A Praça: área do Local PRACA do Motor — praça aberta com fonte central
   (apenas visual) e portas para a Casa (LocalParaSul), a Cabana dos
   Alquimistas (LocalParaNorte), o Posto de Guarda (LocalParaLeste) e a Casa
   de Fazenda (LocalParaOeste) do grafo do Motor. */
public partial class AreaPraca : AreaLocal
{
    private static readonly Vector2 TamanhoArea = new Vector2(800, 800);

    public AreaPraca(Local local)
        : base(local)
    {
    }

    protected override Vector2 PosicaoSpawn => new Vector2(TamanhoArea.X / 2, 300f);

    // O Rato da Praça fica ao lado da fonte, ao alcance do jogador
    protected override List<Vector2> PosicoesInimigos => new List<Vector2>
    {
        new Vector2(550, 450),
    };

    protected override void CriaCenario()
    {
        // Chão (pavimentado)
        var chao = new Polygon2D
        {
            Color = new Color(0.28f, 0.26f, 0.23f),
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

        // Fonte central (apenas visual)
        var fonte = new Polygon2D { Color = new Color(0.40f, 0.55f, 0.65f) };
        fonte.Polygon = CriaCirculo(60f, 16);
        fonte.Position = new Vector2(TamanhoArea.X / 2, TamanhoArea.Y / 2);
        AddChild(fonte);

        // Porta para a Casa (parede sul)
        var portaSul = new ColorRect
        {
            Position = new Vector2(370, TamanhoArea.Y - 8),
            Size = new Vector2(60, 16),
            Color = new Color(0.45f, 0.30f, 0.18f),
        };
        AddChild(portaSul);

        // Porta para a Cabana dos Alquimistas (parede norte)
        var portaNorte = new ColorRect
        {
            Position = new Vector2(520, -8),
            Size = new Vector2(60, 16),
            Color = new Color(0.45f, 0.30f, 0.18f),
        };
        AddChild(portaNorte);

        // Porta para o Posto de Guarda (parede leste)
        var portaLeste = new ColorRect
        {
            Position = new Vector2(TamanhoArea.X - 8, 370),
            Size = new Vector2(16, 60),
            Color = new Color(0.45f, 0.30f, 0.18f),
        };
        AddChild(portaLeste);

        // Porta para a Casa de Fazenda (parede oeste)
        var portaOeste = new ColorRect
        {
            Position = new Vector2(-8, 370),
            Size = new Vector2(16, 60),
            Color = new Color(0.45f, 0.30f, 0.18f),
        };
        AddChild(portaOeste);

        // Saída: porta sul -> LocalParaSul (Casa); entrada na Casa perto da porta sul dela
        var saidaSul = CriaSaida(new Vector2(400, 700), Local.LocalParaSul);
        saidaSul.PosicaoEntradaDestino = new Vector2(455, 300);

        // Saída: porta norte -> LocalParaNorte (Cabana dos Alquimistas);
        // entrada na Cabana perto da porta sul dela
        var saidaNorte = CriaSaida(new Vector2(550, 60), Local.LocalParaNorte);
        saidaNorte.PosicaoEntradaDestino = new Vector2(300, 345);

        // Saída: porta leste -> LocalParaLeste (Posto de Guarda);
        // entrada no Posto perto da porta oeste dele
        var saidaLeste = CriaSaida(new Vector2(740, 400), Local.LocalParaLeste);
        saidaLeste.PosicaoEntradaDestino = new Vector2(100, 200);

        // Saída: porta oeste -> LocalParaOeste (Casa de Fazenda);
        // entrada na Fazenda perto da porta leste dela
        var saidaOeste = CriaSaida(new Vector2(60, 400), Local.LocalParaOeste);
        saidaOeste.PosicaoEntradaDestino = new Vector2(500, 200);
    }

    private void AdicionaParede(Vector2 posicao, Vector2 tamanho)
    {
        var parede = new StaticBody2D { Position = posicao };
        var forma = new CollisionShape2D { Shape = new RectangleShape2D { Size = tamanho } };
        parede.AddChild(forma);
        AddChild(parede);
    }

    private static Vector2[] CriaCirculo(float raio, int pontos)
    {
        var pts = new Vector2[pontos];
        for (int i = 0; i < pontos; i++)
        {
            float angulo = (float)(Math.Tau * i / pontos);
            pts[i] = new Vector2(MathF.Cos(angulo) * raio, MathF.Sin(angulo) * raio);
        }

        return pts;
    }
}
