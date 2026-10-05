using System.Linq;
using System.Threading.Tasks;
using Godot;
using Motor;

/* Raiz do jogo: dona da Partida (todas as regras estão no Motor), do mundo
   2D (a área ativa = apresentação visual do Local atual) e da UI.
   O Godot não decide nada de gameplay: apresenta o estado, recebe o Local de
   destino (das saídas de cada área 2D), delega a Partida.MoverPara e exibe
   o resultado. */
public partial class Jogo : Node2D
{
    // Alcance do ataque corpo a corpo (apresentação): fora dele o golpe não acontece
    private const float ALCANCE_ATAQUE = 50f;

    private Partida _partida;

    private AreaLocal _areaAtiva;

    private Hud _hud;
    private PainelMensagens _mensagens;
    private PainelInventario _painelInventario;
    private PainelQuests _painelQuests;

    public override void _Ready()
    {
        CriaInterface();

        // Mesma inicialização que o Form WinForms fazia no construtor:
        // cria a Partida e faz o primeiro MoverPara para a Casa
        _partida = new Partida();
        foreach (var mensagem in _partida.MoverPara(Mundo.LocalPorID(Mundo.LOCAL_ID_CASA)))
            _mensagens.AdicionaMensagem(mensagem);

        AtualizaInterface();

        GD.Print($"[Jogo] Partida criada. Local: {_partida.Jogador.LocalAtual.Nome}, " +
                 $"Vida: {_partida.Jogador.VidaAtual}/{_partida.Jogador.VidaMaximaEfetiva}, " +
                 $"Ouro: {_partida.Jogador.Ouro}");

        if (DisplayServer.GetName() == "headless")
            _ = AutoTeste();
    }

    // Layout provisório: painéis em coluna vertical, por cima do mundo 2D.
    // As posições não são definitivas (o HUD pode ser reposicionado no futuro).
    private void CriaInterface()
    {
        var camada = new CanvasLayer();
        AddChild(camada);

        var layout = new VBoxContainer
        {
            AnchorLeft = 0f,
            AnchorTop = 0f,
            AnchorRight = 1f,
            AnchorBottom = 1f,
            OffsetLeft = 8f,
            OffsetTop = 8f,
            OffsetRight = -8f,
            OffsetBottom = -8f,
        };
        layout.AddThemeConstantOverride("separation", 8);
        camada.AddChild(layout);

        var titulo = new Label
        {
            Text = "AVENTURA RPG",
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        titulo.AddThemeFontSizeOverride("font_size", 32);
        layout.AddChild(titulo);

        _hud = new Hud();
        layout.AddChild(_hud);

        // Espaço central transparente: o mundo 2D (área ativa) fica atrás da UI
        var espaco = new Control
        {
            SizeFlagsVertical = Control.SizeFlags.ExpandFill,
            SizeFlagsStretchRatio = 2,
        };
        layout.AddChild(espaco);

        _mensagens = new PainelMensagens();
        layout.AddChild(_mensagens);

        // Overlays: inventário (tecla I) e diário de quests (tecla Q) —
        // começam ocultos e ficam sobre a interface principal
        _painelInventario = new PainelInventario { Visible = false };
        camada.AddChild(_painelInventario);
        _painelQuests = new PainelQuests { Visible = false };
        camada.AddChild(_painelQuests);
    }

    // Os Locais que já têm mapa 2D. Novos Locais = nova classe de área +
    // uma linha aqui.
    private AreaLocal CriaArea(Local local)
    {
        return local.ID switch
        {
            Mundo.LOCAL_ID_CASA  => new AreaCasa(local),
            Mundo.LOCAL_ID_PRACA => new AreaPraca(local),
            _ => null, // Local sem área 2D (as próximas fases adicionam)
        };
    }

    // Troca a área visual quando o Motor muda o Local. O estado de RPG
    // (Partida/Jogador) nunca é recriado: só a apresentação muda.
    private void TrocaArea()
    {
        var local = _partida.Jogador.LocalAtual;

        if (ReferenceEquals(_areaAtiva?.Local, local))
            return; // já está na área correta (evita remontar a cena à toa)

        var nova = CriaArea(local);
        if (nova == null)
            return; // Local sem área 2D: mantém a área visual atual

        if (_areaAtiva != null)
            _areaAtiva.Free();

        nova.Cria();
        _areaAtiva = nova;
        AddChild(nova);

        foreach (var saida in nova.Saidas)
            saida.SaidaUsada += MoverPara;
    }

    // Painéis abertos por tecla, via Input Map (project.godot:
    // abrir_inventario = I, abrir_quests = Q; ESC é a ação embutida
    // ui_cancel). As teclas podem ser remapeadas no editor sem tocar no código.
    public override void _UnhandledInput(InputEvent evento)
    {
        if (Input.IsActionJustPressed("abrir_inventario"))
            AlternaPainel(true);
        else if (Input.IsActionJustPressed("abrir_quests"))
            AlternaPainel(false);
        else if (Input.IsActionJustPressed("ui_cancel"))
            FechaPainelAberto();
        else if (Input.IsActionJustPressed("atacar"))
            AtacarInimigo();
    }

    // Abre/fecha o painel pedido (ao abrir, fecha o outro, se estiver aberto).
    // Ao abrir, o conteúdo é relido do estado real do Motor.
    private void AlternaPainel(bool inventario)
    {
        Control abrir  = inventario ? _painelInventario : _painelQuests;
        Control fechar = inventario ? _painelQuests     : _painelInventario;

        if (abrir.Visible)
        {
            abrir.Visible = false;
        }
        else
        {
            fechar.Visible = false;
            if (inventario)
                _painelInventario.Atualizar(_partida.Jogador);
            else
                _painelQuests.Atualizar(_partida.Jogador);
            abrir.Visible = true;
        }

        AtualizaBloqueioMovimento();
    }

    // ESC fecha o painel que estiver aberto (sem menu principal nesta fase)
    private void FechaPainelAberto()
    {
        if (!_painelInventario.Visible && !_painelQuests.Visible)
            return;

        _painelInventario.Visible = false;
        _painelQuests.Visible = false;
        AtualizaBloqueioMovimento();
    }

    // Com um painel aberto, o jogador não recebe input de WASD
    // (apenas apresentação — o Motor não sabe disso)
    private void AtualizaBloqueioMovimento()
    {
        bool painelAberto = _painelInventario.Visible || _painelQuests.Visible;
        if (_areaAtiva != null)
            _areaAtiva.Jogador.Ativo = !painelAberto;
    }

    // Única entrada de movimento, independente de quem originou a solicitação
    // (as saídas de cada área 2D). As regras (gate, cura, quests, monstros)
    // são todas da Partida.
    private void MoverPara(Local destino)
    {
        if (destino == null)
            return;

        foreach (var mensagem in _partida.MoverPara(destino))
            _mensagens.AdicionaMensagem(mensagem);

        AtualizaInterface();
    }

    // Ataque corpo a corpo: o Godot só decide se o jogador está ao alcance do
    // inimigo; todas as regras (dano, crítico, morte, XP, ouro, loot) são do
    // Motor (Partida.AtaqueDoJogador). IsActionJustPressed já limita a um
    // golpe por pressionada — sem cooldown extra.
    private void AtacarInimigo()
    {
        if (_areaAtiva?.Inimigo == null)
            return;

        var arma = ObtemArmaAtual();
        if (arma == null)
            return;

        // Fora do alcance: nada acontece (alcance é apresentação, não regra do Motor)
        if (_areaAtiva.Jogador.Position.DistanceTo(_areaAtiva.Inimigo.Position) > ALCANCE_ATAQUE)
            return;

        foreach (var mensagem in _partida.AtaqueDoJogador(arma))
            _mensagens.AdicionaMensagem(mensagem);

        AtualizaInterface();
    }

    // A arma em uso: a primeira Arma do inventário (o protótipo sempre tem a
    // espada; a escolha de arma entra em uma fase posterior)
    private Arma ObtemArmaAtual()
    {
        foreach (var ii in _partida.Jogador.Inventario)
            if (ii.Detalhes is Arma arma && ii.Quantidade > 0)
                return arma;

        return null;
    }

    private void AtualizaInterface()
    {
        var jogador = _partida.Jogador;
        _hud.Atualizar(jogador);
        AtualizaBloqueioMovimento();
        TrocaArea();

        // As saídas da área sempre refletem o Local atual do Motor
        if (_areaAtiva != null)
            foreach (var saida in _areaAtiva.Saidas)
                saida.Atualizar(jogador.LocalAtual);

        // O inimigo visual reflete o MonstroAtual do Motor (aparece, perde HP
        // ou desaparece conforme o estado real)
        _areaAtiva?.AtualizaInimigo(_partida.MonstroAtual);

        // Mantém os painéis abertos sincronizados com o estado real do Motor
        if (_painelInventario.Visible)
            _painelInventario.Atualizar(jogador);
        if (_painelQuests.Visible)
            _painelQuests.Atualizar(jogador);
    }

    // Validação automática (apenas em modo headless): percorre os fluxos de
    // navegação, os painéis e o mundo 2D (movimento, colisão, transição
    // Casa <-> Praça) pela mesma rota da apresentação e confere as decisões
    // do Motor.
    private async Task AutoTeste()
    {
        int falhas = 0;

        void Verificar(bool ok, string nome)
        {
            GD.Print($"[AutoTeste] {(ok ? "OK   " : "FALHA")} {nome}");
            if (!ok)
                falhas++;
        }

        async Task Frames(int n)
        {
            for (int i = 0; i < n; i++)
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        }

        var jogador = _partida.Jogador;

        Verificar(jogador.LocalAtual.ID == Mundo.LOCAL_ID_CASA, "início na Casa");
        Verificar(jogador.VidaAtual == 15 && jogador.VidaMaximaEfetiva == 15, "HP 15/15");
        Verificar(jogador.Ouro == 0, "Ouro 0");
        Verificar(jogador.PontosExperiencia == 0, "XP 0");
        Verificar(jogador.Level == 1, "Level 1");

        MoverPara(Mundo.LocalPorID(Mundo.LOCAL_ID_PRACA));
        Verificar(jogador.LocalAtual.ID == Mundo.LOCAL_ID_PRACA, "Casa -> Praça");

        MoverPara(Mundo.LocalPorID(Mundo.LOCAL_ID_POSTO_DE_GUARDA));
        Verificar(jogador.LocalAtual.ID == Mundo.LOCAL_ID_PRACA, "Posto bloqueado sem o Passe (fica na Praça)");
        Verificar(_mensagens.Texto.Contains("Passe de Aventureiro"), "mensagem de gate vem do Motor");

        jogador.AdicioneItemAoInventario(Mundo.ItemPorID(Mundo.ITEM_ID_PASSE_AVENTUREIRO));
        MoverPara(Mundo.LocalPorID(Mundo.LOCAL_ID_POSTO_DE_GUARDA));
        Verificar(jogador.LocalAtual.ID == Mundo.LOCAL_ID_POSTO_DE_GUARDA, "Posto liberado com o Passe");

        MoverPara(Mundo.LocalPorID(Mundo.LOCAL_ID_PRACA));
        MoverPara(Mundo.LocalPorID(Mundo.LOCAL_ID_CASA));
        MoverPara(Mundo.LocalPorID(Mundo.LOCAL_ID_PRACA));
        MoverPara(Mundo.LocalPorID(Mundo.LOCAL_ID_CABANA_DOS_ALQUIMISTAS));
        Verificar(jogador.LocalAtual.ID == Mundo.LOCAL_ID_CABANA_DOS_ALQUIMISTAS, "Casa -> Praça -> Cabana dos Alquimistas");
        Verificar(_mensagens.Texto.Contains("quest"), "mensagem da quest dos Alquimistas vem do Motor");

        // Input Map: ações e teclas configuradas (project.godot)
        Verificar(InputMap.HasAction("abrir_inventario") && InputMap.HasAction("abrir_quests")
                && InputMap.HasAction("interagir")
                && InputMap.HasAction("atacar")
                && InputMap.HasAction("mover_cima") && InputMap.HasAction("mover_baixo")
                && InputMap.HasAction("mover_esquerda") && InputMap.HasAction("mover_direita"),
            "Input Map: todas as ações existem");
        Verificar(ConteemTecla("abrir_inventario", Key.I), "tecla I mapeada para abrir_inventario");
        Verificar(ConteemTecla("abrir_quests", Key.Q), "tecla Q mapeada para abrir_quests");
        Verificar(ConteemTecla("interagir", Key.E), "tecla E mapeada para interagir");
        Verificar(ConteemTecla("atacar", Key.Space), "tecla Espaço mapeada para atacar");
        Verificar(ConteemTecla("mover_cima", Key.W) && ConteemTecla("mover_cima", Key.Up), "mover_cima mapeia W + seta cima");
        Verificar(ConteemTecla("mover_baixo", Key.S) && ConteemTecla("mover_baixo", Key.Down), "mover_baixo mapeia S + seta baixo");
        Verificar(ConteemTecla("mover_esquerda", Key.A) && ConteemTecla("mover_esquerda", Key.Left), "mover_esquerda mapeia A + seta esquerda");
        Verificar(ConteemTecla("mover_direita", Key.D) && ConteemTecla("mover_direita", Key.Right), "mover_direita mapeia D + seta direita");

        Verificar(!_painelInventario.Visible && !_painelQuests.Visible, "painéis começam ocultos");

        AlternaPainel(true);
        Verificar(_painelInventario.Visible && !_painelQuests.Visible, "I abre o inventário");
        Verificar(_painelInventario.TextoConteudo.Contains("Espada Enferrujada")
                && _painelInventario.TextoConteudo.Contains("Passe de Aventureiro"),
            "inventário mostra o estado real do jogador (Espada + Passe)");

        AlternaPainel(false);
        Verificar(_painelQuests.Visible && !_painelInventario.Visible, "Q troca diretamente para o diário de quests");
        Verificar(_painelQuests.TextoConteudo.Contains("Limpe o Jardim dos Alquimistas")
                && _painelQuests.TextoConteudo.Contains("Objetivo:"),
            "diário mostra a quest recebida do Motor");

        AlternaPainel(false);
        Verificar(!_painelQuests.Visible && !_painelInventario.Visible, "Q fecha o diário");

        AlternaPainel(true);
        FechaPainelAberto();
        Verificar(!_painelInventario.Visible && !_painelQuests.Visible, "ESC fecha o painel aberto");

        // ---- Mundo 2D: movimento, colisão e a transição Casa <-> Praça ----
        // O jogador (estado do Motor) está na Cabana por causa dos checks de
        // quest acima; a área da Casa só é válida com o Motor na Casa
        MoverPara(Mundo.LocalPorID(Mundo.LOCAL_ID_PRACA));
        MoverPara(Mundo.LocalPorID(Mundo.LOCAL_ID_CASA));

        await Frames(3);

        Verificar(_areaAtiva != null && _areaAtiva.Local.ID == Mundo.LOCAL_ID_CASA, "área visual inicial = Casa");

        var jogador2d = _areaAtiva.Jogador;
        Vector2 posicaoInicial = jogador2d.Position;
        Verificar(jogador2d != null && posicaoInicial.IsFinite(), "jogador 2D existe com posição inicial válida");

        Input.ActionPress("mover_direita");
        await Frames(20);
        Input.ActionRelease("mover_direita");
        Verificar(jogador2d.Position.X > posicaoInicial.X + 10f, "WASD move o personagem (mover_direita)");

        Input.ActionPress("mover_baixo");
        await Frames(100);
        Input.ActionRelease("mover_baixo");
        Verificar(jogador2d.Position.Y < 400f, "colisão impede atravessar as paredes/limites");

        // Manipulação de teste: coloca o jogador dentro da área da saída
        var saidaCasa = _areaAtiva.Saidas[0];
        jogador2d.Position = saidaCasa.Position;
        await Frames(10);
        Verificar(saidaCasa.NaProximidade, "proximidade da saída ativa a interação");

        saidaCasa.Interagir();
        Verificar(_partida.Jogador.LocalAtual.ID == Mundo.LOCAL_ID_PRACA, "E chama Partida.MoverPara(Praça) e o Motor muda o Local");
        Verificar(_areaAtiva != null && _areaAtiva.Local.ID == Mundo.LOCAL_ID_PRACA, "representação visual troca para a Praça");
        Verificar(_hud.TextoLocal.Contains("Praça"), "HUD reflete o novo Local");
        Verificar(_areaAtiva.Jogador != null && _areaAtiva.Jogador.Position.IsFinite(), "jogador aparece na Praça");

        // ---- Combate espacial: o Rato da Praça (MonstroAtual do Motor) ----
        // Manipulação de teste: posiciona o jogador em relação ao inimigo
        await Frames(3);

        Verificar(_areaAtiva.Inimigo != null, "um inimigo existe na Praça");
        Verificar(_partida.MonstroAtual != null && _partida.MonstroAtual.ID == Mundo.MONSTRO_ID_RATO, "MonstroAtual do Motor = Rato da Praça");

        // Manipulação de teste: drop determinístico (100%) para o check de loot
        _partida.MonstroAtual.LootTable.Clear();
        _partida.MonstroAtual.LootTable.Add(new ItemLoot(Mundo.ItemPorID(Mundo.ITEM_ID_PELO_DE_RATO), 100, true));

        var jogadorPraca = _areaAtiva.Jogador;
        var inimigo = _areaAtiva.Inimigo;

        // ---- Perseguição: o Rato detecta o jogador e o acompanha ----
        Verificar(jogadorPraca.Position.DistanceTo(inimigo.Position) > Inimigo2D.Percepcao,
            "jogador e Rato começam separados (fora da percepção)");

        Vector2 ratoParado = inimigo.Position;
        await Frames(30);
        Verificar(inimigo.Position == ratoParado, "Rato permanece parado com o jogador fora da percepção");

        jogadorPraca.Position = inimigo.Position + new Vector2(-100f, 0f);
        await Frames(40);
        Verificar(jogadorPraca.Position.DistanceTo(inimigo.Position) < 100f,
            "jogador na percepção: Rato se aproxima (distância diminui)");

        await Frames(60);
        float distParada = jogadorPraca.Position.DistanceTo(inimigo.Position);
        Verificar(distParada < Inimigo2D.DistanciaParada + 15f && distParada > Inimigo2D.DistanciaParada - 10f,
            "Rato para na distância mínima (não fica sobreposto ao jogador)");

        Vector2 posJogadorAntes = jogadorPraca.Position;
        Input.ActionPress("mover_direita");
        await Frames(10);
        Input.ActionRelease("mover_direita");
        Verificar(jogadorPraca.Position.X > posJogadorAntes.X + 5f,
            "jogador continua se movendo com o Rato por perto");

        // O Rato persegue o jogador no canto da Praça: deve permanecer na sala
        jogadorPraca.Position = new Vector2(760f, 760f);
        inimigo.Position = new Vector2(400f, 400f);
        await Frames(120);
        Verificar(inimigo.Position.X > 0f && inimigo.Position.X < 800f
                && inimigo.Position.Y > 0f && inimigo.Position.Y < 800f,
            "Rato respeita os limites da Praça (colisão com as paredes)");

        int hpAntes = _partida.MonstroAtual.VidaAtual;

        // Fora do alcance: o ataque não faz nada
        jogadorPraca.Position = inimigo.Position + new Vector2(-200f, 0f);
        await Frames(3);
        AtacarInimigo();
        Verificar(_partida.MonstroAtual.VidaAtual == hpAntes, "ataque fora do alcance não causa dano");

        // No alcance: o Motor aplica o dano
        jogadorPraca.Position = inimigo.Position + new Vector2(-40f, 0f);
        await Frames(3);
        AtacarInimigo();
        Verificar(_partida.MonstroAtual == null || _partida.MonstroAtual.VidaAtual < hpAntes, "ataque dentro do alcance causa dano");
        Verificar(_areaAtiva.Inimigo != null || _partida.MonstroAtual == null, "HP visual acompanha o Motor (o inimigo só some se o Motor o matou)");

        // Derrota: ataca até o Motor zerar o HP
        int golpes = 0;
        while (_partida.MonstroAtual != null && golpes < 10)
        {
            AtacarInimigo();
            golpes++;
        }
        Verificar(_partida.MonstroAtual == null, "inimigo morre quando o HP chega a zero (Motor)");
        Verificar(_areaAtiva.Inimigo == null, "representação visual do inimigo é removida");
        Verificar(jogador.PontosExperiencia == 3 && jogador.Ouro == 10, "XP (3) e ouro (10) da morte creditados pelo Motor");
        Verificar(jogador.Inventario.Any(ii => ii.Detalhes.ID == Mundo.ITEM_ID_PELO_DE_RATO && ii.Quantidade > 0), "loot da morte vai para o inventário (garantia do item comum)");
        Verificar(jogador.VidaAtual == 15, "inimigo estático não ataca (vida do jogador intacta)");

        // Volta: Praça -> Casa pela saída da Praça
        var saidaPraca = _areaAtiva.Saidas[0];
        _areaAtiva.Jogador.Position = saidaPraca.Position;
        await Frames(10);
        Verificar(saidaPraca.NaProximidade, "saída da Praça detecta o jogador");

        saidaPraca.Interagir();
        Verificar(_partida.Jogador.LocalAtual.ID == Mundo.LOCAL_ID_CASA, "E na Praça devolve o Local para a Casa");
        Verificar(_areaAtiva != null && _areaAtiva.Local.ID == Mundo.LOCAL_ID_CASA, "representação visual volta para a Casa");
        Verificar(_areaAtiva.Jogador != null && _areaAtiva.Jogador.Position.IsFinite(), "jogador aparece na Casa");

        // Os painéis continuam funcionando sobre o mundo 2D
        Vector2 posComPainel = _areaAtiva.Jogador.Position;
        AlternaPainel(true);
        Verificar(_painelInventario.Visible, "inventário continua abrindo sobre o mundo 2D");
        Input.ActionPress("mover_direita");
        await Frames(10);
        Input.ActionRelease("mover_direita");
        Verificar(_areaAtiva.Jogador.Position == posComPainel, "jogador bloqueado com o painel aberto");
        AlternaPainel(false); // troca para o diário de quests
        Verificar(_painelQuests.Visible && !_painelInventario.Visible, "quests continuam abrindo sobre o mundo 2D");

        AlternaPainel(false);
        Verificar(!_painelInventario.Visible && !_painelQuests.Visible, "teclas continuam fechando os painéis");

        GD.Print(falhas == 0 ? "[AutoTeste] TODOS OS CHECKS OK" : $"[AutoTeste] {falhas} CHECK(S) FALHARAM");
    }

    private static bool ConteemTecla(string acao, Key tecla)
    {
        // O Godot normaliza teclas especiais (setas) para PhysicalKeycode ao
        // carregar o Input Map; letras chegam como Keycode virtual
        foreach (var evento in InputMap.ActionGetEvents(acao))
        {
            if (evento is InputEventKey teclaEvento &&
                (teclaEvento.Keycode == tecla || teclaEvento.PhysicalKeycode == tecla))
                return true;
        }

        return false;
    }
}
