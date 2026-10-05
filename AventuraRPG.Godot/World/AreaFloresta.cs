using System;
using System.Collections.Generic;
using Godot;
using Motor;

/* A Floresta: área do Local CAMPO_DAS_ARANHAS do Motor (nome "Floresta") —
   mata aberta com a Aranha gigante (Local.MonstroVivoAqui do Motor,
   representada pelo Inimigo2D) e uma porta oeste que leva à Ponte
   (LocalParaOeste do grafo do Motor). */
public partial class AreaFloresta : AreaLocal
{
    private static readonly Vector2 TamanhoArea = new Vector2(800, 600);

    public AreaFloresta(Local local)
        : base(local)
    {
    }

    protected override Vector2 PosicaoSpawn => new Vector2(TamanhoArea.X / 2, TamanhoArea.Y / 2);

    // A Aranha gigante (MonstrosVivosAqui do Motor) fica no meio da mata, longe da porta
    protected override List<Vector2> PosicoesInimigos => new List<Vector2>
    {
        new Vector2(500, 300),
    };

    protected override void CriaCenario()
    {
        // Chão (mata)
        var chao = new Polygon2D
        {
            Color = new Color(0.15f, 0.25f, 0.15f),
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

        // Árvores (apenas visual)
        AdicionaArvore(new Vector2(250, 150));
        AdicionaArvore(new Vector2(560, 160));
        AdicionaArvore(new Vector2(300, 440));
        AdicionaArvore(new Vector2(620, 430));

        // Porta para a Ponte (parede oeste)
        var porta = new ColorRect
        {
            Position = new Vector2(-8, 270),
            Size = new Vector2(16, 60),
            Color = new Color(0.45f, 0.30f, 0.18f),
        };
        AddChild(porta);

        // Saída: porta oeste -> LocalParaOeste (Ponte); entrada na Ponte na extremidade leste
        var saida = CriaSaida(new Vector2(100, 300), Local.LocalParaOeste);
        saida.PosicaoEntradaDestino = new Vector2(700, 150);
    }

    private void AdicionaArvore(Vector2 posicao)
    {
        var arvore = new Polygon2D { Color = new Color(0.12f, 0.35f, 0.18f) };
        arvore.Polygon = CriaCirculo(36f, 16);
        arvore.Position = posicao;
        AddChild(arvore);
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
