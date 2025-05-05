namespace cmd_parser
{
    public class CmdLine
    {
        private readonly string _command;
        public string[] args { get; set; } = new string[0];

        private void AddArgument(ref string? inString)
        {
            args = args.Append(inString).ToArray()!;
            inString = null;
        }

        public CmdLine(string command)
        {
            _command = command;

            // Cut line in args
            string? currentString = null;

            for (int i = 0; i < _command.Length; i++)
            {
                if (_command[i] != ' ')
                {
                    currentString = currentString == null ? _command[i].ToString() : currentString + _command[i];
                }
                else if (currentString != null)
                {
                    AddArgument(ref currentString);
                }
                if (currentString != null && i + 1 == _command.Length)
                {
                    AddArgument(ref currentString);
                }
            }
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var inputLine = Console.ReadLine();
            if (inputLine != null)
            {
                Console.WriteLine($"line is \"{inputLine}\"");
            }
            else
            {
                return;
            }

            CmdLine cmdLine = new CmdLine(inputLine);

            Console.WriteLine($"Found {cmdLine.args.Length} arguments: ");
            foreach (var arg in cmdLine.args)
            {
                Console.WriteLine($"\"{arg}\"");
            }
        }
    }
}
