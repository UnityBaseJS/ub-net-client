using System.Configuration;

namespace Softengi.UbClient.Configuration
{
	public class UnityBaseConnectionConfiguration : ConfigurationElement
	{
		[ConfigurationProperty("baseUri", DefaultValue = "http://localhost:888", IsRequired = true)]
		[RegexStringValidator(@"^(https?:\/\/)?([\da-z\.-]+)\.([a-z\.]{2,6})([\/\w \.-]*)*\/?$")]
		public string BaseUri => (string) base["baseUri"];

		[ConfigurationProperty("authenticationMethod", DefaultValue = "admin", IsRequired = true)]
		[StringValidator(MinLength = 2)]
		[RegexStringValidator("^(ub|ubip|negotiate)$")]
		public string AuthenticationMethod => (string) base["authenticationMethod"];

		[ConfigurationProperty("userName")]
		public string UserName => (string) base["userName"];

		[ConfigurationProperty("password")]
		public string Password => (string) base["password"];
	}
}