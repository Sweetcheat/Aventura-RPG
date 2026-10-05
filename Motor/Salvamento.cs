using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Motor
{
    /*  Save/Load (Fase 14): converte o estado do jogo da Partida em uma
        estrutura de dados simples (IDs + números, sem objetos de domínio) e
        grava/lê em um arquivo JSON. O caminho do arquivo é decisão da
        apresentação (user:// no Godot; caminhos temporários nos testes) —
        o Motor só lê/escreve texto e não conhece o Godot.

        O arquivo representa ESTADO DO JOGO, não estado visual: local, vida,
        ouro, XP, inventário, arma selecionada, quests e monstros vivos (com
        o HP individual de cada um). Não há nada de apresentação no save:
        posições 2D, câmera, HUD e sprites vivem no Godot e são
        reconstruídos a partir do Local salvo.

        Também não é persistido nada que venha dos templates do Mundo:
        nomes, dano, XP, ouro e LootTable dos monstros, e as propriedades dos
        itens (o save guarda só os IDs). O level não é salvo: é derivado do XP. */
    public static class Salvamento
    {
        public class EstadoInventarioItem
        {
            public int ItemId { get; set; }
            public int Quantidade { get; set; }
        }

        public class EstadoQuest
        {
            public int QuestId { get; set; }
            public bool Completado { get; set; }
        }

        public class EstadoMonstro
        {
            public int MonstroId { get; set; }
            public int VidaAtual { get; set; }
        }

        public class EstadoJogo
        {
            public int LocalId { get; set; }
            public int VidaAtual { get; set; }
            public int VidaMaxima { get; set; }
            public int Ouro { get; set; }
            public int PontosExperiencia { get; set; }
            public int ArmaId { get; set; }
            public bool RecebeuRecompensaPorrete { get; set; }
            public List<EstadoInventarioItem> Inventario { get; set; } = new List<EstadoInventarioItem>();
            public List<EstadoQuest> Quests { get; set; } = new List<EstadoQuest>();
            public List<EstadoMonstro> Monstros { get; set; } = new List<EstadoMonstro>();
        }

        private static readonly JsonSerializerOptions Opcoes = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
        };

        // Estado atual da Partida => estrutura serializável
        public static EstadoJogo ParaEstado(Partida partida)
        {
            var estado = new EstadoJogo
            {
                LocalId = partida.Jogador.LocalAtual?.ID ?? 0,
                VidaAtual = partida.Jogador.VidaAtual,
                VidaMaxima = partida.Jogador.VidaMaxima,
                Ouro = partida.Jogador.Ouro,
                PontosExperiencia = partida.Jogador.PontosExperiencia,
                ArmaId = partida.ArmaSelecionada?.ID ?? 0,
                RecebeuRecompensaPorrete = partida.Jogador.RecebeuRecompensaPorrete,
            };

            foreach (var item in partida.Jogador.Inventario)
                estado.Inventario.Add(new EstadoInventarioItem { ItemId = item.Detalhes.ID, Quantidade = item.Quantidade });

            foreach (var quest in partida.Jogador.Quests)
                estado.Quests.Add(new EstadoQuest { QuestId = quest.Detalhes.ID, Completado = quest.Completado });

            // Monstros vivos (a ordem da lista é mantida); os mortos não
            // existem mais na Partida e por isso não entram no save
            foreach (var monstro in partida.MonstrosAtuais)
                estado.Monstros.Add(new EstadoMonstro { MonstroId = monstro.ID, VidaAtual = monstro.VidaAtual });

            return estado;
        }

        // Grava o estado em JSON no caminho informado. false = falha de escrita.
        public static bool EscreverArquivo(string caminho, EstadoJogo estado)
        {
            try
            {
                File.WriteAllText(caminho, JsonSerializer.Serialize(estado, Opcoes));
                return true;
            }
            catch (IOException)
            {
                return false;
            }
        }

        // Lê e valida o arquivo. null = sem arquivo, JSON inválido/corrompido,
        // estrutura incompleta ou qualquer ID inexistente no Mundo (save de
        // versão incompatível). Quem lê valida tudo antes de aplicar nada:
        // a Partida não deve ser alterada se o save for inválido.
        public static EstadoJogo LerArquivo(string caminho)
        {
            EstadoJogo estado;
            try
            {
                estado = JsonSerializer.Deserialize<EstadoJogo>(File.ReadAllText(caminho), Opcoes);
            }
            catch (Exception)
            {
                return null; // arquivo ausente, ilegível ou JSON corrompido
            }

            if (estado == null)
                return null;
            if (estado.Inventario == null || estado.Quests == null || estado.Monstros == null)
                return null; // estrutura incompleta
            if (Mundo.LocalPorID(estado.LocalId) == null)
                return null; // local inexistente no mundo

            foreach (var item in estado.Inventario)
                if (Mundo.ItemPorID(item.ItemId) == null)
                    return null; // item inexistente no mundo

            foreach (var quest in estado.Quests)
                if (Mundo.QuestPorID(quest.QuestId) == null)
                    return null; // quest inexistente no mundo

            foreach (var monstro in estado.Monstros)
                if (Mundo.MonstroPorID(monstro.MonstroId) == null)
                    return null; // monstro inexistente no mundo

            if (!(Mundo.ItemPorID(estado.ArmaId) is Arma))
                return null; // arma ausente ou não é uma arma

            return estado;
        }
    }
}
