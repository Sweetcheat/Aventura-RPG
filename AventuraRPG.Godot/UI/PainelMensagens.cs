using Godot;

/* Área de mensagens: apresenta as mensagens retornadas pelo Motor (Partida).
   Não gera, altera ou reescreve nenhum texto de gameplay. */
public partial class PainelMensagens : PanelContainer
{
    private RichTextLabel _texto;

    // Texto completo (usado apenas pela validação automática em modo headless).
    // GetParsedText() lê o conteúdo real do tag stack: a propriedade Text é um
    // buffer separado que não reflete textos adicionados via AppendText
    public string Texto => _texto != null ? _texto.GetParsedText() : "";

    public override void _Ready()
    {
        AddThemeStyleboxOverride("panel", new StyleBoxFlat { BgColor = new Color(0.08f, 0.09f, 0.11f) });

        var margem = new MarginContainer
        {
            SizeFlagsVertical = Control.SizeFlags.ExpandFill,
        };
        margem.AddThemeConstantOverride("margin_left", 8);
        margem.AddThemeConstantOverride("margin_right", 8);
        margem.AddThemeConstantOverride("margin_top", 4);
        margem.AddThemeConstantOverride("margin_bottom", 4);
        AddChild(margem);

        _texto = new RichTextLabel
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill,
        };
        margem.AddChild(_texto);
    }

    public void AdicionaMensagem(string mensagem)
    {
        _texto.AppendText(mensagem);
        _texto.ScrollToLine(_texto.GetLineCount());
    }
}
