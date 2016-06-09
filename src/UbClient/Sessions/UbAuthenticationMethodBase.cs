namespace Softengi.UbClient.Sessions
{
	public abstract class UbAuthenticationMethodBase
	{
		internal abstract UbSession Authenticate(UbTransport transport);
	}
}