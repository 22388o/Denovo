﻿// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;
using System.Collections.Generic;
using Xunit;

namespace Tests.Bitcoin.Blockchain.Scripts.Operations.Conditionals
{
    public class IFOpTests
    {
        [Fact]
        public void ConstructorTest()
        {
            IFOp op = new(null, null);

            Assert.Equal(OP.IF, op.OpValue);
            Helper.ComparePrivateField(op, "runWithTrue", true);
        }


        public static IEnumerable<object[]> GetRunCases()
        {
            yield return new object[]
            {
                new IOperation[] { new MockOp(true, Errors.ForTesting) },
                new IOperation[] { new MockOp(false, Errors.ForTesting) },
                OpTestCaseHelper.TrueBytes,
                true, // checkRes
                true, // runRes
                Errors.None // error
            };
            yield return new object[]
            {
                new IOperation[] { new MockOp(false, Errors.ForTesting) },
                new IOperation[] { new MockOp(false, Errors.ForTesting) },
                OpTestCaseHelper.TrueBytes,
                true, // checkRes
                false, // runRes
                Errors.ForTesting
            };
            yield return new object[]
            {
                new IOperation[] { new MockOp(false, Errors.ForTesting) },
                new IOperation[] { new MockOp(true, Errors.ForTesting) },
                OpTestCaseHelper.FalseBytes,
                true, // checkRes
                true, // runRes
                Errors.None
            };
            yield return new object[]
            {
                new IOperation[] { new MockOp(false, Errors.ForTesting) },
                new IOperation[] { new MockOp(false, Errors.ForTesting) },
                OpTestCaseHelper.FalseBytes,
                true, // checkRes
                false, // runRes
                Errors.ForTesting
            };
            // Fail on checking the popped data
            yield return new object[]
            {
                new IOperation[] { new MockOp(false, Errors.ForTesting) },
                new IOperation[] { new MockOp(false, Errors.ForTesting) },
                OpTestCaseHelper.b7,
                false, // checkRes
                false, // runRes
                Errors.InvalidConditionalBool
            };
            // Null ElseOps
            yield return new object[]
            {
                new IOperation[] { new MockOp(true, Errors.ForTesting) },
                null,
                OpTestCaseHelper.TrueBytes,
                true, // checkRes
                true, // runRes
                Errors.None
            };
            // Null ElseOps (trying to run ElseOps
            yield return new object[]
            {
                new IOperation[] { new MockOp(false, Errors.ForTesting) },
                null,
                OpTestCaseHelper.FalseBytes,
                true, // checkRes
                true, // runRes
                Errors.None
            };
        }
        [Theory]
        [MemberData(nameof(GetRunCases))]
        public void RunTest(IOperation[] main, IOperation[] other, byte[] popData, bool checkRes, bool runResult, Errors expErr)
        {
            MockOpData data = new(FuncCallName.Pop)
            {
                _itemCount = 1,
                conditionalBoolCheckResult = checkRes,
                expectedConditionalBoolBytes = popData,
                popData = new byte[][] { popData }
            };

            IFOp op = new(main, other);
            bool b = op.Run(data, out Errors error);

            Assert.Equal(runResult, b);
            Assert.Equal(expErr, error);
        }

        [Fact]
        public void Run_FailTest()
        {
            IFOp op = new(null, null);
            MockOpData data = new()
            {
                _itemCount = 0
            };

            bool b = op.Run(data, out Errors error);

            Assert.False(b);
            Assert.Equal(Errors.NotEnoughStackItems, error);
        }
    }
}
