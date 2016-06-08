using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Softengi.UbClient
{
	static internal class CryptoHelper
	{
		static internal string Nsha256(string value)
		{
			return string.Join(
				string.Empty,
				SHA256
					.Create()
					.ComputeHash(Encoding.Default.GetBytes(value))
					.Select(b => b.ToString("x2"))
				);
		}
	}
}