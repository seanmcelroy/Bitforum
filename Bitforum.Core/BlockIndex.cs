// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BlockIndex.cs" company="Sean McElroy">
//   Copyright Sean McElroy 2016.  Released under the terms of the MIT License
// </copyright>
// <summary>
//   A index of blocks from the block chain stored in local storage
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Bitforum.Core
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using JetBrains.Annotations;

    using Newtonsoft.Json;

    /// <summary>
    /// A index of blocks from the block chain stored in local storage
    /// </summary>
    public class BlockIndex : List<BlockIndexEntry>
    {
        /// <summary>
        /// Saves the index to local storage
        /// </summary>
        /// <param name="directory">The directory path to which to save the block file</param>
        /// <param name="filename">The filename of the block file</param>
        public void SaveToFile(string directory = null, [NotNull] string filename = "index.json")
        {
            var dir = directory ?? Block.GetBlockDirectory();

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            using (var fs = new FileStream(
                Path.Combine(dir, filename),
                FileMode.Create,
                FileAccess.Write,
                FileShare.None))
            using (var sw = new StreamWriter(fs))
            {
                sw.Write(JsonConvert.SerializeObject(this));
                sw.Close();
            }
        }

        /// <summary>
        /// Validates a block index
        /// </summary>
        /// <param name="genesisFound">A value indicating whether the genesis block was located in the index</param>
        /// <param name="continuous">A value indicated whether every block was found to be continuously linked</param>
        /// <returns>A value indicating whether the block chain was successfully verified</returns>
        public bool Verify(out bool genesisFound, out bool continuous)
        {
            genesisFound = false;
            continuous = true;

            string lastBlockHash = null;
            foreach (var entry in this)
            {
                if (lastBlockHash == null)
                {
                    // First node
                    if (string.Equals(entry.PreviousBlockHash, "00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", StringComparison.Ordinal))
                    {
                        // And we have the very start of the block chain, great.
                        genesisFound = true;
                    }

                    lastBlockHash = entry.Hash;
                }
                else
                {
                    if (entry.PreviousBlockHash != lastBlockHash)
                    {
                        Console.WriteLine($"BREAK IN CHAIN AT {lastBlockHash}");
                        continuous = false;
                    }

                    lastBlockHash = entry.PreviousBlockHash;
                }

                // TODO: Verify block
            }

            return genesisFound && continuous;
        }
    }
}
