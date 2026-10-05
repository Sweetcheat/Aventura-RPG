using System.Text;
using Godot;
using Motor;

/* Inventário: overlay que apresenta o estado real do jogador
   (Partida -> Jogador -> Inventario, tudo do Motor). O Godot não guarda
   cópia do inventário: o conteúdo é relido do Motor a cada atualização.
   Aberto/fechado com a tecla I (ação abrir_inventario do Input Map,
   configurada no project.godot).
   Somente visualização nesta fase: sem usar/equipar/vender. */
public partial class PainelInventario : Control
{
    private VBoxContainer _lista;

    // Conteúdo completo (usado apenas pela validação automática em modo headless)
    public string TextoConteudo
    {
        get
        {
            if (_lista == null)
                return "";

            var texto = new StringBuilder();
            foreach (var filho in _lista.GetChildren())
            {
                if (filho is Label label)
                    texto.Append(label.Text).Append('\n');
            }

            return texto.ToString();
        }
    }

    public override void _Ready()
    {
        AnchorTop = 0f;
        AnchorLeft = 0f;
        AnchorRight = 1f;
        AnchorBottom = 1f;

        var escurece = new ColorRect
        {
            AnchorTop = 0f,
            AnchorLeft = 0f,
            AnchorRight = 1f,
            AnchorBottom = 1f,
            Color = new Color(0f, 0f, 0f, 0.6f),
        };
        AddChild(escurece);

        var centro = new CenterContainer
        {
            AnchorTop = 0f,
            AnchorLeft = 0f,
            AnchorRight = 1f,
            AnchorBottom = 1f,
        };
        AddChild(centro);

        var caixa = new PanelContainer { CustomMinimumSize = new Vector2(560, 0) };
        caixa.AddThemeStyleboxOverride("panel", new StyleBoxFlat { BgColor = new Color(0.10f, 0.11f, 0.14f) });
        centro.AddChild(caixa);

        var coluna = new VBoxContainer();
        coluna.AddThemeConstantOverride("separation", 10);
        caixa.AddChild(coluna);

        var titulo = new Label
        {
            Text = "INVENTÁRIO",
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        titulo.AddThemeFontSizeOverride("font_size", 24);
        coluna.AddChild(titulo);

        var rolagem = new ScrollContainer
        {
            CustomMinimumSize = new Vector2(0, 260),
            SizeFlagsVertical = Control.SizeFlags.ExpandFill,
        };
        coluna.AddChild(rolagem);

        _lista = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        rolagem.AddChild(_lista);

        var rodape = new Label
        {
            Text = "Pressione I para fechar",
            HorizontalAlignment = HorizontalAlignment.Right,
            Modulate = new Color(1f, 1f, 1f, 0.6f),
        };
        coluna.AddChild(rodape);
    }

    // Relê o estado real do Motor (nenhuma cópia do inventário fica aqui)
    public void Atualizar(Jogador jogador)
    {
        foreach (var filho in _lista.GetChildren())
        {
            _lista.RemoveChild(filho);
            filho.Free();
        }

        if (jogador.Inventario.Count == 0)
        {
            _lista.AddChild(new Label { Text = "Inventário vazio." });
            return;
        }

        foreach (var item in jogador.Inventario)
        {
            var nome = new Label { Text = item.Detalhes.Nome };
            nome.AddThemeFontSizeOverride("font_size", 18);
            _lista.AddChild(nome);

            var quantidade = new Label
            {
                Text = $"    Quantidade: {item.Quantidade}",
                Modulate = new Color(1f, 1f, 1f, 0.7f),
            };
            _lista.AddChild(quantidade);
        }
    }
}
