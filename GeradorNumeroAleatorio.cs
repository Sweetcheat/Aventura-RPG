using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;


namespace Motor
{
    // Estou usando uma classe chamada 'RANDOM' do .NET FRAMEWORK, só que a geração de numeros 'não é bem aleatória',
    // como o jogo é simples, não tem problemas. Mas se fosse um projeto maior, seria interessante usar um algoritmo melhor e mais COMPLEXO.
    public static class GeradorNumeroAleatorio
    {
        private static Random _gerador = new Random();

        public static int NumeroEntre(int valorMinimo, int valorMaximo)
        {
            return _gerador.Next(valorMinimo, valorMaximo);
        }
    }
}

  // ESSE É UM GERADOR DE NUMEROS ALEATÓRIOS COMPLEXO.
   // NÃO FOI DESENVOLVIDO POR MIM. Eu procurei e encontrei esse, e a principio funciona bem com o jogo.
/*   
   public static class GeradorNumeroAleatorio
    {
        private static readonly RNGCryptoServiceProvider _gerador = new RNGCryptoServiceProvider();
 
        public static int NumeroEntre(int valorMinimo, int valorMaximo)
        {
            byte[] numeroAleatorio = new byte[1];
 
            _gerador.GetBytes(numeroAleatorio);
 
         double caractereAleatorioDeValorAscii = Convert.ToDouble(numeroAleatorio[0]);
 
         // Aqui estou usando Math.Max, e subtraindo 0.00000000001,
          // para certificar que o "multiplicador" sempre estará entre 0.0 e .99999999999
           // Do contrário, é possivel que o numero seja 1, aí pode causar problemas no ciclo.

         double multiplicador = Math.Max(0, (caractereAleatorioDeValorAscii / 255d) - 0.00000000001d);
 
           // Aqui é necessário adicionar +1 à variável, para permitir a conclusão do ciclo com Math.Floor
           
            int alcance = valorMaximo - valorMinimo + 1;
 
            double valorAleatorioEmAlcance = Math.Floor(multiplicador * alcance); 
 
            return (int)(valorMinimo + valorAleatorioEmAlcance);
        }
    }
}
 */
