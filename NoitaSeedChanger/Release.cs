using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Collections.Generic;

namespace NoitaSeedChanger
{
    class Release
    {
        private static readonly string hashFile = "/_version_hash.txt";

        private static string gameHash = "";
        private static Dictionary<string, int[]> targets = null;
        private const string final = "b71aff760755c178c9f09ebd1e61ddc5272ceb4b";
        //TODO: FIXME
        private const string beta = "cac8fef90391e9409e8be422ec8322bb0b2cde2e";
        private const string old = "c0ba23bc0c325a0dc06604f114ee8217112a23af";

        //TODO: these belong in an XML file, like <Release hash="xxx"><target addr="0x12345678" /></Release> so that they are user configurable.

        private static readonly int[] p_Final = new int[] {
            0x145059C,0x14DE1E0             // final
        };
        //TODO: dunno the beta targets yet
        private static readonly int[] p_Beta = new int[] {
            0x1427B74, 0x1434BC4, 0x14C0178, 0x1432458  // beta branch
        };
        private static readonly int[] p_Old = new int[] {
            0x14136D4, 0x1420798, 0x14ABCF0             // old version
        };
        private static readonly int[] empty = new int[] { };

        static Release()
        {
            if (targets == null)
                LoadTargets();
        }

        public static int[] Targets
        {
            get
            {
                int[] target = targets[gameHash];
                if (target == null)
                    return empty;
                return target;
            }
        }
        //TODO: load from XML
        private static void LoadTargets()
        {
            targets = new Dictionary<string, int[]>();
            targets.Add(final, p_Final);
            targets.Add(beta, p_Beta);
            targets.Add(old, p_Old);
        }

        public static void Set(Process game)
        {
            try
            {
                string[] lines = File.ReadAllLines(GetHashFile(game), Encoding.UTF8);
                gameHash = lines[0];

                if (targets[gameHash].Length == 0)
                {
                    Helper.Error("Current game version not supported!");
                    Helper.Error("Closing NSC in 10 seconds.");
                    Thread.Sleep(10000);
                    Process.GetCurrentProcess().Kill();
                }
            }
            catch (Exception e)
            {
                Helper.Error(e.Message);
                Helper.Error("Closing NSC in 10 seconds.");
                Thread.Sleep(10000);
                throw;
            }

        }
        private static string GetHashFile(Process game)
        {
            try
            {
                return Path.GetDirectoryName(game.MainModule.FileName) + hashFile;
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
