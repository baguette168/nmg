namespace NMG;

public static class ListCommand
{
    public static int Run(string[] args)
    {
        Console.WriteLine("NMG v0.2.0 — Memory Shell Compatibility Matrix");
        Console.WriteLine();
        Console.WriteLine("Memory Shell Types:");
        Console.WriteLine("  vpp            VirtualPathProvider     IIS/ASP.NET      .NET 2.0+");
        Console.WriteLine("  route          RouteBase               MVC/WebAPI       .NET 3.5+");
        Console.WriteLine("  httplistener   HttpListener            Standalone       .NET 2.0+");
        Console.WriteLine();
        Console.WriteLine("Shell Protocols:");
        Console.WriteLine("  godzilla       Godzilla 4.x            AES-128          C#");
        Console.WriteLine("  behinder       Behinder 4.x            AES-128          .NET");
        Console.WriteLine("  antsword       AntSword                Base64           Multi");
        Console.WriteLine();
        Console.WriteLine("Compatibility:");
        Console.WriteLine("  Type           Godzilla  Behinder  AntSword");
        Console.WriteLine("  vpp              YES       YES       YES");
        Console.WriteLine("  route            YES       YES       YES");
        Console.WriteLine("  httplistener     YES        --        --");
        Console.WriteLine();
        Console.WriteLine("Output Formats:");
        Console.WriteLine("  aspx           Self-contained ASPX injector page");
        Console.WriteLine("  b64            Raw Base64 (for Assembly.Load delivery)");
        Console.WriteLine("  dll            Raw DLL binary");
        Console.WriteLine();
        Console.WriteLine("Security Features:");
        Console.WriteLine("  - Trigger name randomization (WAF evasion)");
        Console.WriteLine("  - XOR string obfuscation (anti-static-analysis)");
        Console.WriteLine("  - Deterministic compilation (reproducible builds)");
        Console.WriteLine("  - Dual VPP registration (official API + field replacement)");
        return 0;
    }
}
