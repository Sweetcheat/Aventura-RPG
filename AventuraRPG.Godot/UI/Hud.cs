using Godot;
using Motor;

/* HUD: mostra o estado real do jogador (vem da Partida/Jogador do Motor);
   o Godot não guarda cópia independente de estado.
   Posição e aparência são provisórias: o HUD pode ser reposicionado no
   futuro (topo/lateral/inferior) sem tocar nas regras. */
public partial class Hud : PanelContainer
{
	private Label _labelLocal;
	private Label _labelVida;
	private Label _labelExperiencia;
	private Label _labelLevel;
	private Label _labelOuro;

	public override void _Ready()
	{
		AddThemeStyleboxOverride("panel", new StyleBoxFlat { BgColor = new Color(0.10f, 0.11f, 0.14f) });

		var linha = new HBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
		linha.AddThemeConstantOverride("separation", 24);
		AddChild(linha);

		_labelLocal       = CriaRotulo(linha);
		_labelVida        = CriaRotulo(linha);
		_labelExperiencia = CriaRotulo(linha);
		_labelLevel       = CriaRotulo(linha);
		_labelOuro        = CriaRotulo(linha);
	}

	private Label CriaRotulo(HBoxContainer linha)
	{
		var rotulo = new Label { Text = " " };
		linha.AddChild(rotulo);
		return rotulo;
	}

	// Texto do rótulo de Local (usado apenas pela validação automática em modo headless)
	public string TextoLocal => _labelLocal != null ? _labelLocal.Text : "";

	public void Atualizar(Jogador jogador)
	{
		_labelLocal.Text       = $"Local: {jogador.LocalAtual.Nome}";
		_labelVida.Text        = $"HP: {jogador.VidaAtual}/{jogador.VidaMaximaEfetiva}";
		_labelExperiencia.Text = $"XP: {jogador.PontosExperiencia}";
		_labelLevel.Text       = $"LV: {jogador.Level}";
		_labelOuro.Text        = $"Ouro: {jogador.Ouro}";
	}
}
