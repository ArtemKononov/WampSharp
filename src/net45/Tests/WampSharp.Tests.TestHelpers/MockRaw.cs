﻿using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace WampSharp.Tests.TestHelpers
{
    public class MockRaw
    {
        private readonly object mValue;

        public MockRaw(object value)
        {
            MockRaw raw = value as MockRaw;
            object[] rawArray = value as object[];

            if (raw != null)
            {
                mValue = Clone(raw.Value);
            }
            else if (rawArray != null && rawArray.GetType() == typeof(object[]))
            {
                mValue = ConvertToMockRawArray(rawArray);
            }
            else
            {
                mValue = Clone(value);
            }
        }


        private static object ConvertToMockRawArray(object[] value)
        {
            return value.Select(x => new MockRaw(x)).ToArray();
        }

        private static object Clone(object value)
        {
            if (value == null)
            {
                return null;
            }

            object clone;
            if (TryClone(value, out clone))
            {
                return clone;
            }
            else
            {
                // Anonymous type
                Type type = value.GetType();

                if (type.IsDefined(typeof (CompilerGeneratedAttribute), true))
                {
                    object[] properties =
                        type.GetProperties()
                            .Select(x => x.GetValue(value, null)).ToArray();

                    return Activator.CreateInstance(type, properties);
                }
            }

            return value;
        }

#if !NETCORE

        private static bool TryClone(object value, out object result)
        {
            result = null;
            ICloneable cloneable = value as ICloneable;

            if (cloneable != null)
            {
                result = cloneable.Clone();
                return true;
            }

            return false;
        }

#else
        private static bool TryClone(object value, out object result)
        {
            result = null;
            return false;
        }

#endif

        public object Value
        {
            get
            {
                return mValue;
            }
        }
    }

#if WINDOWS_UWP
    internal static class ReflectionExtensions
    {
        public static bool IsDefined(this Type type, Type attributeType, bool inherit)
        {
            return type.GetTypeInfo().IsDefined(attributeType, inherit);
        }
    }
#endif
}