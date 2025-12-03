using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using HC.Core.Domain;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HC.Core.Infrastructure;

/// <summary>
/// Based on https://andrewlock.net/strongly-typed-ids-in-ef-core-using-strongly-typed-entity-ids-to-avoid-primitive-obsession-part-4/
/// </summary>
public class StronglyTypedIdValueConverterSelector(ValueConverterSelectorDependencies dependencies) : ValueConverterSelector(dependencies)
{
    private readonly ConcurrentDictionary<(Type ModelClrType, Type ProviderClrType), ValueConverterInfo> _converters
        = new();
    public override IEnumerable<ValueConverterInfo> Select(Type modelClrType, Type? providerClrType = null)
    {
        IEnumerable<ValueConverterInfo> baseConverters = base.Select(modelClrType, providerClrType);
        foreach (ValueConverterInfo converter in baseConverters)
            yield return converter;
        Type? underlyingModelType = UnwrapNullableType(modelClrType);
        Type? underlyingProviderType = UnwrapNullableType(providerClrType);
        if (underlyingProviderType is null || underlyingProviderType == typeof(Guid))
        {
            bool isTypedIdValue = typeof(Id).IsAssignableFrom(underlyingModelType);
            if (isTypedIdValue && underlyingModelType is not null)
            {
                Type converterType = typeof(TypedIdValueConverter<>).MakeGenericType(underlyingModelType);
                yield return _converters.GetOrAdd((underlyingModelType, typeof(Guid)), _ =>
                {
                    if (underlyingModelType is null)
                        throw new InvalidOperationException("Underlying model type cannot be null.");

                    return new ValueConverterInfo(
                    modelClrType: modelClrType,
                    providerClrType: typeof(Guid),
                    factory: valueConverterInfo =>
                    {
                        ValueConverter instance = (ValueConverter?)Activator.CreateInstance(converterType, valueConverterInfo.MappingHints)
                        ?? throw new InvalidOperationException($"Failed to create an instance of {converterType}");
                        return instance!;
                    });
                });
            }
        }
    }

    private static Type? UnwrapNullableType(Type? type)
    {
        if (type is null)
            return null;
        return Nullable.GetUnderlyingType(type) ?? type;
    }
}
