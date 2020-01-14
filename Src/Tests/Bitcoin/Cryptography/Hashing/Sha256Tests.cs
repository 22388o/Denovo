﻿// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.Hashing;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using Xunit;

namespace Tests.Bitcoin.Cryptography.Hashing
{
    public class Sha256Tests
    {
        [Theory]
        [MemberData(nameof(HashTestCaseHelper.GetCommonHashCases), parameters: "SHA256", MemberType = typeof(HashTestCaseHelper))]
        public void ComputeHashTest(byte[] message, byte[] expectedHash)
        {
            using Sha256 sha = new Sha256();
            byte[] actualHash = sha.ComputeHash(message);
            Assert.Equal(expectedHash, actualHash);
        }

        [Fact]
        public void ComputeHash_AMillionATest()
        {
            using Sha256 sha = new Sha256();
            byte[] actualHash = sha.ComputeHash(HashTestCaseHelper.GetAMillionA());
            byte[] expectedHash = Helper.HexToBytes("cdc76e5c9914fb9281a1c7e284d73e67f1809a48a497200e046d39ccc7112cd0");

            Assert.Equal(expectedHash, actualHash);
        }

        [Fact]
        public void ComputeHash_ReuseTest()
        {
            byte[] msg1 = Encoding.UTF8.GetBytes("The quick brown fox jumps over the lazy dog");
            byte[] msg2 = Encoding.UTF8.GetBytes("The quick brown fox jumps over the lazy cog");
            byte[] exp1 = Helper.HexToBytes("d7a8fbb307d7809469ca9abcb0082e4f8d5651e46d3cdb762d02d0bf37c9e592");
            byte[] exp2 = Helper.HexToBytes("e4c4d8f3bf76b692de791a173e05321150f7a345b46484fe427f6acc7ecc81be");

            using Sha256 sha = new Sha256();
            byte[] act1 = sha.ComputeHash(msg1);
            byte[] act2 = sha.ComputeHash(msg2);

            Assert.Equal(exp1, act1);
            Assert.Equal(exp2, act2);
        }

        [Fact]
        public void ComputeHash_WithIndexTest()
        {
            using Sha256 sha = new Sha256();
            byte[] data = Helper.HexToBytes("123fab54686520717569636b2062726f776e20666f78206a756d7073206f76657220746865206c617a7920646f67f3a25c92");
            byte[] actualHash = sha.ComputeHash(data, 3, 43);
            byte[] expectedHash = Helper.HexToBytes("d7a8fbb307d7809469ca9abcb0082e4f8d5651e46d3cdb762d02d0bf37c9e592");

            Assert.Equal(expectedHash, actualHash);
        }

        [Fact]
        public void ComputeHash_DoubleTest()
        {
            using Sha256 sha = new Sha256(true);
            var data = Helper.HexToBytes("fb8049137747e712628240cf6d7056ea2870170cb7d9bc713d91e901b514c6ae7d7dda3cd03ea1b99cf85046a505f3590541123d3f8f2c22c4d7d6e65de65c4ebb9251f09619");
            byte[] actualHash = sha.ComputeHash(data);
            byte[] expectedHash = Helper.HexToBytes("d2cee8d3cfaf1819c55cce1214d01cdef1d97446719ccfaad4d76d912a8126f9");

            Assert.Equal(expectedHash, actualHash);
        }


        [Fact]
        public void ComputeHash_ExceptionsTest()
        {
            byte[] goodBa = { 1, 2, 3 };
            Sha256 sha = new Sha256();

            Assert.Throws<ArgumentNullException>(() => sha.ComputeHash(null));
            Assert.Throws<ArgumentNullException>(() => sha.ComputeHash(null, 0, 1));
            Assert.Throws<IndexOutOfRangeException>(() => sha.ComputeHash(goodBa, 0, 5));
            Assert.Throws<IndexOutOfRangeException>(() => sha.ComputeHash(goodBa, 10, 1));

            sha.Dispose();
            Assert.Throws<ObjectDisposedException>(() => sha.ComputeHash(goodBa));
            Assert.Throws<ObjectDisposedException>(() => sha.ComputeHash(goodBa, 0, 2));
        }


        [Theory]
        [MemberData(nameof(HashTestCaseHelper.GetNistShortCases), parameters: "Sha256", MemberType = typeof(HashTestCaseHelper))]
        public void ComputeHash_NistShortTest(byte[] message, byte[] expected)
        {
            using Sha256 sha = new Sha256(false);
            byte[] actual = sha.ComputeHash(message);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(HashTestCaseHelper.GetNistLongCases), parameters: "Sha256", MemberType = typeof(HashTestCaseHelper))]
        public void ComputeHash_NistLongTest(byte[] message, byte[] expected)
        {
            using Sha256 sha = new Sha256(false);
            byte[] actual = sha.ComputeHash(message);
            Assert.Equal(expected, actual);
        }


        [Fact]
        public void ComputeHash_NistMonteCarloTest()
        {
            byte[] seed = Helper.HexToBytes("6d1e72ad03ddeb5de891e572e2396f8da015d899ef0e79503152d6010a3fe691");
            JObject jObjs = Helper.ReadResource<JObject>("Sha256NistTestData");
            int size = 32;
            byte[] toHash = new byte[3 * size];

            byte[] M0 = seed;
            byte[] M1 = seed;
            byte[] M2 = seed;

            using Sha256 sha = new Sha256(false);

            foreach (var item in jObjs["MonteCarlo"])
            {
                byte[] expected = Helper.HexToBytes(item.ToString());
                for (int i = 0; i < 1000; i++)
                {
                    Buffer.BlockCopy(M0, 0, toHash, 0, size);
                    Buffer.BlockCopy(M1, 0, toHash, size, size);
                    Buffer.BlockCopy(M2, 0, toHash, size * 2, size);

                    M0 = M1;
                    M1 = M2;
                    M2 = sha.ComputeHash(toHash);
                }
                M0 = M2;
                M1 = M2;

                Assert.Equal(expected, M2);
            }
        }



        // The original MAJ() and CH() functions are defined differently in RFC documentation.
        // The two following tests act as proof that the changed functions are giving the same results.
        private uint CH_Original(uint x, uint y, uint z)
        {
            return (x & y) ^ ((~x) & z);
        }
        private uint CH_Changed(uint x, uint y, uint z)
        {
            return z ^ (x & (y ^ z));
        }
        [Fact]
        public void CH_Test()
        {
            for (uint x = 0b00; x <= 0b11; x++)
            {
                for (uint y = 0b00; y <= 0b11; y++)
                {
                    for (uint z = 0b00; z <= 0b11; z++)
                    {
                        Assert.Equal(CH_Original(x, y, z), CH_Changed(x, y, z));
                    }
                }
            }
        }


        private uint MAJ_Original(uint x, uint y, uint z)
        {
            return (x & y) ^ (x & z) ^ (y & z);
        }
        private uint MAJ_Changed(uint x, uint y, uint z)
        {
            return (x & y) | (z & (x | y));
        }
        [Fact]
        public void MAJ_Test()
        {
            for (uint x = 0b00; x <= 0b11; x++)
            {
                for (uint y = 0b00; y <= 0b11; y++)
                {
                    for (uint z = 0b00; z <= 0b11; z++)
                    {
                        Assert.Equal(MAJ_Original(x, y, z), MAJ_Changed(x, y, z));
                    }
                }
            }
        }

        [Fact]
        public void MessageLengthTest()
        {
            int len = 0x493657ad;
            long msgLen = (long)len << 3; // *8

            byte[] first = new byte[8];
            byte[] second = new byte[8];

            // Message length is an Int32 type multiplied by 8 to convert to bit length for padding 
            // so the maximum result is:
            // int.MaxValue * 8 = 17179869176 or int.MaxValue << 3
            // 00000000_00000000_00000000_00000011_11111111_11111111_11111111_11111000
            // This means the first 3 (out of 8) bytes will always be zero

            first[7] = (byte)msgLen;
            first[6] = (byte)(msgLen >> 8);
            first[5] = (byte)(msgLen >> 16);
            first[4] = (byte)(msgLen >> 24);
            first[3] = (byte)(msgLen >> 32);
            first[2] = (byte)(msgLen >> 40); // must be zero
            first[1] = (byte)(msgLen >> 48); // must be zero
            first[0] = (byte)(msgLen >> 56); // must be zero

            // ****** The alternative way: ******/
            // msgLen = len << 3
            // [7] = (byte)msgLen           = (byte)(len << 3)
            // [6] = (byte)(msgLen >> 8)    = (byte)((len << 3) >> 8) = (byte)(len >> 5)
            // ...
            // [3] = (byte)(msgLen >> 32)   = (byte)((len << 3) >> 32) = (byte)(len >> 29)

            // [2] = (byte)(msgLen >> 40)   = (byte)((len << 3) >> 40) = (byte)(len >> 37)
            // Assuming len were Int64, 37 bit shift is getting rid of the first 32+3 bits so it must be zero
            second[7] = (byte)(len << 3);
            second[6] = (byte)(len >> 5);
            second[5] = (byte)(len >> 13);
            second[4] = (byte)(len >> 21);
            second[3] = (byte)(len >> 29);
            //second[2] = (byte)(len >> 37); shifts are bigger than 32, won't work as long as len is Int32
            //second[1] = (byte)(len >> 45); 
            //second[0] = (byte)(len >> 53); 

            Assert.Equal(first, second);
            Assert.Equal(0, first[0]);
            Assert.Equal(0, first[1]);
            Assert.Equal(0, first[2]);
        }

    }
}
