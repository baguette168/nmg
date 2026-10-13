namespace NMG;

public static class Program
{
    public static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            PrintHelp();
            return 0;
        }

        switch (args[0].ToLowerInvariant())
        {
            case "generate":
                return GenerateCommand.Run(args.Skip(1).ToArray());
            case "probe":
                return ProbeCommand.Run(args.Skip(1).ToArray());
            case "version":
                Console.WriteLine("nmg v0.1.0 (.NET Memory-shell Generator)");
                return 0;
            case "help":
            case "--help":
            case "-h":
                PrintHelp();
                return 0;
            default:
                Console.Error.WriteLine($"Unknown command: {args[0]}");
                PrintHelp();
                return 1;
        }
    }

    static void PrintHelp()
    {
        Console.WriteLine("""
            NMG (.NET Memory-shell Generator) v0.1.0

            Usage:
              nmg generate [options]    Generate memory shell payload
              nmg version               Show version
              nmg help                  Show this help

            Generate options:
              --type <vpp|route|filter|httplistener|httpmodule>   Memory shell type (P1: vpp only)
              --shell <godzilla|behinder|rawcmd>                  Shell protocol (P1: godzilla only)
              --pass <password>                                    Connection password
              [--key <key>]                                        Encryption key (default: auto-derive)
              [--path <virtual-path>]                             Trigger path (default: random)
              [--header <Name:Value>]                              Trigger header (optional)
              [--target <net40|net472|net20>]                     Target framework (default: net40)
              [--format <aspx|b64|dll>]                           Output format (default: aspx)
              [--seed <n>]                                        Randomization seed (deterministic output)
              [-o <output-file>]                                   Output file path
            """);
    }
}
