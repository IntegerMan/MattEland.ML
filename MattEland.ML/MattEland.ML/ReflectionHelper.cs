using System.Reflection;
using System.Text;

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
        => (T?)GetReflectedValue(obj, fieldName);

    public static object? GetReflectedValue(this object obj, string fieldName)
    {
        FieldInfo? field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
        
        return field?.GetValue(obj);
    }    

    public static string GetShortTypeName(this Type type)
    {
        StringBuilder sb = new();
        if (type.DeclaringType != null)
        {
            sb.Append(GetShortTypeName(type.DeclaringType));
        }

        string name = type.Name;
        if (name.Contains('`'))
        {
            name = name[..name.IndexOf('`')];
        }
        sb.Append(name);

        if (type.GenericTypeArguments.Any())
        {
            sb.Append('<');
            sb.Append(string.Join(',', type.GenericTypeArguments.Select(GetShortTypeName)));
            sb.Append('>');
        }

        return sb.ToString();
    }
}