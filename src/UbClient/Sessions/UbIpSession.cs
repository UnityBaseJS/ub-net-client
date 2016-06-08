namespace Softengi.UbClient.Sessions
{
	internal class UbIpSession : UbSession
	{
		internal UbIpSession(string logonName, string secretWord, string sessionID) :
			base(UbAuthSchema.UBIP, logonName, secretWord, sessionID)
		{}

		protected override string Signature()
		{
			return LogonName;
		}
	}
}