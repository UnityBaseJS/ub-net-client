using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Softengi.UbClient.Sessions
{
	public sealed class Crc32Algorithm : HashAlgorithm
	{
		public Crc32Algorithm() : this(DEFAULT_POLYNOMIAL, DEFAULT_SEED)
		{}

		public Crc32Algorithm(uint polynomial, uint seed)
		{
			_table = InitializeTable(polynomial);
			_seed = _hash = seed;
		}

		public override void Initialize()
		{
			_hash = _seed;
		}

		static public uint Compute(byte[] buffer)
		{
			return ~CalculateHash(InitializeTable(DEFAULT_POLYNOMIAL), DEFAULT_SEED, buffer, 0, buffer.Length);
		}

		static public uint Compute(uint seed, byte[] buffer)
		{
			return ~CalculateHash(InitializeTable(DEFAULT_POLYNOMIAL), seed, buffer, 0, buffer.Length);
		}

		static public uint Compute(uint polynomial, uint seed, byte[] buffer)
		{
			return ~CalculateHash(InitializeTable(polynomial), seed, buffer, 0, buffer.Length);
		}

		public override int HashSize => 32;

		protected override void HashCore(byte[] buffer, int start, int length)
		{
			_hash = CalculateHash(_table, _hash, buffer, start, length);
		}

		protected override byte[] HashFinal()
		{
			return HashValue = UintToBigEndianBytes(~_hash);
		}

		static private uint[] InitializeTable(uint polynomial)
		{
			if (_tables.ContainsKey(polynomial))
				return _tables[polynomial];

			var table = new uint[256];
			for (var i = 255; i >= 0; i--)
			{
				var c = Reverse((uint) i, 32);
				for (var j = 0; j < 8; j++)
					c = ((c*2) ^ ((c >> 31)%2*polynomial)) >> 0;
				table[i] = Reverse(c, 32);
			}

			return _tables[polynomial] = table;
		}

		static private uint Reverse(uint x, uint n)
		{
			uint b = 0;
			while (n != 0)
			{
				b = b*2 + x%2;
				x /= 2;
				x -= x%1;
				n--;
			}
			return b;
		}

		static private uint CalculateHash(uint[] table, uint seed, byte[] buffer, int start, int size)
		{
			var crc = seed;
			for (var i = start; i < size - start; i++)
				crc = (crc >> 8) ^ table[buffer[i] ^ crc & 0xff];
			return crc;
		}

		static private byte[] UintToBigEndianBytes(uint uint32)
		{
			var result = BitConverter.GetBytes(uint32);
			if (BitConverter.IsLittleEndian)
				Array.Reverse(result);
			return result;
		}

		private const uint DEFAULT_POLYNOMIAL = 0x04C11DB7;
		private const uint DEFAULT_SEED = 0xffffffff;

		static private readonly Dictionary<uint, uint[]> _tables = new Dictionary<uint, uint[]>();

		private readonly uint _seed;
		private readonly uint[] _table;
		private uint _hash;
	}
}