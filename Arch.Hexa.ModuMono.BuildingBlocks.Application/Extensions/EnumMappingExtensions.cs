using System.Collections.Concurrent;

namespace Arch.Hexa.ModuMono.BuildingBlocks.Application.Extensions;

public static class EnumMappingExtensions
{
    private static readonly ConcurrentDictionary<(Type From, Type To), Delegate> Cache = new();

    public static TTo MapToEnum<TTo>(this Enum from, bool ignoreCase = false)
        where TTo : struct, Enum
    {
        if (from is null) throw new ArgumentNullException(nameof(from));

        var fromType = from.GetType();
        var toType = typeof(TTo);

        var key = (fromType, toType);

        var mapper = (Func<Enum, bool, TTo>)Cache.GetOrAdd(key, _ =>
        {
            return (Enum value, bool ic) =>
            {
                var name = Enum.GetName(fromType, value);
                if (name is null)
                    throw new ArgumentException($"Value '{value}' is not defined in enum '{fromType.FullName}'.");

                if (!Enum.TryParse<TTo>(name, ignoreCase: ic, out var parsed))
                    throw new InvalidOperationException(
                        $"Cannot map enum name '{name}' from '{fromType.FullName}' to '{toType.FullName}'.");

                if (!Enum.IsDefined(toType, parsed))
                    throw new InvalidOperationException(
                        $"Mapped value '{parsed}' is not a defined member of '{toType.FullName}'.");

                return parsed;
            };
        });

        return mapper(from, ignoreCase);
    }
}
