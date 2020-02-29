﻿// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Transactions;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.EllipticCurve;
using System;

namespace Autarkysoft.Bitcoin.Blockchain.Scripts.Operations
{
    /// <summary>
    /// An advanced implementation of a last-in-first-out (LIFO) collection (similar to <see cref="System.Collections.Stack"/>)
    /// to be used with <see cref="IOperation"/>s as their data provider.
    /// <para/>All the functions in this class skip checks (eg. checking ItemCount before popping an item). 
    /// Caller must perform checks.
    /// <para/>All indexes are zero based whether it is normal index from beginning or index from end:
    /// Item at the end (length-1) is at index 0.
    /// </summary>
    public class OpData : IOpData
    {
        /// <summary>
        /// Initializes a new instance of <see cref="OpData"/> with default capacity
        /// </summary>
        public OpData() : this(DefaultCapacity)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="OpData"/> with the given capacity.
        /// </summary>
        /// <param name="cap">Capacity</param>
        public OpData(int cap)
        {
            if (cap < DefaultCapacity)
            {
                cap = DefaultCapacity;
            }
            holder = new byte[cap][];

            Calc = new EllipticCurveCalculator();
        }

        /// <summary>
        /// Initializes a new instance of <see cref="OpData"/> with the given data array.
        /// </summary>
        /// <param name="dataToPush">An array of byte arrays to put in the stack</param>
        public OpData(byte[][] dataToPush)
        {
            if (dataToPush == null || dataToPush.Length == 0)
            {
                holder = new byte[DefaultCapacity][];
            }
            else
            {
                int len = (dataToPush.Length < DefaultCapacity) ? DefaultCapacity : dataToPush.Length + DefaultCapacity;
                holder = new byte[len][];
                ItemCount = dataToPush.Length;
                Array.Copy(dataToPush, holder, dataToPush.Length);
            }

            Calc = new EllipticCurveCalculator();
        }



        private const int DefaultCapacity = 10;
        // Don't rename (used by test through reflection).
        private byte[][] holder;

        // TODO: complete the code for signing part + tests
        private readonly ITransaction Tx;
        private readonly ITransaction PrvTx;
        private readonly int TxInIndex;
        private readonly int TxOutIndex;



        /// <inheritdoc/>
        public EllipticCurveCalculator Calc { get; private set; }


        /// <inheritdoc/>
        public byte[] GetBytesToSign(SigHashType sht, IRedeemScript redeem = null)
        {
            return Tx.GetBytesToSign(PrvTx, TxInIndex, sht, redeem);
        }


        /// <inheritdoc/>
        public int ItemCount { get; private set; }



        /// <inheritdoc/>
        public byte[] Peek()
        {
            return holder[ItemCount - 1];
        }

        /// <inheritdoc/>
        public byte[][] Peek(int count)
        {
            byte[][] res = new byte[count][];
            Array.Copy(holder, ItemCount - count, res, 0, count);

            return res;
        }

        /// <inheritdoc/>
        public byte[] PeekAtIndex(int index)
        {
            return holder[ItemCount - 1 - index];
        }


        /// <inheritdoc/>
        public byte[] Pop()
        {
            byte[] res = holder[--ItemCount];
            holder[ItemCount] = null;
            return res;
        }

        /// <inheritdoc/>
        public byte[][] Pop(int count)
        {
            byte[][] res = new byte[count][];
            Array.Copy(holder, ItemCount - count, res, 0, count);
            for (int i = 0; i < count; i++)
            {
                holder[--ItemCount] = null;
            }

            return res;
        }

        /// <inheritdoc/>
        public byte[] PopAtIndex(int index)
        {
            int realIndex = ItemCount - 1 - index;
            byte[] res = holder[realIndex];
            ItemCount--;

            if (index != 0)
            {
                Array.Copy(holder, realIndex + 1, holder, realIndex, index /*index works as count (copy items after realindex)*/);
            }
            holder[ItemCount] = null;

            return res;
        }


        /// <inheritdoc/>
        public void Push(byte[] data)
        {
            if (ItemCount == holder.Length)
            {
                // Instead of doubling we add the default value since we don't need that many new items 
                // eg. 10->20 but 20->30 instead of 40.
                byte[][] holder2 = new byte[holder.Length + DefaultCapacity][];
                Array.Copy(holder, 0, holder2, 0, ItemCount);
                holder = holder2;
            }

            holder[ItemCount++] = data;
        }

        /// <inheritdoc/>
        public void Push(byte[][] data)
        {
            if (ItemCount + data.Length > holder.Length)
            {
                byte[][] holder2 = new byte[ItemCount + data.Length + DefaultCapacity][];
                Array.Copy(holder, 0, holder2, 0, ItemCount);
                holder = holder2;
            }

            Array.Copy(data, 0, holder, ItemCount, data.Length);
            ItemCount += data.Length;
        }

        /// <inheritdoc/>
        public void Insert(byte[] data, int index)
        {
            // only call if index < itemcount
            if (ItemCount == holder.Length)
            {
                // Instead of doubling we add the default value since we don't need that many new items.
                byte[][] holder2 = new byte[holder.Length + DefaultCapacity][];
                Array.Copy(holder, 0, holder2, 0, ItemCount);
                holder = holder2;
            }

            int realIndex = ItemCount - index;
            if (realIndex < ItemCount)
            {
                Array.Copy(holder, realIndex, holder, realIndex + 1, ItemCount - realIndex);
            }
            holder[realIndex] = data;
            ItemCount++;
        }

        /// <inheritdoc/>
        public void Insert(byte[][] data, int index)
        {
            int realIndex = ItemCount - index;
            if (ItemCount + data.Length > holder.Length)
            {
                byte[][] holder2 = new byte[ItemCount + data.Length + DefaultCapacity][];

                Array.Copy(holder, 0, holder2, 0, realIndex);
                Array.Copy(data, 0, holder2, realIndex, data.Length);
                Array.Copy(holder, realIndex, holder2, realIndex + data.Length, ItemCount - realIndex);

                holder = holder2;
            }
            else
            {
                Array.Copy(holder, realIndex, holder, realIndex + data.Length, ItemCount - realIndex);
                Array.Copy(data, 0, holder, realIndex, data.Length);
            }

            ItemCount += data.Length;
        }



        // Don't rename (used by test through reflection).
        private byte[][] altHolder;

        /// <inheritdoc/>
        public int AltItemCount { get; private set; }

        /// <inheritdoc/>
        public byte[] AltPop()
        {
            byte[] res = altHolder[--AltItemCount];
            altHolder[AltItemCount] = null;
            return res;
        }

        /// <inheritdoc/>
        public void AltPush(byte[] data)
        {
            if (altHolder == null)
            {
                // We set alt-holder here to keep it null for majority of cases that never use this stack.
                altHolder = new byte[DefaultCapacity][];
            }
            if (AltItemCount == altHolder.Length)
            {
                // Instead of doubling we add the default value since we don't need that many new items 
                // eg. 10->20 but 20->30 instead of 40.
                byte[][] temp = new byte[altHolder.Length + DefaultCapacity][];
                Array.Copy(altHolder, 0, temp, 0, AltItemCount);
                altHolder = temp;
            }

            altHolder[AltItemCount++] = data;
        }
    }
}
