using System.Configuration;

namespace Softengi.UbClient.Configuration
{
	public class UnityBaseConfigurationSection : ConfigurationSection
	{
		[ConfigurationProperty("connection")]
		public UnityBaseConnectionConfiguration Connection
		{
			get { return (UnityBaseConnectionConfiguration) this["connection"]; }
			set { this["connection"] = value; }
		}
	}
}