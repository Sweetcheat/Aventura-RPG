using System;


namespace Motor
{
    // Estou usando uma classe chamada 'RANDOM' do .NET FRAMEWORK, só que a geração de numeros 'não é bem aleatória',
    // como o jogo é simples, não tem problemas. Mas se fosse um projeto maior, seria interessante usar um algoritmo melhor e mais COMPLEXO.
    public static class GeradorNumeroAleatorio
    {
        private static Random _gerador = new Random();

        // CORREÇÃO: valorMaximo + 1 para que o valor máximo seja inclusivo.
        // Random.Next(min, max) é exclusivo no max, então sem o +1 o dano máximo nunca seria atingido.
        public static int NumeroEntre(int valorMinimo, int valorMaximo)
        {
            if (valorMinimo > valorMaximo)
                throw new ArgumentOutOfRangeException("valorMinimo", "O valor mínimo não pode ser maior que o valor máximo.");

            return _gerador.Next(valorMinimo, valorMaximo + 1);
        }
    }
}

