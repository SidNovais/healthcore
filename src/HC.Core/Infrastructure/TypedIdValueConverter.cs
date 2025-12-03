using System;
using HC.Core.Domain;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HC.Core.Infrastructure;

public class TypedIdValueConverter<TTypedIdValue>(
    ConverterMappingHints? mappingHints = null
) : ValueConverter<TTypedIdValue, Guid>(
    id => id.Value,
    value => Create(value),
    mappingHints
)
where TTypedIdValue : Id
{
    private static TTypedIdValue Create(Guid id)
    {
        object instance = Activator.CreateInstance(typeof(TTypedIdValue), id)
        ?? throw new InvalidOperationException($"Unable to create an instance of type {typeof(TTypedIdValue)} with value {id}.");
        return (TTypedIdValue)instance;
    }
}
