using System.Collections;
using System.ComponentModel;

namespace HardDev.CoreUtils.Config;

/// <summary>
/// Represents a default value attribute for collections.
/// </summary>
public class CollectionDefaultValueAttribute : DefaultValueAttribute
{
    /// <summary>
    /// Initializes a new instance of the CollectionDefaultValueAttribute class with the specified collection type and values.
    /// </summary>
    /// <param name="collectionType">The type of collection to create.</param>
    /// <param name="values">An array of values to populate the collection with.</param>
    public CollectionDefaultValueAttribute(Type collectionType, params object[] values)
        : base(CreateDefaultCollection(collectionType, values))
    {
    }

    private static object CreateDefaultCollection(Type collectionType, object[] values)
    {
        if (collectionType.IsArray)
        {
            var elementType = collectionType.GetElementType();
            var array = Array.CreateInstance(elementType!, values.Length);
            values.CopyTo(array, 0);
            return array;
        }

        if (collectionType.IsGenericType)
        {
            if (collectionType.GetGenericTypeDefinition() == typeof(List<>))
            {
                var elementType = collectionType.GetGenericArguments().Single();
                var listType = typeof(List<>).MakeGenericType(elementType);
                var list = Activator.CreateInstance(listType) as IList;
                foreach (var value in values)
                {
                    list!.Add(value);
                }

                return list;
            }

            if (collectionType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                var keyType = collectionType.GetGenericArguments()[0];
                var valueType = collectionType.GetGenericArguments()[1];
                var dictionaryType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
                var dictionary = Activator.CreateInstance(dictionaryType) as IDictionary;

                if (values.Length % 2 != 0)
                {
                    throw new ArgumentException("For a dictionary, an even number of values is required.");
                }

                for (var i = 0; i < values.Length; i += 2)
                {
                    dictionary!.Add(values[i], values[i + 1]);
                }

                return dictionary;
            }
        }

        throw new ArgumentException("Unsupported collection type.", nameof(collectionType));
    }
}