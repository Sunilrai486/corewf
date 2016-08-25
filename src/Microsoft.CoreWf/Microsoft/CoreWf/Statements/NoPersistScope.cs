﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CoreWf.Runtime;
using Microsoft.CoreWf.Validation;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Microsoft.CoreWf.Statements
{
    //[ContentProperty("Body")]
    public sealed class NoPersistScope : NativeActivity
    {
        private static Constraint s_constraint;

        private Variable<NoPersistHandle> _noPersistHandle;

        public NoPersistScope()
        {
            _noPersistHandle = new Variable<NoPersistHandle>();
            this.Constraints.Add(NoPersistScope.Constraint);
        }

        [DefaultValue(null)]
        public Activity Body
        {
            get;
            set;
        }

        private static Constraint Constraint
        {
            get
            {
                if (s_constraint == null)
                {
                    s_constraint = NoPersistScope.NoPersistInScope();
                }

                return s_constraint;
            }
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            metadata.AddChild(this.Body);
            metadata.AddImplementationVariable(_noPersistHandle);
        }

        protected override void Execute(NativeActivityContext context)
        {
            if (this.Body != null)
            {
                NoPersistHandle handle = _noPersistHandle.Get(context);
                handle.Enter(context);
                context.ScheduleActivity(this.Body);
            }
        }

        private static Constraint NoPersistInScope()
        {
            DelegateInArgument<ValidationContext> validationContext = new DelegateInArgument<ValidationContext>("validationContext");
            DelegateInArgument<NoPersistScope> noPersistScope = new DelegateInArgument<NoPersistScope>("noPersistScope");
            Variable<bool> isConstraintSatisfied = new Variable<bool>("isConstraintSatisfied", true);
            Variable<IEnumerable<Activity>> childActivities = new Variable<IEnumerable<Activity>>("childActivities");
            Variable<string> constraintViolationMessage = new Variable<string>("constraintViolationMessage");

            return new Constraint<NoPersistScope>
            {
                Body = new ActivityAction<NoPersistScope, ValidationContext>
                {
                    Argument1 = noPersistScope,
                    Argument2 = validationContext,
                    Handler = new Sequence
                    {
                        Variables =
                        {
                            isConstraintSatisfied,
                            childActivities,
                            constraintViolationMessage,
                        },
                        Activities =
                        {
                            new Assign<IEnumerable<Activity>>
                            {
                                To = childActivities,
                                Value = new GetChildSubtree
                                {
                                    ValidationContext = validationContext,
                                },
                            },
                            new Assign<bool>
                            {
                                To = isConstraintSatisfied,
                                Value = new CheckNoPersistInDescendants
                                {
                                    NoPersistScope = noPersistScope,
                                    DescendantActivities = childActivities,
                                    ConstraintViolationMessage = constraintViolationMessage,
                                },
                            },
                            new AssertValidation
                            {
                                Assertion = isConstraintSatisfied,
                                Message = constraintViolationMessage,
                            },
                        }
                    }
                }
            };
        }

        private sealed class CheckNoPersistInDescendants : CodeActivity<bool>
        {
            [RequiredArgument]
            public InArgument<NoPersistScope> NoPersistScope { get; set; }

            [RequiredArgument]
            public InArgument<IEnumerable<Activity>> DescendantActivities { get; set; }

            [RequiredArgument]
            public OutArgument<string> ConstraintViolationMessage { get; set; }

            protected override void CacheMetadata(CodeActivityMetadata metadata)
            {
                Collection<RuntimeArgument> runtimeArguments = new Collection<RuntimeArgument>();

                RuntimeArgument noPersistScopeArgument = new RuntimeArgument("NoPersistScope", typeof(NoPersistScope), ArgumentDirection.In);
                metadata.Bind(this.NoPersistScope, noPersistScopeArgument);
                runtimeArguments.Add(noPersistScopeArgument);

                RuntimeArgument descendantActivitiesArgument = new RuntimeArgument("DescendantActivities", typeof(IEnumerable<Activity>), ArgumentDirection.In);
                metadata.Bind(this.DescendantActivities, descendantActivitiesArgument);
                runtimeArguments.Add(descendantActivitiesArgument);

                RuntimeArgument constraintViolationMessageArgument = new RuntimeArgument("ConstraintViolationMessage", typeof(string), ArgumentDirection.Out);
                metadata.Bind(this.ConstraintViolationMessage, constraintViolationMessageArgument);
                runtimeArguments.Add(constraintViolationMessageArgument);

                RuntimeArgument resultArgument = new RuntimeArgument("Result", typeof(bool), ArgumentDirection.Out);
                metadata.Bind(this.Result, resultArgument);
                runtimeArguments.Add(resultArgument);

                metadata.SetArgumentsCollection(runtimeArguments);
            }

            protected override bool Execute(CodeActivityContext context)
            {
                IEnumerable<Activity> descendantActivities = this.DescendantActivities.Get(context);
                Fx.Assert(descendantActivities != null, "this.DescendantActivities cannot evaluate to null.");

                Persist firstPersist = descendantActivities.OfType<Persist>().FirstOrDefault();
                if (firstPersist != null)
                {
                    NoPersistScope noPersistScope = this.NoPersistScope.Get(context);
                    Fx.Assert(noPersistScope != null, "this.NoPersistScope cannot evaluate to null.");

                    string constraintViolationMessage = SR.NoPersistScopeCannotContainPersist(noPersistScope.DisplayName, firstPersist.DisplayName);
                    this.ConstraintViolationMessage.Set(context, constraintViolationMessage);
                    return false;
                }

                return true;
            }
        }
    }
}
