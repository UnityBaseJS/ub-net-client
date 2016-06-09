namespace Softengi.UbClient.Sessions
{
	public abstract class AuthenticationBase
	{
		internal abstract void Authenticate(UbTransport transport);
		internal abstract string AuthHeader();
	}
}