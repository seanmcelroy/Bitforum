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
            Console.WriteLine("Generating genesis block...");
            var genesisBlock = new Block
                                   {
                                       BlockNumber = 0,
                                       Posts = new[] 
                                       {
                                           new Post
                                           {
                                                Forum = "alt.privacy",
                                                MessageId = Guid.NewGuid().ToString("N"),
                                                ConversationId = Guid.NewGuid().ToString("N"),
                                                From = "Sean McElroy <me@seanmcelroy.com;0>",
                                                DateEpoch = Convert.ToInt64((new DateTime(1981, 01, 26, 0, 0, 0, DateTimeKind.Local) - new DateTime(1970, 01, 01, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds),
                                                Subject = "First post",
                                                Body = "Ex nihilo nihil fit"
                                           }
                                       }
                                   };

            Console.WriteLine("Generating genesis block hash...");
            genesisBlock.GenerateGenesisHash();
            Debug.Assert(genesisBlock.BlockHash != null, "genesisBlock.BlockHash != null");
            Console.WriteLine($"Genesis hash: {BitConverter.ToString(genesisBlock.BlockHash).Replace("-", "")}");

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
                            Forum = "alt.privacy",
                            MessageId = Guid.NewGuid().ToString("N"),
                            ConversationId = genesisBlock.Posts[0].ConversationId,
                            From = "Sean McElroy <me@seanmcelroy.com;0>",
                            DateEpoch = Convert.ToInt64((DateTime.UtcNow - new DateTime(1970, 01, 01, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds),
                            Subject = "First post",
                            Body = "Ex nihilo nihil fit"
                        }
                });

            Debug.Assert(exodusBlock.BlockHash != null, "exodusBlock.BlockHash != null");
            Console.WriteLine($"Exodus hash: {BitConverter.ToString(exodusBlock.BlockHash).Replace("-", "")}");

            // Mine a reply.
            Console.ReadLine();
        }
    }
}