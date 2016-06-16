UnityBase .Net Client
=====================

This project is a .Net Client for [UnityBase](https://unitybase.info/) application server.

It allows to build .Net application which may perform the following operations:
* Authenticate
* Query and update entity data
* Execute custom entity methods
* Work with documents.

Connection to UnityBase server from .Net managed code could be useful for scenarios like:
* Create a backend service, like integration agent
* Create a command line utility
* Create a Windows GUI which needs exchange data with UnityBase
* Connect an existing .Net application with UnityBase

The project is available as [NuGet packet](https://www.nuget.org/packages/Softengi.UbClient/).

Samples
=======

Hello World
-----------

Connect and output list of users to console:
```C#
	var cn = new UbConnection(new Uri("http://localhost:888/"), AuthMethod.Ub("user", "password"));
	var users = cn.Select("uba_user", new[] {"ID", "name"});
	foreach (var user in users)
	{
		Console.WriteLine($"{user["ID"]} {user["name"]}");
	}
```

Specifying connection parameters in config file
-----------------------------------------------

Put the following in `app.config` file:
```XML
<configuration>
	<configSections>
		<section name="unityBase"
		         type="Softengi.UbClient.Configuration.UnityBaseConfigurationSection, Softengi.UbClient" />
	</configSections>

	<unityBase>
		<connection
			baseUri="http://localhost:888/"
			authenticationMethod="ub"
			userName="admin"
			password="admin"
		/>
	</unityBase>

	...
</configuration>
```

Than in code, create `UbConnection` instance:
```C#
var ubConfig = (UnityBaseConfigurationSection) ConfigurationManager.GetSection("unityBase");
var ubConnection = new UbConnection(ubConfig.Connection);
```

Using MEF together with UB client
---------------------------------

Put the following class in a place where MEF composition container could reach it:
```C#
internal sealed class UbConnectionFactory
{
	[Export]
	public UbConnection Connection
	{
		get
		{
			if (_connection != null) return _connection;

			lock (_sync)
			{
				if (_connection == null)
				{
					var ubConfig = (UnityBaseConfigurationSection) ConfigurationManager.GetSection("unityBase");
					_connection = new UbConnection(ubConfig.Connection);
				}
			}

			return _connection;
		}
	}

	private volatile UbConnection _connection;
	static private readonly object _sync = new object();
}
```

Than in any composable part, it would be possible to just declare import:
```C#
[Export]
public class MyService
{
	[Import]
	public UbConnection UbConnection { get; set; }

	...
}
```
