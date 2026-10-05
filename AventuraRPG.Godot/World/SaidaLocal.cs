using System;
using Godot;
using Motor;

/* Uma saída no cenário 2D: a área onde o jogador pode interagir (tecla E,
   ação "interagir" do Input Map) para ir a outro Local. O destino é o Local
   REAL do grafo do Motor (a apresentação não duplica o mundo) e a decisão
   do movimento é sempre da Partida.MoverPara (Motor).
   O prompt só aparece quando o jogador está perto E o Local atual do Motor
   é a área que esta saída representa. */
public partial class SaidaLocal : Area2D
{
    // O Local da área onde esta saída está (ex.: a Casa)
    public Local Area { get; set; }
    // O destino, resolvido no grafo do Motor (ex.: a Praça)
    public Local Destino { get; set; }

    // A apresentação solicita o movimento (mesmo fluxo dos antigos botões:
    // o destino já resolvido vai para a Partida)
    public event Action<Local> SaidaUsada;

    private Local _localAtual;
    private bool _jogadorProximo;
    private Label _prompt;

    public bool NaProximidade => _jogadorProximo;
    public bool PodeInteragir => _jogadorProximo && ReferenceEquals(_localAtual, Area);

    public override void _Ready()
    {
        var forma = new CollisionShape2D { Shape = new RectangleShape2D { Size = new Vector2(90, 70) } };
        AddChild(forma);

        _prompt = new Label
        {
            Text = "E — Ir para " + (Destino != null ? Destino.Nome : "?"),
            Visible = false,
            Position = new Vector2(-80, -36),
        };
        AddChild(_prompt);

        BodyEntered += _AoEntrarCorpo;
        BodyExited += _AoSairDoCorpo;
    }

    private void _AoEntrarCorpo(Node2D corpo)
    {
        if (corpo is Jogador2D)
        {
            _jogadorProximo = true;
            AtualizaPrompt();
        }
    }

    private void _AoSairDoCorpo(Node2D corpo)
    {
        if (corpo is Jogador2D)
        {
            _jogadorProximo = false;
            AtualizaPrompt();
        }
    }

    // O Local atual vem do Motor (a apresentação atualiza após cada movimento)
    public void Atualizar(Local local)
    {
        _localAtual = local;
        AtualizaPrompt();
    }

    private void AtualizaPrompt()
    {
        _prompt.Visible = PodeInteragir;
    }

    public override void _UnhandledInput(InputEvent evento)
    {
        if (Input.IsActionJustPressed("interagir"))
            Interagir();
    }

    // Única entrada da saída: só o Motor decide o movimento
    public void Interagir()
    {
        if (!PodeInteragir)
            return;

        SaidaUsada?.Invoke(Destino);
    }
}
