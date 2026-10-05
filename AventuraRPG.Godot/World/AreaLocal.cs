using System;
using System.Collections.Generic;
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
    // O inimigo (apresentação) desta área: a representação visual do
    // Partida.MonstroAtual, quando o Local tem um monstro vivo
    public Inimigo2D Inimigo { get; private set; }
    // Repassa o sinal do inimigo (estava em posição de atacar) para o Jogo,
    // que pergunta ao Motor as regras de dano/morte
    public event Action InimigoAtacou;

    protected AreaLocal(Local local)
    {
        Local = local;
    }

    // Ponto de spawn do jogador nesta área
    protected abstract Vector2 PosicaoSpawn { get; }

    // O cenário da área (chão, paredes, portas)
    protected abstract void CriaCenario();

    // Onde o inimigo aparece nesta área (nulo = a área não tem inimigo)
    protected virtual Vector2? PosicaoInimigo => null;

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

    // Sincroniza o inimigo visual com o estado real do Motor:
    // monstro vivo => inimigo presente (barra de HP atualizada);
    // monstro morto/ausente => inimigo removido.
    public void AtualizaInimigo(Monstro monstro)
    {
        if (monstro == null)
        {
            if (Inimigo != null)
            {
                Inimigo.Free();
                Inimigo = null;
            }
            return;
        }

        if (Inimigo == null)
        {
            var posicao = PosicaoInimigo;
            if (posicao == null)
                return; // esta área não tem ponto para o inimigo

            Inimigo = new Inimigo2D(monstro, Jogador) { Position = posicao.Value };
            Inimigo.AtaqueSolicitado += () => InimigoAtacou?.Invoke();
            AddChild(Inimigo);
        }

        Inimigo.AtualizarHp();
    }
}
