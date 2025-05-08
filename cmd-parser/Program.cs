namespace cmd_parser
{
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
