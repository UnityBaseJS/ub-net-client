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

Connect and output list of users to console:
```C#
	var cn = new UbConnection(new Uri("http://localhost:888/"), AuthMethod.Ub("user", "password"));
	var users = cn.Select("uba_user", new[] {"ID", "name"});
	foreach (var user in users)
	{
		Console.WriteLine($"{user["ID"]} {user["name"]}");
	}
```
