using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Motor;

/* Base das áreas 2D: cada área é a representação visual de um Local
   existente do Motor. A área monta o próprio cenário (chão, paredes,
   portas), cria o jogador no spawn e cria as saídas (SaidaLocal) com o
   destino resolvido no grafo do Motor.
   Novos Locais = novas classes pequenas (AreaCasa, AreaPraca, ...); o Jogo
   apenas orquestra (troca a área quando o Motor muda o Local). */
public abstract partial class AreaLocal : Node2D
{
    // O Local do Motor que esta área representa
    public Local Local { get; }
    // O jogador (apresentação) desta área
    public Jogador2D Jogador { get; private set; }
    // As saídas da área
    public List<SaidaLocal> Saidas { get; } = new List<SaidaLocal>();
    // Os inimigos (apresentação) desta área: a representação visual de cada
    // monstro da Partida.MonstrosAtuais (uma área pode ter vários)
    public List<Inimigo2D> Inimigos { get; private set; } = new List<Inimigo2D>();
    // Repassa o sinal do inimigo (estava em posição de atacar, com a fonte)
    // para o Jogo, que pergunta ao Motor as regras de dano/morte
    public event Action<Inimigo2D> InimigoAtacou;

    protected AreaLocal(Local local)
    {
        Local = local;
    }

    // Ponto de spawn do jogador nesta área
    protected abstract Vector2 PosicaoSpawn { get; }

    // O cenário da área (chão, paredes, portas)
    protected abstract void CriaCenario();

    // Onde os inimigos aparecem nesta área (lista vazia = a área não tem
    // inimigo; a i-ésima posição é do i-ésimo monstro do local)
    protected virtual List<Vector2> PosicoesInimigos => new List<Vector2>();

    // Monta a área completa (cenário + jogador + saídas).
    // Chamada pelo Jogo antes de a área entrar na cena. Se spawnDestino é
    // informado (entrada contextual vinda de uma saída), o jogador aparece
    // lá; senão, no ponto padrão da área.
    public void Cria(Vector2? spawnDestino = null)
    {
        CriaCenario();

        Jogador = new Jogador2D { Position = spawnDestino ?? PosicaoSpawn };
        AddChild(Jogador);
    }

    // Cria uma saída para um Local vizinho do grafo (ex.: Local.LocalParaNorte)
    // e devolve a saída criada (para a área definir o ponto de entrada no destino)
    protected SaidaLocal CriaSaida(Vector2 posicao, Local destino)
    {
        if (destino == null)
            return null;

        var saida = new SaidaLocal
        {
            Area = Local,
            Destino = destino,
            Position = posicao,
        };
        AddChild(saida);
        Saidas.Add(saida);
        return saida;
    }

    // Sincroniza os inimigos visuais com o estado real do Motor (a identidade
    // é a referência da instância Monstro, a mesma que o Motor altera):
    // cada monstro vivo => um inimigo presente (barra de HP atualizada);
    // monstro morto/ausente => inimigo removido (os demais continuam).
    public void AtualizaInimigos(List<Monstro> monstros)
    {
        // Remove os inimigos cujo monstro saiu da luta (morreu ou o local
        // mudou). O Free é adiado em timer (fora do passo de física):
        // liberar um CharacterBody2D durante a física deixa o corpo fantasma
        // no espaço físico e novos corpos colidem com ele
        for (int i = Inimigos.Count - 1; i >= 0; i--)
        {
            var inimigo = Inimigos[i];
            if (monstros == null || !monstros.Contains(inimigo.Monstro))
            {
                Inimigos.RemoveAt(i);
                inimigo.ProcessMode = Node.ProcessModeEnum.Disabled;
                GetTree().CreateTimer(0.2f).Timeout += inimigo.Free;
            }
        }

        if (monstros == null)
            return;

        // Cria um inimigo para cada monstro vivo ainda sem representação
        var posicoes = PosicoesInimigos;
        int proximaPosicao = 0;
        foreach (var monstro in monstros)
        {
            if (Inimigos.Any(i => ReferenceEquals(i.Monstro, monstro)))
                continue;

            if (proximaPosicao >= posicoes.Count)
                break; // esta área não tem ponto para mais inimigos

            var inimigo = new Inimigo2D(monstro, Jogador) { Position = posicoes[proximaPosicao] };
            inimigo.AtaqueSolicitado += i => InimigoAtacou?.Invoke(i);
            AddChild(inimigo);
            Inimigos.Add(inimigo);
            proximaPosicao++;
        }

        foreach (var inimigo in Inimigos)
            inimigo.AtualizarHp();
    }
}
