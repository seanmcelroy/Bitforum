// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Identity.cs" company="Sean McElroy">
//   Copyright Sean McElroy 2016.  Released under the terms of the MIT License
// </copyright>
// <summary>
//   A key pair and proof of work that establishes an identity that can post to the block chain
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Bitforum.Core
{
    using System;
    using System.Diagnostics;
    using System.IO;

    using JetBrains.Annotations;

    using log4net;

    using Org.BouncyCastle.Asn1.Sec;
    using Org.BouncyCastle.Crypto.Generators;
    using Org.BouncyCastle.Crypto.Parameters;
    using Org.BouncyCastle.Security;

    /// <summary>
    /// A key pair and proof of work that establishes an identity that can post to the block chain
    /// </summary>
    public class Identity
    {
        /// <summary>
        /// The logging utility instance to use to log events from this class
        /// </summary>
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Identity));

        /// <summary>
        /// Gets or sets the date the message was sent, expressed in the number seconds since the epoch (January 1, 1970 GMT)
        /// </summary>
        public uint DateEpoch { get; set; }

        public byte[] PrivateKey { get; set; }

        public byte[] PublicKeyX { get; set; }

        public byte[] PublicKeyY { get; set; }
        
        /// <summary>
        /// Gets or sets the hash of the <see cref="Block"/> against which this identity was formed.  This would be the tail of the
        /// block chain at the start of the identity generation.  Miners would not hash an identity announcement post if the date of
        /// the block hash selected for an identity formation is too old.
        /// </summary>
        public byte[] BlockHash { get; set; }

        /// <summary>
        /// Gets or sets the nonce used to hash the public key components to make this identity meet the required difficulty
        /// </summary>
        public long Nonce { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Identity"/> class.
        /// This generates a new key pair
        /// </summary>
        /// <returns>The new identity key pair, without any nonce proof of work generated</returns>
        [Pure, NotNull]
        public static Identity Generate()
        {
            var ret = new Identity
                          {
                              DateEpoch = Convert.ToUInt32((DateTime.UtcNow - new DateTime(1970, 01, 01, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds)
                          };

            // Elliptic Curve
            var ec = SecNamedCurves.GetByName("secp256k1");
            Debug.Assert(ec != null, "ec != null");
            var domainParams = new ECDomainParameters(ec.Curve, ec.G, ec.N, ec.H);
            var random = new SecureRandom();

            var keyGen = new ECKeyPairGenerator();
            var keyParams = new ECKeyGenerationParameters(domainParams, random);
            keyGen.Init(keyParams);
            var keyPair = keyGen.GenerateKeyPair();

            Debug.Assert(keyPair != null, "keyPair != null");
            var privateKeyParams = keyPair.Private as ECPrivateKeyParameters;
            var publicKeyParams = keyPair.Public as ECPublicKeyParameters;

            // Get Private Key
            Debug.Assert(privateKeyParams != null, "privateKeyParams != null");
            var privD = privateKeyParams.D;
            Debug.Assert(privD != null, "privD != null");
            ret.PrivateKey = privD.ToByteArray();

            Debug.Assert(publicKeyParams != null, "publicKeyParams != null");
            var qa = ec.G.Multiply(privD);
            Debug.Assert(qa != null, "qa != null");
            Debug.Assert(qa.X != null, "qa.X != null");
            ret.PublicKeyX = qa.X.ToBigInteger().ToByteArrayUnsigned();
            Debug.Assert(qa.Y != null, "qa.Y != null");
            ret.PublicKeyY = qa.Y.ToBigInteger().ToByteArrayUnsigned();

            return ret;
        }
        
        public void MineIdentity([NotNull] string tailBlockHash, byte target)
        {
            if (string.IsNullOrWhiteSpace(tailBlockHash))
            {
                throw new ArgumentNullException(nameof(tailBlockHash));
            }

            this.BlockHash = HashUtility.StringToByteArray(tailBlockHash);

            byte[] identityBytes;

            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                bw.Write(this.DateEpoch);
                bw.Write(this.PublicKeyX);
                bw.Write(this.PublicKeyY);
                bw.Write(this.BlockHash);

                identityBytes = ms.ToArray();
            }

            Logger.Info($"Mining identity nonce, target difficulty: {target}...");
            var nonce = HashUtility.HashForZeroCount(identityBytes, target);
            this.Nonce = nonce;
            Logger.Info($"...Finished mining identity nonce: {nonce}");
        }
    }
}
