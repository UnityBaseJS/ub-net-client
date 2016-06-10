UnityBase .Net Client
=====================

This project is a .Net Client for [UnityBase](https://unitybase.info/) application server.
It allows to build .Net application which may perform the following operations:
* Authenticate
* Query and update entity data
* Execute custom entity methods
* Work with documents.

The project is available as [NuGet packet](https://www.nuget.org/packages/Softengi.UbClient/).

Samples
=======

Connect and output list of users to console:
```C#
	var cn = new UbConnection(new Uri("http://localhost:888/"), AuthMethod.Ub("user", "password"));
	var users = cn.Select("uba_user", new[] {"ID", "name"});
	foreach (var user in users)
	{
		Console.WriteLine($"{user["ID"]} {user["name"]}");
	}
```
