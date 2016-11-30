// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Block.cs" company="Sean McElroy">
//   Copyright Sean McElroy 2016.  Released under the terms of the MIT License
// </copyright>
// <summary>
//   A block is a unit of mining work that contains posts and the hashes that conform to the difficulty target
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Bitforum.Console
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Security.Cryptography;

    using JetBrains.Annotations;

    /// <summary>
    /// A block is a unit of mining work that contains posts and the hashes that conform to the difficulty target
    /// </summary>
    [PublicAPI]
    public class Block
    {
        /// <summary>
        /// An instance of a hashing algorithm
        /// </summary>
        private static readonly SHA512 Hasher = SHA512.Create();

        /// <summary>
        /// Gets or sets the version of the Bitforum protocol this block is using for its layout
        /// </summary>
        public short Version { get; set; }

        public long BlockNumber { get; set; }

        public byte[] BlockHash { get; set; }

        public long Nonce { get; set; }

        public long PreviousBlockNumber { get; set; }

        [CanBeNull]
        public byte[] PreviousBlockHash { get; set; }

        public Post[] Posts { get; set; }

        [NotNull, Pure]
        public static Block MineBlock([NotNull] Block tail, [NotNull] List<Post> unconfirmedPosts)
        {
            var newBlock = new Block
            {
                Version = 1,
                BlockNumber = tail.BlockNumber + 1,
                PreviousBlockNumber = tail.BlockNumber,
                PreviousBlockHash = tail.BlockHash,
                Posts = unconfirmedPosts.ToArray()
            };

            var minedHash = newBlock.HashForZeroCount(3);

            Debug.Assert(minedHash != null, "minedHash != null");
            newBlock.BlockHash = minedHash.Item1;
            newBlock.Nonce = minedHash.Item2;

            return newBlock;
        }

        [NotNull, Pure]
        public byte[] PreparePreNonceArrayForHashing([NotNull] byte[] merkleRoot, bool genesisBlock = false)
        {
            var bytes = new List<byte>();
            if (!genesisBlock)
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                bytes.AddRange(this.PreviousBlockHash);
            }

            bytes.AddRange(merkleRoot);

            bytes.AddRange(BitConverter.GetBytes(0L));

            return bytes.ToArray();
        }

        [NotNull, Pure]
        public byte[] HashForNonce(long nonce, [NotNull] byte[] preNonceArray)
        {
            Array.Copy(BitConverter.GetBytes(nonce), 0, preNonceArray, preNonceArray.Length - 8, 8);
            return Hasher.ComputeHash(preNonceArray);
        }

        public void GenerateGenesisHash()
        {
            var minedHash = this.HashForZeroCount(1, true);
            Debug.Assert(minedHash != null, "minedHash != null");
            this.BlockHash = minedHash.Item1;
            this.Nonce = minedHash.Item2;
        }

        /// <summary>
        /// Generates the Merkle root hash of the unconfirmed posts that are part of this mined block
        /// </summary>
        /// <returns>The Merkle root hash of the unconfirmed posts</returns>
        [NotNull, Pure]
        private byte[] GenerateMerkleRoot()
        {
            var leafHashes = new List<byte[]>();
            using (var sha = SHA512.Create())
            {
                Debug.Assert(sha != null, "sha != null");
                Debug.Assert(this.Posts != null, "this.Posts");
                foreach (var t in this.Posts)
                {
                    leafHashes.Add(sha.ComputeHash(sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(t.Concat()))));
                }
            }

            var intermediateHashes = leafHashes.ToArray();
            do
            {
                intermediateHashes = this.GenerateMerkleTree(intermediateHashes);
            }
            while (intermediateHashes.Length > 1);

            Debug.Assert(intermediateHashes != null, "intermediateHashes != null");
            Debug.Assert(intermediateHashes.Length == 1, "intermediateHashes.Length == 1");
            Debug.Assert(intermediateHashes[0] != null, "intermediateHashes[0] != null");
            return intermediateHashes[0];
        }

        [NotNull, Pure]
        private byte[][] GenerateMerkleTree([NotNull] byte[][] hashes)
        {
            var results = new List<byte[]>();

            for (var i = 0; i < hashes.Length; i += 2)
            {
                var hashA = hashes[i];
                var hashB = !(i + 1 < hashes.Length) ? hashes[i] : hashes[i + 1];
                Debug.Assert(hashB != null, "hashB != null");
                var hashCombined = new byte[hashA.Length + hashB.Length];
                Array.Copy(hashA, 0, hashCombined, 0, hashA.Length);
                Array.Copy(hashB, 0, hashCombined, hashA.Length - 1, hashB.Length);
                results.Add(Hasher.ComputeHash(Hasher.ComputeHash(hashCombined)));
            }

            return results.ToArray();
        }

        [CanBeNull, Pure]
        private Tuple<byte[], long> HashForZeroCount(int zeroCount, bool genesisBlock = false)
        {
            Console.WriteLine();
            var best = 0;

            var merkleRoot = this.GenerateMerkleRoot();
            Debug.Assert(merkleRoot != null, "merkleRoot != null");
            var preNonceArray = this.PreparePreNonceArrayForHashing(merkleRoot, genesisBlock);

            for (var l = 0L; l < long.MaxValue; l++)
            {
                var candidateHash = this.HashForNonce(l, preNonceArray);
                var success = true;

                for (var i = 0; i < zeroCount; i++)
                {
                    if (candidateHash[i] != 0)
                    {
                        success = false;
                        break;
                    }

                    best = Math.Max(best, i + 1);
                }

                if (l % 10000 == 0)
                {
                    Console.Write($"\rBest: {best} of {zeroCount} (#{l:N0})");
                }

                if (success)
                {
                    Console.Write($"\rBest: {best} of {zeroCount} (#{l:N0})");
                    Console.WriteLine($"FOUND: #{l:N0}: {BitConverter.ToString(candidateHash).Replace("-", "")}");
                    return new Tuple<byte[], long>(candidateHash, l);
                }
            }

            return null;
        }
    }
}
