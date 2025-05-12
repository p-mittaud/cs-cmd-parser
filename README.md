# cs-cmd-parser
A project used to parse command lines in C#.

## How to use
Use one of the constructor of the CmdLine class:
`CmdLine(string command, CmdConfig[]? inCmdConfig = null, bool bContainsCmd = true)`
if you want to parse a string, or
`CmdLine(string[] inArgs, CmdConfig[]? cmdConfig = null, bool bContainsCmd = true)`
if you want to parse a string array.

`CmdConfig` is a record used to parse the options of your command line. You can only setup options at construction.
If `bContainsCmd` is true, the first word of the input will be considered as the main command.

Once the CmdLine is created, you can access the main command, cmdOptions and their values and the base arguments of the command.

## Coming soon
- [ ] Add of exception to throw errors
- [ ] Removal of defaults options prefixes
- [ ] Add of a license
