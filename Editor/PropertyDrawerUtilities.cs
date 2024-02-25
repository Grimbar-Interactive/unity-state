using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using GI.UnityToolkit.Attributes;
#endif

namespace GI.UnityToolkit.State.Editor
{
    static class PropertyDrawerUtilities
    {
        public static T GetAttribute<T>(SerializedProperty property) where T : class
        {
            var attributes = GetAttributes<T>(property);
            return (attributes.Length > 0) ? attributes[0] : null;
        }

        public static T[] GetAttributes<T>(SerializedProperty property) where T : class
        {
            var fieldInfo = ReflectionUtility.GetField(GetTargetObjectWithProperty(property), property.name);
            if (fieldInfo == null)
            {
                return new T[] { };
            }

            return (T[])fieldInfo.GetCustomAttributes(typeof(T), true);
        }

        public static void CallOnValueChangedCallbacks(SerializedProperty property)
        {
            var onValueChangedAttributes = GetAttributes<OnValueChangedAttribute>(property);
            if (onValueChangedAttributes.Length == 0)
            {
                return;
            }

            var target = GetTargetObjectWithProperty(property);
            property.serializedObject
                .ApplyModifiedProperties(); // We must apply modifications so that the new value is updated in the serialized object

            foreach (var onValueChangedAttribute in onValueChangedAttributes)
            {
#if ODIN_INSPECTOR
                var callbackMethod = ReflectionUtility.GetMethod(target, onValueChangedAttribute.Action);
#else
                var callbackMethod = ReflectionUtility.GetMethod(target, onValueChangedAttribute.CallbackName);
#endif
                if (callbackMethod != null &&
                    callbackMethod.ReturnType == typeof(void) &&
                    callbackMethod.GetParameters().Length == 0)
                {
                    callbackMethod.Invoke(target, new object[] { });
                }
                else
                {
                    var warning =
                        $"{onValueChangedAttribute.GetType().Name} can invoke only methods with 'void' return type and 0 parameters";
                    Debug.LogWarning(warning, property.serializedObject.targetObject);
                }
            }
        }

        /// <summary>
        /// Gets the object the property represents.
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public static object GetTargetObjectOfProperty(SerializedProperty property)
        {
            if (property == null)
            {
                return null;
            }

            var path = property.propertyPath.Replace(".Array.data[", "[");
            object obj = property.serializedObject.targetObject;
            var elements = path.Split('.');

            foreach (var element in elements)
            {
                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("[", StringComparison.Ordinal));
                    var index = Convert.ToInt32(element.Substring(element.IndexOf("[", StringComparison.Ordinal)).Replace("[", "")
                        .Replace("]", ""));
                    obj = GetValue_Imp(obj, elementName, index);
                }
                else
                {
                    obj = GetValue_Imp(obj, element);
                }
            }

            return obj;
        }

        /// <summary>
        /// Gets the object that the property is a member of
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public static object GetTargetObjectWithProperty(SerializedProperty property)
        {
            var path = property.propertyPath.Replace(".Array.data[", "[");
            object obj = property.serializedObject.targetObject;
            var elements = path.Split('.');

            for (var i = 0; i < elements.Length - 1; i++)
            {
                var element = elements[i];
                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("[", StringComparison.Ordinal));
                    var index = Convert.ToInt32(element.Substring(element.IndexOf("[", StringComparison.Ordinal)).Replace("[", "")
                        .Replace("]", ""));
                    obj = GetValue_Imp(obj, elementName, index);
                }
                else
                {
                    obj = GetValue_Imp(obj, element);
                }
            }

            return obj;
        }

        private static object GetValue_Imp(object source, string name)
        {
            if (source == null)
            {
                return null;
            }

            var type = source.GetType();

            while (type != null)
            {
                var field = type.GetField(name,
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (field != null)
                {
                    return field.GetValue(source);
                }

                var property = type.GetProperty(name,
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (property != null)
                {
                    return property.GetValue(source, null);
                }

                type = type.BaseType;
            }

            return null;
        }

        private static object GetValue_Imp(object source, string name, int index)
        {
            var enumerable = GetValue_Imp(source, name) as IEnumerable;
            if (enumerable == null)
            {
                return null;
            }

            var enumerator = enumerable.GetEnumerator();
            for (var i = 0; i <= index; i++)
            {
                if (!enumerator.MoveNext())
                {
                    return null;
                }
            }

            return enumerator.Current;
        }

        static class ReflectionUtility
        {
            public static IEnumerable<FieldInfo> GetAllFields(object target, Func<FieldInfo, bool> predicate)
            {
                if (target == null)
                {
                    Debug.LogError("The target object is null. Check for missing scripts.");
                    yield break;
                }

                var types = GetSelfAndBaseTypes(target);

                for (var i = types.Count - 1; i >= 0; i--)
                {
                    var fieldInfos = types[i]
                        .GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic |
                                   BindingFlags.Public | BindingFlags.DeclaredOnly)
                        .Where(predicate);

                    foreach (var fieldInfo in fieldInfos)
                    {
                        yield return fieldInfo;
                    }
                }
            }

            public static IEnumerable<MethodInfo> GetAllMethods(object target, Func<MethodInfo, bool> predicate)
            {
                if (target == null)
                {
                    Debug.LogError("The target object is null. Check for missing scripts.");
                    yield break;
                }

                var types = GetSelfAndBaseTypes(target);

                for (var i = types.Count - 1; i >= 0; i--)
                {
                    var methodInfos = types[i]
                        .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic |
                                    BindingFlags.Public | BindingFlags.DeclaredOnly)
                        .Where(predicate);

                    foreach (var methodInfo in methodInfos)
                    {
                        yield return methodInfo;
                    }
                }
            }

            public static FieldInfo GetField(object target, string fieldName)
            {
                return GetAllFields(target, f => f.Name.Equals(fieldName, StringComparison.Ordinal)).FirstOrDefault();
            }

            public static MethodInfo GetMethod(object target, string methodName)
            {
                return GetAllMethods(target, m => m.Name.Equals(methodName, StringComparison.Ordinal)).FirstOrDefault();
            }

            /// <summary>
            ///		Get type and all base types of target, sorted as following:
            ///		<para />[target's type, base type, base's base type, ...]
            /// </summary>
            /// <param name="target"></param>
            /// <returns></returns>
            private static List<Type> GetSelfAndBaseTypes(object target)
            {
                var types = new List<Type>()
                {
                    target.GetType()
                };

                while (types.Last().BaseType != null)
                {
                    types.Add(types.Last().BaseType);
                }

                return types;
            }
        }
    }
}