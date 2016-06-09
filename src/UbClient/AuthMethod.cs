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
			//var localPath = baseUri.LocalPath;
			//appName = localPath == "/" ? "/" : localPath.Trim('/').ToLower();

			return new UbAuthentication(login, password, appName);
		}

		static public AuthenticationBase UbIp(string login)
		{
			return new UbIpAuthentication(login);
		}

		// TODO: ???!!
		static public AuthenticationBase Kerberos(string password)
		{
			return new KerberosAuthentication(password);
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