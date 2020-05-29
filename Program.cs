using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;

namespace PacMan
{
    class Program
    {
        const int LINHAS = 20;
        const int COLUNAS = 20;
        
        static char[,] mapa;

        static int pacX = 1; //colunas = top
        static int pacY = 1; //linhas = left
        static char pac = (char)PacPosicao.DIREITA;
        static PacPosicao pacPosicaoAtual = PacPosicao.DIREITA;
        static bool pacAnimado = true;  

        static int vidas = 3;
        static int pontos = 0;
        static int quatDots = 0;
        static bool contaDots = true;

        static char ghost = 'A';
        static int ghostX = 5;
        static int ghostY = 5;
        static int ghostSentido = 1;
        static int ghostDurSentido = 5;

        static void Main(string[] args)
        {

            Console.WindowHeight = LINHAS + 3;
            Console.WindowWidth = (COLUNAS * 2);

            //inicialização dos gameobjects (elementos)
            inicializaMapa();

            //Debug
            //carregaObstaculo();
            //Console.ReadKey();

            //gameloop
            while (true)
            {
                Console.Clear();
                
                desenhaMapa();

                //entrada de dados (leitura do comando do usuário)
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo keyInfo = Console.ReadKey();
                    moverJogador(keyInfo);
                }
                //processamento
                animaPacMan();
                comeCome();
                moveGhost();

                System.Threading.Thread.Sleep(100);
            }

        }

        static void moveGhost()
        {
            //algoritmos de comportamento aleatório
            if (ghostDurSentido > 0)
            {
                ghostDurSentido--;
            }
            else
            {
                ghostDurSentido = new Random().Next(1, mapa.GetLength(0));
                ghostSentido = new Random().Next(1, 5); //cima, baixo, direita, esquerda
            }

            switch (ghostSentido)
            {
                case 1: //cima
                    if (verificaPosicao(ghostX, ghostY - 1))
                    {
                        ghostY--;
                    }
                    else
                    {
                        ghostDurSentido = 0;
                    }
                    break;
                case 2: //baixo
                    if (verificaPosicao(ghostX, ghostY + 1))
                    {
                        ghostY++;
                    }
                    else
                    {
                        ghostDurSentido = 0;
                    }
                    break;
                case 3: //direita
                    if (verificaPosicao(ghostX + 1, ghostY))
                    {
                        ghostX++;
                    }
                    else
                    {
                        ghostDurSentido = 0;
                    }
                    break;
                case 4:
                    if (verificaPosicao(ghostX - 1, ghostY))
                    {
                        ghostX--;
                    }
                    else
                    {
                        ghostDurSentido = 0;
                    }
                    break;
            }

            //verificar a colisão
            if ((pacX == ghostX) && (pacY == ghostY))
            {
                vidas--;
                pacPosicaoAtual = PacPosicao.MORREU;
                desenhaMapa();

                Console.Beep();

                if (vidas > 0)
                {
                    //reposicionar os personagens
                    pacX = 1;
                    pacY = 1;

                    ghostX = 5;
                    ghostY = 5;
                }
                else
                {
                    imprimeMensagem("Game Over");
                    vidas = 3;
                }

                Console.ReadKey();

            }

        }

        public static void moverJogador(ConsoleKeyInfo keyInfo)
        {
            if (keyInfo.Key == ConsoleKey.UpArrow)
            {
                //diminuir o valor da posição do jogador y
                if (pacY > 0)
                {
                    //verificar atingiu um obstáculo
                    if (verificaPosicao(pacX, pacY - 1))
                    {
                        pacY--;
                    }
                }
                pacPosicaoAtual = PacPosicao.CIMA;
            }
            else if (keyInfo.Key == ConsoleKey.DownArrow)
            {
                //aumentar o valor da posição do jogador y
                if (pacY < (mapa.GetLength(0) - 1))
                {
                    if (verificaPosicao(pacX, pacY + 1))
                    {
                        pacY++;
                    }
                }
                pacPosicaoAtual = PacPosicao.BAIXO;
            }
            else if (keyInfo.Key == ConsoleKey.RightArrow)
            {
                //aumentar o valor da posição do jogador x
                if (pacX < (mapa.GetLength(1) - 1))
                {
                    if (verificaPosicao(pacX + 1, pacY))
                    {
                        pacX++;
                    }
                }
                pacPosicaoAtual = PacPosicao.DIREITA;
            }
            else if (keyInfo.Key == ConsoleKey.LeftArrow)
            {
                //diminuir o valor da posição do jogador x
                if (pacX > 0)
                {
                    if (verificaPosicao(pacX - 1, pacY))
                    {
                        pacX--;
                    }
                }
                pacPosicaoAtual = PacPosicao.ESQUERDA;
            }
        }

        static void animaPacMan()
        {
            if (pacAnimado)
            {
                pacAnimado = false;
                pac = (char)PacPosicao.FECHADO;
            }
            else
            {
                pacAnimado = true;
                pac = (char)pacPosicaoAtual;
            }
        }

        static void comeCome()
        {
            if (mapa[pacY, pacX] == '.')
            {
                mapa[pacY, pacX] = ' ';
                quatDots--; //temos que carregar os dots
                pontos += 10;

                //adicionar som
                Console.Beep(116, 100);
                Console.Beep(163, 80);
            }

            if (quatDots == 0)
            {
                imprimeMensagem("Parabéns! Você ganhou.");
                Console.ReadKey();
                inicializaMapa(); //restart (próximo level)
            }

        }

        static void imprimeMensagem(string mensagem)
        {
            //imprimir no meio da tela
            Console.SetCursorPosition(((Console.WindowWidth / 2) - (mensagem.Length / 2)), 
                (Console.WindowHeight /2) - 1);
            Console.WriteLine(mensagem);
        }

        static bool verificaPosicao(int x, int y)
        {
            if (mapa[y, x] == 'H')//tá invertido mesmo!
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static void inicializaMapa()
        {

            mapa = new char[LINHAS, COLUNAS];

            contaDots = true;

            //popular
            for (int i = 0; i < mapa.GetLength(0); i++)//linhas
            {
                for (int j = 0; j < mapa.GetLength(1); j++)//coluna
                {
                    //colocar H na primeira linha: i == 0
                    //colocar H na última linha: i == (linhas - 1)
                    //colocar H na primeira coluna: j == 0
                    //colocar H na última coluna: j == (colunas - 1)
                    if ((i == 0) 
                        || (i == (mapa.GetLength(0) - 1)) 
                        || (j == 0) 
                        || (j == (mapa.GetLength(1) - 1)))
                    {
                        mapa[i, j] = 'H';
                    }
                    else
                    {
                        mapa[i, j] = '.';
                        quatDots++;
                    }
                }
            }

            posicionaPersonagens();

        }

        static void posicionaPersonagens()
        {
            pacX = 1;
            pacY = 1;
            pac = (char)PacPosicao.DIREITA;
        }

        //varre o mapa
        public static void desenhaMapa()
        {
            Console.SetCursorPosition(0,0);
            Console.WriteLine("{0}UP\t\tSCORE\n\t\t{1}", vidas, pontos);

            for (int i = 0; i < mapa.GetLength(0); i++) //linhas
            {
                for (int j = 0; j < mapa.GetLength(1); j++)//colunas
                {
                    Console.Write(mapa[i, j] + " ");
                }
                Console.WriteLine();
            }

            carregaObstaculo();
            desenha(pacX, pacY, pac, ConsoleColor.Yellow);
            desenha(ghostX, ghostY, ghost, ConsoleColor.Red);

            Console.SetCursorPosition((mapa.GetLength(1) * 2) - 2, mapa.GetLength(0) + 1);
        }

        public static void desenha(int x, int y, char caracter, ConsoleColor cor)
        {
            //a impressão é em relação a tela
            Console.SetCursorPosition(x * 2, y + 2);
            Console.ForegroundColor = cor;
            Console.Write(caracter);
            Console.ResetColor();
        }

        public static void carregaObstaculo()
        {
            var pastaTrabalho = Environment.CurrentDirectory;
            var caminho = Directory.GetParent(pastaTrabalho).Parent.Parent.FullName;
            var arquivo = Path.Combine(caminho, @"Dados\obstaculos.txt");

            //Debug
            //Console.WriteLine("Caminho: {0}", arquivo);

            if (File.Exists(arquivo))
            {
                //Console.WriteLine("Arquivo encontrado...");
                string[] linhas = File.ReadAllLines(arquivo);

                int i, j;

                foreach(string linha in linhas)
                {
                    if (!(linha.Trim() == "")) //não é linha em branco
                    {
                        string[] valores = linha.Split(",");
                        i = Convert.ToInt32(valores[0]);
                        j = Convert.ToInt32(valores[1]);

                        mapa[i, j] = 'H';
                        if (contaDots)
                        {
                            quatDots--;
                        }
                            
                    }
                }
                contaDots = false;
            }
            

        }

    }

    enum PacPosicao
    {
        CIMA = 'v',
        BAIXO = '^',
        DIREITA = '<',
        ESQUERDA = '>',
        MORREU = '*',
        FECHADO = 'o'
    }

}
