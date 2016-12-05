// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Sean McElroy">
//   Copyright Sean McElroy 2016.  Released under the terms of the MIT License
// </copyright>
// <summary>
//   Entry point for console test harness
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Bitforum.Console
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using Core;

    using log4net.Config;

    using Server;

    /// <summary>
    /// Entry point for console test harness
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Entry point for console test harness
        /// </summary>
        public static void Main()
        {
            // Setup LOG4NET
            XmlConfigurator.Configure();

            // Get password to use as an identity vault XOR key
            string secret = null;
            while (secret == null)
            {
                Console.Write("Enter password to unlock identity store: ");
                secret = GetMaskedPassword();
            }

            var spinner = new ConsoleSpinner();
            byte[] secretDerivedKey;
            var calculatePasswordHashTask = Task.Run(() => { secretDerivedKey = HashUtility.CalculatePasswordHash(secret); });
            do
            {
                spinner.Turn();
                Thread.Sleep(60);
            }
            while (!calculatePasswordHashTask.IsCompleted);
            
            var server1 = new Server(Block.GetBlockDirectory("1"));
            var startResult1 = server1.Start();
            var genesisFoundLocal = startResult1.GenesisBlockFound;

            var newIdentity = server1.CreateIdentity();

            if (!genesisFoundLocal)
            {
                server1.GenerateGenesisBlock();
            }

            // Mine a reply.
            Console.ReadLine();
        }

        private static string GetMaskedPassword()
        {
            var pass = string.Empty;
            ConsoleKeyInfo key;

            // Stops Receving Keys Once Enter is Pressed
            do
            {
                key = Console.ReadKey(true);

                // Backspace Should Not Work
                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    pass += key.KeyChar;
                    Console.Write("*");
                }
                else
                {
                    if (key.Key == ConsoleKey.Backspace && pass.Length > 0)
                    {
                        pass = pass.Substring(0, pass.Length - 1);
                        Console.Write("\b \b");
                    }
                }
            }
            while (key.Key != ConsoleKey.Enter);

            Console.WriteLine();
            return pass;
        }
    }
}