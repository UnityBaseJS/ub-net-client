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
		static public UbAuthenticationMethodBase Ub(string login, string password, string appName = "/")
		{
			//var localPath = baseUri.LocalPath;
			//appName = localPath == "/" ? "/" : localPath.Trim('/').ToLower();

			return new UbAuthenticationMethod(login, password, appName);
		}

		static public UbAuthenticationMethodBase UbIp(string login)
		{
			return new UbIpAuthenticationMethod(login);
		}

		static public UbAuthenticationMethodBase Negotiate()
		{
			return new UbNegotiateAuthenticationMethod();
		}
	}
}