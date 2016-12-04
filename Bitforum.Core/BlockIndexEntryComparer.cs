// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BlockIndexEntryComparer.cs" company="Sean McElroy">
//   Copyright Sean McElroy 2016.  Released under the terms of the MIT License
// </copyright>
// <summary>
//   A comparer that is used to sort a <see cref="BlockIndex" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Bitforum.Core
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A comparer that is used to sort a <see cref="BlockIndex"/>
    /// </summary>
    public class BlockIndexEntryComparer : IComparer<BlockIndexEntry>
    {
        /// <inheritdoc />
        public int Compare(BlockIndexEntry x, BlockIndexEntry y)
        {
            if (x == null)
            {
                return -1;
            }

            if (y == null)
            {
                return 1;
            }

            if (string.Equals(x.PreviousBlockHash, y.Hash, StringComparison.Ordinal))
            {
                return 1;
            }

            if (string.Equals(y.PreviousBlockHash, x.Hash, StringComparison.Ordinal))
            {
                return -1;
            }

            if (string.Equals(x.Hash, y.Hash, StringComparison.Ordinal))
            {
                return 0;
            }

            return string.Compare(x.Hash, y.Hash, StringComparison.Ordinal);
        }
    }
}
