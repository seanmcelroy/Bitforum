// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HashUtility.cs" company="Sean McElroy">
//   Copyright Sean McElroy 2016.  Released under the terms of the MIT License
// </copyright>
// <summary>
//   Convienence utilities for hashing data
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Bitforum.Core
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;

    using CryptSharp.Utility;

    using JetBrains.Annotations;
    
    /// <summary>
    /// Convenience utilities for hashing data
    /// </summary>
    public static class HashUtility
    {
        /// <summary>
        /// An instance of a hashing algorithm
        /// </summary>
        private static readonly SHA512 Hasher = SHA512.Create();

        /// <summary>
        /// Computes a hash for this block for the given pre-calculated header and a given nonce value
        /// </summary>
        /// <param name="nonce">The value to which to apply to the last eight bytes of the <paramref name="preNonceArray"/> before calculating the hash</param>
        /// <param name="preNonceArray">The output from the byte array for the header of this block</param>
        /// <returns>The hash for given pre-calculated header and a given nonce value</returns>
        [NotNull, Pure]
        public static byte[] HashForNonce(long nonce, [NotNull] byte[] preNonceArray)
        {
            Array.Copy(BitConverter.GetBytes(nonce), 0, preNonceArray, preNonceArray.Length - 8, 8);
            return Hasher.ComputeHash(preNonceArray);
        }

        /// <summary>
        /// Finds the nonce value that would make this block have the <paramref name="zeroCount"/> number of zeros at the
        /// start of its hash value
        /// </summary>
        /// <param name="input">
        /// The input byte array to hash
        /// </param>
        /// <param name="zeroCount">
        /// The number of zeros for which to find the first nonce
        /// </param>
        /// <returns>
        /// The nonce value that makes this block's header have the required number of leading zeros
        /// </returns>
        [Pure]
        public static uint HashForZeroCount([NotNull] byte[] input, int zeroCount)
        {
            var best = 0;

            for (var l = 0U; l < uint.MaxValue; l++)
            {
                var candidateHash = HashForNonce(l, input);
                var success = false;
                var candidateHashString = BitConverter.ToString(candidateHash).Replace("-", string.Empty);

                var i = 0;
                foreach (var c in candidateHashString)
                {
                    if (c == '0')
                    {
                        i++;
                        if (i == zeroCount)
                        {
                            success = true;
                            best = Math.Max(best, i);
                            break;
                        }
                    }
                    else
                    {
                        best = Math.Max(best, i);
                        break;
                    }
                }

                if (l % 10000 == 0)
                {
                    Console.Write($"\rBest: {best} of {zeroCount} (#{l:N0})");
                }

                if (success)
                {
                    Console.Write($"\rBest: {best} of {zeroCount} (#{l:N0})");
                    return l;
                }
            }

            throw new InvalidOperationException($"Unable to find any hash that returns {zeroCount} zeros");
        }

        /// <summary>
        /// Converts a hex string into a byte array
        /// </summary>
        /// <param name="hex">The hex string representation of a byte array</param>
        /// <returns>Byte array version of the hex string</returns>
        [NotNull, Pure]
        public static byte[] StringToByteArray([NotNull] string hex)
        {
            if (string.IsNullOrWhiteSpace(hex))
            {
                throw new ArgumentNullException(nameof(hex));
            }

            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        [NotNull, Pure]
        public static byte[] CalculatePasswordHash([NotNull] string password)
        {
            var keyBytes = Encoding.UTF8.GetBytes(password);
            var saltBytes = Encoding.UTF8.GetBytes(new string(password.ToUpperInvariant().Reverse().ToArray()));
            var cost = 262144;
            var blockSize = 8;
            var parallel = 1;
            var derivedKeyLength = 256;

            var bytes = SCrypt.ComputeDerivedKey(keyBytes, saltBytes, cost, blockSize, parallel, null, derivedKeyLength);
            Debug.Assert(bytes != null, "bytes != null");
            return bytes;
        }
    }
}
