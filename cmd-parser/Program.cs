namespace cmd_parser
{
    public class CmdLine
    {
        private readonly string _command;
        public string[] args { get; set; } = new string[0];

        public string[] optionsCmd { get; set; } = new string[0];

        private void AddArg(ref string? inString)
        {
            args = args.Append(inString).ToArray()!;
            inString = null;
        }

        private void CreateOptionsCmd()
        {
            // TODO: read from config existing option possibilities: for now, if arg was read from a quote string, it is considered as an option if it contains the "--" start
            foreach (var arg in args)
            {
                if (arg.StartsWith("--") && !arg.Contains(" "))
                {
                    optionsCmd = optionsCmd.Append(arg).ToArray();
                }
            }
        }

        public bool ContainsOption(string inOption)
        {
            return optionsCmd.Contains(inOption);
        }

        public CmdLine(string command)
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

            CreateOptionsCmd();
        }

        public CmdLine(string[] inArgs)
        {
            _command = String.Empty;
            args = inArgs;

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

            CmdLine cmdLine = new CmdLine(inputLine);

            Console.WriteLine($"Found {cmdLine.args.Length} arguments: ");
            foreach (var arg in cmdLine.args)
            {
                Console.WriteLine($"{arg}");
            }
            foreach (var option in cmdLine.optionsCmd)
            {
                Console.WriteLine($"{option}");
            }
        }
    }
}
