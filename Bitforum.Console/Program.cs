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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;

    using Core;

    using Newtonsoft.Json;

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
            var genesisFoundLocal = false;

            Console.Write("Loading block index... ");
            if (!File.Exists(Path.Combine(Block.GetBlockDirectory(), "index.json")))
            {
                Console.Write("MISSING... ");
                if (!Directory.Exists(Block.GetBlockDirectory()))
                {
                    // TODO: Get block chain; bootstrap local storage
                }
                else
                {
                    Console.Write("\r\n\tRebuilding... ");
                    var index = new BlockIndex();
                    var spinner = new ConsoleSpiner();
                    foreach (var blockFile in Directory.GetFiles(Block.GetBlockDirectory()))
                    {
                        Debug.Assert(blockFile != null, "blockFile != null");
                        var header = BlockHeader.ReadFromFile(blockFile);
                        index.Add(new BlockIndexEntry(blockFile, header));
                        spinner.Turn();
                    }

                    index.Sort(new BlockIndexEntryComparer());

                    // Walk the list to ensure no holes.
                    Console.Write("\r\n\tVerifying... ");
                    bool genesisFound, continuous;
                    if (!index.Verify(out genesisFound, out continuous))
                    {
                        if (!genesisFound)
                        {
                            Console.WriteLine("GENESIS BLOCK NOT FOUND");
                        }

                        if (!continuous)
                        {
                            Console.WriteLine("BREAK IN CHAIN");
                        }

                        if (genesisFound && continuous)
                        {
                            Console.WriteLine();
                        }
                    }
                    else
                    {
                        Console.WriteLine("DONE");
                        genesisFoundLocal = genesisFound;
                    }

                    Console.Write("\tSaving... ");
                    index.SaveToFile();
                    Console.WriteLine("DONE");
                }
            }
            else
            {
                Console.Write("\r\n\tReading... ");

                // TODO: Read index
                BlockIndex index;
                using (var sr = new StreamReader(Path.Combine(Block.GetBlockDirectory(), "index.json"), System.Text.Encoding.UTF8))
                {
                    var indexString = sr.ReadToEnd();
                    index = JsonConvert.DeserializeObject<BlockIndex>(indexString);
                    sr.Close();
                }

                if (index != null)
                {
                    Console.WriteLine("DONE");
                    index.Sort(new BlockIndexEntryComparer());

                    // Walk the list to ensure no holes.
                    Console.Write("\tVerifying... ");
                    bool genesisFound, continuous;
                    if (!index.Verify(out genesisFound, out continuous))
                    {
                        if (!genesisFound)
                        {
                            Console.WriteLine("GENESIS BLOCK NOT FOUND");
                        }

                        if (!continuous)
                        {
                            Console.WriteLine("BREAK IN CHAIN");
                        }
                    }
                    else
                    {
                        Console.WriteLine("DONE");
                        genesisFoundLocal = genesisFound;
                    }
                }
                else
                {
                    // TODO: Could not open index
                    Console.WriteLine("FAILED");
                }
            }

            if (!genesisFoundLocal)
            {
                Console.WriteLine("Generating genesis block...");
                var genesisBlock = new Block
                                       {
                                           Posts = new[] 
                                           {
                                                new Post
                                                {
                                                    Header = new PostHeader
                                                                {
                                                                    Forum = "alt.privacy",
                                                                    MessageId = Guid.NewGuid().ToString("N"),
                                                                    ConversationId = Guid.NewGuid().ToString("N"),
                                                                    From = "Sean McElroy <me@seanmcelroy.com;0>",
                                                                    DateEpoch = Convert.ToUInt32((new DateTime(1981, 01, 26, 0, 0, 0, DateTimeKind.Local) - new DateTime(1970, 01, 01, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds),
                                                                    Subject = "First post",
                                                                    BodyContentType = "text/plain"
                                                                },
                                                    Body = System.Text.Encoding.UTF8.GetBytes("Ex nihilo nihil fit")
                                                }
                                           }
                                       };

                Console.WriteLine("Generating genesis block hash...");
                genesisBlock.GenerateGenesisHash();
                Debug.Assert(genesisBlock.Header != null, "genesisBlock.Header != null");
                Console.WriteLine($"Genesis hash: {BitConverter.ToString(genesisBlock.Header.GetHash()).Replace("-", "")}");

                Console.WriteLine("Generating exodus block...");
                Debug.Assert(genesisBlock.Posts != null, "genesisBlock.Posts != null");
                Debug.Assert(genesisBlock.Posts.Length == 1, "genesisBlock.Posts.Length == 1");
                Debug.Assert(genesisBlock.Posts[0] != null, "genesisBlock.Posts[0] != null");
                var exodusBlock = Block.MineBlock(
                    genesisBlock, 
                    new List<Post>
                    {
                        new Post
                            {
                                Header = new PostHeader
                                                {
                                                    Forum = "alt.privacy",
                                                    MessageId = Guid.NewGuid().ToString("N"),
                                                    ConversationId = genesisBlock.Posts[0].Header?.ConversationId,
                                                    From = "Sean McElroy <me@seanmcelroy.com;0>",
                                                    DateEpoch = Convert.ToUInt32((DateTime.UtcNow - new DateTime(1970, 01, 01, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds),
                                                    Subject = "RE: First post",
                                                    BodyContentType = "text/plain"
                                                },
                                Body = System.Text.Encoding.UTF8.GetBytes("We can easily forgive a child who is afraid of the dark; the real tragedy of life is when men are afraid of the light.")
                            }
                    });

                Debug.Assert(exodusBlock.Header != null, "exodusBlock.Header != null");
                Console.WriteLine($"Exodus hash: {BitConverter.ToString(exodusBlock.Header.GetHash()).Replace("-", "")}");

                Console.Write("Verifying exodus hash... ");
                Console.WriteLine($"{exodusBlock.Verify().ToString().ToUpperInvariant()}");

                genesisBlock.SaveToFile();
                exodusBlock.SaveToFile();
            }

            // Mine a reply.
            Console.ReadLine();
        }
    }
}