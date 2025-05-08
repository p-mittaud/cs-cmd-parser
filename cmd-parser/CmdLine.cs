namespace cmd_parser
{
    /// <summary>
    /// Holds the configuration of a single command
    /// Contains the description, if the command has args, how many, ...
    /// </summary>
    public record CmdConfig
    {
        public string name { get; set; } = default!;
        public string? description { get; set; }
        public bool hasArgs { get; set; }
        // TODO: for now, argCount is absolute
        public int? argCount { get; set; }
    }

    public class CmdLine
    {
        private readonly string _command;
        public string[] args { get; set; } = new string[0];

        public string[] optionsCmd { get; set; } = new string[0];

        public Dictionary<string, string[]> cmdOptions { get; set; } = new Dictionary<string, string[]>();

        public readonly CmdConfig[] _cmdConfig;

        private void AddArg(ref string? inString)
        {
            args = args.Append(inString).ToArray()!;
            inString = null;
        }

        private void CreateOptionsCmd()
        {
            // TODO: read from config existing option possibilities: for now, if arg was read from a quote string, it is considered as an option if it contains the "--" start

            // TODO: add "isolated-args": arguments which are not bound to a command

            int expectedArgCount = 0;
            string? currentOption = null;
            foreach (var arg in args)
            {
                if (expectedArgCount > 0 && currentOption != null)
                {
                    cmdOptions[currentOption] = cmdOptions[currentOption].Append(arg).ToArray();
                    if (--expectedArgCount == 0)
                    {
                        currentOption = null;
                    }
                }
                else if (arg.StartsWith("--") && !arg.Contains(" "))
                {
                    optionsCmd = optionsCmd.Append(arg).ToArray();

                    var cmdOption = Array.Find(_cmdConfig, elem => elem.name == arg);
                    if (cmdOption != null)
                    {
                        if (cmdOptions.ContainsKey(cmdOption.name))
                        {
                            Console.WriteLine($"Warning: command {cmdOption.name} has already been added to args!");
                            continue;
                        }

                        cmdOptions.Add(cmdOption.name, new string[0]);
                        if (cmdOption.hasArgs && cmdOption.argCount != null)
                        {
                            currentOption = cmdOption.name;
                            expectedArgCount = cmdOption.argCount.Value;
                        }
                        else if (cmdOption.hasArgs)
                        {
                            Console.WriteLine($"CmdOption {cmdOption.name} is set to hasArgs = true but argCount is null");
                        }
                    }
                }
            }
        }

        public bool ContainsOption(string inOption)
        {
            return optionsCmd.Contains(inOption);
        }

        public CmdLine(string command, CmdConfig[]? inCmdConfig = null)
        {
            _command = command;

            // Cut line in args
            string? currentString = null;
            bool bInQuotes = false;

            for (int i = 0; i < _command.Length; i++)
            {
                if (_command[i] == '"')
                {
                    if (bInQuotes)
                    {
                        AddArg(ref currentString);
                    }
                    bInQuotes = !bInQuotes;
                    continue;
                }
                else if (bInQuotes || _command[i] != ' ')
                {
                    currentString = currentString == null ? _command[i].ToString() : currentString + _command[i];
                }
                else if (currentString != null)
                {
                    AddArg(ref currentString);
                }
                if (currentString != null && i + 1 == _command.Length)
                {
                    AddArg(ref currentString);
                }
            }

            if (bInQuotes)
            {
                // throw exception, invalid commandLine
                Console.Error.WriteLine("Invalid Command received in CommandLine!");
            }

            _cmdConfig = inCmdConfig ?? new CmdConfig[0];

            CreateOptionsCmd();
        }

        public CmdLine(string[] inArgs, CmdConfig[]? cmdConfig = null)
        {
            _command = String.Empty;
            args = inArgs;

            _cmdConfig = cmdConfig ?? new CmdConfig[0];

            CreateOptionsCmd();
        }
    }
}