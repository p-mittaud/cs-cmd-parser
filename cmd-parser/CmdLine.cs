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

        public string? cmd { get; set; }

        public string[] baseArgs { get; set; } = new string[0];

        public Dictionary<string, string[]> cmdOptions { get; set; } = new Dictionary<string, string[]>();

        public readonly CmdConfig[] _cmdConfig;

        private void AddArg(ref string? inString)
        {
            args = args.Append(inString).ToArray()!;
            inString = null;
        }

        public CmdLine(string command, CmdConfig[]? inCmdConfig = null, bool bContainsCmd = true)
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

            if (bContainsCmd)
            {
                cmd = args[0];
            }

            _cmdConfig = ValidateCmdConfigArray(inCmdConfig) ?? new CmdConfig[0];

            CreateOptionsCmd();
        }

        public CmdLine(string[] inArgs, CmdConfig[]? cmdConfig = null, bool bContainsCmd = true)
        {
            args = inArgs;

            if (bContainsCmd)
            {
                cmd = args[0];
            }

            _cmdConfig = ValidateCmdConfigArray(cmdConfig) ?? new CmdConfig[0];

            CreateOptionsCmd();
        }

        private CmdConfig[]? ValidateCmdConfigArray(CmdConfig[]? cmdConfigArray)
        {
            if (cmdConfigArray == null)
            {
                return null;
            }

            foreach (var cmdConfig in cmdConfigArray)
            {
                if (String.IsNullOrEmpty(cmdConfig.name) || cmdConfig.name.Contains(" "))
                {
                    Console.Error.WriteLine("Received invalid cmdCondif: Invalid name!");
                    return null;
                }
                if (cmdConfig.hasArgs && (cmdConfig.argCount == null || cmdConfig.argCount <= 0))
                {
                    Console.Error.WriteLine($"Received invalid cmdCondif: Invalid argCount for cmd {cmdConfig.name}!");
                    return null;
                }
            }
            return cmdConfigArray;
        }

        private void CreateOptionsCmd()
        {
            int expectedArgCount = 0;
            string? currentOption = null;
            foreach (var arg in args.Skip(cmd != null ? 1 : 0))
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
                    else
                    {
                        baseArgs = baseArgs.Append(arg).ToArray();
                    }
                }
                else
                {
                    baseArgs = baseArgs.Append(arg).ToArray();
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