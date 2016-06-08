namespace Softengi.UbClient.Sessions
{
	internal class UbNegotiateSession : UbSession
	{
		internal UbNegotiateSession(string logonName, string sessionID) :
			base(UbAuthSchema.Negotiate, logonName, logonName, sessionID)
		{}
	}
}