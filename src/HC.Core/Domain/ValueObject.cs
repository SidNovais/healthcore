using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HC.Core.Domain;

public abstract class ValueObject : IEquatable<ValueObject>
{
    private IList<PropertyInfo>? _properties;
    private IList<FieldInfo>? _fields;
    public static bool operator ==(ValueObject leftValueObject, ValueObject rightValueObject)
    {
        if (object.Equals(leftValueObject, null))
        {
            if (object.Equals(rightValueObject, null)) return true;
            return false;
        }
        return leftValueObject.Equals(rightValueObject);
    }

    public static bool operator !=(ValueObject leftValueObject, ValueObject rightValueObject)
    {
        return !(leftValueObject == rightValueObject);
    }

    public bool Equals(ValueObject? valueObject)
    {
        return Equals(valueObject as object);
    }

    public override bool Equals(object? obj)
    {
        if (obj == null || GetType() != obj.GetType()) return false;
        return GetProperties().All(property => PropertiesAreEqual(obj, property))
          && GetFields().All(field => FieldsAreEqual(obj, field))
        ;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            foreach (PropertyInfo property in GetProperties())
            {
                object? value = property.GetValue(this, null);
                hash = HashValue(hash, value);
            }
            foreach (FieldInfo field in GetFields())
            {
                object? value = field.GetValue(this);
                hash = HashValue(hash, value);
            }
            return hash;
        }
    }

    protected static void CheckRule(IBusinessRule businessRule)
    {
        ArgumentNullException.ThrowIfNull(businessRule, "BusinessRule cannot be null");
        if (businessRule.IsBroken()) throw new BaseBusinessRuleException(businessRule);
    }

    private bool PropertiesAreEqual(object @object, PropertyInfo propertyInfo)
    {
        return object.Equals(propertyInfo.GetValue(this, null), propertyInfo.GetValue(@object, null));
    }

    private bool FieldsAreEqual(object @object, FieldInfo fieldInfo)
    {
        return object.Equals(fieldInfo.GetValue(this), fieldInfo.GetValue(@object));
    }

    private IEnumerable<PropertyInfo> GetProperties()
    {
        _properties ??= [.. GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)];
        return _properties;
    }

    private IEnumerable<FieldInfo> GetFields()
    {
        _fields ??= [.. GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)];
        return _fields;
    }

    private static int HashValue(int seed, object? value)
    {
        int currentHash = value?.GetHashCode() ?? 0;
        return (seed * 23) + currentHash;
    }
}
