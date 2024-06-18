using System.Reflection;

namespace MattEland.ML;

public static class ReflectionHelper
{
    public static Dictionary<string, object?> AsReflectedDictionary(this object? obj)
    {
        if (obj == null) return new Dictionary<string, object?>();
        
        IEnumerable<FieldInfo> fields = obj.GetType()
            .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        return fields.ToDictionary(f => f.Name, f => f.GetValue(obj));
    }

    public static T? GetReflectedValue<T>(this object obj, string fieldName)
    {
        FieldInfo? field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
        if (field == null)
        {
            throw new InvalidOperationException($"{obj.GetType().FullName} does not have a reflectable property or field named {fieldName}");
        }

        return (T?)field.GetValue(obj);
    }
}