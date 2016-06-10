using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Softengi.UbClient.Sessions
{
	static public class Crypto
	{
		static public string Nsha256(string value)
		{
			return string.Join(
				string.Empty,
				SHA256
					.Create()
					.ComputeHash(Encoding.Default.GetBytes(value))
					.Select(b => b.ToString("x2"))
				);
		}

		static public string Signature(string sessionID, string sessionWord, string sessionPasswordHash)
		{
			var timeStampI = (int) Math.Floor((DateTime.Now - _startTime).TotalSeconds);
			var hexaTime = timeStampI.ToString("x8");

			return sessionID + hexaTime + Crc32(sessionWord + sessionPasswordHash + hexaTime).ToString("x8");
		}

		static public uint Crc32(string value)
		{
			var hash = _crc32Inst
				.ComputeHash(Encoding.Default.GetBytes(value))
				.Reverse()
				.ToArray();
			return BitConverter.ToUInt32(hash, 0);
		}

		static public string Hexa8(string value)
		{
			int num;
			if (!int.TryParse(value, out num))
				throw new ArgumentOutOfRangeException(nameof(value));
			return num.ToString("x8");
		}

		static private readonly DateTime _startTime = new DateTime(1970, 01, 01);

		static private readonly Crc32Algorithm _crc32Inst = new Crc32Algorithm(0x04C11DB7, 0xFFFFFFFF);
	}
}