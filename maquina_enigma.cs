using System;
using System.Collections.Generic;
using System.Linq;

namespace EnigmaBlindada
{
    interface ICifrador
    {
        string Cifrar(string texto);
    }

    class Rotor
    {
        private readonly char[] mapaDirecto;
        private readonly char[] mapaInverso;
        private int posicion;

        public Rotor(string cableado, int posicionInicial)
        {
            if (string.IsNullOrWhiteSpace(cableado) || cableado.Length != 26 || cableado.Distinct().Count() != 26)
                throw new ArgumentException("Cableado de rotor inválido.");
            mapaDirecto = cableado.ToUpper().ToCharArray();
            mapaInverso = new char[26];
            for (int i = 0; i < 26; i++)
                mapaInverso[mapaDirecto[i] - 'A'] = (char)('A' + i);
            posicion = posicionInicial % 26;
        }

        public char Adelante(char letra)
        {
            int indice = (letra - 'A' + posicion) % 26;
            return mapaDirecto[indice];
        }

        public char Atras(char letra)
        {
            int indice = (mapaInverso[letra - 'A'] - 'A' - posicion + 26) % 26;
            return (char)('A' + indice);
        }

        public void Avanzar()
        {
            posicion = (posicion + 1) % 26;
        }

        public bool EnMarca()
        {
            return posicion == 0;
        }
    }

    class Reflector
    {
        private readonly Dictionary<char, char> mapa;

        public Reflector(string cableado)
        {
            if (string.IsNullOrWhiteSpace(cableado) || cableado.Length != 26 || cableado.Distinct().Count() != 26)
                throw new ArgumentException("Cableado de reflector inválido.");
            mapa = new Dictionary<char, char>();
            for (int i = 0; i < 26; i++)
            {
                char a = (char)('A' + i);
                char b = cableado[i];
                if (mapa.ContainsKey(b) && mapa[b] != a)
                    throw new ArgumentException("Reflector no simétrico.");
                mapa[a] = b;
                mapa[b] = a;
            }
        }

        public char Reflejar(char letra)
        {
            return mapa[letra];
        }
    }

    class Tablero
    {
        private readonly Dictionary<char, char> conexiones;

        public Tablero(Dictionary<char, char> pares)
        {
            conexiones = new Dictionary<char, char>();
            foreach (var par in pares)
            {
                char a = char.ToUpper(par.Key);
                char b = char.ToUpper(par.Value);
                if (a == b || conexiones.ContainsKey(a) || conexiones.ContainsKey(b))
                    throw new ArgumentException("Par inválido en el tablero.");
                conexiones[a] = b;
                conexiones[b] = a;
            }
        }

        public char Intercambiar(char letra)
        {
            return conexiones.ContainsKey(letra) ? conexiones[letra] : letra;
        }
    }

    class MaquinaEnigma : ICifrador
    {
        private readonly Rotor[] rotores;
        private readonly Reflector reflector;
        private readonly Tablero tablero;

        public MaquinaEnigma(Rotor[] rotores, Reflector reflector, Tablero tablero)
        {
            if (rotores == null || rotores.Length == 0)
                throw new ArgumentException("Debe haber al menos un rotor.");
            this.rotores = rotores;
            this.reflector = reflector;
            this.tablero = tablero;
        }

        public string Cifrar(string texto)
        {
            var resultado = new List<char>();
            foreach (char caracter in texto.ToUpper())
            {
                if (!char.IsLetter(caracter)) continue;
                AvanzarRotores();
                char letra = tablero.Intercambiar(caracter);
                foreach (var rotor in rotores)
                    letra = rotor.Adelante(letra);
                letra = reflector.Reflejar(letra);
                for (int i = rotores.Length - 1; i >= 0; i--)
                    letra = rotores[i].Atras(letra);
                letra = tablero.Intercambiar(letra);
                resultado.Add(letra);
            }
            return new string(resultado.ToArray());
        }

        private void AvanzarRotores()
        {
            rotores[0].Avanzar();
            for (int i = 1; i < rotores.Length; i++)
            {
                if (rotores[i - 1].EnMarca())
                    rotores[i].Avanzar();
                else break;
            }
        }
    }

    class Programa
    {
        static void Main()
        {
            var rotorA = new Rotor("EKMFLGDQVZNTOWYHXUSPAIBRCJ", 0);
            var rotorB = new Rotor("AJDKSIRUXBLHWTMCQGZNPYFVOE", 0);
            var rotorC = new Rotor("BDFHJLCPRTXVZNYEIWGAKMUSQO", 0);
            var reflector = new Reflector("YRUHQSLDPXNGOKMIEBFZCWVJAT");
            var tablero = new Tablero(new Dictionary<char, char> {
                { 'A', 'M' }, { 'G', 'L' }, { 'E', 'T' }
            });
            var enigma = new MaquinaEnigma(new[] { rotorA, rotorB, rotorC }, reflector, tablero);
            Console.Write("Texto a cifrar: ");
            string entrada = Console.ReadLine();
            string salida = enigma.Cifrar(entrada);
            Console.WriteLine("Texto cifrado: " + salida);
        }
    }
}
