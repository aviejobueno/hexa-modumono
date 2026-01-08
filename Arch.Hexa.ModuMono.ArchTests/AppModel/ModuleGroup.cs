using System.Reflection;

namespace Arch.Hexa.ModuMono.ArchTests.AppModel;

public record ModuleGroup(
    string Name,
    Assembly? Api,
    Assembly? Application,
    Assembly? Domain,
    Assembly? Infrastructure);