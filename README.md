Disk Report System
==================

Initial Comments
----------------

 This system uses NUnit tested re-usable code to fetch sizes of disks on servers found in TSM and VMWare. See NUnitTests folder for available tests. Both these sources of data are organized as plugin code for the option of adding more plugins later. The code can be compiled as a console application and used with command line arguments, or it can be compiled as an dll and the code re-used to fetch servers represented as objects for usage in your own system.


Prerequisites
-------------
For the full functionality of the code, the following prerequisites must be in order:

	1. .Net 4.5
	2. Configuration files 
	3. TSM AdminClient(if you are not running in Debug mode)
	4. [Kraggs.TSM.Utils](https://github.com/raggsokk/Kraggs.TSM.Utils)
	5. Reference compiled dlls from [VMwareChatter] (https://github.com/itnifl/VMwareChatter)
	
Also NUnit with Monodevelop is being used, I have no experience if it might work or not out of the box if you are using some other IDE.
In Visual Studio this can be fixed using Nuget and the command: Install-Package NUnit -Version 2.6.4

* To fetch nodes from TSM, a TSM AdminClient needs to be installed.
* Also for communication with TSM servers, [Kraggs.TSM.Utils](https://github.com/raggsokk/Kraggs.TSM.Utils) has to be a part of the project at compile time and placed with the executable when run.
* The following is not a requirements: Microsoft Excel needs to be installed on the computer running this solution in order to generate Excel reports if the CreateExcelMSDoc class is going to be used.
* NPOI is included in the project so that you can generate Excel reports without having Excel installed.
* Configuration files for what vCenter, API and TSM server to talk to, and they credentials that are needed. Examples are found in the debug directory, and in the directory config&Dependencies. These files need to be configured correctly in the same folder as the main dll or executeable (depending on how it this solution has been compiled) and placed with the executable when it is run.
* Needed dll's for talking to a vCenter server are included in this project, but these are also automatically installed in the GAC if PowerCli is installed on the system.


Installing Prerequisites
------------------------
See corresponding guides for the various prerequisites. These guides are not provided here.

Installing
----------
* Compile as console application and run the exe. It will inform you of the command line arguments you can give. 
* Compile as dll and attach to your project to re-use code.

Upgrading
---------
* Pull from repository and compile again.

Removing
--------
Stop the application.

Delete the top level directory of the application.

How To Re-Use the Code
----------------------
Compile as dll and include in your project. 

There is one method TsmMethods named GetAllNodesData, and one in class VmMethods named GetAllNodesData. Each class represents a communication plugin. Each fetch all nodes/guests from tsm/vmware and return them as objects with a list of all found instances. These methods talk to the tsm or vCenter servers based on the configuration file given as a string name argument to the methods. The signatures are:
```
public T1 GetAllNodesData<T1, T2>(string sourceConfigFileName, string nameFilter, out List<Exception> outExceptions) where T1 : IComNodeList<T2>, new() 
			where T2 : IComNode, new()
```
Use intellisense in Visual Studio to find the properties and methods of the objects returned, or browse to them in the source code.

Thanks to
---------
* dotnetfollower.com (I modified your code) - http://dotnetfollower.com/wordpress/2012/03/c-simple-command-line-arguments-parser/
* NPOI - https://npoi.codeplex.com/

Maintainers
-----------
Current maintainers of the Disk Report System:

Atle Holm (atle@team-holm.net)
