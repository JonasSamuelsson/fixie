﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Fixie
{
    public static class ReflectionExtensions
    {
        public static bool Void(this MethodInfo method)
        {
            return method.ReturnType == typeof(void);
        }

        public static bool Has<TAttribute>(this MethodInfo method) where TAttribute : Attribute
        {
            return method.GetCustomAttributes<TAttribute>(false).Any();
        }

        public static bool Async(this MethodInfo method)
        {
            return method.Has<AsyncStateMachineAttribute>();
        }

        public static bool IsDispose(this MethodInfo method)
        {
            var hasDisposeSignature = method.Name == "Dispose" && method.Void() && method.GetParameters().Length == 0;

            if (!hasDisposeSignature)
                return false;

            return method.ReflectedType.GetInterfaces().Any(type => type == typeof(IDisposable));
        }

        public static bool IsInNamespace(this Type type, string ns)
        {
            var actual = type.Namespace;

            if (ns == null)
                return actual == null;

            if (actual == null)
                return false;

            return actual == ns || actual.StartsWith(ns + ".");
        }
    }
}