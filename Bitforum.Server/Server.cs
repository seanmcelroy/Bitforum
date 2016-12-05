// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Server.cs" company="Sean McElroy">
//   Copyright Sean McElroy 2016.  Released under the terms of the MIT License
// </copyright>
// <summary>
//   The server instance class that handles logic around a running a block chain node
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Bitforum.Server
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Threading.Tasks;

    using Core;

    using JetBrains.Annotations;

    using log4net;

    using Newtonsoft.Json;

    /// <summary>
    /// The server instance class that handles logic around a running a block chain node
    /// </summary>
    public class Server
    {
        /// <summary>
        /// The logging utility instance to use to log events from this class
        /// </summary>
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Server));

        /// <summary>
        /// The working directory for the server instance
        /// </summary>
        [NotNull]
        private readonly string _directory;

        /// <summary>
        /// Initializes a new instance of the <see cref="Server"/> class.
        /// </summary>
        /// <param name="directory">The working directory for the server instance</param>
        public Server([NotNull] string directory)
        {
            this._directory = directory;
        }

        /// <summary>
        /// Starts the server instance
        /// </summary>
        /// <returns>The result of the start up attempt</returns>
        [NotNull]
        public ServerStartResult Start()
        {
            Logger.Info("Starting server");

            var genesisFound = this.LoadIndex();

            return new ServerStartResult
                       {
                           GenesisBlockFound = genesisFound
                       };
        }

        [Pure, CanBeNull]
        public Identity CreateIdentity()
        {
            var protoIdentity = Identity.Generate();

            var tailBlockHash = Block.GetLatestBlockHash(this._directory);
            if (tailBlockHash == null)
            {
                Logger.Warn("Unable to locate tail block hash for identity mining.");
                return null;    
            }

            protoIdentity.MineIdentity(tailBlockHash, 6);

            return protoIdentity;
        }

        public void GenerateGenesisBlock()
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

            genesisBlock.SaveToFile(this._directory);
            exodusBlock.SaveToFile(this._directory);

            // Create index
            this.LoadIndex();
        }

        /// <summary>
        /// Loads the block chain index from the server, or regenerates it if it could not be located
        /// </summary>
        /// <returns>A value indicating whether the genesis block could be located</returns>
        private bool LoadIndex()
        {
            Logger.Info("Loading block index... ");
            var genesisFoundLocal = false;

            var indexPath = Path.Combine(this._directory, "index.json");
            if (!File.Exists(indexPath))
            {
                Logger.Warn($"No index found at {indexPath}!");
                if (!Directory.Exists(this._directory))
                {
                    // TODO: Get block chain; bootstrap local storage
                }
                else
                {
                    Logger.Warn($"Rebuilding index from blocks located in: {this._directory}...");
                    var index = new BlockIndex();
                    Parallel.ForEach(
                        Directory.GetFiles(this._directory, "*.block"),
                        blockFile =>
                        {
                            Logger.Debug($"Reading block header from {blockFile}");
                            try
                            {
                                Debug.Assert(blockFile != null, "blockFile != null");
                                var header = BlockHeader.ReadFromFile(blockFile);
                                index.Add(new BlockIndexEntry(blockFile, header));
                            }
                            catch (Exception ex)
                            {
                                Logger.Error($"Problem reading block header from {blockFile}: {ex.Message}", ex);
                                throw;
                            }
                        });

                    index.Sort(new BlockIndexEntryComparer());

                    // Walk the list to ensure no holes.
                    Logger.Info("Verifying the integrity of the block chain...");
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
                        Logger.Info("...Verification of the block chain complete.");
                        genesisFoundLocal = genesisFound;
                    }

                    Console.Write("\tSaving... ");
                    index.SaveToFile(this._directory);
                    Console.WriteLine("DONE");
                }
            }
            else
            {
                Logger.Info($"Reading index found at {indexPath}...");

                // TODO: Read index
                BlockIndex index;
                using (var sr = new StreamReader(indexPath, System.Text.Encoding.UTF8))
                {
                    var indexString = sr.ReadToEnd();
                    index = JsonConvert.DeserializeObject<BlockIndex>(indexString);
                    sr.Close();
                }

                if (index != null)
                {
                    Logger.Info("...Read of index file complete.");
                    index.Sort(new BlockIndexEntryComparer());

                    // Walk the list to ensure no holes.
                    Logger.Info("Verifying the integrity of the block chain...");
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
                        Logger.Info("...Verification of the block chain complete.");
                        genesisFoundLocal = genesisFound;
                    }
                }
                else
                {
                    // TODO: Could not open index
                    Console.WriteLine("FAILED");
                }
            }

            return genesisFoundLocal;
        }
    }
}
