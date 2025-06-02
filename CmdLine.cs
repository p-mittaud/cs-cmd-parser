namespace cmd_parser
{
    public class ArgumentInvalidException : ArgumentException
    {
        public ArgumentInvalidException() { }
        public ArgumentInvalidException(string? paramName, string? message)
            : base(paramName, message) { }
    }

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
                throw new ArgumentInvalidException(nameof(command), $"command contains an invalid number of quotes character!");
            }

            if (bContainsCmd)
            {
                cmd = args[0];
            }

            cmdConfigs = ValidateCmdConfigArray(cmdConfigArray) ?? new CmdConfig[0];

            baseArgs = CreateOptionsCmd();
        }

        public CmdLine(string[] inArgs, CmdConfig[]? cmdConfigArray = null, bool bContainsCmd = true)
        {
            args = inArgs;

            if (bContainsCmd)
            {
                cmd = args[0];
            }

            cmdConfigs = ValidateCmdConfigArray(cmdConfigArray) ?? new CmdConfig[0];

            baseArgs = CreateOptionsCmd();
        }

        private CmdConfig[]? ValidateCmdConfigArray(CmdConfig[]? cmdConfigArray)
        {
            if (cmdConfigArray == null)
            {
                return null;
            }

            foreach (var cmdConfig in cmdConfigArray)
            {
                if (cmdConfig.name == null)
                {
                    throw new ArgumentNullException(nameof(cmdConfig.name), "A CmdConfig contains a null name!");
                }
                else if (cmdConfig.name.Length == 0)
                {
                    throw new ArgumentInvalidException(nameof(cmdConfig.name), "A CmdConfig contains an empty name!");
                }
                else if (cmdConfig.name.Contains(" "))
                {
                    throw new ArgumentInvalidException(nameof(cmdConfig.name), $"A CmdConfig contains an name with a space: \"{cmdConfig.name}\"!");
                }
                else if (cmdConfigArray.Count(elem => elem.name == cmdConfig.name) != 1)
                {
                    throw new ArgumentInvalidException(nameof(cmdConfig.name), $"cmdConfigArray contains multiple times the name {cmdConfig.name}!");
                }

                if (cmdConfig.hasArgs)
                {
                    if (cmdConfig.argCount == null)
                    {
                        throw new ArgumentNullException(nameof(cmdConfig.argCount), $"cmdConfig {cmdConfig.name} argCount is null!");
                    }
                    else if (cmdConfig.argCount <= 0)
                    {
                        throw new ArgumentOutOfRangeException(nameof(cmdConfig.argCount), $"cmdConfig {cmdConfig.name} argCount ({cmdConfig.argCount}) must be greater than 0!");
                    }
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
                            throw new ArgumentInvalidException(nameof(cmdOption.name), $"command contains multiple time the option \"{cmdOption.name}\"");
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
                    throw;
                }
            }

            return optionValues;
        }

        public override string? ToString()
        {
            string res = "";
            if (cmd != null)
            {
                res += "cmd:\n\t" + cmd + "\n";
            }
            if (baseArgs.Length > 0)
            {
                res += "baseArgs:\n\t" + baseArgs[0];
                foreach (var arg in baseArgs.Skip(1))
                {
                    res += " " + arg;
                }
                res += "\n";
            }
            if (cmdOptions.Count > 0)
            {
                res += "options:\n";
                foreach (var arg in cmdOptions)
                {
                    string pair = "\t" + arg.Key;
                    if (arg.Value.Length > 0)
                    {
                        pair += ": " + arg.Value[0];
                    }
                    foreach (var val in arg.Value.Skip(1))
                    {
                        pair += ", " + val;
                    }
                    pair += "\n";

                    res += pair;
                }
            }
            if (cmdConfigs.Length > 0)
            {
                res += "config:\n";
                foreach (var arg in cmdConfigs)
                {
                    string line = "\t" + arg.name;
                    line += (arg.argCount == null ? ", args: none" : $", args: {arg.argCount}") + "\n";
                    res += line;
                }
            }

            return res;
        }
    }
}