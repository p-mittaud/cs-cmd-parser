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
        public int? argCount { get; set; }
    }

    public class CmdLine
    {
        public string[] args { get; set; } = new string[0];

        public Dictionary<string, string[]> cmdOptions { get; set; } = new Dictionary<string, string[]>();

        public readonly CmdConfig[] _cmdConfig;

        private void AddArg(ref string? inString)
        {
            args = args.Append(inString).ToArray()!;
            inString = null;
        }

        public CmdLine(string command, CmdConfig[]? inCmdConfig = null)
        {
            // Cut line in args
            string? currentString = null;
            bool bInQuotes = false;

            for (int i = 0; i < command.Length; i++)
            {
                if (command[i] == '"')
                {
                    if (bInQuotes)
                    {
                        AddArg(ref currentString);
                    }
                    bInQuotes = !bInQuotes;
                    continue;
                }
                else if (bInQuotes || command[i] != ' ')
                {
                    currentString = currentString == null ? command[i].ToString() : currentString + command[i];
                }
                else if (currentString != null)
                {
                    AddArg(ref currentString);
                }
                if (currentString != null && i + 1 == command.Length)
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
            args = inArgs;

            _cmdConfig = cmdConfig ?? new CmdConfig[0];

            CreateOptionsCmd();
        }

        private void CreateOptionsCmd()
        {
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
            return cmdOptions.ContainsKey(inOption);
        }

        public T[]? GetOptionValues<T>(string inOption)
        {
            if (!cmdOptions.ContainsKey(inOption))
            {
                return null;
            }

            T[] optionValues = new T[0];

            foreach (var arg in cmdOptions[inOption])
            {
                try
                {
                    var convertedArg = (T)Convert.ChangeType(arg, typeof(T));
                    if (convertedArg != null)
                    {
                        optionValues = optionValues.Append(convertedArg).ToArray();
                    }
                    else
                    {
                        Console.WriteLine($"Failed to convert {arg} to type {typeof(T).FullName}");
                        return null;
                    }
                }
                catch (System.FormatException)
                {
                    Console.WriteLine($"Failed to convert {arg} to type {typeof(T).FullName}");
                    return null;
                }
            }

            return optionValues;
        }
    }
}