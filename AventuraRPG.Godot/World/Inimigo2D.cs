using System;
using Godot;
using Motor;

/* O inimigo no mundo 2D: representação visual do Partida.MonstroAtual
   do Motor (corpo, nome e barra de HP) que persegue o jogador.
   Não detém estado RPG próprio: a barra é relida do Monstro (a mesma
   instância que o Motor altera).
   Toda a lógica espacial (percepção, perseguição, ataque, colisão) é do
   Godot: o Motor não conhece posição, velocidade nem física. O inimigo
   apenas sinaliza (AtaqueSolicitado) que está em posição de atacar; as
   regras de dano/morte/respawn são aplicadas pelo Motor. */
public partial class Inimigo2D : CharacterBody2D
{
    // Perseguição (apenas apresentação): percebe o jogador dentro de
    // Percepcao, o persegue a Velocidade (mais lento que o jogador) e
    // para a DistanciaParada dele, sem ficar sobreposto.
    public const float Percepcao = 150f;
    public const float DistanciaParada = 30f;
    public const float Velocidade = 80f;
    // Dentro desta distância o inimigo está em posição de atacar (o mesmo
    // alcance do ataque do jogador no Jogo); a cadência (AtaqueCooldown)
    // é apenas apresentação — o dano é todo do Motor.
    public const float DistanciaAtaque = 50f;
    public const double AtaqueCooldown = 1.0; // segundos entre ataques

    // O Jogo assina para aplicar as regras (Partida.AtaqueDoMonstro); o
    // inimigo não conhece dano, morte, ouro nem respawn
    public event Action AtaqueSolicitado;

    private readonly Monstro _monstro;
    private readonly Jogador2D _jogador;
    private double _tempoSemAtaque;

    private ColorRect _barraFundo;
    private ColorRect _barraVida;

    public Inimigo2D(Monstro monstro, Jogador2D jogador)
    {
        _monstro = monstro;
        _jogador = jogador;
    }

    public override void _Ready()
    {
        // Colisão (círculo) — a mesma solução do jogador: via MoveAndSlide
        // o inimigo respeita as paredes da área
        var forma = new CollisionShape2D { Shape = new CircleShape2D { Radius = 10f } };
        AddChild(forma);

        // Visual simples (placeholder quadrado, sem arte)
        var corpo = new ColorRect
        {
            Position = new Vector2(-12, -12),
            Size = new Vector2(24, 24),
            Color = new Color(0.65f, 0.20f, 0.20f),
        };
        AddChild(corpo);

        var rotulo = new Label
        {
            Text = _monstro.Nome,
            Position = new Vector2(-60, -36),
            Size = new Vector2(120, 18),
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        AddChild(rotulo);

        _barraFundo = new ColorRect
        {
            Position = new Vector2(-30, 16),
            Size = new Vector2(60, 6),
            Color = new Color(0.15f, 0.10f, 0.10f),
        };
        AddChild(_barraFundo);

        _barraVida = new ColorRect
        {
            Position = new Vector2(-30, 16),
            Color = new Color(0.70f, 0.15f, 0.15f),
        };
        AddChild(_barraVida);

        AtualizarHp();
    }

    public override void _PhysicsProcess(double delta)
    {
        float distancia = Position.DistanceTo(_jogador.Position);

        // Fora da percepção: fica parado (sem aggro persistente).
        // Dentro: persegue em linha reta até a distância de parada.
        if (distancia > Percepcao || distancia < DistanciaParada)
            Velocity = Vector2.Zero;
        else
            Velocity = (_jogador.Position - Position).Normalized() * Velocidade;

        MoveAndSlide();

        // Ataque espacial: dentro do alcance, a cada AtaqueCooldown o inimigo
        // sinaliza que está em posição de atacar (o Jogo pergunta ao Motor).
        // Sair da percepção (o jogador fugiu) zera o cooldown.
        if (distancia > Percepcao)
        {
            _tempoSemAtaque = 0;
        }
        else if (distancia <= DistanciaAtaque)
        {
            _tempoSemAtaque += delta;
            if (_tempoSemAtaque >= AtaqueCooldown)
            {
                _tempoSemAtaque = 0;
                AtaqueSolicitado?.Invoke();
            }
        }
    }

    // A barra reflete o estado real do Motor (Monstro.VidaAtual)
    public void AtualizarHp()
    {
        float razao = _monstro.VidaMaxima > 0 ? (float)_monstro.VidaAtual / _monstro.VidaMaxima : 0f;
        _barraVida.Size = new Vector2(60f * Mathf.Clamp(razao, 0f, 1f), 6f);
    }
}
