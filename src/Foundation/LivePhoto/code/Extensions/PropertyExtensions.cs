using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Glass.Mapper;
using Sitecore.Collections;
using Sitecore.Diagnostics;

namespace Paragon.Foundation.LivePhoto.Extensions
{
    public class PropertyExtensions
    {
        public static BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | 
                                           BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;

        public static PropertyInfo GetProperty(Type type, string name)
        {
            var propertyInfo = (PropertyInfo) null;

            try
            {
                propertyInfo = type.GetProperty(name, Flags);
            }
            catch (AmbiguousMatchException ex)
            {
                Log.Error("Live Photo GetProperty AmbiguousMatchException", ex);
            }

            if (propertyInfo != null)
                return propertyInfo;

            foreach (var type1 in type.GetInterfaces())
            {
                try
                {
                    propertyInfo = type1.GetProperty(name);

                    if (propertyInfo != null)
                        return propertyInfo;
                }
                catch (AmbiguousMatchException ex)
                {
                    Log.Error("Live Photo GetProperty AmbiguousMatchException", ex);
                }
            }

            return propertyInfo;
        }
        
        public static IEnumerable<PropertyInfo> GetAllProperties(Type type)
        {
            var typeList = new List<Type> {type};

            if (type.IsInterface)
                typeList.AddRange(type.GetInterfaces());

            return (from t in typeList
                from propertyInfo in t.GetProperties(Flags)
                select GetProperty(propertyInfo.DeclaringType, propertyInfo.Name)).ToList();
        }

        public static NameValueCollection GetPropertiesCollection(object target, bool lowerCaseName = false,
            bool underscoreForHyphens = true)
        {
            var nameValueCollection = new NameValueCollection();

            if (target == null)
                return nameValueCollection;

            foreach (var propertyInfo in GetAllProperties(target.GetType()))
            {
                var obj = propertyInfo.GetValue(target, null);

                if (obj == null)
                    continue;

                var name = lowerCaseName ? propertyInfo.Name.ToLower() : propertyInfo.Name;

                if (underscoreForHyphens)
                    name = name.Replace("_", "-");

                nameValueCollection.Add(name, obj.ToString());
            }

            return nameValueCollection;
        }

        public static string ConvertAttributes(SafeDictionary<string> attributes, string quotationMark)
        {
            if (attributes == null || !attributes.Any())
                return string.Empty;

            var stringBuilder = new StringBuilder();

            using (var enumerator = attributes.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var current = enumerator.Current;
                    stringBuilder.AppendFormat("{0}={2}{1}{2} ", current.Key, current.Value ?? string.Empty,
                        quotationMark);
                }
            }

            return stringBuilder.ToString();
        }

        public static SafeDictionary<string> AddAttribute(SafeDictionary<string> collection, string name, 
            string value)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(value))
                return collection;

            if (collection.ContainsKey(name))
            {
                // Concate Value if key exists and value isn't empty, ie. class
                var keyVal = collection.FirstOrDefault(k => k.Key.Equals(name));

                if (string.IsNullOrWhiteSpace(keyVal.Value))
                    return collection;

                collection.Remove(name);

                // Exising Value could have multiple values ie. multiple css classes.
                var valList = keyVal.Value.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries).ToList();

                if (!valList.Contains(value))
                    valList.Add(value);

                value = string.Join(" ", valList);

                collection.Add(name, string.Concat(keyVal.Value, value));
            }
            else if (!string.IsNullOrWhiteSpace(name))
                collection.Add(name, value);

            return collection;
        }

        public static object GetTargetObjectOfLamba<T>(Expression<Func<T, object>> field, T model,
            out MemberExpression memberExpression)
        {
            if (field.Parameters.Any())
                throw new MapperException($"To many parameters in linq expression {field.Body}");

            var fieldBody = field.Body as UnaryExpression;

            if (fieldBody != null)
                memberExpression = fieldBody.Operand as MemberExpression;
            else
            {
                if (!(field.Body is MemberExpression))
                    throw new MapperException($"Expression doesn't evaluate to a member {field.Body}");

                memberExpression = (MemberExpression) field.Body;
            }

            return
                Expression.Lambda(memberExpression.Expression, field.Parameters).Compile().DynamicInvoke(model);
        }
    }
}   