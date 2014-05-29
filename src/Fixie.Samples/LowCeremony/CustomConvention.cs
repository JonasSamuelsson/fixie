﻿using System;
using System.Linq;
using System.Reflection;
using Fixie.Behaviors;
using Fixie.Conventions;

namespace Fixie.Samples.LowCeremony
{
    public class CustomConvention : Convention
    {
        static readonly string[] LifecycleMethods = new[] { "FixtureSetUp", "FixtureTearDown", "SetUp", "TearDown" };

        public CustomConvention()
        {
            Classes
                .Where(type => type.IsInNamespace(GetType().Namespace))
                .NameEndsWith("Tests");

            Methods
                .Where(method => method.IsVoid())
                .Where(method => LifecycleMethods.All(x => x != method.Name));

            ClassExecution
                .CreateInstancePerClass()
                .SortCases((caseA, caseB) => String.Compare(caseA.Name, caseB.Name, StringComparison.Ordinal));

            InstanceExecution
                .Wrap<CallFixtureSetUpTearDownMethodsByName>();

            CaseExecution
                .Wrap<CallSetUpTearDownMethodsByName>();
        }

        class CallSetUpTearDownMethodsByName : CaseBehavior
        {
            public void Execute(CaseExecution caseExecution, Action next)
            {
                caseExecution.Case.Class.TryInvoke("SetUp", caseExecution.Instance);
                next();
                caseExecution.Case.Class.TryInvoke("TearDown", caseExecution.Instance);
            }
        }

        class CallFixtureSetUpTearDownMethodsByName : InstanceBehavior
        {
            public void Execute(InstanceExecution instanceExecution, Action next)
            {
                instanceExecution.TestClass.TryInvoke("FixtureSetUp", instanceExecution.Instance);
                next();
                instanceExecution.TestClass.TryInvoke("FixtureTearDown", instanceExecution.Instance);
            }
        }
    }

    public static class BehaviorBuilderExtensions
    {
        public static void TryInvoke(this Type type, string method, object instance)
        {
            var lifecycleMethod =
                new MethodFilter()
                    .Where(x => x.HasSignature(typeof(void), method))
                    .Filter(type)
                    .SingleOrDefault();

            if (lifecycleMethod == null)
                return;

            try
            {
                lifecycleMethod.Invoke(instance, null);
            }
            catch (TargetInvocationException exception)
            {
                throw new PreservedException(exception.InnerException);
            }
        }
    }
}