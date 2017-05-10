using System.Collections.Generic;
using System.Collections.Specialized;
using Sitecore.Collections;

namespace Paragon.Foundation.LivePhoto.Extensions
{
    public static class CollectionExtensions
    {
        public static NameValueCollection ToNameValueCollection<TKey, TValue>(this SafeDictionary<TKey, TValue> dict)
        {
            var nameValueCollection = new NameValueCollection();

            foreach (var kvp in dict)
            {
                var value = string.Empty;

                if (kvp.Value != null)
                    value = kvp.Value.ToString();

                nameValueCollection.Add(kvp.Key.ToString(), value);
            }

            return nameValueCollection;
        }
    }
}