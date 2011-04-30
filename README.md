IL.View is an open-source Silverlight .NET assembly browser and decompiler.

Developer quick start
===========================

The solution is configured to use IIS Express (port 55923) by default. 
This can be changed within "IL.View.Web" project settings.

When running in Out-of-Browser (OOB) mode IL.View caches all the assemblies that were dropped from the desktop and/or resolved during decompilation.

When running in the In-Browser mode IL.View supports only in-memory caching due to the Silverlight security restrictions.

##Configuring remote repositories

There is a simple Remote Repository implementation provided out-of-box. For testing purposes it allows to resolve "mscorlib"
assembly from Silverlight SDK.

To configure default repository launch application, navigate to "Settings"/"Repositories" and add the following address 
as a new repository:

http://localhost:55923/AssemblyRepository.svc/repository

