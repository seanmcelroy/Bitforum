// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Post.cs" company="Sean McElroy">
//   Copyright Sean McElroy 2016.  Released under the terms of the MIT License
// </copyright>
// <summary>
//   A post is a message on a named forum
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
    /// A post is a message on a named forum
    /// </summary>
    [PublicAPI]
    public class Post
    {
        /// <summary>
        /// Gets or sets the header metadata of the post
        /// </summary>
        public PostHeader Header { get; set; }

        /// <summary>
        /// Gets or sets the body of the message
        /// </summary>
        public byte[] Body { get; set; }

        /// <summary>
        /// Gets the serialized byte representation of a post
        /// </summary>
        /// <returns>The byte representation of a post</returns>
        [NotNull, Pure]
        public byte[] ToByteArray()
        {
            if (this.Header == null)
            {
                throw new InvalidOperationException("Post header is null!");
            }

            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                var bodyBytes = this.Body ?? new byte[0];
                bw.Write(this.Header.ToByteArray(Convert.ToUInt64(bodyBytes.GetLongLength(0))));
                bw.Write(bodyBytes);
                return ms.ToArray();
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
