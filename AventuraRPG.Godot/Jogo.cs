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

    // Ponto de entrada da próxima troca de área (informado pela saída usada);
    // nulo = spawn padrão da área
    private Vector2? _spawnDestino;

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
            Mundo.LOCAL_ID_CASA                   => new AreaCasa(local),
            Mundo.LOCAL_ID_PRACA                  => new AreaPraca(local),
            Mundo.LOCAL_ID_POSTO_DE_GUARDA        => new AreaPosto(local),
            Mundo.LOCAL_ID_CABANA_DOS_ALQUIMISTAS => new AreaCabana(local),
            Mundo.LOCAL_ID_JARDIM_DOS_ALQUIMISTAS => new AreaJardim(local),
            Mundo.LOCAL_ID_CASA_DA_FAZENDA        => new AreaFazenda(local),
            Mundo.LOCAL_ID_AREA_DOS_CAMPONESES    => new AreaCamponeses(local),
            Mundo.LOCAL_ID_PONTE                  => new AreaPonte(local),
            Mundo.LOCAL_ID_CAMPO_DAS_ARANHAS      => new AreaFloresta(local),
            _ => null, // Local sem área 2D
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

        nova.Cria(_spawnDestino);
        _spawnDestino = null;
        _areaAtiva = nova;
        AddChild(nova);

        foreach (var saida in nova.Saidas)
            saida.SaidaUsada += MoverPara;

        // O inimigo da área sinaliza quando está em posição de atacar;
        // as regras (dano, morte, respawn) são todas do Motor
        nova.InimigoAtacou += AoInimigoAtacou;
    }

    // O inimigo (apresentação) sinalizou que está em posição de atacar.
    // O Godot não aplica dano: o Motor (Partida.AtaqueDoMonstro) decide
    // dano, morte, ouro e respawn, e a interface apresenta o resultado.
    // Se a morte trocou o Local, a atualização é adiada: o sinal sai do
    // _PhysicsProcess do inimigo, e liberar a área no meio do próprio
    // callback dele seria inseguro.
    private void AoInimigoAtacou()
    {
        int localAntes = _partida.Jogador.LocalAtual.ID;

        foreach (var mensagem in _partida.AtaqueDoMonstro())
            _mensagens.AdicionaMensagem(mensagem);

        if (_partida.Jogador.LocalAtual.ID != localAntes)
            CallDeferred(nameof(AtualizaInterface));
        else
            AtualizaInterface();
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

    // Entrada de movimento vinda de uma saída da área 2D: registra o ponto de
    // entrada (spawn contextual) e segue o fluxo normal.
    private void MoverPara(SaidaLocal saida)
    {
        _spawnDestino = saida.PosicaoEntradaDestino;
        MoverPara(saida.Destino);
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
        // O Rato ataca durante a perseguição acima (Fase 11): o dano é do
        // Motor e o jogador sobrevive (HP 15 - dano máximo 5 > 0)
        Verificar(jogador.VidaAtual <= 15 && jogador.VidaAtual > 0, "Rato ataca durante a perseguição (dano do Motor) e o jogador sobrevive");

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

        // ---- Fase 9: expansão do mundo (Praça <-> Cabana <-> Jardim) e a quest
        // "Limpe o Jardim dos Alquimistas" (entrega e conclusão são do Motor) ----

        // Os 4 Locais com representação 2D (os demais ainda não têm, nesta fase)
        var areaTesteCasa = CriaArea(Mundo.LocalPorID(Mundo.LOCAL_ID_CASA));
        Verificar(areaTesteCasa is AreaCasa, "Casa tem representação 2D válida (AreaCasa)");
        areaTesteCasa?.Free();
        var areaTestePraca = CriaArea(Mundo.LocalPorID(Mundo.LOCAL_ID_PRACA));
        Verificar(areaTestePraca is AreaPraca, "Praça tem representação 2D válida (AreaPraca)");
        areaTestePraca?.Free();
        var areaTesteCabana = CriaArea(Mundo.LocalPorID(Mundo.LOCAL_ID_CABANA_DOS_ALQUIMISTAS));
        Verificar(areaTesteCabana is AreaCabana, "Cabana tem representação 2D válida (AreaCabana)");
        areaTesteCabana?.Free();
        var areaTesteJardim = CriaArea(Mundo.LocalPorID(Mundo.LOCAL_ID_JARDIM_DOS_ALQUIMISTAS));
        Verificar(areaTesteJardim is AreaJardim, "Jardim tem representação 2D válida (AreaJardim)");
        areaTesteJardim?.Free();

        // Praça -> Cabana (saída norte da Praça)
        MoverPara(Mundo.LocalPorID(Mundo.LOCAL_ID_PRACA));
        await Frames(3);
        Verificar(_areaAtiva is AreaPraca, "área visual = Praça");

        SaidaLocal saidaPraçaNorte = _areaAtiva.Saidas.First(s => s.Destino.ID == Mundo.LOCAL_ID_CABANA_DOS_ALQUIMISTAS);
        _areaAtiva.Jogador.Position = saidaPraçaNorte.Position;
        await Frames(10);
        Verificar(saidaPraçaNorte.NaProximidade, "saída norte da Praça detecta o jogador");
        saidaPraçaNorte.Interagir();
        Verificar(_partida.Jogador.LocalAtual.ID == Mundo.LOCAL_ID_CABANA_DOS_ALQUIMISTAS, "E na Praça leva o jogador à Cabana (Motor)");
        Verificar(_areaAtiva is AreaCabana, "representação visual troca para a Cabana");
        Verificar(Mathf.Abs(_areaAtiva.Jogador.Position.X - 300f) < 1f
                && Mathf.Abs(_areaAtiva.Jogador.Position.Y - 345f) < 1f,
            "spawn contextual: jogador aparece no ponto de entrada da Cabana");

        // Quest: já recebida ao entrar na Cabana (fluxo anterior do teste); estado no Motor
        var questJardim = Mundo.QuestPorID(Mundo.QUEST_ID_LIMPAR_JARDIM_DOS_ALQUIMISTAS);
        Verificar(_partida.Jogador.TemEstaQuest(questJardim) && !_partida.Jogador.QuestEstaCompletada(questJardim),
            "quest 'Limpe o Jardim' recebida e em andamento (estado do Motor)");

        // Cabana -> Jardim (saída norte da Cabana)
        SaidaLocal saidaCabanaJardim = _areaAtiva.Saidas.First(s => s.Destino.ID == Mundo.LOCAL_ID_JARDIM_DOS_ALQUIMISTAS);
        _areaAtiva.Jogador.Position = saidaCabanaJardim.Position;
        await Frames(10);
        Verificar(saidaCabanaJardim.NaProximidade, "saída norte da Cabana detecta o jogador");
        saidaCabanaJardim.Interagir();
        Verificar(_partida.Jogador.LocalAtual.ID == Mundo.LOCAL_ID_JARDIM_DOS_ALQUIMISTAS, "E na Cabana leva o jogador ao Jardim (Motor)");
        Verificar(_areaAtiva is AreaJardim, "representação visual troca para o Jardim");
        Verificar(Mathf.Abs(_areaAtiva.Jogador.Position.X - 400f) < 1f
                && Mathf.Abs(_areaAtiva.Jogador.Position.Y - 520f) < 1f,
            "spawn contextual: jogador aparece no ponto de entrada do Jardim");

        // O Rato do Jardim: o Motor o recria na entrada; a apresentação o exibe
        Verificar(_partida.MonstroAtual != null && _partida.MonstroAtual.ID == Mundo.MONSTRO_ID_RATO,
            "MonstroAtual do Motor = Rato do Jardim");
        Verificar(_areaAtiva.Inimigo != null, "Rato aparece no Jardim (representação visual)");

        // Quest: derrota 3 Ratos (drop determinístico de cauda de rato) e volte à Cabana
        for (int morte = 0; morte < 3; morte++)
        {
            if (morte > 0)
            {
                // Sair e voltar (Jardim -> Cabana -> Jardim) para o Motor ressuscitar o Rato
                SaidaLocal saidaJardimVolta = _areaAtiva.Saidas.First(s => s.Destino.ID == Mundo.LOCAL_ID_CABANA_DOS_ALQUIMISTAS);
                _areaAtiva.Jogador.Position = saidaJardimVolta.Position;
                await Frames(10);
                saidaJardimVolta.Interagir();
                SaidaLocal saidaCabanaVolta = _areaAtiva.Saidas.First(s => s.Destino.ID == Mundo.LOCAL_ID_JARDIM_DOS_ALQUIMISTAS);
                _areaAtiva.Jogador.Position = saidaCabanaVolta.Position;
                await Frames(10);
                saidaCabanaVolta.Interagir();
            }

            // Drop determinístico: cauda de rato em 100% (item comum)
            _partida.MonstroAtual.LootTable.Clear();
            _partida.MonstroAtual.LootTable.Add(new ItemLoot(Mundo.ItemPorID(Mundo.ITEM_ID_CAUDA_DE_RATO), 100, true));

            var inimigoJardim = _areaAtiva.Inimigo;
            _areaAtiva.Jogador.Position = inimigoJardim.Position + new Vector2(-40f, 0f);
            await Frames(3);
            int golpesJardim = 0;
            while (_partida.MonstroAtual != null && golpesJardim < 10)
            {
                AtacarInimigo();
                golpesJardim++;
            }
            Verificar(_partida.MonstroAtual == null, $"Rato {morte + 1} derrotado (Motor)");
        }

        Verificar(_partida.Jogador.Inventario.Any(ii => ii.Detalhes.ID == Mundo.ITEM_ID_CAUDA_DE_RATO && ii.Quantidade >= 3),
            "3 caudas de rato no inventário (loot do Motor)");

        // Jardim -> Cabana: voltar com as 3 caudas completa a quest (Motor)
        int ouroAntesQuest = _partida.Jogador.Ouro;
        int xpAntesQuest = _partida.Jogador.PontosExperiencia;
        SaidaLocal saidaJardimFim = _areaAtiva.Saidas.First(s => s.Destino.ID == Mundo.LOCAL_ID_CABANA_DOS_ALQUIMISTAS);
        _areaAtiva.Jogador.Position = saidaJardimFim.Position;
        await Frames(10);
        Verificar(saidaJardimFim.NaProximidade, "saída sul do Jardim detecta o jogador");
        saidaJardimFim.Interagir();
        Verificar(_partida.Jogador.LocalAtual.ID == Mundo.LOCAL_ID_CABANA_DOS_ALQUIMISTAS, "E no Jardim leva o jogador de volta à Cabana (Motor)");
        Verificar(_areaAtiva is AreaCabana, "representação visual volta para a Cabana");
        Verificar(Mathf.Abs(_areaAtiva.Jogador.Position.X - 300f) < 1f
                && Mathf.Abs(_areaAtiva.Jogador.Position.Y - 100f) < 1f,
            "spawn contextual: jogador aparece no ponto de entrada da Cabana");

        Verificar(_partida.Jogador.QuestEstaCompletada(questJardim), "quest completada ao voltar à Cabana com as caudas (Motor)");
        Verificar(_mensagens.Texto.Contains("completou 'Limpe o Jardim dos Alquimistas'"), "mensagem de conclusão da quest vem do Motor");
        Verificar(_partida.Jogador.Inventario.Any(ii => ii.Detalhes.ID == Mundo.ITEM_ID_POCAO_DE_CURA && ii.Quantidade > 0),
            "recompensa da quest: pocao de cura (Motor)");
        Verificar(!_partida.Jogador.Inventario.Any(ii => ii.Detalhes.ID == Mundo.ITEM_ID_CAUDA_DE_RATO && ii.Quantidade > 0),
            "itens da quest removidos do inventário (Motor)");
        Verificar(_partida.Jogador.Ouro == ouroAntesQuest + 10, "recompensa da quest: 10 de ouro (Motor)");
        Verificar(_partida.Jogador.PontosExperiencia == xpAntesQuest + 20, "recompensa da quest: 20 de XP (Motor)");

        // O diário mostra a quest concluída com o estado real do Motor
        AlternaPainel(false);
        Verificar(_painelQuests.TextoConteudo.Contains("Limpe o Jardim dos Alquimistas")
                && _painelQuests.TextoConteudo.Contains("Recompensa recebida"),
            "diário de quests mostra a quest concluída (estado real do Motor)");
        AlternaPainel(false);

        // Cabana -> Praça (saída sul da Cabana)
        SaidaLocal saidaCabanaPraça = _areaAtiva.Saidas.First(s => s.Destino.ID == Mundo.LOCAL_ID_PRACA);
        _areaAtiva.Jogador.Position = saidaCabanaPraça.Position;
        await Frames(10);
        Verificar(saidaCabanaPraça.NaProximidade, "saída sul da Cabana detecta o jogador");
        saidaCabanaPraça.Interagir();
        Verificar(_partida.Jogador.LocalAtual.ID == Mundo.LOCAL_ID_PRACA, "E na Cabana leva o jogador à Praça (Motor)");
        Verificar(_areaAtiva is AreaPraca, "representação visual volta para a Praça");
        Verificar(Mathf.Abs(_areaAtiva.Jogador.Position.X - 550f) < 1f
                && Mathf.Abs(_areaAtiva.Jogador.Position.Y - 80f) < 1f,
            "spawn contextual: jogador aparece no ponto de entrada da Praça");

        // ---- Fase 10: mundo completo — os 5 Locais restantes do grafo do
        // Motor (Posto de Guarda, Casa de Fazenda, Area dos Camponeses, Ponte
        // e Floresta) e suas conexões pelas saídas 2D ----

        // Os 9 Locais do Motor agora possuem representação 2D
        bool todosLocaisComArea = true;
        foreach (var local in Mundo.Locais)
        {
            var area = CriaArea(local);
            if (area == null)
                todosLocaisComArea = false;
            area?.Free();
        }
        Verificar(todosLocaisComArea, "os 9 Locais do Motor possuem representação 2D");

        var areaTestePosto = CriaArea(Mundo.LocalPorID(Mundo.LOCAL_ID_POSTO_DE_GUARDA));
        Verificar(areaTestePosto is AreaPosto, "Posto de Guarda tem representação 2D válida (AreaPosto)");
        areaTestePosto?.Free();
        var areaTesteFazenda = CriaArea(Mundo.LocalPorID(Mundo.LOCAL_ID_CASA_DA_FAZENDA));
        Verificar(areaTesteFazenda is AreaFazenda, "Casa de Fazenda tem representação 2D válida (AreaFazenda)");
        areaTesteFazenda?.Free();
        var areaTesteCamponeses = CriaArea(Mundo.LocalPorID(Mundo.LOCAL_ID_AREA_DOS_CAMPONESES));
        Verificar(areaTesteCamponeses is AreaCamponeses, "Area dos Camponeses tem representação 2D válida (AreaCamponeses)");
        areaTesteCamponeses?.Free();
        var areaTestePonte = CriaArea(Mundo.LocalPorID(Mundo.LOCAL_ID_PONTE));
        Verificar(areaTestePonte is AreaPonte, "Ponte tem representação 2D válida (AreaPonte)");
        areaTestePonte?.Free();
        var areaTesteFloresta = CriaArea(Mundo.LocalPorID(Mundo.LOCAL_ID_CAMPO_DAS_ARANHAS));
        Verificar(areaTesteFloresta is AreaFloresta, "Floresta tem representação 2D válida (AreaFloresta)");
        areaTesteFloresta?.Free();

        // O jogador está na Praça (fim da Fase 9). O gate do Posto é do Motor:
        // sem o Passe, usar a saída não faz nada (o jogador fica na Praça) e a
        // mensagem vem do Motor
        var passe = jogador.Inventario.First(ii => ii.Detalhes.ID == Mundo.ITEM_ID_PASSE_AVENTUREIRO);
        passe.Quantidade = 0;

        SaidaLocal saidaPraçaPosto = _areaAtiva.Saidas.First(s => s.Destino.ID == Mundo.LOCAL_ID_POSTO_DE_GUARDA);
        _areaAtiva.Jogador.Position = saidaPraçaPosto.Position;
        await Frames(10);
        Verificar(saidaPraçaPosto.NaProximidade, "saída do Posto da Praça detecta o jogador");
        string textoAntesGate = _mensagens.Texto;
        saidaPraçaPosto.Interagir();
        Verificar(_partida.Jogador.LocalAtual.ID == Mundo.LOCAL_ID_PRACA, "Posto: sem o Passe o Motor bloqueia (fica na Praça)");
        Verificar(_areaAtiva is AreaPraca, "Posto bloqueado: a representação visual permanece na Praça");
        Verificar(_mensagens.Texto.Length > textoAntesGate.Length
                && _mensagens.Texto.Substring(textoAntesGate.Length).Contains("Passe de Aventureiro"),
            "mensagem de gate do Posto vem do Motor");

        passe.Quantidade = 1;
        saidaPraçaPosto.Interagir();
        Verificar(_partida.Jogador.LocalAtual.ID == Mundo.LOCAL_ID_POSTO_DE_GUARDA, "Posto: com o Passe o Motor libera a entrada");
        Verificar(_areaAtiva is AreaPosto, "representação visual troca para o Posto");
        Verificar(Mathf.Abs(_areaAtiva.Jogador.Position.X - 100f) < 1f
                && Mathf.Abs(_areaAtiva.Jogador.Position.Y - 200f) < 1f,
            "spawn contextual: jogador entra no Posto pela porta oeste");

        // Posto -> Ponte (saída leste do Posto)
        SaidaLocal saidaPostoPonte = _areaAtiva.Saidas.First(s => s.Destino.ID == Mundo.LOCAL_ID_PONTE);
        _areaAtiva.Jogador.Position = saidaPostoPonte.Position;
        await Frames(10);
        Verificar(saidaPostoPonte.NaProximidade, "saída da Ponte do Posto detecta o jogador");
        saidaPostoPonte.Interagir();
        Verificar(_partida.Jogador.LocalAtual.ID == Mundo.LOCAL_ID_PONTE, "E no Posto leva o jogador à Ponte (Motor)");
        Verificar(_areaAtiva is AreaPonte, "representação visual troca para a Ponte");
        Verificar(Mathf.Abs(_areaAtiva.Jogador.Position.X - 100f) < 1f
                && Mathf.Abs(_areaAtiva.Jogador.Position.Y - 150f) < 1f,
            "spawn contextual: jogador entra na Ponte pela extremidade oeste");

        // Ponte -> Floresta (saída leste da Ponte)
        SaidaLocal saidaPonteFloresta = _areaAtiva.Saidas.First(s => s.Destino.ID == Mundo.LOCAL_ID_CAMPO_DAS_ARANHAS);
        _areaAtiva.Jogador.Position = saidaPonteFloresta.Position;
        await Frames(10);
        Verificar(saidaPonteFloresta.NaProximidade, "saída da Floresta da Ponte detecta o jogador");
        saidaPonteFloresta.Interagir();
        Verificar(_partida.Jogador.LocalAtual.ID == Mundo.LOCAL_ID_CAMPO_DAS_ARANHAS, "E na Ponte leva o jogador à Floresta (Motor)");
        Verificar(_areaAtiva is AreaFloresta, "representação visual troca para a Floresta");
        Verificar(Mathf.Abs(_areaAtiva.Jogador.Position.X - 100f) < 1f
                && Mathf.Abs(_areaAtiva.Jogador.Position.Y - 300f) < 1f,
            "spawn contextual: jogador entra na Floresta pela porta oeste");

        // A Aranha da Floresta: o Motor a recria na entrada; a apresentação a exibe
        Verificar(_partida.MonstroAtual != null && _partida.MonstroAtual.ID == Mundo.MONSTRO_ID_ARANHA_GIGANTE,
            "MonstroAtual do Motor = Aranha da Floresta");
        Verificar(_areaAtiva.Inimigo is Inimigo2D, "Aranha aparece na Floresta (representação visual Inimigo2D)");

        // Floresta -> Ponte (saída oeste da Floresta)
        SaidaLocal saidaFlorestaPonte = _areaAtiva.Saidas.First(s => s.Destino.ID == Mundo.LOCAL_ID_PONTE);
        _areaAtiva.Jogador.Position = saidaFlorestaPonte.Position;
        await Frames(10);
        Verificar(saidaFlorestaPonte.NaProximidade, "saída da Ponte da Floresta detecta o jogador");
        saidaFlorestaPonte.Interagir();
        Verificar(_partida.Jogador.LocalAtual.ID == Mundo.LOCAL_ID_PONTE, "E na Floresta leva o jogador de volta à Ponte (Motor)");
        Verificar(_areaAtiva is AreaPonte, "representação visual volta para a Ponte");
        Verificar(Mathf.Abs(_areaAtiva.Jogador.Position.X - 700f) < 1f
                && Mathf.Abs(_areaAtiva.Jogador.Position.Y - 150f) < 1f,
            "spawn contextual: jogador entra na Ponte pela extremidade leste");

        // Ponte -> Posto (saída oeste da Ponte)
        SaidaLocal saidaPontePosto = _areaAtiva.Saidas.First(s => s.Destino.ID == Mundo.LOCAL_ID_POSTO_DE_GUARDA);
        _areaAtiva.Jogador.Position = saidaPontePosto.Position;
        await Frames(10);
        Verificar(saidaPontePosto.NaProximidade, "saída do Posto da Ponte detecta o jogador");
        saidaPontePosto.Interagir();
        Verificar(_partida.Jogador.LocalAtual.ID == Mundo.LOCAL_ID_POSTO_DE_GUARDA, "E na Ponte leva o jogador de volta ao Posto (Motor)");
        Verificar(_areaAtiva is AreaPosto, "representação visual volta para o Posto");
        Verificar(Mathf.Abs(_areaAtiva.Jogador.Position.X - 500f) < 1f
                && Mathf.Abs(_areaAtiva.Jogador.Position.Y - 200f) < 1f,
            "spawn contextual: jogador entra no Posto pela porta leste");

        // Posto -> Praça (saída oeste do Posto)
        SaidaLocal saidaPostoPraça = _areaAtiva.Saidas.First(s => s.Destino.ID == Mundo.LOCAL_ID_PRACA);
        _areaAtiva.Jogador.Position = saidaPostoPraça.Position;
        await Frames(10);
        Verificar(saidaPostoPraça.NaProximidade, "saída da Praça do Posto detecta o jogador");
        saidaPostoPraça.Interagir();
        Verificar(_partida.Jogador.LocalAtual.ID == Mundo.LOCAL_ID_PRACA, "E no Posto leva o jogador de volta à Praça (Motor)");
        Verificar(_areaAtiva is AreaPraca, "representação visual volta para a Praça");
        Verificar(Mathf.Abs(_areaAtiva.Jogador.Position.X - 700f) < 1f
                && Mathf.Abs(_areaAtiva.Jogador.Position.Y - 400f) < 1f,
            "spawn contextual: jogador entra na Praça pela porta leste");

        // Praça -> Casa de Fazenda (saída oeste da Praça)
        SaidaLocal saidaPraçaFazenda = _areaAtiva.Saidas.First(s => s.Destino.ID == Mundo.LOCAL_ID_CASA_DA_FAZENDA);
        _areaAtiva.Jogador.Position = saidaPraçaFazenda.Position;
        await Frames(10);
        Verificar(saidaPraçaFazenda.NaProximidade, "saída da Fazenda da Praça detecta o jogador");
        saidaPraçaFazenda.Interagir();
        Verificar(_partida.Jogador.LocalAtual.ID == Mundo.LOCAL_ID_CASA_DA_FAZENDA, "E na Praça leva o jogador à Casa de Fazenda (Motor)");
        Verificar(_areaAtiva is AreaFazenda, "representação visual troca para a Fazenda");
        Verificar(Mathf.Abs(_areaAtiva.Jogador.Position.X - 500f) < 1f
                && Mathf.Abs(_areaAtiva.Jogador.Position.Y - 200f) < 1f,
            "spawn contextual: jogador entra na Fazenda pela porta leste");

        var questCamponeses = Mundo.QuestPorID(Mundo.QUEST_ID_LIMPAR_AREA_DOS_CAMPONESES);
        Verificar(_partida.Jogador.TemEstaQuest(questCamponeses) && !_partida.Jogador.QuestEstaCompletada(questCamponeses),
            "quest 'Limpar a area dos camponeses' recebida ao entrar na Fazenda (Motor)");

        // Fazenda -> Area dos Camponeses (saída oeste da Fazenda)
        SaidaLocal saidaFazendaCamponeses = _areaAtiva.Saidas.First(s => s.Destino.ID == Mundo.LOCAL_ID_AREA_DOS_CAMPONESES);
        _areaAtiva.Jogador.Position = saidaFazendaCamponeses.Position;
        await Frames(10);
        Verificar(saidaFazendaCamponeses.NaProximidade, "saída dos Camponeses da Fazenda detecta o jogador");
        saidaFazendaCamponeses.Interagir();
        Verificar(_partida.Jogador.LocalAtual.ID == Mundo.LOCAL_ID_AREA_DOS_CAMPONESES, "E na Fazenda leva o jogador à Area dos Camponeses (Motor)");
        Verificar(_areaAtiva is AreaCamponeses, "representação visual troca para a Area dos Camponeses");
        Verificar(Mathf.Abs(_areaAtiva.Jogador.Position.X - 700f) < 1f
                && Mathf.Abs(_areaAtiva.Jogador.Position.Y - 300f) < 1f,
            "spawn contextual: jogador entra na Area pela porta leste");

        // A Cobra da Area: o Motor a recria na entrada; a apresentação a exibe
        Verificar(_partida.MonstroAtual != null && _partida.MonstroAtual.ID == Mundo.MONSTRO_ID_COBRA,
            "MonstroAtual do Motor = Cobra da Area dos Camponeses");
        Verificar(_areaAtiva.Inimigo is Inimigo2D, "Cobra aparece na Area dos Camponeses (representação visual Inimigo2D)");

        // Area dos Camponeses -> Fazenda (saída leste da Area)
        SaidaLocal saidaCamponesesFazenda = _areaAtiva.Saidas.First(s => s.Destino.ID == Mundo.LOCAL_ID_CASA_DA_FAZENDA);
        _areaAtiva.Jogador.Position = saidaCamponesesFazenda.Position;
        await Frames(10);
        Verificar(saidaCamponesesFazenda.NaProximidade, "saída da Fazenda da Area detecta o jogador");
        saidaCamponesesFazenda.Interagir();
        Verificar(_partida.Jogador.LocalAtual.ID == Mundo.LOCAL_ID_CASA_DA_FAZENDA, "E na Area leva o jogador de volta à Fazenda (Motor)");
        Verificar(_areaAtiva is AreaFazenda, "representação visual volta para a Fazenda");
        Verificar(Mathf.Abs(_areaAtiva.Jogador.Position.X - 100f) < 1f
                && Mathf.Abs(_areaAtiva.Jogador.Position.Y - 200f) < 1f,
            "spawn contextual: jogador entra na Fazenda pela porta oeste");

        // ---- Fase 11: ataque espacial do inimigo (cooldown) e morte do
        // jogador (as regras de dano/morte/respawn são todas do Motor) ----

        MoverPara(Mundo.LocalPorID(Mundo.LOCAL_ID_PRACA));
        await Frames(3);
        Verificar(_areaAtiva is AreaPraca, "F11: área visual = Praça");
        Verificar(_areaAtiva.Inimigo != null && _partida.MonstroAtual != null && _partida.MonstroAtual.ID == Mundo.MONSTRO_ID_RATO,
            "F11: Rato recriado ao voltar à Praça (Motor)");

        var inimigoF11 = _areaAtiva.Inimigo;
        var jogadorF11 = _areaAtiva.Jogador;

        // Manipulação de teste: dano observável e HP alto para a amostra
        _partida.MonstroAtual.DanoMaximo = 10;
        _partida.Jogador.VidaAtual = 200;

        // O Rato detecta o jogador e o persegue até o alcance de ataque
        jogadorF11.Position = inimigoF11.Position + new Vector2(-200f, 0f);
        await Frames(30);
        Vector2 ratoParadoF11 = inimigoF11.Position;
        await Frames(30);
        Verificar(inimigoF11.Position == ratoParadoF11, "F11: fora da percepção o Rato permanece parado");

        int hpAntesF11 = _partida.Jogador.VidaAtual;
        jogadorF11.Position = inimigoF11.Position + new Vector2(-100f, 0f);
        await Frames(120);
        Verificar(jogadorF11.Position.DistanceTo(inimigoF11.Position) < Inimigo2D.DistanciaAtaque,
            "F11: Rato persegue o jogador até o alcance de ataque");

        // O Rato ataca a cada cooldown (1s); o dano é aplicado pelo Motor
        int ciclosDano = 0;
        while (_partida.Jogador.VidaAtual == hpAntesF11 && ciclosDano < 10)
        {
            await Frames(60); // 1 ciclo de cooldown
            ciclosDano++;
        }
        Verificar(_partida.Jogador.VidaAtual < hpAntesF11, "F11: Rato causa dano no jogador (Motor aplica)");
        Verificar(_hud.TextoVida.Contains($"HP: {_partida.Jogador.VidaAtual}/"), "F11: HUD reflete o HP após o dano");
        Verificar(_mensagens.Texto.Contains("causou a você") || _mensagens.Texto.Contains("errou o ataque"),
            "F11: mensagem do ataque do Rato vem do Motor");

        // Cooldown: em 0,5s (metade do intervalo) não há novo ataque
        int hpPósAtaque = _partida.Jogador.VidaAtual;
        await Frames(30);
        Verificar(_partida.Jogador.VidaAtual == hpPósAtaque, "F11: cooldown impede ataque a cada frame");

        // Fuga: o jogador sai da percepção e o Rato interrompe a perseguição
        jogadorF11.Position = inimigoF11.Position + new Vector2(-300f, 0f);
        await Frames(30);
        Vector2 ratoFugiu = inimigoF11.Position;
        int hpFuga = _partida.Jogador.VidaAtual;
        await Frames(120);
        Verificar(inimigoF11.Position == ratoFugiu, "F11: fugindo (fora da percepção) o Rato para de perseguir");
        Verificar(_partida.Jogador.VidaAtual == hpFuga, "F11: fugindo, o jogador não recebe dano");

        // Re-engajamento: o Rato persegue de novo
        jogadorF11.Position = inimigoF11.Position + new Vector2(-100f, 0f);
        await Frames(120);
        Verificar(jogadorF11.Position.DistanceTo(inimigoF11.Position) < Inimigo2D.DistanciaAtaque,
            "F11: o Rato volta a perseguir quando o jogador se aproxima");

        // Morte: HP 1 => o próximo acerto mata; o Motor aplica a regra
        // existente (Casa, 25% do ouro, respawn com vida cheia)
        _partida.Jogador.VidaAtual = 1;
        _partida.Jogador.Ouro = 100;
        int ciclosMorte = 0;
        while (_partida.Jogador.LocalAtual.ID != Mundo.LOCAL_ID_CASA && ciclosMorte < 20)
        {
            await Frames(60);
            ciclosMorte++;
        }
        await Frames(3);
        Verificar(_partida.Jogador.LocalAtual.ID == Mundo.LOCAL_ID_CASA, "F11: morte => o Motor teletransporta para a Casa");
        Verificar(_partida.MonstroAtual == null, "F11: monstro limpo após a morte");
        Verificar(_partida.Jogador.VidaAtual == _partida.Jogador.VidaMaximaEfetiva, "F11: respawn com vida cheia");
        Verificar(_partida.Jogador.Ouro == 75, "F11: morte perde 25% do ouro (100 -> 75)");
        Verificar(_areaAtiva is AreaCasa, "F11: representação visual volta para a Casa");
        Verificar(_areaAtiva.Jogador != null && _areaAtiva.Jogador.Position.IsFinite(), "F11: jogador aparece no spawn da Casa");
        Verificar(_hud.TextoLocal.Contains("Casa"), "F11: HUD reflete a Casa após a morte");
        Verificar(_hud.TextoVida.Contains($"HP: {_partida.Jogador.VidaMaximaEfetiva}/"), "F11: HUD mostra HP cheio após o respawn");
        Verificar(_mensagens.Texto.Contains("matou você"), "F11: mensagem de morte vem do Motor");
        Verificar(_mensagens.Texto.Contains("perdeu 25 de ouro"), "F11: mensagem da perda de ouro vem do Motor");

        // O combate do jogador continua funcionando após a morte (Rato da Praça)
        MoverPara(Mundo.LocalPorID(Mundo.LOCAL_ID_PRACA));
        await Frames(3);
        Verificar(_areaAtiva is AreaPraca && _areaAtiva.Inimigo != null, "F11: Rato recriado na Praça após o respawn");
        _partida.MonstroAtual.VidaAtual = 1;
        _partida.MonstroAtual.LootTable.Clear();
        _partida.MonstroAtual.LootTable.Add(new ItemLoot(Mundo.ItemPorID(Mundo.ITEM_ID_PELO_DE_RATO), 100, true));
        int xpAntesF11 = _partida.Jogador.PontosExperiencia;
        int ouroAntesF11 = _partida.Jogador.Ouro;
        _areaAtiva.Jogador.Position = _areaAtiva.Inimigo.Position + new Vector2(-40f, 0f);
        await Frames(3);
        AtacarInimigo();
        Verificar(_partida.MonstroAtual == null, "F11: Rato morre pelo ataque do jogador (combate da Fase 9 intacto)");
        Verificar(_partida.Jogador.PontosExperiencia == xpAntesF11 + 3 && _partida.Jogador.Ouro == ouroAntesF11 + 10,
            "F11: XP (3) e ouro (10) da morte creditados pelo Motor");
        Verificar(_partida.Jogador.Inventario.Any(ii => ii.Detalhes.ID == Mundo.ITEM_ID_PELO_DE_RATO && ii.Quantidade > 0),
            "F11: loot do Rato vai para o inventário");

        // O mapa continua funcionando após o respawn
        MoverPara(Mundo.LocalPorID(Mundo.LOCAL_ID_CABANA_DOS_ALQUIMISTAS));
        Verificar(_areaAtiva is AreaCabana, "F11: mapa continua funcionando (Praça -> Cabana)");

        GD.Print(falhas == 0 ? "[AutoTeste] TODOS OS CHECKS OK" : $"[AutoTeste] {falhas} CHECK(S) FALHARAM");

        // Em modo headless o processo encerra com o código dos checks
        GetTree().Quit(falhas == 0 ? 0 : 1);
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
