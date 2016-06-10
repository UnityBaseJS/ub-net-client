using Softengi.UbClient.Sessions;

namespace Softengi.UbClient
{
	static public class AuthMethod
	{
		/// <summary>
		/// Create UB authentication method.
		/// </summary>
		/// <param name="login">User login.</param>
		/// <param name="password">User password.</param>
		/// <param name="appName">Name of UB app, or "/", if UB hosted at root.</param>
		static public AuthenticationBase Ub(string login, string password, string appName = "/")
		{
			return new UbAuthentication(login, password, appName);
		}

		static public AuthenticationBase UbIp(string login)
		{
			return new UbIpAuthentication(login);
		}

		static public AuthenticationBase Kerberos()
		{
			return new KerberosAuthentication();
		}

		/*

		/// <summary>
		/// Not supported yet at .Net client.
		/// </summary>
		Cert,

		/// <summary>
		/// Not supported yet at .Net client.
		/// </summary>
		Basic
		 */
	}
}