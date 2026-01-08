using System.Reflection;
using Arch.Hexa.ModuMono.ArchTests.AppModel;
using Microsoft.EntityFrameworkCore;
using NetArchTest.Rules;

namespace Arch.Hexa.ModuMono.ArchTests;

public class AppArchitectureRulesTests
{
    private static readonly AppArchitectureModel Model = AppArchitectureModel.DiscoverFromTestAssembly(typeof(AppArchitectureRulesTests).Assembly, rootNamespace: "Architecture.Pocs.ModularMonolith");

    private static readonly string[] ForbiddenInDomain =
    [
        "Microsoft.EntityFrameworkCore",
        "Microsoft.AspNetCore",
        "HotChocolate",
        "MediatR"
    ];

    private static readonly string[] ForbiddenInApplication =
    [
        "Microsoft.EntityFrameworkCore",
        "Microsoft.AspNetCore",
        "HotChocolate"
    ];


    private static string FailureMessage(TestResult result)
        => result.IsSuccessful ? string.Empty
            : (result.FailingTypeNames is { Count: > 0 } failing
                ? string.Join(Environment.NewLine, failing)
                : "Architecture rule failed but no failing types were returned.");

    private static bool IsTestAssembly(Assembly asm)
    {
        var n = asm.GetName().Name ?? "";
        return n.EndsWith(".Tests", StringComparison.Ordinal)
               || n.EndsWith(".ArchTests", StringComparison.Ordinal);
    }

    private static IEnumerable<Assembly> ProductionAssemblies()
    {
        var testAsm = typeof(AppArchitectureRulesTests).Assembly;
        return Model.All.Where(a => a != testAsm && !IsTestAssembly(a));
    }

    private static Type[] SafeGetTypes(Assembly asm)
    {
        try
        {
            return asm.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(t => t is not null).Cast<Type>().ToArray();
        }
    }




    // ---------------------------
    // Layering rules
    // ---------------------------



    [Fact]
    public void ApiShouldNotReferenceBuildingBlocksDomain()
    {
        var bbApp = Model.BuildingBlocksDomainName;
        if (string.IsNullOrWhiteSpace(bbApp))
            return;

        foreach (var api in Model.Apis)
        {
            var result = Types.InAssembly(api)
                .ShouldNot()
                .HaveDependencyOn(bbApp)
                .GetResult();

            Assert.True(result.IsSuccessful,
                $"{api.GetName().Name} -> {bbApp}{Environment.NewLine}{FailureMessage(result)}");
        }
    }

    [Fact]
    public void ApiShouldNotReferenceBuildingBlocksInfrastructure()
    {
        var bbApp = Model.BuildingBlocksInfrastructureName;
        if (string.IsNullOrWhiteSpace(bbApp))
            return;

        foreach (var api in Model.Apis)
        {
            var result = Types.InAssembly(api)
                .ShouldNot()
                .HaveDependencyOn(bbApp)
                .GetResult();

            Assert.True(result.IsSuccessful,
                $"{api.GetName().Name} -> {bbApp}{Environment.NewLine}{FailureMessage(result)}");
        }
    }

    [Fact]
    public void ApiShouldNotReferenceDomain()
    {
        var forbidden = Model.DomainNames.ToArray();

        foreach (var api in Model.Apis)
        {
            var result = Types.InAssembly(api)
                .ShouldNot()
                .HaveDependencyOnAny(forbidden)
                .GetResult();

            Assert.True(result.IsSuccessful,
                $"{api.GetName().Name}{Environment.NewLine}{FailureMessage(result)}");
        }
    }

    [Fact]
    public void ApiShouldNotReferenceInfrastructure()
    {
        var forbidden = Model.InfrastructureNames.ToArray();

        foreach (var api in Model.Apis)
        {
            var result = Types.InAssembly(api)
                .ShouldNot()
                .HaveDependencyOnAny(forbidden)
                .GetResult();

            Assert.True(result.IsSuccessful,
                $"{api.GetName().Name}{Environment.NewLine}{FailureMessage(result)}");
        }
    }

    [Fact]
    public void DomainShouldNotReferenceApi()
    {
        var forbidden = Model.ApiNames.ToArray();

        foreach (var domain in Model.Domains)
        {
            var result = Types.InAssembly(domain)
                .ShouldNot()
                .HaveDependencyOnAny(forbidden)
                .GetResult();

            Assert.True(result.IsSuccessful,
                $"{domain.GetName().Name}{Environment.NewLine}{FailureMessage(result)}");
        }
    }

    [Fact]
    public void DomainShouldNotReferenceApplication()
    {
        var forbidden = Model.ApplicationNames.ToArray();

        foreach (var domain in Model.Domains)
        {
            var result = Types.InAssembly(domain)
                .ShouldNot()
                .HaveDependencyOnAny(forbidden)
                .GetResult();

            Assert.True(result.IsSuccessful,
                $"{domain.GetName().Name}{Environment.NewLine}{FailureMessage(result)}");
        }
    }

    [Fact]
    public void DomainShouldNotReferenceInfrastructure()
    {
        var forbidden = Model.InfrastructureNames.ToArray();

        foreach (var domain in Model.Domains)
        {
            var result = Types.InAssembly(domain)
                .ShouldNot()
                .HaveDependencyOnAny(forbidden)
                .GetResult();

            Assert.True(result.IsSuccessful,
                $"{domain.GetName().Name}{Environment.NewLine}{FailureMessage(result)}");
        }
    }

    [Fact]
    public void DomainShouldNotReferenceBuildingBlocksApi()
    {
        var bbApp = Model.BuildingBlocksApiName;
        if (string.IsNullOrWhiteSpace(bbApp))
            return;

        foreach (var domain in Model.Domains)
        {
            var result = Types.InAssembly(domain)
                .ShouldNot()
                .HaveDependencyOn(bbApp)
                .GetResult();

            Assert.True(result.IsSuccessful,
                $"{domain.GetName().Name} -> {bbApp}{Environment.NewLine}{FailureMessage(result)}");
        }
    }

    [Fact]
    public void DomainShouldNotReferenceBuildingBlocksApplication()
    {
        var bbApp = Model.BuildingBlocksApplicationName;
        if (string.IsNullOrWhiteSpace(bbApp))
            return;

        foreach (var domain in Model.Domains)
        {
            var result = Types.InAssembly(domain)
                .ShouldNot()
                .HaveDependencyOn(bbApp)
                .GetResult();

            Assert.True(result.IsSuccessful,
                $"{domain.GetName().Name} -> {bbApp}{Environment.NewLine}{FailureMessage(result)}");
        }
    }

    [Fact]
    public void DomainShouldNotReferenceBuildingBlocksInfrastructure()
    {
        var bbApp = Model.BuildingBlocksInfrastructureName;
        if (string.IsNullOrWhiteSpace(bbApp))
            return;

        foreach (var domain in Model.Domains)
        {
            var result = Types.InAssembly(domain)
                .ShouldNot()
                .HaveDependencyOn(bbApp)
                .GetResult();

            Assert.True(result.IsSuccessful,
                $"{domain.GetName().Name} -> {bbApp}{Environment.NewLine}{FailureMessage(result)}");
        }
    }

    [Fact]
    public void DomainShouldNotDependOnForbiddenLibraries()
    {
        foreach (var domain in Model.Domains)
        {
            var result = Types.InAssembly(domain)
                .ShouldNot()
                .HaveDependencyOnAny(ForbiddenInDomain)
                .GetResult();

            Assert.True(result.IsSuccessful, $"{domain.GetName().Name}{Environment.NewLine}{FailureMessage(result)}");
        }
    }



    [Fact]
    public void ApplicationShouldNotReferenceApiOrInfrastructure()
    {
        var forbidden = Model.InfrastructureNames
            .Concat(Model.ApiNames)
            .ToArray();

        foreach (var app in Model.Applications)
        {
            var result = Types.InAssembly(app)
                .ShouldNot()
                .HaveDependencyOnAny(forbidden)
                .GetResult();

            Assert.True(result.IsSuccessful, $"{app.GetName().Name}{Environment.NewLine}{FailureMessage(result)}");
        }
    }

    [Fact]
    public void ApplicationShouldNotDependOnForbiddenLibraries()
    {
        foreach (var app in Model.Applications)
        {
            var result = Types.InAssembly(app)
                .ShouldNot()
                .HaveDependencyOnAny(ForbiddenInApplication)
                .GetResult();

            Assert.True(result.IsSuccessful, $"{app.GetName().Name}{Environment.NewLine}{FailureMessage(result)}");
        }
    }

    [Fact]
    public void ApplicationShouldNotReferenceBuildingBlocksApi()
    {
        var bbApp = Model.BuildingBlocksApiName;
        if (string.IsNullOrWhiteSpace(bbApp))
            return;

        foreach (var app in Model.Applications)
        {
            var result = Types.InAssembly(app)
                .ShouldNot()
                .HaveDependencyOn(bbApp)
                .GetResult();

            Assert.True(result.IsSuccessful,
                $"{app.GetName().Name} -> {bbApp}{Environment.NewLine}{FailureMessage(result)}");
        }
    }

    [Fact]
    public void ApplicationShouldNotReferenceBuildingBlocksDomain()
    {
        var bbApp = Model.BuildingBlocksDomainName;
        if (string.IsNullOrWhiteSpace(bbApp))
            return;

        foreach (var app in Model.Applications)
        {
            var result = Types.InAssembly(app)
                .ShouldNot()
                .HaveDependencyOn(bbApp)
                .GetResult();

            Assert.True(result.IsSuccessful,
                $"{app.GetName().Name} -> {bbApp}{Environment.NewLine}{FailureMessage(result)}");
        }
    }

    [Fact]
    public void ApplicationShouldNotReferenceBuildingBlocksInfrastructure()
    {
        var bbApp = Model.BuildingBlocksInfrastructureName;
        if (string.IsNullOrWhiteSpace(bbApp))
            return;

        foreach (var app in Model.Applications)
        {
            var result = Types.InAssembly(app)
                .ShouldNot()
                .HaveDependencyOn(bbApp)
                .GetResult();

            Assert.True(result.IsSuccessful,
                $"{app.GetName().Name} -> {bbApp}{Environment.NewLine}{FailureMessage(result)}");
        }
    }





    [Fact]
    public void InfrastructureShouldNotReferenceApi()
    {
        var forbidden = Model.ApiNames
            .Concat(Model.ApiNames)
            .ToArray();

        foreach (var infra in Model.Infrastructures)
        {
            var result = Types.InAssembly(infra)
                .ShouldNot()
                .HaveDependencyOnAny(forbidden)
                .GetResult();

            Assert.True(result.IsSuccessful, $"{infra.GetName().Name}{Environment.NewLine}{FailureMessage(result)}");
        }
    }

    [Fact]
    public void InfrastructureShouldNotReferenceBuildingBlocksApi()
    {
        var bbApp = Model.BuildingBlocksApiName;
        if (string.IsNullOrWhiteSpace(bbApp))
            return;

        foreach (var infra in Model.Infrastructures)
        {
            var result = Types.InAssembly(infra)
                .ShouldNot()
                .HaveDependencyOn(bbApp)
                .GetResult();

            Assert.True(result.IsSuccessful,
                $"{infra.GetName().Name} -> {bbApp}{Environment.NewLine}{FailureMessage(result)}");
        }
    }

    [Fact]
    public void InfrastructureShouldNotReferenceBuildingBlocksDomain()
    {
        var bbApp = Model.BuildingBlocksDomainName;
        if (string.IsNullOrWhiteSpace(bbApp))
            return;

        foreach (var infra in Model.Infrastructures)
        {
            var result = Types.InAssembly(infra)
                .ShouldNot()
                .HaveDependencyOn(bbApp)
                .GetResult();

            Assert.True(result.IsSuccessful,
                $"{infra.GetName().Name} -> {bbApp}{Environment.NewLine}{FailureMessage(result)}");
        }
    }


    // ---------------------------
    // BuildingBlocks "per layer"
    // ---------------------------

    [Fact]
    public void OnlyApisCanDependOnBuildingBlocksApi()
    {
        var bbApi = Model.BuildingBlocksApiName;
        if (string.IsNullOrWhiteSpace(bbApi))
            return;

        var testAssembly = typeof(AppArchitectureRulesTests).Assembly;

        var nonApis = Model.All
            .Except(Model.Apis)
            .Where(a => a != testAssembly) // exclude the test project itself
            .Where(a => !(a.GetName().Name?.EndsWith(".ArchTests", StringComparison.Ordinal) ?? false)) // optional
            .ToArray();

        foreach (var asm in nonApis)
        {
            var result = Types.InAssembly(asm)
                .ShouldNot()
                .HaveDependencyOn(bbApi)
                .GetResult();

            Assert.True(result.IsSuccessful,
                $"{asm.GetName().Name} -> {bbApi}{Environment.NewLine}{FailureMessage(result)}");
        }
    }

    [Fact]
    public void OnlyInfrastructureCanDependOnBuildingBlocksInfrastructure()
    {
        var bbApi = Model.BuildingBlocksInfrastructureName;
        if (string.IsNullOrWhiteSpace(bbApi))
            return;

        var testAssembly = typeof(AppArchitectureRulesTests).Assembly;

        var nonApps = Model.All
            .Except(Model.Infrastructures)
            .Where(a => a != testAssembly) // exclude the test project itself
            .Where(a => !(a.GetName().Name?.EndsWith(".ArchTests", StringComparison.Ordinal) ?? false)) // optional
            .ToArray();

        foreach (var asm in nonApps)
        {
            var result = Types.InAssembly(asm)
                .ShouldNot()
                .HaveDependencyOn(bbApi)
                .GetResult();

            Assert.True(result.IsSuccessful,
                $"{asm.GetName().Name} -> {bbApi}{Environment.NewLine}{FailureMessage(result)}");
        }
    }



    [Fact]
    public void BuildingBlocksDomainMustNotDependOnOtherBuildingBlocks()
    {
        var bbDomain = Model.BuildingBlocksDomain;
        if (bbDomain is null) return;

        var forbidden = new[]
        {
            Model.BuildingBlocksApplicationName,
            Model.BuildingBlocksInfrastructureName,
            Model.BuildingBlocksApiName
        }
        .Where(n => !string.IsNullOrWhiteSpace(n))
        .Cast<string>()
        .ToArray();

        if (forbidden.Length == 0) return;

        var result = Types.InAssembly(bbDomain)
            .ShouldNot()
            .HaveDependencyOnAny(forbidden)
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"{bbDomain.GetName().Name}{Environment.NewLine}{FailureMessage(result)}");
    }

    [Fact]
    public void BuildingBlocksApplicationCanDependOnlyOnBuildingBlocksDomain()
    {
        var bbApp = Model.BuildingBlocksApplication;
        if (bbApp is null) return;

        // Allowed: BB.Domain
        // Forbidden: BB.Infrastructure, BB.Api
        var forbidden = new[]
        {
            Model.BuildingBlocksInfrastructureName,
            Model.BuildingBlocksApiName
        }
        .Where(n => !string.IsNullOrWhiteSpace(n))
        .Cast<string>()
        .ToArray();

        if (forbidden.Length == 0) return;

        var result = Types.InAssembly(bbApp)
            .ShouldNot()
            .HaveDependencyOnAny(forbidden)
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"{bbApp.GetName().Name}{Environment.NewLine}{FailureMessage(result)}");
    }

    [Fact]
    public void BuildingBlocksInfrastructureCanDependOnlyOnBuildingBlocksDomainAndApplication()
    {
        var bbInfra = Model.BuildingBlocksInfrastructure;
        if (bbInfra is null) return;

        // Allowed: BB.Domain, BB.Application
        // Forbidden: BB.Api
        var bbApi = Model.BuildingBlocksApiName;
        if (string.IsNullOrWhiteSpace(bbApi)) return;

        var result = Types.InAssembly(bbInfra)
            .ShouldNot()
            .HaveDependencyOn(bbApi)
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"{bbInfra.GetName().Name}{Environment.NewLine}{FailureMessage(result)}");
    }

    [Fact]
    public void BuildingBlocksApiCanDependOnlyOnBuildingBlocksDomainAndApplication()
    {
        var bbApiAsm = Model.BuildingBlocksApi;
        if (bbApiAsm is null) return;

        // Allowed: BB.Domain, BB.Application
        // Forbidden: BB.Infrastructure
        var bbInfra = Model.BuildingBlocksInfrastructureName;
        if (string.IsNullOrWhiteSpace(bbInfra)) return;

        var result = Types.InAssembly(bbApiAsm)
            .ShouldNot()
            .HaveDependencyOn(bbInfra)
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"{bbApiAsm.GetName().Name}{Environment.NewLine}{FailureMessage(result)}");
    }

    // ---------------------------
    // Modules isolation
    // ---------------------------

    [Fact]
    public void ModulesShouldBeIsolatedNoModuleDependsOnOtherModule()
    {
        // BuildingBlocks are excluded from module isolation checks by design.
        foreach (var module in Model.Modules)
        {
            var moduleAssemblies = new[] { module.Domain, module.Application, module.Infrastructure, module.Api }
                .Where(a => a is not null)
                .Cast<Assembly>()
                .ToArray();

            var otherModuleAssemblyNames = Model.Modules
                .Where(m => !string.Equals(m.Name, module.Name, StringComparison.Ordinal))
                .SelectMany(m => new[] { m.Domain, m.Application, m.Infrastructure, m.Api })
                .Where(a => a is not null)
                .Select(a => a!.GetName().Name!)
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            foreach (var asm in moduleAssemblies)
            {
                var result = Types.InAssembly(asm)
                    .ShouldNot()
                    .HaveDependencyOnAny(otherModuleAssemblyNames)
                    .GetResult();

                Assert.True(result.IsSuccessful, $"{asm.GetName().Name}{Environment.NewLine}{FailureMessage(result)}");
            }
        }
    }

    // ---------------------------
    // Ports & Adapters (repositories/clients)
    // ---------------------------

    [Fact]
    public void RepositoryAndClientPortsMustBeInterfacesInDomainOrApplication()
    {
        foreach (var asm in Model.Domains.Concat(Model.Applications))
        {
            // Repositories
            var repoResult = Types.InAssembly(asm)
                .That()
                .HaveNameEndingWith("Repository")
                .Should()
                .BeInterfaces()
                .GetResult();

            Assert.True(repoResult.IsSuccessful,
                $"{asm.GetName().Name} (Repository ports){Environment.NewLine}{FailureMessage(repoResult)}");

            // Clients/Gateways (optional pattern)
            var clientResult = Types.InAssembly(asm)
                .That()
                .HaveNameEndingWith("Client")
                .Or()
                .HaveNameEndingWith("Gateway")
                .Should()
                .BeInterfaces()
                .GetResult();

            Assert.True(clientResult.IsSuccessful,
                $"{asm.GetName().Name} (Client/Gateway ports){Environment.NewLine}{FailureMessage(clientResult)}");
        }
    }

    [Fact]
    public void InfrastructureRepositoriesAndClientsMustImplementPortInterface()
    {
        // This rule is easier with reflection: for each concrete adapter, ensure it implements I{ClassName}
        var portAssemblies = Model.Domains.Concat(Model.Applications).ToArray();
        var portTypes = portAssemblies
            .SelectMany(SafeGetTypes)
            .Where(t => t is { IsInterface: true })
            .ToDictionary(t => t.Name, t => t, StringComparer.Ordinal);

        foreach (var infra in Model.Infrastructures)
        {
            var infraTypes = SafeGetTypes(infra);

            var adapters = infraTypes
                .Where(t => t is { IsClass: true, IsAbstract: false })
                .Where(t =>
                    t.Name.EndsWith("Repository", StringComparison.Ordinal) ||
                    t.Name.EndsWith("Client", StringComparison.Ordinal) ||
                    t.Name.EndsWith("Gateway", StringComparison.Ordinal))
                .ToList();

            foreach (var adapter in adapters)
            {
                var expectedInterfaceName = "I" + adapter.Name;

                if (!portTypes.TryGetValue(expectedInterfaceName, out var portInterface))
                {
                    var message = $"{infra.GetName().Name}: Adapter '{adapter.FullName}' must implement '{expectedInterfaceName}' declared in a Domain/Application assembly, but it was not found.";
                    Assert.Fail(message);
                }

                if (!portInterface.IsAssignableFrom(adapter))
                {
                    var message = $"{infra.GetName().Name}: Adapter '{adapter.FullName}' does not implement expected port interface '{portInterface.FullName}'.";
                    Assert.Fail(message);
                }
            }
        }
    }

    // ---------------------------
    // Api should be thin (no EF Core, no DbContext, no Domain)
    // ---------------------------

    [Fact]
    public void ApiShouldNotDependOnEntityFrameworkCore()
    {
        foreach (var api in Model.Apis)
        {
            var result = Types.InAssembly(api)
                .ShouldNot()
                .HaveDependencyOnAny("Microsoft.EntityFrameworkCore")
                .GetResult();

            Assert.True(result.IsSuccessful,
                $"{api.GetName().Name}{Environment.NewLine}{FailureMessage(result)}");
        }
    }

    [Fact]
    public void DbContextMustLiveOnlyInInfrastructure()
    {
        // Non-infrastructure assemblies must not define DbContext subclasses
        var nonInfra = ProductionAssemblies().Except(Model.Infrastructures).ToArray();

        foreach (var asm in nonInfra)
        {
            var offenders = SafeGetTypes(asm)
                .Where(t => t is { IsClass: true, IsAbstract: false })
                .Where(t => typeof(DbContext).IsAssignableFrom(t))
                .Select(t => t.FullName ?? t.Name)
                .ToList();

            Assert.True(offenders.Count == 0,
                $"{asm.GetName().Name} defines DbContext types, which is forbidden outside Infrastructure:{Environment.NewLine}" +
                string.Join(Environment.NewLine, offenders));
        }
    }

    // ---------------------------
    // Avoid "shared DbContext" between modules (best-effort)
    // ---------------------------

    [Fact]
    public void EachModuleShouldHaveDbContextOnlyInItsInfrastructureAssembly()
    {
        // For each module, if it has a DbContext, it must be declared only in that module's Infrastructure assembly.
        foreach (var module in Model.Modules)
        {
            if (module.Infrastructure is null)
                continue;

            var infraAsm = module.Infrastructure;
            var infraDbContexts = SafeGetTypes(infraAsm)
                .Where(t => t is { IsClass: true, IsAbstract: false })
                .Where(t => typeof(DbContext).IsAssignableFrom(t))
                .ToList();

            // If you want to enforce "exactly one DbContext per module", replace <= with == 1
            if (infraDbContexts.Count == 0)
                continue;

            // Ensure no other assembly in the module defines a DbContext
            var moduleOtherAssemblies = new[] { module.Domain, module.Application, module.Api }
                .Where(a => a is not null)
                .Cast<Assembly>()
                .ToArray();

            foreach (var asm in moduleOtherAssemblies)
            {
                var offenders = SafeGetTypes(asm)
                    .Where(t => t is { IsClass: true, IsAbstract: false })
                    .Where(t => typeof(DbContext).IsAssignableFrom(t))
                    .Select(t => t.FullName ?? t.Name)
                    .ToList();

                Assert.True(offenders.Count == 0,
                    $"{asm.GetName().Name} defines DbContext types for module '{module.Name}'. DbContext must live in Infrastructure only.");
            }
        }
    }

    // ---------------------------
    // BuildingBlocks isolation
    // ---------------------------

    [Fact]
    public void BuildingBlocksMustNotDependOnModulesStrict()
    {
        // Strict rule: BuildingBlocks must not depend on any module assembly (even if a module wasn't discovered).
        var forbiddenModuleAssemblyNames = Model.All
            .Select(a => a.GetName().Name ?? string.Empty)
            .Where(n => n.Contains(".Modules.", StringComparison.Ordinal))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (forbiddenModuleAssemblyNames.Length == 0)
            return;

        foreach (var bb in Model.BuildingBlocksAll)
        {
            var result = Types.InAssembly(bb)
                .ShouldNot()
                .HaveDependencyOnAny(forbiddenModuleAssemblyNames)
                .GetResult();

            Assert.True(result.IsSuccessful,
                $"{bb.GetName().Name}{Environment.NewLine}{FailureMessage(result)}");
        }
    }

}