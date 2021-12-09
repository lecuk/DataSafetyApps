using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace lexLinearRandomGenerator.Tests
{
    [TestClass]
    public class LinearIntegerGeneratorTests
    {
        [TestMethod]
        public void TestPeriodLength1()
        {
            IRandomGenerator<uint> random = new LinearIntegerGenerator(1, 40, 1, 1);
            TestPeriodLength(random, 40);
        }

        [TestMethod]
        public void TestPeriodLength2()
        {
            IRandomGenerator<uint> random = new LinearIntegerGenerator(1, 10, 2, 1);
            TestPeriodLength(random, 4);
        }

        [TestMethod]
        public void TestPeriodLength3()
        {
            IRandomGenerator<uint> random = new LinearIntegerGenerator(1, 10, 1, 7);
            TestPeriodLength(random, 10);
        }

        [TestMethod]
        public void TestSequence1()
        {
            IRandomGenerator<uint> random = new LinearIntegerGenerator(2, 31, 7, 3);
            TestSequence(random, new uint[]
            {
                17, 29, 20, 19, 12, 25, 23, 9, 4, 0, 3, 24, 16, 22, 2, 17
            });
        }

        [TestMethod]
        public void TestSequence2()
        {
            IRandomGenerator<uint> random = new LinearIntegerGenerator(35, 100, 21, 31);
            TestSequence(random, new uint[]
            {
                66, 17, 88, 79, 90, 21, 72, 43, 34, 45, 76, 27, 98, 89, 0
            });
        }

        [TestMethod]
        public void TestSequence3()
        {
            IRandomGenerator<uint> random = new LinearIntegerGenerator(1, 65537, 75, 74);
            TestSequence(random, new uint[]
            {
                149, 11249, 57305, 38044, 35283, 24819, 26463, 18689, 25472, 9901, 21742, 57836, 12332,
                7456, 34978, 1944, 14800, 61482, 23634, 3125, 37838, 19833, 45735, 22275, 32274, 61292,
                9384, 48504, 33339, 10093, 36142, 23707, 8600, 55241, 14318, 25332, 64938, 20686, 44173
            });
		}

		[TestMethod]
		public void TestSequence4()
		{
			IRandomGenerator<uint> random = new LinearIntegerGenerator(1, 2147483647, 2147483629, 1);
			TestSequence(random, new uint[]
			{
				2147483630, 307, 2147478122, 99451, 2145693530, 32222107, 1567485722, 1850028063, 1059233219
			});
		}

		[TestMethod]
        public void TestInvalidInit1()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            {
                IRandomGenerator<uint> random = new LinearIntegerGenerator(1, 0, 1, 2);
            });
        }

        [TestMethod]
        public void TestInvalidInit2()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            {
                IRandomGenerator<uint> random = new LinearIntegerGenerator(1, 2, 6, 4);
            });
        }

        private void TestPeriodLength(IRandomGenerator<uint> random, uint expectedPeriod)
        {
            uint period = 0;
            uint startNumber = random.Next();
            uint curNumber = startNumber + 1;

            while (curNumber != startNumber)
            {
                if (period > expectedPeriod)
                {
                    Assert.Fail("Period is longer than expected period.");
                }

                curNumber = random.Next();
                period++;
            }

            if (period < expectedPeriod)
            {
                Assert.Fail("Period is shorter than expected period.");
            }
        }

        private void TestSequence(IRandomGenerator<uint> random, uint[] values)
        {
            uint n = (uint)values.LongLength;

            for (uint i = 0; i < n; ++i)
            {
                uint next = random.Next();
                Assert.AreEqual(values[i], next);
            }
        }
    }
}
