namespace cmd_parser
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter a command line: ");
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

            CmdLine cmdLine = new CmdLine(inputLine, config, bContainsCmd:false);

            foreach (var pair in cmdLine.cmdOptions)
            {
                Console.WriteLine($"key: {pair.Key}");
                foreach (var value in pair.Value)
                {
                    Console.WriteLine($"\t{value}");
                }
            }
            if (cmdLine.cmd != null)
            {
                Console.WriteLine($"Cmd: {cmdLine.cmd}");
            }
            if (cmdLine.baseArgs.Length != 0)
            {
                string tmpStr = cmdLine.baseArgs[0];
                for (int i = 1; i < cmdLine.baseArgs.Length; i++)
                {
                    tmpStr += " " + cmdLine.baseArgs[i];
                }
                Console.WriteLine($"Found args: {tmpStr}");
            }

            var values = cmdLine.GetOptionValues<DateTime>("--int-input");
            if (values != null)
            {
                foreach (var value in values)
                {
                    Console.WriteLine(value);
                }
            }
        }
    }
}
