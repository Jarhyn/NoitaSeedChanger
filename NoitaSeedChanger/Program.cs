﻿using System;
using System.Diagnostics;
using System.IO;

namespace NoitaSeedChanger
{
    class Program
    {
        private static string gameName = "noita";
        public static Process game = null;
        private static readonly string listFile = AppDomain.CurrentDomain.BaseDirectory + "seeds.txt";

        public static int release = 0;
        public static uint seed = 0;        // 1 to 4294967295 
        private static bool restart = false;

        private static readonly int[] pointer = new int[] { 
            0x14136D4, 0x1420798, 0x14ABCF0, // release branch
            0x142F24C, 0x14BCEA8, 0x1431900, // beta branch
            0x177712C, 0x1801640, 0x1777AC8, // old version
        };

        static void Main(string[] args)
        {
            // hooked Restart function to CancelKeyPress event
            Console.CancelKeyPress += new ConsoleCancelEventHandler(RestartApp);

            if (!File.Exists(listFile)) // check if seedlist.txt exists
            {
                string[] lines = { "12345678:Test Seed" };
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
            Release.Set();

            // writes seed to given memory address for the correct version
            if (game.WaitForInputIdle())
            {
                switch (release)
                {
                    case 0: // release
                        ChangeSeed(pointer[0], pointer[1], pointer[2]);
                        break;
                    case 1: // beta
                        ChangeSeed(pointer[3], pointer[4], pointer[5]);
                        break;
                    case 2: // old
                        ChangeSeed(pointer[6], pointer[7], pointer[8]);
                        break;
                    default:
                        break;
                }
            }
            Helper.WriteLine("Seed changed to: " + seed);
            Console.WriteLine("Idle until Noita restarts.");

            game.WaitForExit();
            game = null;

            restart = true;
            goto Restart;
        }

        private static void ChangeSeed(int p1, int p2, int p3)
        {
            while (Memory.Read(game.Handle, p1) != seed && Memory.Read(game.Handle, p2) != seed)
            {
                if (Memory.Read(game.Handle, p1) > 0 || Memory.Read(game.Handle, p2) > 0)
                {
                    Memory.Write(game.Handle, p1, seed);
                    Memory.Write(game.Handle, p2, seed);
                    Memory.Write(game.Handle, p3, seed);
                }
            }
        }

        public static void RestartApp(object sender, EventArgs e)
        {
            Process.Start(AppDomain.CurrentDomain.BaseDirectory + "NoitaSeedChanger.exe");
            Process.GetCurrentProcess().Kill();
        }
    }
}
