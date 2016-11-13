using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motor
{
   public static class Mundo
    {
       /*Isso é uma lista de variáveis estáticas. Funcionam de forma similar a propriedades em uma classe. 
         Aqui estou trazendo todos os objetos para o mundo e então vou ler eles no decorrer do programa.
       */  

        public static readonly List<Item> Itens = new List<Item>();
        public static readonly List<Monstro> Monstros = new List<Monstro>();
        public static readonly List<Quest> Quests = new List<Quest>();
        public static readonly List<Local> Locais = new List<Local>();
 
       /* A partir daqui temos valores constantes pois nunca irão mudar.
          Vou usar essas constantes, que tem nome meio em português, para não ter que lembrar o ID de cada objeto no jogo.
          Sem saber o nome, se eu tivesse que criar uma aranha gigante para o jogador lutar, eu teria que lembrar o ID da aranha.
          Já com as constantes, eu poderia dizer assim: "pegue um o monstro em 'MUNDO' onde o ID é igual a MONSTRO_ID_ARANHA_GIGANTE."
          É assim que a maioria dos programadores de jogos determinam os objetos nos jogos.
       */   

        // Itens do jogo.

        public const int ITEM_ID_ESPADA_ENFERRUJADA = 1;
        public const int ITEM_ID_CAUDA_DE_RATO = 2;
        public const int ITEM_ID_PELO_DE_RATO = 3;
        public const int ITEM_ID_PRESA_DE_COBRA = 4;
        public const int ITEM_ID_PELE_DE_COBRA = 5;
        public const int ITEM_ID_PORRETE = 6;
        public const int ITEM_ID_POCAO_DE_CURA = 7;
        public const int ITEM_ID_PRESA_DE_ARANHA = 8;
        public const int ITEM_ID_SEDA_DE_ARANHA = 9;
        public const int ITEM_ID_PASSE_AVENTUREIRO = 10;

       // MONSTROS (que não são tão monstros assim, futuramente adicionarei mais)
 
        public const int MONSTRO_ID_RATO = 1;
        public const int MONSTRO_ID_COBRA = 2;
        public const int MONSTRO_ID_ARANHA_GIGANTE = 3;

       // QUESTS (também pretendo adicionar mais)
 
        public const int QUEST_ID_LIMPAR_JARDIM_DOS_ALQUIMISTAS = 1;
        public const int QUEST_ID_LIMPAR_AREA_DOS_CAMPONESES = 2;

       // LOCAIS (mais locais também está em meus planos)
 
        public const int LOCAL_ID_CASA = 1;
        public const int LOCAL_ID_PRACA = 2;
        public const int LOCAL_ID_POSTO_DE_GUARDA = 3;
        public const int LOCAL_ID_CABANA_DOS_ALQUIMISTAS = 4;
        public const int LOCAL_ID_JARDIM_DOS_ALQUIMISTAS = 5;
        public const int LOCAL_ID_CASA_DA_FAZENDA = 6;
        public const int LOCAL_ID_AREA_DOS_CAMPONESES = 7;
        public const int LOCAL_ID_PONTE = 8;
        public const int LOCAL_ID_CAMPO_DAS_ARANHAS = 9;

 
       /* Isso é um construtor estático. Você deve estar pensando: "Pera, não dá para instanciar uma classe estática, 
          então por que ela tem um construtor? Pois é para isso que um construtor é usado, para instanciar um objeto!"
          Com a classe estática, o codigo do construtor vai ser o primeiro a ser lido pela primeira vez quando alguém usar
          alguma coisa na classe. Então, quando o jogo começar eu quero mostrar informações sobre o local atual do jogador,
          e quando eu precisar pegar algum dado da classe MUNDO, o metodo construtor vai ser lido, e as listas serão preenchidas.
       */   

       /* Dentro do construtor, eu criei 4 metodos para preencher listas diferentes.
          Eu não quero ter que separar os metodos, eu até poderia colocar todo o codigo dentro de cada metodo de uma vez,
          porém dessa maneira fica mais fácil de ler e trabalhar.
       */   

        static Mundo()
        {
            ColoqueItems();
            ColoqueMonstros();
            ColoqueQuests();
            ColoqueLocais();
        }

       /* A partir daqui são os metodos que eu usei para adicionar objetos ao mundo do jogo, adicionando eles a lista estática aqui em cima
          Usando o metodo "Add()" em uma lista de variavel ou propriedade, dá para adicionar um objeto a esta lista.
          Na primeira linha do metodo "ColoqueItems()", eu estou adicionando um "objeto Arma novo" a lista de itens.
          Quando eu uso "new Arma()", o construtor da classe da Arma retorna um objeto "Arma" com os parametros que eu passei.
          Uma vez que está tudo dentro de "Itens.Add()", aquele objeto é adicionado a lista de Itens.
       */   

        private static void ColoqueItems()
        {
            Itens.Add(new Arma(ITEM_ID_ESPADA_ENFERRUJADA, "Espada Enferrujada", "Espadas Enferrujadas", 0, 5));
            Itens.Add(new Item(ITEM_ID_CAUDA_DE_RATO, "Cauda de rato", "Caudas de rato"));
            Itens.Add(new Item(ITEM_ID_PELO_DE_RATO, "Pelo de rato", "Pelos de rato"));
            Itens.Add(new Item(ITEM_ID_PRESA_DE_COBRA, "Presa de cobra", "Presas de cobra"));
            Itens.Add(new Item(ITEM_ID_PELE_DE_COBRA, "Pele de cobra", "Pele de cobras"));
            Itens.Add(new Arma(ITEM_ID_PORRETE, "Porrete", "Porretes", 3, 10));
            Itens.Add(new PocaoCura(ITEM_ID_POCAO_DE_CURA, "Pocao de cura", "Pocoes de cura", 5));
            Itens.Add(new Item(ITEM_ID_PRESA_DE_ARANHA, "Presa de aranha", "Presas de Aranha"));
            Itens.Add(new Item(ITEM_ID_SEDA_DE_ARANHA, "Seda de aranha", "Sedas de Aranha"));
            Itens.Add(new Item(ITEM_ID_PASSE_AVENTUREIRO, "Passe de Aventureiro", "Passes de Aventureiro"));
        }

        private static void ColoqueMonstros()
        {
            Monstro rato = new Monstro(MONSTRO_ID_RATO, "Rato", 5, 3, 10, 3, 3);
            rato.LootTable.Add(new ItemLoot(ItemPorID(ITEM_ID_CAUDA_DE_RATO), 75, false));
            rato.LootTable.Add(new ItemLoot(ItemPorID(ITEM_ID_PELO_DE_RATO), 75, true));
 
            Monstro cobra = new Monstro(MONSTRO_ID_COBRA, "Cobra", 5, 3, 10, 3, 3);
            cobra.LootTable.Add(new ItemLoot(ItemPorID(ITEM_ID_PRESA_DE_COBRA), 75, false));
            cobra.LootTable.Add(new ItemLoot(ItemPorID(ITEM_ID_PELE_DE_COBRA), 75, true));
 
            Monstro aranhaGigante = new Monstro(MONSTRO_ID_ARANHA_GIGANTE, "Aranha gigante", 20, 20, 40, 10, 10);
            aranhaGigante.LootTable.Add(new ItemLoot(ItemPorID(ITEM_ID_PRESA_DE_ARANHA), 75, true));
            aranhaGigante.LootTable.Add(new ItemLoot(ItemPorID(ITEM_ID_SEDA_DE_ARANHA), 25, false));
 
            Monstros.Add(rato);
            Monstros.Add(cobra);
            Monstros.Add(aranhaGigante);
        }

        private static void ColoqueQuests()
        {
            Quest limparJardimAlquimista =
                new Quest(QUEST_ID_LIMPAR_JARDIM_DOS_ALQUIMISTAS,
                    "Limpe o Jardim dos Alquimistas",
                    "Mate os ratos no jardim dos alquimistas e traga 3 caudas de rato. Você irá receber uma pocao de cura e 10 moedas de ouro.", 20, 10);

            limparJardimAlquimista.QuestCompletadaItem.Add(new QuestCompletadaItem(ItemPorID(ITEM_ID_CAUDA_DE_RATO), 3));

            limparJardimAlquimista.ItemRecompensa = ItemPorID(ITEM_ID_POCAO_DE_CURA);
 
            Quest limparAreaCamponeses =
                new Quest(QUEST_ID_LIMPAR_AREA_DOS_CAMPONESES,
                    "Limpar a área dos camponeses",
                    "Mate cobras na area dos camponeses e traga 3 presas de cobra. Você irá receber um passe de aventureiro e 20 moedas de ouro.", 20, 20);

            limparAreaCamponeses.QuestCompletadaItem.Add(new QuestCompletadaItem(ItemPorID(ITEM_ID_PRESA_DE_COBRA), 3));

            limparAreaCamponeses.ItemRecompensa = ItemPorID(ITEM_ID_PASSE_AVENTUREIRO);

            Quests.Add(limparJardimAlquimista);
            Quests.Add(limparAreaCamponeses);
        }

        private static void ColoqueLocais()
        {
            // Cria cada local
            Local casa = new Local(LOCAL_ID_CASA, "Casa", "Está é a sua casa. Você precisa limpar esse lugar.", null, null, null);

            Local praca = new Local(LOCAL_ID_PRACA, "Praça", "Você vê uma fonte grande.", null, null, null);

            Local cabanaAlquimistas = new Local(LOCAL_ID_CABANA_DOS_ALQUIMISTAS, "Cabana dos Alquimistas", "Há várias plantas esquisitas nas prateleiras.", null, null, null);
            cabanaAlquimistas.QuestDisponivelAqui = QuestPorID(QUEST_ID_LIMPAR_JARDIM_DOS_ALQUIMISTAS);

            Local jardimAlquimistas = new Local(LOCAL_ID_JARDIM_DOS_ALQUIMISTAS, "Jardim dos Alquimistas", "Várias plantas estão crescendo aqui.", null, null, null);
            jardimAlquimistas.MonstroVivoAqui = MonstroPorID(MONSTRO_ID_RATO);

            Local casaFazenda = new Local(LOCAL_ID_CASA_DA_FAZENDA, "Casa de Fazenda", "Há uma pequena casa de fazenda aqui, com um fazendeiro na frente.", null, null, null);
            casaFazenda.QuestDisponivelAqui = QuestPorID(QUEST_ID_LIMPAR_AREA_DOS_CAMPONESES);

            Local areaCamponeses = new Local(LOCAL_ID_AREA_DOS_CAMPONESES, "Area dos Camponeses", "Você vê vegetais crescendo aqui.", null, null, null);
            areaCamponeses.MonstroVivoAqui = MonstroPorID(MONSTRO_ID_COBRA);

            Local postoGuarda = new Local(LOCAL_ID_POSTO_DE_GUARDA, "Posto de Guarda", "Há um grande e forte guarda aqui.", ItemPorID(ITEM_ID_PASSE_AVENTUREIRO), null, null);

            Local ponte = new Local(LOCAL_ID_PONTE, "Ponte", "Uma ponte de pedra que cruza um rio grande.", null, null, null);

            Local campoAranha = new Local(LOCAL_ID_CAMPO_DAS_ARANHAS, "Floresta", "Você vê teias de aranha cobrindo as árvores.", null, null, null);
            campoAranha.MonstroVivoAqui = MonstroPorID(MONSTRO_ID_ARANHA_GIGANTE);
 
           
            /* liga os locais juntos
               aqui vai ligar ao mundo com a classe Local
               aqui foi necessário cuidado, mas muito cuidado. Tive que desenhar o mapa pra poder saber quais locais 
               vão se ligar com o outro. (obviamente)
            */   

            casa.LocalParaNorte = praca;

            praca.LocalParaNorte = cabanaAlquimistas; 
            praca.LocalParaSul = casa;
            praca.LocalParaLeste = postoGuarda;
            praca.LocalParaOeste = casaFazenda;

            casaFazenda.LocalParaLeste = praca;
            casaFazenda.LocalParaOeste = areaCamponeses;

            areaCamponeses.LocalParaLeste = casaFazenda;

            cabanaAlquimistas.LocalParaSul = praca;
            cabanaAlquimistas.LocalParaNorte = jardimAlquimistas;

            jardimAlquimistas.LocalParaSul = cabanaAlquimistas;

            postoGuarda.LocalParaLeste = ponte;
            postoGuarda.LocalParaOeste = praca;

            ponte.LocalParaOeste = postoGuarda;
            ponte.LocalParaLeste = campoAranha;

            campoAranha.LocalParaOeste = ponte;
 
            // Adiciona os locais a lista estática.

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

       /* Estes são os metodos que eu posso usar para pegar valores das listas estáticas. Eu poderia acessar as listas lá em cima diretamente,
          uma vez que elas são publicas. Porém, eu farei de uma maneira diferente. */
       
       /* Eu passo o ID de um objeto que eu quero obter da lista (usando os IDs das constantes lá em cima).
          O metodo irá olhar em cada item da lista (usando o "foreach") e ver se o ID que eu passei corresponde ao ID do objeto.
          Se sim, vai retornar o objeto para mim.
          Se chegar no final da lista, e não bater (no qual isso nunca deve acontecer), o metodo retorna "null".(nada) 
       */
      
        public static Item ItemPorID(int id)
        {
            foreach(Item item in Itens)
            {
                if(item.ID == id)
                {
                    return item;
                }
            }
 
            return null;
        }
 
        public static Monstro MonstroPorID(int id)
        {
            foreach(Monstro monstro in Monstros)
            {
                if(monstro.ID == id)
                {
                    return monstro;
                }
            }
 
            return null;
        }
 
        public static Quest QuestPorID(int id)
        {
            foreach(Quest quest in Quests)
            {
                if(quest.ID == id)
                {
                    return quest;
                }
            }
 
            return null;
        }
 
        public static Local LocalPorID(int id)
        {
            foreach(Local local in Locais)
            {
                if(local.ID == id)
                {
                    return local;
                }
            }
 
            return null;

        }
    }
}
