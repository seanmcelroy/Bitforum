// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PostHeader.cs" company="Sean McElroy">
//   Copyright Sean McElroy 2016.  Released under the terms of the MIT License
// </copyright>
// <summary>
//   A post header is the metadata about a message on a named forum
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
    /// A post header is the metadata about a message on a named forum
    /// </summary>
    [PublicAPI]
    public class PostHeader
    {
        /// <summary>
        /// Gets or sets the name of the newsgroup for the message
        /// </summary>
        public string Forum { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the message
        /// </summary>
        [NotNull]
        public string MessageId { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the conversation.  Replies to a thread will have the same conversation identifier.
        /// </summary>
        [CanBeNull]
        public string ConversationId { get; set; }

        /// <summary>
        /// Gets or sets the identifying text about the sender.  This may or may not be a standard e-mail address; it could also be a certificate.
        /// </summary>
        [NotNull]
        public string From { get; set; }

        /// <summary>
        /// Gets or sets the date the message was sent, expressed in the number seconds since the epoch (January 1, 1970 GMT)
        /// </summary>
        public uint DateEpoch { get; set; }

        /// <summary>
        /// Gets or sets the subject line of the message
        /// </summary>
        [CanBeNull]
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the content type of the body
        /// </summary>
        [NotNull]
        public string BodyContentType { get; set; }

        /// <summary>
        /// Gets the serialized byte representation of a post
        /// </summary>
        /// <param name="bodySize">The size of the post's body, in bytes</param>
        /// <returns>The byte representation of a post</returns>
        [NotNull, Pure]
        public byte[] ToByteArray(ulong bodySize)
        {
            if (string.IsNullOrWhiteSpace(this.Forum))
            {
                throw new InvalidOperationException("Forum not specified");
            }

            if (string.IsNullOrWhiteSpace(this.MessageId))
            {
                throw new InvalidOperationException("MessageId not specified");
            }

            if (string.IsNullOrWhiteSpace(this.ConversationId))
            {
                throw new InvalidOperationException("ConversationId not specified");
            }

            if (string.IsNullOrWhiteSpace(this.From))
            {
                throw new InvalidOperationException("From not specified");
            }

            if (string.IsNullOrWhiteSpace(this.BodyContentType))
            {
                throw new InvalidOperationException("BodyContentType not specified");
            }

            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                var forumBytes = System.Text.Encoding.UTF8.GetBytes(this.Forum.Substring(0, Math.Min(255, this.Forum.Length)));
                var messageIdBytes = System.Text.Encoding.UTF8.GetBytes(this.MessageId.Substring(0, Math.Min(255, this.MessageId.Length)));
                var conversationIdBytes = System.Text.Encoding.UTF8.GetBytes(this.ConversationId.Substring(0, Math.Min(255, this.ConversationId.Length)));
                var fromBytes = System.Text.Encoding.UTF8.GetBytes(this.From.Substring(0, Math.Min(ushort.MaxValue, this.From.Length)));
                var subjectBytes = string.IsNullOrWhiteSpace(this.Subject) ? new byte[0] : System.Text.Encoding.UTF8.GetBytes(this.Subject);
                var bodyContentTypeBytes = System.Text.Encoding.UTF8.GetBytes(this.BodyContentType.Substring(0, Math.Min(255, this.BodyContentType.Length)));

                // Header
                bw.Write(1); // VERSION
                bw.Write(8); // Length of DateEpoch
                bw.Write((byte)forumBytes.Length);
                bw.Write((byte)messageIdBytes.Length);
                bw.Write((byte)conversationIdBytes.Length);
                bw.Write((ushort)fromBytes.Length);
                bw.Write((ushort)subjectBytes.Length);
                bw.Write((byte)bodyContentTypeBytes.Length);
                bw.Write(bodySize);

                // Data
                bw.Write(this.DateEpoch);
                bw.Write(forumBytes);
                bw.Write(messageIdBytes);
                bw.Write(conversationIdBytes);
                bw.Write(fromBytes);
                bw.Write(subjectBytes);
                bw.Write(bodyContentTypeBytes);

                return ms.ToArray();
            }
        }

        /// <summary>
        /// Hashes the byte representation of the post using the <see cref="SHA512"/> hashing algorithm.
        /// </summary>
        /// <param name="bodySize">The size of the post's body, in bytes</param>
        /// <returns>The hashed output of <see cref="ToByteArray"/></returns>
        [NotNull, Pure]
        public byte[] GetHash(ulong bodySize)
        {
            using (var sha = SHA512.Create())
            {
                Debug.Assert(sha != null, "sha != null");
                return sha.ComputeHash(this.ToByteArray(bodySize));
            }
        }
    }
}
