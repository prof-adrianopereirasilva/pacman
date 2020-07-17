using System;
using System.IO;
using System.Runtime.InteropServices;//macOS

namespace JogoPacMan
{
    class PacMan
    {

        //Biblioteca no MacOS para setar o tamanho da janela no mac
        [DllImport("libc")]
        private static extern int system(string exec);

        const int LINHAS = 31;
        const int COLUNAS = 28;

        static char[,] mapa;

        static int vidas = 3;
        static int pontos = 0;
        static int dots = 0;
        static bool contaDots = false;

        const int tempoThread = 160;
        const int tempoEnergizado = 50;

        //elementos do jogo
        const ConsoleColor paredeCor = ConsoleColor.DarkBlue;

        //pacman
        static int pacXInicial = 13;
        static int pacYInicial = 23;//linha
        static int pacX; 
        static int pacY; 
        static char pac; 
        static bool pacAnimado = true;
        static bool pacEnergizado = false;
        static int pacEnergizadoTempo = tempoEnergizado;
        static PacSprites PacSpritesAtual = PacSprites.DIREITA;

        //Ghosts
        //seria bem mais fácil se fosse OO ;D

        static char ghost = '\u156C';

        //Blinky
        static int gBlinkyXInicial = 11;
        static int gBlinkyYInicial = 13;
        static int gBlinkyX;
        static int gBlinkyY;
        static int gBlinkySentido = 1;
        static int gBlinkyDurSentido = 5;
        static bool gBlinkyVivo = true;

        //Pinky
        static int gPinkyXInicial = 13;
        static int gPinkyYInicial = 14;
        static int gPinkyX;
        static int gPinkyY;
        static int gPinkySentido = 1;
        static int gPinkyDurSentido = 5;
        static bool gPinkyVivo = true;

        //Inky
        static int gInkyXInicial = 14;
        static int gInkyYInicial = 14;
        static int gInkyX;
        static int gInkyY;
        static int gInkySentido = 1;
        static int gInkyDurSentido = 5;
        static bool gInkyVivo = true;

        //Clyde
        static int gClydeXInicial = 15;
        static int gClydeYInicial = 14;
        static int gClydeX;
        static int gClydeY;
        static int gClydeSentido = 1;
        static int gClydeDurSentido = 5;
        static bool gClydeVivo = true;

        static void Main(string[] args)
        {
            //para ajustar o tamanho da janela no MacOS
            system(@"printf '\e[8;" + (LINHAS + 3) + ";" + (COLUNAS * 2) + "t'");

            //ajustar janela no windows
            //Console.WindowHeight = LINHAS + 3;
            //Console.WindowWidth = COLUNAS * 2;

            Console.Clear();

            inicializa();

            while (true)
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo keyInfo = Console.ReadKey();
                    //mover o jogador
                    moverJogador(keyInfo);
                }
                animaPacMan();
                movimentaGhosts();
                desenhaElementos();
                //controla a velocidade do nosso loop
                System.Threading.Thread.Sleep(tempoThread);
            }

        }

        public static void inicializa()
        {
            mapa = new char[LINHAS, COLUNAS];
            //Preencher o vetor
            //for linha
            for (int i = 0; i < mapa.GetLength(0); i++)
            {
                //for para coluna
                for (int j = 0; j < mapa.GetLength(1); j++)
                {
                    //colocar PAREDE:
                    //na primeira linha: i == 0
                    //na última linha: i == (linhas - 1)
                    //na primeira coluna: j == 0
                    //na última coluna: j == (colunas - 1)
                    //restante colocar dots
                    if ((i == 0 || (i == LINHAS - 1)) || (j == 0 || (j == COLUNAS - 1)))
                    {
                        mapa[i, j] = (char)ElementoTipo.PAREDE;
                    }
                    else
                    {
                        mapa[i, j] = (char)ElementoTipo.DOT;
                        dots++;
                    }
                }
            }
            //carregar elementos no mapa (paredes extras; fantasmas; pílulas)
            carregaElemento(ElementoTipo.PAREDE);
            carregaElemento(ElementoTipo.GHOSTZONE);
            carregaElemento(ElementoTipo.PILULA);

            posicionaPersonagens();

        }

        public static void posicionaPersonagens()
        {
            pac = (char)PacSprites.DIREITA;
            pacX = pacXInicial; 
            pacY = pacYInicial; 

            gBlinkyX = gBlinkyXInicial;
            gBlinkyY = gBlinkyYInicial;

            gPinkyX = gPinkyXInicial;
            gPinkyY = gPinkyYInicial;

            gInkyX = gInkyXInicial;
            gInkyY = gInkyYInicial;

            gClydeX = gClydeXInicial;
            gClydeY = gClydeYInicial;
        }

        public static void desenhaElementos()
        {
            //imprimindo o vetor
            Console.SetCursorPosition(0, 0);
            Console.Write("{0}UP", vidas);

            Console.SetCursorPosition(((Console.WindowWidth / 2) - 2), 0);
            Console.Write("SCORE");

            Console.SetCursorPosition(((Console.WindowWidth / 2) -
                (pontos.ToString().Length / 2)), 1);
            Console.WriteLine("{0}", pontos);

            //for para linha
            for (int i = 0; i < mapa.GetLength(0); i++)
            {
                //for para coluna
                for (int j = 0; j < mapa.GetLength(1); j++)
                {
                    if (mapa[i, j] == (char)ElementoTipo.PAREDE)
                    {
                        Console.ForegroundColor = paredeCor;
                    }
                    else
                    {
                        Console.ResetColor();
                    }
                    Console.Write(mapa[i, j] + " ");//para não ficar muito junto
                    if (j == (mapa.GetLength(1) - 1))
                    {
                        Console.Write("\n");
                    }
                }

            }

            desenha(pacX, pacY, pac, ConsoleColor.Yellow);

            if (pacEnergizado)
            {
                ConsoleColor cor = ConsoleColor.White;

                desenha(gBlinkyX, gBlinkyY, ghost, cor);
                desenha(gPinkyX, gPinkyY, ghost, cor);
                desenha(gInkyX, gInkyY, ghost, cor);
                desenha(gClydeX, gClydeY, ghost, cor);
            }
            else
            {
                desenha(gBlinkyX, gBlinkyY, ghost, ConsoleColor.DarkRed);
                desenha(gPinkyX, gPinkyY, ghost, ConsoleColor.Magenta);
                desenha(gInkyX, gInkyY, ghost, ConsoleColor.Blue);
                desenha(gClydeX, gClydeY, ghost, ConsoleColor.Red);
            }

            Console.SetCursorPosition((mapa.GetLength(1) * 2) - 1, mapa.GetLength(0) + 2);
        }

        public static void carregaElemento(ElementoTipo tipo)
        {
            var workingDir = Environment.CurrentDirectory;
            var caminho = Directory.GetParent(workingDir).Parent.Parent.FullName;
            string arquivo = string.Empty;

            switch (tipo)
            {
                case ElementoTipo.PAREDE:
                    arquivo = Path.Combine(caminho, @"Data/paredes.txt");
                    break;
                case ElementoTipo.GHOSTZONE:
                    arquivo = Path.Combine(caminho, @"Data/espacos.txt");
                    break;
                case ElementoTipo.PILULA:
                    arquivo = Path.Combine(caminho, @"Data/pilulas.txt");
                    break;
            }

            //verifica se existe o arquivo
            if (File.Exists(arquivo))
            {
                //quebra o texto em linhas
                string[] linhas = File.ReadAllLines(arquivo);

                int i;
                int j;

                //percorre as linhas
                foreach (var linha in linhas)
                {
                    if (linha.Trim() != "")
                    {
                        //dividi a linhas em posições da matriz
                        string[] valores = linha.Split(",");
                        i = Convert.ToInt16(valores[0]);
                        j = Convert.ToInt16(valores[1]);

                        if (tipo != ElementoTipo.PILULA)
                        {
                            //remover o dot que foi substituido
                            dots--;
                        }
                        mapa[i, j] = (char)tipo;
                    }
                }
            }
        }

        public static void moverJogador(ConsoleKeyInfo keyInfo)
        {

            //verificar se pertou seta para cima ou seta para baixo
            if (keyInfo.Key == ConsoleKey.UpArrow)
            {
                //diminui o valor da posição do jogador
                //se o jogador não chegou o topo da janela
                if (pacY > 0)
                {
                    if (verificaPosicao(pacY - 1, pacX))
                        pacY--;
                }
                else
                {
                    pacY = mapa.GetLength(0) - 1;
                }
                PacSpritesAtual = PacSprites.CIMA;
            }
            else if (keyInfo.Key == ConsoleKey.DownArrow)
            {
                //aumenta o valor da posição do jogador
                //se o jogador não chegou na base da janela
                if (pacY < (mapa.GetLength(0) - 1))
                {
                    //verificar colisão com obstáculo
                    if (verificaPosicao(pacY + 1, pacX))
                        pacY++;
                }
                else
                {
                    //aparece no topo
                    pacY = 0;
                }
                PacSpritesAtual = PacSprites.BAIXO;
            }
            else if (keyInfo.Key == ConsoleKey.RightArrow)
            {
                if (pacX < (mapa.GetLength(1) - 1))
                {
                    if (verificaPosicao(pacY, pacX + 1))
                        pacX += 1;
                }
                else
                {
                    //atravessa para o lado esquerdo da tela
                    pacX = 0;
                }
                PacSpritesAtual = PacSprites.DIREITA;
            }
            else if (keyInfo.Key == ConsoleKey.LeftArrow)
            {
                if (pacX > 0)
                {
                    if (verificaPosicao(pacY, pacX - 1))
                        pacX -= 1;
                }
                else
                {
                    pacX = mapa.GetLength(1) - 1;
                }
                PacSpritesAtual = PacSprites.ESQUERDA;
            }

            comecome();
        }

        static void comecome()
        {
            //verificar dot (ponto)
            if (mapa[pacY, pacX] == (char)ElementoTipo.DOT)
            {
                pontos += 10;
                dots--;
                mapa[pacY, pacX] = ' ';

                //tocar som (no MacOS não funciona)
                //Console.Beep(116, 100);
                //Console.Beep(163, 80);
            }

            //verifica pílula
            if (mapa[pacY, pacX] == (char)ElementoTipo.PILULA)
            {
                pontos += 30;
                dots--;
                mapa[pacY, pacX] = ' ';
                pacEnergizado = true;
            }

            //verificar se o jogador ganhou
            if (dots == 0)
            {
                imprimeMensagem("Parabéns! Você ganhou.");
                Console.ReadKey();
                inicializa();
            }
        }

        static bool verificaPosicao(int linha, int coluna)
        {
            if (mapa[linha, coluna] == (char)ElementoTipo.PAREDE)
                return false;
            else
                return true;
        }

        static void desenha(int coluna, int linha, char caracter, ConsoleColor cor)
        {
            //considerar os espaços em left e o topo (legenda)
            //a impressão é em relação a tela
            Console.SetCursorPosition(coluna * 2, linha + 2);
            Console.ForegroundColor = cor;
            Console.Write(caracter);
            Console.ResetColor();
        }

        static void animaPacMan()
        {
            if (pacAnimado)
            {
                pac = '\u041E';
                pacAnimado = false;
            }
            else
            {
                pac = (char)PacSpritesAtual;
                pacAnimado = true;
            }
            if (pacEnergizado)
            {
                if (pacEnergizadoTempo > 0)
                {
                    pacEnergizadoTempo--;
                }
                else
                {
                    pacEnergizadoTempo = tempoEnergizado;
                    pacEnergizado = false;

                    //voltar a vida de todos os fantasmas
                    gBlinkyVivo = true;
                    gPinkyVivo = true;
                    gInkyVivo = true;
                    gClydeVivo = true;
                }  
            }
        }

        /*
         * No pacman original os 4 inimigos, cada um tem um IA (algoritmo)
         * diferente:
         * 1) ia pelo caminho mais próximo até o pacman
         * 2) ia para o lugar pra onde o pacman provavelmente iria
         * para fugir do primeiro fantasma
         * 3) ia para o lugar para onde o pacman provavelmente
         * iria para fugir do primeiro e do segundo fantasma
         * 4) Movimentos aleatórios
         *
         * Optei pelo desenvolvimento de um método para um comportamento
         * aleatório (não está sendo realizado uma busca). A estratégias
         * que utilizei foi a seguinte:
         *  - Escolha de um sentido aleatóriamente;
         *  - Escolha de um tempo de duração para o sentido
         */

        static void ghostIAAleatorio(ref int gX, ref int gY,
            ref int gDurSentido, ref int gSentido, ref bool gVivo)
        {
            if (gVivo)
            {
                //aleatóriamente

                if (gDurSentido > 0)
                {
                    gDurSentido--;
                }
                else
                {
                    gSentido = new Random().Next(1, 5);
                    if (gSentido == 1 || gSentido == 2)
                    {
                        gDurSentido = new Random().Next(1, mapa.GetLength(0));
                    }
                    else
                    {
                        gDurSentido = new Random().Next(1, mapa.GetLength(1));
                    }
                }

                switch (gSentido)
                {
                    case 1: //cima
                        if (verificaPosicao(gY - 1, gX))
                            gY--;
                        else
                            gDurSentido = 0;
                        break;
                    case 2: //baixo
                        if (verificaPosicao(gY + 1, gX))
                            gY++;
                        else
                            gDurSentido = 0;
                        break;
                    case 3: //direita
                        if (verificaPosicao(gY, gX + 1))
                            gX += 1;
                        else
                            gDurSentido = 0;
                        break;
                    case 4: //esquerda
                        if (verificaPosicao(gY, gX - 1))
                            gX -= 1;
                        else
                            gDurSentido = 0;
                        break;
                }
                //Verificar se houve colisão
                verificaColisao(ref gX, ref gY, ref gVivo);
            }
            
        }

        static void verificaColisao(ref int gX, ref int gY, ref bool gVivo)
        {
            if ((pacX == gX) && (pacY == gY))
            {
                if (pacEnergizado)
                {
                    //deveria voltar para posição inicial
                    pontos += 50;
                    gX = 14;
                    gY = 13;
                    gVivo = false;
                }
                else
                {
                    vidas--;
                    PacSpritesAtual = PacSprites.MORREU;

                    Console.Beep();

                    if (vidas == 0)
                    {
                        imprimeMensagem("Game Over!");
                        vidas = 3;
                        inicializa();
                    }
                    Console.ReadKey();
                    posicionaPersonagens();
                }
            }
        }

        static void movimentaGhosts()
        {
            ghostIAAleatorio(ref gBlinkyX, ref gBlinkyY, ref gBlinkyDurSentido,
                ref gBlinkySentido, ref gBlinkyVivo);

            ghostIAAleatorio(ref gPinkyX, ref gPinkyY, ref gPinkyDurSentido,
                ref gPinkySentido, ref gPinkyVivo);
            ghostIAAleatorio(ref gInkyX, ref gInkyY, ref gInkyDurSentido,
                ref gInkySentido, ref gInkyVivo);
            ghostIAAleatorio(ref gClydeX, ref gClydeY, ref gClydeDurSentido,
                ref gClydeSentido, ref gClydeVivo);

        }

        static void imprimeMensagem(string mensagem)
        {
            Console.SetCursorPosition((Console.WindowWidth / 2) - (mensagem.Length / 2), (Console.WindowHeight / 2) - 1);
            Console.WriteLine(mensagem);
        }


    }

    public enum PacSprites
    {
        //direita \u15E7  esquerda \u15E4  cima \u15E2  baixo \u15E3
        CIMA = '\u15E2',
        BAIXO = '\u15E3',
        DIREITA = '\u15E7',
        ESQUERDA = '\u15E4',
        MORREU = '\u2296'
    }

    public enum ElementoTipo
    {
        PAREDE = '#',
        GHOSTZONE = ' ',
        PILULA = '\u25CF',
        DOT = '.'
    }

}
