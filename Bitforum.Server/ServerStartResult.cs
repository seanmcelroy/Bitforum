// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ServerStartResult.cs" company="Sean McElroy">
//   Copyright Sean McElroy 2016.  Released under the terms of the MIT License
// </copyright>
// <summary>
//   The result of the request to start a <see cref="Server" /> instance
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Bitforum.Server
{
    /// <summary>
    /// The result of the request to start a <see cref="Server"/> instance
    /// </summary>
    public sealed class ServerStartResult
    {
        /// <summary>
        /// Gets a value indicating whether the genesis block was located in local storage
        /// </summary>
        public bool GenesisBlockFound { get; internal set; }
    }
}
