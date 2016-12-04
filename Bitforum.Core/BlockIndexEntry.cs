// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BlockIndexEntry.cs" company="Sean McElroy">
//   Copyright Sean McElroy 2016.  Released under the terms of the MIT License
// </copyright>
// <summary>
//   A block index entry is a node in a <see cref="BlockIndex" /> that allows for fast searching of blocks
//   in the block chain
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Bitforum.Core
{
    using System;

    using JetBrains.Annotations;

    /// <summary>
    /// A block index entry is a node in a <see cref="BlockIndex"/> that allows for fast searching of blocks
    /// in the block chain
    /// </summary>
    [PublicAPI]
    public sealed class BlockIndexEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BlockIndexEntry"/> class.
        /// </summary>
        // ReSharper disable once NotNullMemberIsNotInitialized
        public BlockIndexEntry()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlockIndexEntry"/> class.
        /// </summary>
        /// <param name="path">
        /// The path to the block file
        /// </param>
        /// <param name="blockHeader">
        /// The block header from which to populate this index entry
        /// </param>
        public BlockIndexEntry([NotNull] string path, [NotNull] BlockHeader blockHeader)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (blockHeader == null)
            {
                throw new ArgumentNullException(nameof(blockHeader));
            }

            if (blockHeader.PreviousBlockHeaderHash == null)
            {
                throw new ArgumentException($"PreviousBlockHeaderHash is null in block at: {path}", nameof(blockHeader));
            }

            this.Path = System.IO.Path.GetFileName(path);
            this.Hash = BitConverter.ToString(blockHeader.GetHash()).Replace("-", string.Empty);
            this.PreviousBlockHash = BitConverter.ToString(blockHeader.PreviousBlockHeaderHash).Replace("-", string.Empty);
        }

        /// <summary>
        /// Gets or sets the path to the block file on disk
        /// </summary>
        [NotNull]
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the hash of the block
        /// </summary>
        [NotNull]
        public string Hash { get; set; }

        /// <summary>
        /// Gets or sets the hash of the block immediately previous to this one
        /// </summary>
        [NotNull]
        public string PreviousBlockHash { get; set; }
    }
}
