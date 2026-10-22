using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Newtonsoft.Json;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace Network_Pro_Ev_tapsirigi3
{
    public class GameZone
    {
        static public IPAddress? IP { get; set; }
        public int local_port { get; set; }
        public int remote_port { get; set; }
        public string? GamerNameA { get; set; }
        public string? GamerNameB { get; set; }
        public string? GameWinnerName { get; set; }

        public string[,] game = new string[3, 3];


        public GameZone()
        {
            GameArenaStart();
        }

        public void ShowGame()
        {
            Console.WriteLine("\n=======================");
            Console.WriteLine("    B1    B2   B3\n    ----------------");
            for (int i = 0; i < game.GetLength(0); i++)
            {
                Console.Write("A" + (i + 1) + "|");
                for (int k = 0; k < game.GetLength(1); k++)
                {
                    Console.Write($" {game[i, k]} ");
                    Console.Write(" | ");
                }
                Console.WriteLine("\n    ________________\n");
            }
            Console.WriteLine("=======================");
        }

        public void GameArenaStart()
        {
            for (int i = 0; i < 3; i++)
                for (int k = 0; k < 3; k++)
                    game[i, k] = " ";
        }

        public bool MoveGame(string A, string B, string X_O)
        {
            A = "A" + A;
            B = "B" + B;
            string[,] MoveKoord = new string[game.GetLength(0), game.GetLength(1)];
            for (int i = 0; i < MoveKoord.GetLength(0); i++)
            {
                for (int k = 0; k < MoveKoord.GetLength(1); k++)
                {
                    MoveKoord[i, k] = $"A{(i + 1)}B{(k + 1)}";
                }
            }
            for (int i = 0; i < MoveKoord.GetLength(0); i++)
            {
                for (int k = 0; k < MoveKoord.GetLength(1); k++)
                {
                    if (MoveKoord[i, k] == (A + B) && game[i, k] == " ")
                    {
                        game[i, k] = X_O;
                        Console.WriteLine("Kordinat ugurla isarelendi...");
                        return false;
                    }
                    else if (MoveKoord[i, k] == (A + B) && (game[i, k] == "X" || game[i, k] == "O"))
                    {
                        Console.WriteLine("Bu koordinat artiq daxil edilib...");
                        return true;
                    }
                }
            }
            Console.WriteLine("Duzgun koordinat daxil edin(mes: A kord: 1 , B kord: 1)...");
            return true;

        }

        public bool CheckGame()
        {
            string[] symbols = { "X", "O" };
            foreach (var item in symbols)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (game[i, 0] == item && game[i, 1] == item && game[i, 2] == item) return true;
                    if (game[0, i] == item && game[1, i] == item && game[2, i] == item) return true;
                }

                if (game[0, 0] == item && game[1, 1] == item && game[2, 2] == item) return true;
                if (game[0, 2] == item && game[1, 1] == item && game[2, 0] == item) return true;
            }
            return false;
        }
    }



    internal class Program
    {
        public static void JsonSerialize(GameZone gameZone)
        {
            var json = new JsonSerializer();

            using (var sw = new StreamWriter("GameZone.json"))
            {
                using (var jw = new JsonTextWriter(sw))
                {
                    json.Formatting = Newtonsoft.Json.Formatting.Indented;
                    json.TypeNameHandling = TypeNameHandling.All;
                    json.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                    json.Serialize(jw, gameZone);
                }
            }



        }

        public static GameZone JsonDeserialize()
        {
            if (!File.Exists("GameZone.json"))
            {
                return new GameZone();
            }
            var json = new JsonSerializer();
            GameZone? gameZone = null;

            using (var sw = new StreamReader("GameZone.json"))
            {

                using (var jw = new JsonTextReader(sw))
                {

                    json.TypeNameHandling = TypeNameHandling.All;
                    json.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                    gameZone = json.Deserialize<GameZone>(jw);

                }
            }
            if (gameZone == null) { return new GameZone(); }
            return gameZone;

        }

        private static UdpClient? udpClient;
        private static IPEndPoint? remoteEndPoint;

        static void Main(string[] args)
        {
            Console.WriteLine("Tic-Tac-Toe oyununa xos gelmisiniz...\n");
            GameZone gameZone = JsonDeserialize();

            Console.Write("Oyuncu adinizi daxil edin: ");
            gameZone.GamerNameA = Console.ReadLine();

            GameZone.IP = IPAddress.Parse("192.168.1.82");
            Console.Write("Öz portunuzu daxil edin (mes: 27001): ");
            gameZone.local_port = int.Parse(Console.ReadLine()!);

            Console.Write("Diger oyuncunun portunu daxil edin (mes: 27002): ");
            gameZone.remote_port = int.Parse(Console.ReadLine()!);


            udpClient = new UdpClient(gameZone.local_port);
            remoteEndPoint = new IPEndPoint(GameZone.IP, gameZone.remote_port);


            Console.WriteLine("\nSimvolunuzu secin:");
            Console.WriteLine("1) X (Birinci siz baslayirsiniz)");
            Console.WriteLine("2) O (Ikinci siz baslayirsiniz, qarsi terefi gozleyirsiniz)");
            Console.Write("Seciminiz (1 ve ya 2): ");
            var choice = Console.ReadLine()!;

            var isNext = (choice == "1");
            var gamerSymbol = isNext ? "X" : "O";
            var gamer2Symbol = isNext ? "O" : "X";


            if (isNext)
            {
                SendData(gameZone.GamerNameA!);
                Console.WriteLine("Adiniz gonderildi. Qarsi terefin qosulmasi gozlenilir...");
                gameZone.GamerNameB = ReceiveData();
            }
            else
            {
                Console.WriteLine("Qarsi terefin qosulmasi gozlenilir...");
                gameZone.GamerNameB = ReceiveData();
                SendData(gameZone.GamerNameA!);
            }

            Console.Clear();
            Console.WriteLine($"=== OYUN BASLADI ===");
            Console.WriteLine($"Siz: {gameZone.GamerNameA} ({gamerSymbol}) vs {gameZone.GamerNameB} ({gamer2Symbol})\n");
            gameZone.ShowGame();


            int moves = 0;
            while (moves < 9)
            {
                if (isNext)
                {
                    Console.WriteLine($"\n--- Sizin novbeniz ({gamerSymbol}) ---");
                    var IsTrueMove = false;
                    var A_Coord = "";
                    var B_Coord = "";

                    while (!IsTrueMove)
                    {
                        Console.Write("A koordinat reqemini daxil edin (1-3): ");
                        A_Coord = Console.ReadLine()!;
                        Console.Write("B koordinat reqemini daxil edin (1-3): ");
                        B_Coord = Console.ReadLine()!;

                        var Error = gameZone.MoveGame(A_Coord, B_Coord, gamerSymbol);
                        if (!Error) IsTrueMove = true;
                    }

                    SendData(A_Coord);
                    SendData(B_Coord);

                    gameZone.ShowGame();

                    if (gameZone.CheckGame())
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        gameZone.GameWinnerName = gameZone.GamerNameA;
                        Console.WriteLine($"\n{gameZone.GamerNameA} oyunu qazandin...");
                        Console.ResetColor();
                        break;
                    }

                    isNext = false;
                }
                else
                {
                    Console.WriteLine($"\n--- {gameZone.GamerNameB}-nin novbesi ({gamer2Symbol}) gozlenilir... ---");


                    var A_Coord2 = ReceiveData();
                    var B_Coord2 = ReceiveData();

                    gameZone.MoveGame(A_Coord2, B_Coord2, gamer2Symbol);
                    gameZone.ShowGame();

                    if (gameZone.CheckGame())
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        gameZone.GameWinnerName = gameZone.GamerNameB;
                        Console.WriteLine($"\n{gameZone.GamerNameA} oyunu uduzdun...");
                        Console.ResetColor();
                        break;
                    }

                    isNext = true;
                }

                moves++;
            }

            if (moves >= 9)
            {
                Console.WriteLine("\nOyun berabere bitdi...");
            }
            JsonSerialize(gameZone);
            Console.WriteLine("\nCixmaq ucun her hansisa duymeye basin...");
            Console.ReadKey();
        }

        private static void SendData(string data)
        {
            try
            {
                var bytes = Encoding.UTF8.GetBytes(data);
                udpClient!.Send(bytes, bytes.Length, remoteEndPoint!);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static string ReceiveData()
        {
            try
            {
                IPEndPoint ep = new IPEndPoint(IPAddress.Any, 0);
                var bytes = udpClient!.Receive(ref ep);
                return Encoding.UTF8.GetString(bytes);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return "";
            }
        }
    }
}