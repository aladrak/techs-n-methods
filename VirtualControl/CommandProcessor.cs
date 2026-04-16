using System.Text.RegularExpressions;
using VirtualControl.Arrays;
using VirtualControl.Core;

namespace VirtualControl;

public class CommandProcessor
{
    private VirtualArrayBase? _array = null;
    private VirtualVarcharArray? _varcharArray = null;
    
    private static readonly Regex InputRegex = new(
        @"^Input\s*\(\s*(?<index>\d+)\s*,\s*(?<value>""[^""]*""|\d+)\s*\)$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled
    );

    public void Process(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return;
        var cmd = input.Trim();

        try
        {
            if (cmd.StartsWith("Create ", StringComparison.OrdinalIgnoreCase)) HandleCreate(cmd);
            else if (cmd.StartsWith("Open ", StringComparison.OrdinalIgnoreCase)) HandleOpen(cmd);
            else if (cmd.StartsWith("Input ", StringComparison.OrdinalIgnoreCase)) HandleInput(cmd);
            else if (cmd.StartsWith("Print ", StringComparison.OrdinalIgnoreCase)) HandlePrint(cmd);
            else if (cmd.StartsWith("Help", StringComparison.OrdinalIgnoreCase)) HandleHelp(cmd);
            else if (cmd.Equals("Exit", StringComparison.OrdinalIgnoreCase)) HandleExit();
            else Console.WriteLine("Unknown command. Type 'Help' for available commands.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private void HandleCreate(string input)
    {
        var args = input.Substring(7).Trim();
        var brk = args.IndexOf('(');
        if (brk == -1) throw new ArgumentException("Usage: Create <file>(int | char(len) | varchar(len))");

        var fileName = args.Substring(0, brk);
        var typeDef = args.Substring(brk + 1).TrimEnd(')').Trim();

        ArrayType type;
        int len = 0;

        if (typeDef.Equals("int", StringComparison.OrdinalIgnoreCase)) type = ArrayType.Int;
        else if (typeDef.StartsWith("char", StringComparison.OrdinalIgnoreCase)) { type = ArrayType.Char; len = int.Parse(typeDef.Substring(4).Trim()); }
        else if (typeDef.StartsWith("varchar", StringComparison.OrdinalIgnoreCase)) { type = ArrayType.Varchar; len = int.Parse(typeDef.Substring(7).Trim()); }
        else throw new ArgumentException("Unknown type. Use int, char(n), or varchar(n).");

        CloseCurrent();
        if (type == ArrayType.Varchar)
        {
            _varcharArray = new VirtualVarcharArray(10001, len, fileName, true);
        }
        else
        {
            _array = type switch
            {
                ArrayType.Int => new VirtualIntArray(fileName, 10001, true),
                ArrayType.Char => new VirtualCharArray(fileName, 10001, len, true),
                _ => throw new NotSupportedException()
            };
        }

        Console.WriteLine($"Created: {fileName} ({typeDef})");
    }

    private void HandleOpen(string input)
    {
        var path = input.Substring(5).Trim();
        if (!File.Exists(path)) throw new Exception($"File '{path}' not found.");

        CloseCurrent();
        int strLen;
        ArrayType type;
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            if (fs.Length < 15) throw new Exception("Corrupted or empty file");

            fs.Seek(10, SeekOrigin.Begin);
            type = (ArrayType)fs.ReadByte();
            fs.Seek(11, SeekOrigin.Begin);
            byte[] lenBuf = new byte[4];
            fs.Read(lenBuf, 0, 4);
            strLen = BitConverter.ToInt32(lenBuf, 0);
        }

        if (type == ArrayType.Varchar)
        {
            _varcharArray = new VirtualVarcharArray(10001, strLen, path, false);
            _array = null;
        }
        else
        {
            _array = type switch
            {
                ArrayType.Int => new VirtualIntArray(path, 10001, false),
                ArrayType.Char => new VirtualCharArray(path, 10001, strLen, false),
                _ => throw new NotSupportedException()
            };
            _varcharArray = null;
        }
        Console.WriteLine($"Opened: {path}");
    }

    private void HandleInput(string input)
    {
        if (_array == null && _varcharArray == null) throw new InvalidOperationException("No array is open. Use 'Create' or 'Open' first.");

        var match = InputRegex.Match(input);
        if (!match.Success)
            throw new ArgumentException("Usage: Input (index, value)");

        long index = long.Parse(match.Groups["index"].Value);
        string rawValue = match.Groups["value"].Value;

        object value = rawValue.StartsWith('"') 
            ? rawValue.Substring(1, rawValue.Length - 2) 
            : int.Parse(rawValue);
        if (_varcharArray != null)
            _varcharArray.Set(index, (string)value);
        else
        {
            if (!_array.Write(index, value))
                throw new InvalidOperationException($"Failed to write at index {index}.");
        }

        Console.WriteLine($"Written to [{index}]");
    }

    private void HandlePrint(string input)
    {
        var content = input.Substring(6).Trim().Trim('(', ')');
        long index = long.Parse(content);
        if (_array == null && _varcharArray == null) throw new InvalidOperationException("No array is open.");

        if (_varcharArray != null)
        {
            var val = _varcharArray.Get(index);
            if (val != null)
                Console.WriteLine($"[{index}] = {val}");
            else
                Console.WriteLine($"[{index}] is empty or not initialized.");
        }
        else
        {
            if (_array.TryRead(index, out var val))
                Console.WriteLine($"[{index}] = {val}");
            else
                Console.WriteLine($"[{index}] is empty or not initialized.");
        }
    }

    private void HandleHelp(string input)
    {
        const string helpText = @"Available commands:
Create <file>(int|char(len)|varchar(len)) - create new virtual array
Open <file>                               - open existing array
Input (index, value)                      - write value (strings in quotes)
Print (index)                             - read value
Help [output.txt]                         - show this help
Exit                                      - close files and exit";

        var args = input.Substring(4).Trim();
        if (!string.IsNullOrEmpty(args) && File.Exists(args))
        {
            File.WriteAllText(args, helpText);
            Console.WriteLine($"Help written to '{args}'");
        }
        else Console.WriteLine(helpText);
    }

    private void HandleExit()
    {
        CloseCurrent();
        Console.WriteLine("Files saved. Exiting...");
        Environment.Exit(0);
    }
    private void CloseCurrent() { _array?.Dispose(); _array = null; _varcharArray?.Dispose(); _varcharArray = null; }
}