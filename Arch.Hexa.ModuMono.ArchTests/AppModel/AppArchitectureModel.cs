using System.Reflection;
using System.Runtime.Loader;

namespace Arch.Hexa.ModuMono.ArchTests.AppModel;

internal sealed class AppArchitectureModel
{
    public required string RootNamespace { get; init; }

    public required IReadOnlyList<Assembly> All { get; init; }

    // All assemblies by layer (modules + building blocks + any other solution assemblies that match)
    public required IReadOnlyList<Assembly> Apis { get; init; }
    public required IReadOnlyList<Assembly> Applications { get; init; }
    public required IReadOnlyList<Assembly> Domains { get; init; }
    public required IReadOnlyList<Assembly> Infrastructures { get; init; }

    // BuildingBlocks assemblies (transversal)
    public Assembly? BuildingBlocksApi { get; init; }
    public Assembly? BuildingBlocksApplication { get; init; }
    public Assembly? BuildingBlocksDomain { get; init; }
    public Assembly? BuildingBlocksInfrastructure { get; init; }

    public IReadOnlyList<Assembly> BuildingBlocksAll =>
        new[] { BuildingBlocksApi, BuildingBlocksApplication, BuildingBlocksDomain, BuildingBlocksInfrastructure }
            .Where(a => a is not null)
            .Cast<Assembly>()
            .ToList();

    // Modules discovered from naming convention
    public required IReadOnlyList<ModuleGroup> Modules { get; init; }

    public IReadOnlyList<Assembly> ModuleAssemblies =>
        Modules.SelectMany(m => new[] { m.Domain, m.Application, m.Infrastructure, m.Api })
            .Where(a => a is not null)
            .Cast<Assembly>()
            .Distinct()
            .ToList();

    public IReadOnlySet<string> ApiNames => Apis.Select(a => a.GetName().Name!).ToHashSet(StringComparer.Ordinal);
    public IReadOnlySet<string> ApplicationNames => Applications.Select(a => a.GetName().Name!).ToHashSet(StringComparer.Ordinal);
    public IReadOnlySet<string> DomainNames => Domains.Select(a => a.GetName().Name!).ToHashSet(StringComparer.Ordinal);
    public IReadOnlySet<string> InfrastructureNames => Infrastructures.Select(a => a.GetName().Name!).ToHashSet(StringComparer.Ordinal);

    public IReadOnlySet<string> ModuleAssemblyNames => ModuleAssemblies.Select(a => a.GetName().Name!).ToHashSet(StringComparer.Ordinal);

    public string? BuildingBlocksApiName => BuildingBlocksApi?.GetName().Name;
    public string? BuildingBlocksApplicationName => BuildingBlocksApplication?.GetName().Name;
    public string? BuildingBlocksDomainName => BuildingBlocksDomain?.GetName().Name;
    public string? BuildingBlocksInfrastructureName => BuildingBlocksInfrastructure?.GetName().Name;

    public static AppArchitectureModel DiscoverFromTestAssembly(Assembly testAssembly, string rootNamespace)
    {
        // 1) Force-load solution assemblies from the test output folder (bin/...)
        ForceLoadAssembliesFromOutput(rootNamespace);

        // 2) Load referenced assemblies (best-effort). Some may already be loaded.
        TryLoadReferencedAssemblies(testAssembly);

        // 3) Collect all loaded solution assemblies matching the root namespace
        var all = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(a => !a.IsDynamic)
            .Where(a => (a.GetName().Name ?? string.Empty).StartsWith(rootNamespace, StringComparison.Ordinal))
            .Distinct()
            .ToList();

        // 4) Classify assemblies into layers
        static Layer GetLayer(string name)
        {
            if (name.EndsWith(".Domain", StringComparison.Ordinal)) return Layer.Domain;
            if (name.EndsWith(".Application", StringComparison.Ordinal)) return Layer.Application;
            if (name.EndsWith(".Infrastructure", StringComparison.Ordinal)) return Layer.Infrastructure;

            if (name.EndsWith(".Api", StringComparison.Ordinal) ||
                name.EndsWith(".RestApi", StringComparison.Ordinal) ||
                name.EndsWith(".Graphql", StringComparison.Ordinal) ||
                name.EndsWith(".GraphQL", StringComparison.Ordinal))
                return Layer.Api;

            return Layer.Unknown;
        }

        var domains = all.Where(a => GetLayer(a.GetName().Name ?? "") == Layer.Domain).ToList();
        var applications = all.Where(a => GetLayer(a.GetName().Name ?? "") == Layer.Application).ToList();
        var infrastructures = all.Where(a => GetLayer(a.GetName().Name ?? "") == Layer.Infrastructure).ToList();
        var apis = all.Where(a => GetLayer(a.GetName().Name ?? "") == Layer.Api).ToList();

        // 5) Resolve BuildingBlocks assemblies by exact name
        Assembly? FindExact(string fullName)
            => all.FirstOrDefault(a => string.Equals(a.GetName().Name, fullName, StringComparison.Ordinal));

        var bbApi = FindExact($"{rootNamespace}.BuildingBlocks.Api");
        var bbApp = FindExact($"{rootNamespace}.BuildingBlocks.Application");
        var bbDomain = FindExact($"{rootNamespace}.BuildingBlocks.Domain");
        var bbInfra = FindExact($"{rootNamespace}.BuildingBlocks.Infrastructure");

        // 6) Discover module names from "rootNamespace.Modules.<Module>.<Layer>"
        static string? GetModuleName(string asmName, string rootNs)
        {
            var token = $"{rootNs}.Modules.";
            var idx = asmName.IndexOf(token, StringComparison.Ordinal);
            if (idx < 0) return null;

            var rest = asmName[(idx + token.Length)..];
            var parts = rest.Split('.', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length >= 2 ? parts[0] : null;
        }

        static bool IsBuildingBlocks(string asmName, string rootNs)
            => asmName.StartsWith($"{rootNs}.BuildingBlocks.", StringComparison.Ordinal);

        var moduleNames = all
            .Select(a => a.GetName().Name ?? string.Empty)
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Where(n => !IsBuildingBlocks(n, rootNamespace))
            .Select(n => GetModuleName(n, rootNamespace))
            .Where(m => !string.IsNullOrWhiteSpace(m))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(m => m, StringComparer.Ordinal)
            .ToList();

        Assembly? FindModuleAssembly(string module, string suffix)
        {
            var expected = $"{rootNamespace}.Modules.{module}.{suffix}";
            return all.FirstOrDefault(a => string.Equals(a.GetName().Name, expected, StringComparison.Ordinal));
        }

        var modules = moduleNames
            .Select(m => new ModuleGroup(
                Name: m!,
                Api: FindModuleAssembly(m!, "Api") ??
                     FindModuleAssembly(m!, "RestApi") ??
                     FindModuleAssembly(m!, "Graphql") ??
                     FindModuleAssembly(m!, "GraphQL"),
                Application: FindModuleAssembly(m!, "Application"),
                Domain: FindModuleAssembly(m!, "Domain"),
                Infrastructure: FindModuleAssembly(m!, "Infrastructure")))
            .ToList();

        // 7) Create the model
        return new AppArchitectureModel
        {
            RootNamespace = rootNamespace,
            All = all,

            Domains = domains,
            Applications = applications,
            Infrastructures = infrastructures,
            Apis = apis,

            BuildingBlocksApi = bbApi,
            BuildingBlocksApplication = bbApp,
            BuildingBlocksDomain = bbDomain,
            BuildingBlocksInfrastructure = bbInfra,

            Modules = modules
        };
    }

    private static void ForceLoadAssembliesFromOutput(string rootNamespace)
    {
        var baseDir = AppContext.BaseDirectory;

        // English comment only: load DLLs from the test output folder to ensure they are available for reflection
        IEnumerable<string> FindCandidates(SearchOption option)
            => Directory.EnumerateFiles(baseDir, $"{rootNamespace}*.dll", option);

        var candidates = FindCandidates(SearchOption.TopDirectoryOnly).ToList();
        if (candidates.Count == 0)
        {
            // English comment only: some runners place dependencies in subfolders
            candidates = FindCandidates(SearchOption.AllDirectories).ToList();
        }

        foreach (var path in candidates)
        {
            try
            {
                var asmName = AssemblyName.GetAssemblyName(path);

                // English comment only: skip already loaded assemblies
                if (AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().Name == asmName.Name))
                    continue;

                AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
            }
            catch
            {
                // English comment only: ignore load failures (native deps, duplicates, incompatible assemblies, etc.)
            }
        }
    }

    private static void TryLoadReferencedAssemblies(Assembly testAssembly)
    {
        foreach (var reference in testAssembly.GetReferencedAssemblies())
        {
            try
            {
                if (AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().Name == reference.Name))
                    continue;

                Assembly.Load(reference);
            }
            catch
            {
                // English comment only: ignore load failures; the file-based load already handled most cases
            }
        }
    }

    private enum Layer { Unknown, Api, Application, Domain, Infrastructure }

    //public static AppArchitectureModel DiscoverFromTestAssembly(Assembly testAssembly, string rootNamespace)
    //{
    //    // Ensure referenced assemblies are loaded
    //    var referenced = testAssembly
    //        .GetReferencedAssemblies()
    //        .Select(Assembly.Load)
    //        .ToList();

    //    var all = AppDomain.CurrentDomain
    //        .GetAssemblies()
    //        .Concat(referenced)
    //        .Distinct()
    //        .Where(a => !a.IsDynamic)
    //        .ToList();

    //    // Keep only solution assemblies
    //    all = all
    //        .Where(a => (a.GetName().Name ?? string.Empty).StartsWith(rootNamespace, StringComparison.Ordinal))
    //        .ToList();

    //    static Layer GetLayer(string name)
    //    {
    //        if (name.EndsWith(".Domain", StringComparison.Ordinal)) return Layer.Domain;
    //        if (name.EndsWith(".Application", StringComparison.Ordinal)) return Layer.Application;
    //        if (name.EndsWith(".Infrastructure", StringComparison.Ordinal)) return Layer.Infrastructure;

    //        // Common API project suffixes
    //        if (name.EndsWith(".Api", StringComparison.Ordinal) ||
    //            name.EndsWith(".RestApi", StringComparison.Ordinal) ||
    //            name.EndsWith(".Graphql", StringComparison.Ordinal) ||
    //            name.EndsWith(".GraphQL", StringComparison.Ordinal))
    //            return Layer.Api;

    //        return Layer.Unknown;
    //    }

    //    Assembly? FindExactAsm(string fullName)
    //        => all.FirstOrDefault(a => string.Equals(a.GetName().Name, fullName, StringComparison.Ordinal));

    //    // BuildingBlocks are exact names (transversal per layer)
    //    var bbApi = FindExactAsm($"{rootNamespace}.BuildingBlocks.Api");
    //    var bbApp = FindExactAsm($"{rootNamespace}.BuildingBlocks.Application");
    //    var bbDomain = FindExactAsm($"{rootNamespace}.BuildingBlocks.Domain");
    //    var bbInfra = FindExactAsm($"{rootNamespace}.BuildingBlocks.Infrastructure");

    //    var apis = all.Where(a => GetLayer(a.GetName().Name ?? "") == Layer.Api).ToList();
    //    var applications = all.Where(a => GetLayer(a.GetName().Name ?? "") == Layer.Application).ToList();
    //    var domains = all.Where(a => GetLayer(a.GetName().Name ?? "") == Layer.Domain).ToList();
    //    var infrastructures = all.Where(a => GetLayer(a.GetName().Name ?? "") == Layer.Infrastructure).ToList();

    //    // Discover modules by convention: <root>.Modules.<Module>.<Layer>
    //    static string? GetModuleName(string asmName, string rootNs)
    //    {
    //        var token = $"{rootNs}.Modules.";
    //        var idx = asmName.IndexOf(token, StringComparison.Ordinal);
    //        if (idx < 0) return null;

    //        var rest = asmName[(idx + token.Length)..];
    //        var parts = rest.Split('.', StringSplitOptions.RemoveEmptyEntries);
    //        return parts.Length >= 2 ? parts[0] : null;
    //    }

    //    bool IsBuildingBlocks(string asmName)
    //        => asmName.StartsWith($"{rootNamespace}.BuildingBlocks.", StringComparison.Ordinal);

    //    var moduleNames = all
    //        .Select(a => a.GetName().Name ?? "")
    //        .Where(n => !string.IsNullOrWhiteSpace(n))
    //        .Where(n => !IsBuildingBlocks(n))
    //        .Select(n => GetModuleName(n, rootNamespace))
    //        .Where(m => !string.IsNullOrWhiteSpace(m))
    //        .Distinct(StringComparer.Ordinal)
    //        .OrderBy(m => m, StringComparer.Ordinal)
    //        .ToList()!;

    //    Assembly? FindModuleAssembly(string module, string suffix)
    //    {
    //        var expected = $"{rootNamespace}.Modules.{module}.{suffix}";
    //        return all.FirstOrDefault(a => string.Equals(a.GetName().Name, expected, StringComparison.Ordinal));
    //    }

    //    var modules = moduleNames
    //        .Select(m => new ModuleGroup(
    //            Name: m!,
    //            Api: FindModuleAssembly(m!, "Api") ??
    //                 FindModuleAssembly(m!, "RestApi") ??
    //                 FindModuleAssembly(m!, "Graphql") ??
    //                 FindModuleAssembly(m!, "GraphQL"),
    //            Application: FindModuleAssembly(m!, "Application"),
    //            Domain: FindModuleAssembly(m!, "Domain"),
    //            Infrastructure: FindModuleAssembly(m!, "Infrastructure")))
    //        .ToList();

    //    return new AppArchitectureModel
    //    {
    //        RootNamespace = rootNamespace,
    //        All = all,
    //        Apis = apis,
    //        Applications = applications,
    //        Domains = domains,
    //        Infrastructures = infrastructures,
    //        BuildingBlocksApi = bbApi,
    //        BuildingBlocksApplication = bbApp,
    //        BuildingBlocksDomain = bbDomain,
    //        BuildingBlocksInfrastructure = bbInfra,
    //        Modules = modules
    //    };

    //}
}