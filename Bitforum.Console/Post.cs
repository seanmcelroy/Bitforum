// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Post.cs" company="Sean McElroy">
//   Copyright Sean McElroy 2016.  Released under the terms of the MIT License
// </copyright>
// <summary>
//   A post is a message on a named forum
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Bitforum.Console
{
    using System.Diagnostics;
    using System.Security.Cryptography;

    using JetBrains.Annotations;

    /// <summary>
    /// A post is a message on a named forum
    /// </summary>
    [PublicAPI]
    public class Post
    {
        public string Forum { get; set; }

        public string MessageId { get; set; }

        public string ConversationId { get; set; }

        public string From { get; set; }

        public long DateEpoch { get; set; }

        public string Subject { get; set; }

        public string Body { get; set; }

        [NotNull, Pure]
        public string Concat()
        {
            return $"*|{this.Forum}|.|{this.MessageId}|.|{this.ConversationId}|.|{this.From}|.|{this.DateEpoch}|.|{this.Subject}|.|{this.Body}|*";
        }

        [NotNull, Pure]
        public byte[] Hash()
        {
            using (var sha = SHA512.Create())
            {
                Debug.Assert(sha != null, "sha != null");
                return sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(this.Concat()));
            }
        }
    }
}
