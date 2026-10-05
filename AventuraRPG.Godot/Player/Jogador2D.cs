using System;
using Godot;

/* O jogador no mundo 2D: movimentação por WASD/setas (ações mover_* do
   Input Map), colisão com o cenário (paredes/limites) e a câmera que o
   acompanha. Toda a movimentação é apresentação (Godot); o estado do jogo
   (Local, HP, XP...) é do Motor. */
public partial class Jogador2D : CharacterBody2D
{
	private const float Velocidade = 150f;

	// A apresentação pode pausar o jogador (ex.: um painel aberto)
	public bool Ativo { get; set; } = true;

	public override void _Ready()
	{
		// Colisão (círculo)
		var forma = new CollisionShape2D { Shape = new CircleShape2D { Radius = 10f } };
		AddChild(forma);

		// Visual simples (placeholder de círculo, sem arte)
		var corpo = new Polygon2D { Color = new Color(0.85f, 0.65f, 0.35f) };
		corpo.Polygon = CriaCirculo(10f, 12);
		AddChild(corpo);

		// A câmera acompanha o personagem
		AddChild(new Camera2D());
	}

	public override void _PhysicsProcess(double delta)
	{
		var direcao = Vector2.Zero;
		if (Ativo)
		{
			if (Input.IsActionPressed("mover_cima"))
				direcao.Y -= 1f;
			if (Input.IsActionPressed("mover_baixo"))
				direcao.Y += 1f;
			if (Input.IsActionPressed("mover_esquerda"))
				direcao.X -= 1f;
			if (Input.IsActionPressed("mover_direita"))
				direcao.X += 1f;
		}

		Velocity = direcao.Normalized() * Velocidade;
		MoveAndSlide();
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
