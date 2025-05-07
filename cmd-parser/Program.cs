namespace cmd_parser
{
    /// <summary>
    /// Holds the configuration of a single command
    /// Contains the description, if the command has args, how many, ...
    /// </summary>
    public class CmdConfig
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

    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Program entry point!");
            CmdLine startCmd = new CmdLine(args);
            Console.WriteLine($"Found {startCmd.args.Length} arguments in startCmd: ");
            foreach (var arg in startCmd.args)
            {
                Console.WriteLine($"{arg}");
            }

            var inputLine = Console.ReadLine();
            if (inputLine == null)
            {
                return;
            }

            CmdConfig[] config = new CmdConfig[]
            {
                new CmdConfig
                {
                    name = "--test",
                    description = "The first test option.",
                    hasArgs = false,
                },
                new CmdConfig
                {
                    name = "--help",
                    description = "Prints the help on all other commands.",
                    hasArgs = false,
                },
                new CmdConfig
                {
                    name = "--int-input",
                    description = "Prints the help on all other commands.",
                    hasArgs = true,
                    argCount = 3,
                },
                new CmdConfig
                {
                    name = "--int-input-invalid",
                    description = "Prints the help on all other commands.",
                    hasArgs = true,
                }
            };

            CmdLine cmdLine = new CmdLine(inputLine, config);

            Console.WriteLine($"Found {cmdLine.args.Length} arguments: ");
            foreach (var arg in cmdLine.args)
            {
                Console.WriteLine($"{arg}");
            }
            Console.WriteLine($"Found {cmdLine.optionsCmd.Length} options: ");
            foreach (var option in cmdLine.optionsCmd)
            {
                Console.WriteLine($"{option}");
            }
        }
    }
}
