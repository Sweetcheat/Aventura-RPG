using System.Text;
using Godot;
using Motor;

/* Diário de quests: overlay que apresenta o estado real do jogador
   (Partida -> Jogador -> Quests, tudo do Motor). O Motor decide quais
   quests foram recebidas e completadas; o Godot apenas apresenta
   (✓ concluída / – em andamento + objetivo).
   Aberto/fechado com a tecla Q (ação abrir_quests do Input Map,
   configurada no project.godot). */
public partial class PainelQuests : Control
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
            Text = "DIÁRIO DE QUESTS",
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
            Text = "Pressione Q para fechar",
            HorizontalAlignment = HorizontalAlignment.Right,
            Modulate = new Color(1f, 1f, 1f, 0.6f),
        };
        coluna.AddChild(rodape);
    }

    // Relê o estado real do Motor (nenhuma cópia das quests fica aqui)
    public void Atualizar(Jogador jogador)
    {
        foreach (var filho in _lista.GetChildren())
        {
            _lista.RemoveChild(filho);
            filho.Free();
        }

        if (jogador.Quests.Count == 0)
        {
            _lista.AddChild(new Label { Text = "Você ainda não tem quests." });
            return;
        }

        foreach (var quest in jogador.Quests)
        {
            var titulo = new Label { Text = (quest.Completado ? "✓ " : "– ") + quest.Detalhes.Nome };
            titulo.AddThemeFontSizeOverride("font_size", 18);
            _lista.AddChild(titulo);

            var objetivo = new Label
            {
                Text = "Objetivo: " + quest.Detalhes.Descricao,
                AutowrapMode = TextServer.AutowrapMode.Word,
                Modulate = new Color(1f, 1f, 1f, 0.8f),
            };
            _lista.AddChild(objetivo);

            var status = new Label
            {
                Text = quest.Completado ? "    Recompensa recebida" : "    Em andamento",
                Modulate = new Color(1f, 1f, 1f, 0.6f),
            };
            _lista.AddChild(status);
        }
    }
}
