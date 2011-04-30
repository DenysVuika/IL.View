IL.View is an open-source Silverlight .NET assembly browser and decompiler.

Developer quick start
===========================

The solution is configured to use IIS Express (port 55923) by default. 
This can be changed within "IL.View.Web" project settings.

_Note: for the time being "IL.View" expects to be executed as an OOB (Out-of-Browser) application. 
Support for in-browser is under construction._

It is recommended to set "IL.View" silverlight project as the "Default Project" in the solution. 
Debbugging settings can be tuned to automatically start "IL.View" in the OOB mode (pointing to "IL.View.Web" project).

##Configuring remote repositories

There is a simple Remote Repository implementation provided out-of-box. For testing purposes it allows to resolve "mscorlib"
assembly from Silverlight SDK.

To configure default repository launch application, navigate to "Settings"/"Repositories" and add the following address 
as a new repository:

http://localhost:55922/AssemblyRepository.svc/repository

