using System;

namespace lexLinearRandomGenerator
{
	public class LinearIntegerGenerator : IRandomGenerator<uint>
	{
		private uint state;

		public LinearIntegerGenerator(uint initialState, uint m, uint a, uint c)
		{
			M = m;
			A = a;
			C = c;

			AssertInRange(initialState, 1, M);
			this.state = initialState;
		}

		private uint _m;
		public uint M
		{
			get => _m;
			set
			{
				AssertInRange(value, 1, UInt32.MaxValue);
				_m = value;
			}
		}

		private uint _a;
		public uint A
		{
			get => _a;
			set
			{
				AssertInRange(value, 0, M - 1);
				_a = value;
			}
		}

		private uint _c;
		public uint C
		{
			get => _c;
			set
			{
				AssertInRange(value, 0, M - 1);
				_c = value;
			}
		}

		public uint Next()
		{
			ulong next = (ulong)A * state + C;
			// convert to long to avoid overflow
			state = (uint)(next % M);
			return state;
		}

		private void AssertInRange(uint x, uint min, uint max)
		{
			if (x < min || x > max)
			{
				throw new ArgumentOutOfRangeException();
			}
		}
	}
}
