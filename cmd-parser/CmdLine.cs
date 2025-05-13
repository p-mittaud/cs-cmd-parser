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
        public readonly string[] args = new string[0];

        public readonly string? cmd;

        public readonly string[] baseArgs = new string[0];

        public readonly Dictionary<string, string[]> cmdOptions = new Dictionary<string, string[]>();

        public readonly CmdConfig[] cmdConfigs;

        private string[] AddArg(ref string? inString)
        {
            string[] resString = args.Append(inString).ToArray()!;
            inString = null;
            return resString;
        }

        public CmdLine(string command, CmdConfig[]? cmdConfigArray = null, bool bContainsCmd = true)
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
                        args = AddArg(ref currentString);
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
                    args = AddArg(ref currentString);
                }
                if (currentString != null && i + 1 == command.Length)
                {
                    args = AddArg(ref currentString);
                }
            }

            if (bInQuotes)
            {
                // TODO: throw exception, invalid commandLine
                Console.Error.WriteLine("Invalid Command received in CommandLine!");
            }

            if (bContainsCmd)
            {
                cmd = args[0];
            }

            cmdConfigs = ValidateCmdConfigArray(cmdConfigArray) ?? new CmdConfig[0];

            CreateOptionsCmd();
        }

        public CmdLine(string[] inArgs, CmdConfig[]? cmdConfigArray = null, bool bContainsCmd = true)
        {
            args = inArgs;

            if (bContainsCmd)
            {
                cmd = args[0];
            }

            cmdConfigs = ValidateCmdConfigArray(cmdConfigArray) ?? new CmdConfig[0];

            CreateOptionsCmd();
        }

        private CmdConfig[]? ValidateCmdConfigArray(CmdConfig[]? cmdConfigArray)
        {
            if (cmdConfigArray == null)
            {
                return null;
            }

            // Check if all cmdConfig in cmdConfigArray don't share the same name
            // If there is: throw exception

            foreach (var cmdConfig in cmdConfigArray)
            {
                if (String.IsNullOrEmpty(cmdConfig.name) || cmdConfig.name.Contains(" "))
                {
                    // TODO: throw exception: invalid cmdConfig
                    Console.Error.WriteLine("Received invalid cmdConfig: Invalid name!");
                    return null;
                }
                if (cmdConfig.hasArgs && (cmdConfig.argCount == null || cmdConfig.argCount <= 0))
                {
                    // TODO: throw exception: invalid cmdConfig
                    Console.Error.WriteLine($"Received invalid cmdConfig: Invalid argCount for cmd {cmdConfig.name}!");
                    return null;
                }
            }
            return cmdConfigArray;
        }

        private string[] CreateOptionsCmd()
        {
            string[] foundBaseArgs = new string[0];

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
                    continue;
                }
                else if (!arg.Contains(" "))
                {
                    var cmdOption = Array.Find(cmdConfigs, elem => elem.name == arg);
                    if (cmdOption != null)
                    {
                        if (cmdOptions.ContainsKey(cmdOption.name))
                        {
                            // TODO: throw exception if cmdOption already contains the key
                            Console.WriteLine($"Warning: command {cmdOption.name} has already been added to args!");
                        }
                        else
                        {
                            cmdOptions.Add(cmdOption.name, new string[0]);
                            if (cmdOption.hasArgs)
                            {
                                currentOption = cmdOption.name;
                                expectedArgCount = cmdOption.argCount!.Value;
                            }
                        }
                        continue;
                    }
                }
                foundBaseArgs = foundBaseArgs.Append(arg).ToArray();
            }
            return foundBaseArgs;
        }

        public bool ContainsOption(string inOption)
        {
            return cmdOptions.ContainsKey(inOption);
        }

        /// <summary>
        /// Returns values associated to a command option.
        /// </summary>
        /// <typeparam name="T">The type to convert found options.</typeparam>
        /// <param name="inOption">Option name to find.</param>
        /// <returns>Found options or null if empty.</returns>
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
                    // TODO: Create or throw Exception: Invalid format type
                    Console.WriteLine($"Failed to convert {arg} to type {typeof(T).FullName}");
                    return null;
                }
            }

            return optionValues;
        }
    }
}