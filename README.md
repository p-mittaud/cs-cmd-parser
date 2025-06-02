# cs-cmd-parser
A project of a single file to parse your C# command lines.

## How to use
Use one of the constructor of the CmdLine class which takes an string or an array of string.
The command is then cut in different parts. If a CmdConfig has been set, you can easily get parameters of your options.

### Notes
If an error occurs, an exception will be thrown with more details.


### Example
```
CmdConfig[] config = new CmdConfig[] { new CmdConfig { name = "--cmd", hasArgs = false, } };
CmdLine cmdLine = new CmdLine(args, config);
Console.WriteLine(cmdLine.ToString());
```