namespace NMG;

public static class ProbeCommand
{
    public static int Run(string[] args)
    {
        string outputFile = null;
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-o" && i + 1 < args.Length) { outputFile = args[i + 1]; i++; }
        }

        string aspx = Templates.ProbeTemplate.Source;
        if (outputFile != null)
        {
            File.WriteAllText(outputFile, aspx);
            Console.WriteLine($"[+] probe page: {Path.GetFullPath(outputFile)}");
            Console.WriteLine("[+] upload to target and access once");
            Console.WriteLine("[+] response will be JSON with environment details");
        }
        else
        {
            Console.WriteLine(aspx);
        }
        return 0;
    }
}
