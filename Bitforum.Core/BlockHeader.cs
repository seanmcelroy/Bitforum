// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BlockHeader.cs" company="Sean McElroy">
//   Copyright Sean McElroy 2016.  Released under the terms of the MIT License
// </copyright>
// <summary>
//   A block header contains metadata about a block
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Bitforum.Core
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Security.Cryptography;

    using JetBrains.Annotations;

    /// <summary>
    /// A block header contains metadata about a block
    /// </summary>
    [PublicAPI]
    public class BlockHeader
    {
        /// <summary>
        /// An instance of a hashing algorithm
        /// </summary>
        private static readonly SHA512 Hasher = SHA512.Create();

        /// <summary>
        /// Gets or sets the version of the protocol this block is using for its layout
        /// </summary>
        public ushort Version { get; set; }

        /// <summary>
        /// Gets or sets the hash of the previous blocks <see cref="BlockHeader"/>
        /// </summary>
        [CanBeNull]
        public byte[] PreviousBlockHeaderHash { get; set; }

        /// <summary>
        /// Gets or sets the hash of the Merkle root of the messages in this block
        /// </summary>
        [CanBeNull]
        public byte[] MerkleRootHash { get; set; }

        /// <summary>
        /// Gets or sets timestamp the block was mined, in seconds since the epoch
        /// </summary>
        public uint Time { get; set; } = Convert.ToUInt32((DateTime.UtcNow - new DateTime(1970, 01, 01, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds);

        /// <summary>
        /// Gets or sets the current target in compact format
        /// </summary>
        public ushort Bits { get; set; }

        /// <summary>
        /// Gets or sets the nonce applied to the hash that allows it to meet the required difficulty target
        /// </summary>
        public uint Nonce { get; set; }
        
        /// <summary>
        /// Gets the serialized byte representation of a post
        /// </summary>
        /// <returns>The byte representation of a header</returns>
        [NotNull, Pure]
        public byte[] ToByteArray()
        {
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                var previousBlockHeaderHashBytes = this.PreviousBlockHeaderHash ?? new byte[64];

                // Header
                bw.Write(this.Version); // VERSION
                bw.Write(previousBlockHeaderHashBytes, 0, previousBlockHeaderHashBytes.Length);
                bw.Write(this.MerkleRootHash, 0, 64);
                bw.Write(this.Time);
                bw.Write(this.Bits);
                bw.Write(this.Nonce);

                return ms.ToArray();
            }
        }

        public void FromByteArray([NotNull] byte[] bytes)
        {
            using (var ms = new MemoryStream(bytes))
            using (var br = new BinaryReader(ms))
            {
                this.Version = br.ReadUInt16();
                this.PreviousBlockHeaderHash = br.ReadBytes(64);
                this.MerkleRootHash = br.ReadBytes(64);
                this.Time = br.ReadUInt32();
                this.Bits = br.ReadUInt16();
                this.Nonce = br.ReadUInt32();
            }
        }

        /// <summary>
        /// Hashes the byte representation of the post using the <see cref="SHA512"/> hashing algorithm.
        /// </summary>
        /// <returns>The hashed output of <see cref="ToByteArray"/></returns>
        [NotNull, Pure]
        public byte[] GetHash()
        {
            using (var sha = SHA512.Create())
            {
                Debug.Assert(sha != null, "sha != null");
                return sha.ComputeHash(this.ToByteArray());
            }
        }
    }
}
