using System;
using System.Diagnostics;
using System.IO;

namespace NoitaSeedChanger
{
    class Program
    {
        private static string gameName = "noita";
        public static Process game;
        
        private static readonly string listFile = AppDomain.CurrentDomain.BaseDirectory + "seeds.txt";

        public static int release = 0;
        public static uint seed = 0;        // 1 to 4294967295 
        private static bool restart = false;

        
        

        static void Main(string[] args)
        {
            // hooked Restart function to CancelKeyPress event
            Console.CancelKeyPress += new ConsoleCancelEventHandler(RestartApp);

            if (!File.Exists(listFile)) // check if seedlist.txt exists
            {
                string[] lines = { "1234567890:Test Seed" };
                File.Create(listFile).Close();
                File.WriteAllLines(listFile, lines);
            }

            Helper.DrawBanner();

            if (!Seedlist.GetList(listFile)) // get seed from list
            {
                Console.Write(Environment.NewLine);
                Console.Write("Enter Seed> ");
                Console.ForegroundColor = ConsoleColor.White;
                uint.TryParse(Console.ReadLine(), out seed);
                Console.Write(Environment.NewLine);
            }

            if (seed <= 0)  // game stucks on title screen if the seed is less or equal zero
            {
                Helper.Error("Seed invalid! Make sure it's in a range of 1 to 4294967295.");
                seed = Helper.RandomSeed();
                Helper.WriteLine("Generated random seed: " + seed);
                Console.Write(Environment.NewLine);
            }

            Restart:

            if (restart)
            {
                Helper.DrawBanner();
            }

            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("Waiting for noita.exe");

            // checks if noita.exe is running
            bool tryAgain;
            do
            {
                tryAgain = false;


                while (game == null)
                {
                    System.Threading.Thread.Sleep(50);
                    if (Process.GetProcessesByName(gameName).Length > 0)
                    {
                        game = Process.GetProcessesByName(gameName)[0];
                        Console.WriteLine("noita.exe is running");
                        Console.Write(Environment.NewLine);
                    }
                }

                // checks current Noita release and sets 'release' variable
                Release.Set(game);

                //do this in try, because at early game init, the WaitForInputIdle
                //  if the window isn't open yet
                try
                {
                    // writes seed to given memory address for the correct version
                    if (game.WaitForInputIdle())
                    {
                        ChangeSeed(Release.Targets);
                    }
                    Helper.WriteLine("Seed changed to: " + seed);
                    Console.WriteLine("Idle until Noita restarts.");
                }
                catch (Exception e)
                {
                    game = null;
                    tryAgain = true;
                }

                if (game != null)
                    game.WaitForExit();
                game = null;

            } while (tryAgain);
            

            restart = true;
            goto Restart;
        }

        private static void ChangeSeed(params int[] pointer)
        {
            if (pointer.Length > 0)
            {

                while (!CheckSeed(pointer)) //try to update the memory until it's right.
                {
                    if (CheckRead(pointer))
                    {
                        for (int i = 0; i < pointer.Length; i++)
                        {
                            Memory.Write(game.Handle, pointer[i], seed);
                        }

                    }
                }
            }
        }

        private static bool CheckSeed(params int[] pointers)
        {
            foreach(int pointer in pointers)
            {
                if (Memory.Read(game.Handle, pointer) != seed)
                    return false;
            }
            return true;
        }

        private static bool CheckRead(params int[] pointers)
        {
            foreach (int pointer in pointers)
            {
                if (Memory.Read(game.Handle, pointer) > 0)
                    return true;
            }
            return false;
        }

        public static void RestartApp(object sender, EventArgs e)
        {
            Process.Start(AppDomain.CurrentDomain.BaseDirectory + "NoitaSeedChanger.exe");
            Process.GetCurrentProcess().Kill();
        }
    }
}
