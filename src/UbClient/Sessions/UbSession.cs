using System;
using System.Linq;
using System.Text;

namespace Softengi.UbClient.Sessions
{
	internal class UbSession
	{
		internal UbSession(UbAuthSchema authSchema, string logonName, string secretWord, string sessionID)
		{
			_authSchema = authSchema;
			LogonName = logonName;

			SessionID = Hexa8(sessionID.Split('+')[0]);

			_sessionWord = sessionID; // sessionID || '+' || privateKey
			_sessionPasswordHash = secretWord;
		}

		public string AuthHeader()
		{
			return _authSchema + " " + Signature();
		}

		public string SessionID { get; }
		 
		protected virtual string Signature()
		{
			var timeStampI = (int) Math.Floor((DateTime.Now - _startTime).TotalSeconds);
			var hexaTime = timeStampI.ToString("x8");

			return SessionID + hexaTime + Crc32(_sessionWord + _sessionPasswordHash + hexaTime).ToString("x8");
		}

		protected string LogonName { get; }

		static private uint Crc32(string value)
		{
			var hash = _crc32Inst
				.ComputeHash(Encoding.Default.GetBytes(value))
				.Reverse()
				.ToArray();
			return BitConverter.ToUInt32(hash, 0);
		}

		static private string Hexa8(string value)
		{
			int num;
			if (!int.TryParse(value, out num))
				throw new ArgumentOutOfRangeException(nameof(value));
			return num.ToString("x8");
		}

		static private readonly DateTime _startTime = new DateTime(1970, 01, 01);
		static private readonly Crc32Algorithm _crc32Inst = new Crc32Algorithm(0x04C11DB7, 0xFFFFFFFF);

		private readonly UbAuthSchema _authSchema;

		private readonly string _sessionPasswordHash;
		private readonly string _sessionWord;
	}
}