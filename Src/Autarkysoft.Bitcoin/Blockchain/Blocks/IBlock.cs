﻿// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Transactions;

namespace Autarkysoft.Bitcoin.Blockchain.Blocks
{
    /// <summary>
    /// Defines methods and properties that a block class implements.
    /// <para/>Inherits from <see cref="IDeserializable"/>.
    /// </summary>
    public interface IBlock : IDeserializable
    {
        /// <summary>
        /// This block's height (set before verification using the blockchain tip and the
        /// <see cref="BlockHeader.PreviousBlockHeaderHash"/>)
        /// </summary>
        int Height { get; set; }

        /// <summary>
        /// Returns total size (block size when serialized)
        /// </summary>
        int TotalSize { get; }
        /// <summary>
        /// Returns stripped (or base) size (block size with all witnesses removedaa when serialized)
        /// </summary>
        int StrippedSize { get; }
        /// <summary>
        /// Returns block weight defined in BIP-141 as (BaseSize * 3 + TotalSize)
        /// </summary>
        int Weight { get; }

        /// <summary>
        /// The block header
        /// </summary>
        BlockHeader Header { get; set; }

        /// <summary>
        /// List of transactions in this block
        /// </summary>
        ITransaction[] TransactionList { get; set; }


        /// <summary>
        /// Adds stripped serialized size of this instance to the given counter.
        /// </summary>
        /// <param name="counter">Size counter to use</param>
        void AddStrippedSerializedSize(SizeCounter counter);

        /// <summary>
        /// Returns hash of this block using the defined hash function.
        /// <para/>Note that after the first call to this function, the hash result will be stored to avoid future repeated
        /// computation. If any of the properties change this function has to be called with <paramref name="recompute"/> = true
        /// to force re-computation of the hash.
        /// </summary>
        /// <param name="recompute">
        /// Indicates whether the hash should be recomputed or the cached value is still valid
        /// </param>
        /// <returns>Block hash</returns>
        byte[] GetBlockHash(bool recompute);

        /// <summary>
        /// Returns hash of this block as a base-16 encoded string.
        /// <para/>Same as <see cref="GetBlockHash(bool)"/> if any property is changed the ID has to be re-computed.
        /// </summary>
        /// <param name="recompute">
        /// Indicates whether the hash should be recomputed or the cached value is still valid
        /// </param>
        /// <returns>Base-16 encoded block hash</returns>
        string GetBlockID(bool recompute);

        /// <summary>
        /// Returns merkle root of this block using the list of transactions.
        /// </summary>
        /// <returns>Merkle root</returns>
        byte[] ComputeMerkleRoot();

        /// <summary>
        /// Returns merkle root hash of witnesses in this block using the list of transactions.
        /// </summary>
        /// <param name="commitment">32 byte witness commitment</param>
        /// <returns>Merkle root</returns>
        byte[] ComputeWitnessMerkleRoot(byte[] commitment);
    }
}
