using System.Collections.Generic;
using System.Linq;

namespace Motor
{
    public static class Mundo
    {
        /*  Listas estáticas que contêm todos os objetos do mundo do jogo.
            São preenchidas uma única vez pelo construtor estático abaixo. */

        public static readonly List<Item> Itens = new List<Item>();
        public static readonly List<Monstro> Monstros = new List<Monstro>();
        public static readonly List<Quest> Quests = new List<Quest>();
        public static readonly List<Local> Locais = new List<Local>();

        // Constantes de ID — evitam ter que decorar números espalhados pelo código.
        // Exemplo: em vez de MonstroPorID(3), usamos MonstroPorID(MONSTRO_ID_ARANHA_GIGANTE).

        // Itens
        public const int ITEM_ID_ESPADA_ENFERRUJADA  = 1;
        public const int ITEM_ID_CAUDA_DE_RATO        = 2;
        public const int ITEM_ID_PELO_DE_RATO         = 3;
        public const int ITEM_ID_PRESA_DE_COBRA       = 4;
        public const int ITEM_ID_PELE_DE_COBRA        = 5;
        public const int ITEM_ID_PORRETE              = 6;
        public const int ITEM_ID_POCAO_DE_CURA        = 7;
        public const int ITEM_ID_PRESA_DE_ARANHA      = 8;
        public const int ITEM_ID_SEDA_DE_ARANHA       = 9;
        public const int ITEM_ID_PASSE_AVENTUREIRO    = 10;

        // Monstros
        public const int MONSTRO_ID_RATO              = 1;
        public const int MONSTRO_ID_COBRA             = 2;
        public const int MONSTRO_ID_ARANHA_GIGANTE    = 3;

        // Quests
        public const int QUEST_ID_LIMPAR_JARDIM_DOS_ALQUIMISTAS = 1;
        public const int QUEST_ID_LIMPAR_AREA_DOS_CAMPONESES    = 2;

        // Locais
        public const int LOCAL_ID_CASA                    = 1;
        public const int LOCAL_ID_PRACA                   = 2;
        public const int LOCAL_ID_POSTO_DE_GUARDA         = 3;
        public const int LOCAL_ID_CABANA_DOS_ALQUIMISTAS  = 4;
        public const int LOCAL_ID_JARDIM_DOS_ALQUIMISTAS  = 5;
        public const int LOCAL_ID_CASA_DA_FAZENDA         = 6;
        public const int LOCAL_ID_AREA_DOS_CAMPONESES     = 7;
        public const int LOCAL_ID_PONTE                   = 8;
        public const int LOCAL_ID_CAMPO_DAS_ARANHAS       = 9;

        /*  Construtor estático: executado automaticamente na primeira vez que qualquer
            membro desta classe é acessado. Garante que as listas estejam prontas
            antes de qualquer outra parte do jogo tentar usá-las. */
        static Mundo()
        {
            ColoqueItems();
            ColoqueMonstros();
            ColoqueQuests();
            ColoqueLocais();
        }

        private static void ColoqueItems()
        {
            Itens.Add(new Arma(ITEM_ID_ESPADA_ENFERRUJADA, "Espada Enferrujada", "Espadas Enferrujadas", 0, 5));
            Itens.Add(new Item(ITEM_ID_CAUDA_DE_RATO,      "Cauda de rato",      "Caudas de rato"));
            Itens.Add(new Item(ITEM_ID_PELO_DE_RATO,       "Pelo de rato",       "Pelos de rato"));
            Itens.Add(new Item(ITEM_ID_PRESA_DE_COBRA,     "Presa de cobra",     "Presas de cobra"));
            Itens.Add(new Item(ITEM_ID_PELE_DE_COBRA,      "Pele de cobra",      "Pele de cobras"));
            Itens.Add(new Arma(ITEM_ID_PORRETE,            "Porrete",            "Porretes", 3, 10));
            Itens.Add(new PocaoCura(ITEM_ID_POCAO_DE_CURA, "Pocao de cura",      "Pocoes de cura", 5));
            Itens.Add(new Item(ITEM_ID_PRESA_DE_ARANHA,    "Presa de aranha",    "Presas de Aranha"));
            Itens.Add(new Item(ITEM_ID_SEDA_DE_ARANHA,     "Seda de aranha",     "Sedas de Aranha"));
            Itens.Add(new Item(ITEM_ID_PASSE_AVENTUREIRO,  "Passe de Aventureiro","Passes de Aventureiro"));
        }

        private static void ColoqueMonstros()
        {
            var rato = new Monstro(MONSTRO_ID_RATO, "Rato", 5, 3, 10, 3, 3);
            rato.LootTable.Add(new ItemLoot(ItemPorID(ITEM_ID_CAUDA_DE_RATO), 75, false));
            rato.LootTable.Add(new ItemLoot(ItemPorID(ITEM_ID_PELO_DE_RATO),  75, true));

            var cobra = new Monstro(MONSTRO_ID_COBRA, "Cobra", 5, 3, 10, 3, 3);
            cobra.LootTable.Add(new ItemLoot(ItemPorID(ITEM_ID_PRESA_DE_COBRA), 75, false));
            cobra.LootTable.Add(new ItemLoot(ItemPorID(ITEM_ID_PELE_DE_COBRA),  75, true));

            var aranhaGigante = new Monstro(MONSTRO_ID_ARANHA_GIGANTE, "Aranha gigante", 20, 20, 40, 10, 10);
            aranhaGigante.LootTable.Add(new ItemLoot(ItemPorID(ITEM_ID_PRESA_DE_ARANHA), 75, true));
            aranhaGigante.LootTable.Add(new ItemLoot(ItemPorID(ITEM_ID_SEDA_DE_ARANHA),  25, false));

            Monstros.Add(rato);
            Monstros.Add(cobra);
            Monstros.Add(aranhaGigante);
        }

        private static void ColoqueQuests()
        {
            var limparJardimAlquimista = new Quest(
                QUEST_ID_LIMPAR_JARDIM_DOS_ALQUIMISTAS,
                "Limpe o Jardim dos Alquimistas",
                "Mate os ratos no jardim dos alquimistas e traga 3 caudas de rato. Você irá receber uma pocao de cura e 10 moedas de ouro.",
                20, 10);
            limparJardimAlquimista.QuestCompletadaItem.Add(new QuestCompletadaItem(ItemPorID(ITEM_ID_CAUDA_DE_RATO), 3));
            limparJardimAlquimista.ItemRecompensa = ItemPorID(ITEM_ID_POCAO_DE_CURA);

            var limparAreaCamponeses = new Quest(
                QUEST_ID_LIMPAR_AREA_DOS_CAMPONESES,
                "Limpar a área dos camponeses",
                "Mate cobras na area dos camponeses e traga 3 presas de cobra. Você irá receber um passe de aventureiro e 20 moedas de ouro.",
                20, 20);
            limparAreaCamponeses.QuestCompletadaItem.Add(new QuestCompletadaItem(ItemPorID(ITEM_ID_PRESA_DE_COBRA), 3));
            limparAreaCamponeses.ItemRecompensa = ItemPorID(ITEM_ID_PASSE_AVENTUREIRO);

            Quests.Add(limparJardimAlquimista);
            Quests.Add(limparAreaCamponeses);
        }

        private static void ColoqueLocais()
        {
            // Cria cada local
            var casa              = new Local(LOCAL_ID_CASA,                   "Casa",                  "Esta é a sua casa.",                                                  null,                              null,                                         null);
            var praca             = new Local(LOCAL_ID_PRACA,                  "Praça",                 "Você vê uma fonte grande.",                                           null,                              null,                                         null);
            var cabanaAlquimistas = new Local(LOCAL_ID_CABANA_DOS_ALQUIMISTAS, "Cabana dos Alquimistas","Há várias plantas esquisitas nas prateleiras.",                       null,                              QuestPorID(QUEST_ID_LIMPAR_JARDIM_DOS_ALQUIMISTAS), null);
            var jardimAlquimistas = new Local(LOCAL_ID_JARDIM_DOS_ALQUIMISTAS, "Jardim dos Alquimistas","Várias plantas estão crescendo aqui.",                                null,                              null,                                         MonstroPorID(MONSTRO_ID_RATO));
            var casaFazenda       = new Local(LOCAL_ID_CASA_DA_FAZENDA,        "Casa de Fazenda",       "Há uma pequena casa de fazenda aqui, com um fazendeiro na frente.",   null,                              QuestPorID(QUEST_ID_LIMPAR_AREA_DOS_CAMPONESES),    null);
            var areaCamponeses    = new Local(LOCAL_ID_AREA_DOS_CAMPONESES,    "Area dos Camponeses",   "Você vê vegetais crescendo aqui.",                                    null,                              null,                                         MonstroPorID(MONSTRO_ID_COBRA));
            var postoGuarda       = new Local(LOCAL_ID_POSTO_DE_GUARDA,        "Posto de Guarda",       "Há um grande e forte guarda aqui.",                                   ItemPorID(ITEM_ID_PASSE_AVENTUREIRO), null,                                      null);
            var ponte             = new Local(LOCAL_ID_PONTE,                  "Ponte",                 "Uma ponte de pedra que cruza um rio grande.",                         null,                              null,                                         null);
            var campoAranha       = new Local(LOCAL_ID_CAMPO_DAS_ARANHAS,      "Floresta",              "Você vê teias de aranha cobrindo as árvores.",                        null,                              null,                                         MonstroPorID(MONSTRO_ID_ARANHA_GIGANTE));

            // Liga os locais pelo mapa (desenhado à mão para não errar as conexões)
            casa.LocalParaNorte              = praca;

            praca.LocalParaNorte             = cabanaAlquimistas;
            praca.LocalParaSul               = casa;
            praca.LocalParaLeste             = postoGuarda;
            praca.LocalParaOeste             = casaFazenda;

            casaFazenda.LocalParaLeste       = praca;
            casaFazenda.LocalParaOeste       = areaCamponeses;

            areaCamponeses.LocalParaLeste    = casaFazenda;

            cabanaAlquimistas.LocalParaSul   = praca;
            cabanaAlquimistas.LocalParaNorte = jardimAlquimistas;

            jardimAlquimistas.LocalParaSul   = cabanaAlquimistas;

            postoGuarda.LocalParaLeste       = ponte;
            postoGuarda.LocalParaOeste       = praca;

            ponte.LocalParaOeste             = postoGuarda;
            ponte.LocalParaLeste             = campoAranha;

            campoAranha.LocalParaOeste       = ponte;

            // Adiciona todos os locais à lista estática
            Locais.Add(casa);
            Locais.Add(praca);
            Locais.Add(postoGuarda);
            Locais.Add(cabanaAlquimistas);
            Locais.Add(jardimAlquimistas);
            Locais.Add(casaFazenda);
            Locais.Add(areaCamponeses);
            Locais.Add(ponte);
            Locais.Add(campoAranha);
        }

        // Métodos auxiliares para buscar objetos pelo ID nas listas estáticas
        public static Item    ItemPorID(int id)    => Itens.FirstOrDefault(x => x.ID == id);
        public static Monstro MonstroPorID(int id) => Monstros.FirstOrDefault(x => x.ID == id);
        public static Quest   QuestPorID(int id)   => Quests.FirstOrDefault(x => x.ID == id);
        public static Local   LocalPorID(int id)   => Locais.FirstOrDefault(x => x.ID == id);
    }
}
