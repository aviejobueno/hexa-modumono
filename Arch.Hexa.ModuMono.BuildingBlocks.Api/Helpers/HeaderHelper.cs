using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Arch.Hexa.ModuMono.BuildingBlocks.Api.Helpers;

public static class HeaderHelper
{ 
    public static Dictionary<string, string?> ToDictionarySingleValue(IHeaderDictionary? headers)
    {
        return headers?.ToDictionary(
            h => h.Key, string? (h) => h.Value.ToString())!; 
    }

    public static Dictionary<string, string[]> ToDictionaryAllValues(IHeaderDictionary? headers)
    {
        return headers?.ToDictionary(
            h => h.Key,
            h => h.Value.ToArray())!;
    }

    public static Dictionary<string, StringValues> ToDictionaryStringValues(IHeaderDictionary? headers)
    {
        return headers?.ToDictionary(h => h.Key, h => h.Value)!;
    }

}