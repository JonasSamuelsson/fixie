﻿using System;
using System.Collections.Generic;
using Fixie.Behaviors;

namespace Fixie.Conventions
{
    public class CaseBehaviorBuilder
    {
        readonly List<Type> customBehaviors = new List<Type>();

        public CaseBehaviorBuilder()
        {
            SkipPredicate = @case => false;
            SkipReasonProvider = SkipReasonUnknown;
        }

        public BehaviorChain<CaseExecution> BuildBehaviorChain()
        {
            var chain = new BehaviorChain<CaseExecution>();

            foreach (var customBehavior in customBehaviors)
                chain.Add((CaseBehavior)Activator.CreateInstance(customBehavior));

            chain.Add(new Invoke());

            return chain;
        }

        public Func<Case, bool> SkipPredicate { get; private set; }
        public Func<Case, string> SkipReasonProvider { get; private set; }

        public CaseBehaviorBuilder Wrap<TCaseBehavior>() where TCaseBehavior : CaseBehavior
        {
            customBehaviors.Insert(0, typeof(TCaseBehavior));
            return this;
        }

        public CaseBehaviorBuilder Skip(Func<Case, bool> skipPredicate)
        {
            return Skip(skipPredicate, SkipReasonUnknown);
        }

        public CaseBehaviorBuilder Skip(Func<Case, bool> skipPredicate, Func<Case, string> skipReasonProvider)
        {
            SkipPredicate = skipPredicate;
            SkipReasonProvider = skipReasonProvider;
            return this;
        }

        static string SkipReasonUnknown(Case @case)
        {
            return null;
        }
    }
}