﻿// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Blocks;
using System;

namespace Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads
{
    public class MerkleBlockPayload : PayloadBase
    {
        public Block BlockHeader { get; set; }
        private uint _txCount;
        public uint TransactionCount
        {
            get => _txCount;
            set => _txCount = value;
        }

        public byte[][] Hashes { get; set; }

        private byte[] _flags;
        public byte[] Flags
        {
            get => _flags;
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(Flags), "Flags can not be null.");

                _flags = value;
            }
        }

        /// <inheritdoc/>
        public override PayloadType PayloadType => PayloadType.MerkleBlock;


        /// <inheritdoc/>
        public override void Serialize(FastStream stream)
        {
            CompactInt hashCount = new CompactInt(Hashes.Length);
            CompactInt flagsLength = new CompactInt(Flags.Length);

            BlockHeader.SerializeHeader(stream);
            stream.Write(TransactionCount);
            hashCount.WriteToStream(stream);
            foreach (var item in Hashes)
            {
                stream.Write(item);
            }
            flagsLength.WriteToStream(stream);
            stream.Write(Flags);
        }


        /// <inheritdoc/>
        public override bool TryDeserialize(FastStreamReader stream, out string error)
        {
            if (stream is null)
            {
                error = "Stream can not be null.";
                return false;
            }


            if (!BlockHeader.TryDeserializeHeader(stream, out error))
            {
                return false;
            }

            if (!stream.TryReadUInt32(out _txCount))
            {
                error = Err.EndOfStream;
                return false;
            }

            if (!CompactInt.TryRead(stream, out CompactInt hashCount, out error))
            {
                return false;
            }
            if (hashCount > int.MaxValue)
            {
                error = "Hash count is too big.";
                return false;
            }

            Hashes = new byte[(int)hashCount][];
            for (int i = 0; i < (int)hashCount; i++)
            {
                if (!stream.TryReadByteArray(32, out Hashes[i]))
                {
                    return false;
                }
            }

            if (!CompactInt.TryRead(stream, out CompactInt flagsLength, out error))
            {
                return false;
            }
            if (flagsLength > int.MaxValue)
            {
                error = "Flags length is too big.";
                return false;
            }

            if (!stream.TryReadByteArray((int)flagsLength, out _flags))
            {
                error = Err.EndOfStream;
                return false;
            }

            error = null;
            return true;
        }
    }
}
