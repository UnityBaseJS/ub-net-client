using System;

namespace Softengi.UbClient
{
	public enum UbAuthSchema
	{
		/// <summary>
		/// Login + Password
		/// </summary>
		UB,

		/// <summary>
		/// No password, if request comes from the trusted IP, log in successfully.
		/// </summary>
		UBIP,

		/// <summary>
		/// Kerberos
		/// </summary>
		Negotiate,

		/// <summary>
		/// Not supported yet at .Net client.
		/// </summary>
		Cert,

		/// <summary>
		/// Not supported yet at .Net client.
		/// </summary>
		Basic
	}
}