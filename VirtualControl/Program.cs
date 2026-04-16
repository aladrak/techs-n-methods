namespace VirtualControl;

class Program
{
    private static void Main()
    {
        var processor = new CommandProcessor();

        while (true)
        {
            Console.Write("VM> ");
            var line = Console.ReadLine();
            if (line == null) break;

            processor.Process(line);
        }
    }
}