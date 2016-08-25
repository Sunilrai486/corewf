﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Xml.Linq;

namespace Microsoft.CoreWf.Runtime.DurableInstancing
{
    //[Serializable]
    public class InstancePersistenceException : Exception
    {
        private const string CommandNameName = "instancePersistenceCommandName";

        public InstancePersistenceException()
            : base(ToMessage(null))
        {
        }

        public InstancePersistenceException(string message)
            : base(message)
        {
        }

        public InstancePersistenceException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public InstancePersistenceException(XName commandName)
            : this(commandName, ToMessage(commandName))
        {
        }

        public InstancePersistenceException(XName commandName, Exception innerException)
            : this(commandName, ToMessage(commandName), innerException)
        {
        }

        public InstancePersistenceException(XName commandName, string message)
            : base(message)
        {
            CommandName = commandName;
        }

        public InstancePersistenceException(XName commandName, string message, Exception innerException)
            : base(message, innerException)
        {
            CommandName = commandName;
        }

        //[SecurityCritical]
        //protected InstancePersistenceException(SerializationInfo info, StreamingContext context)
        //    : base(info, context)
        //{
        //    CommandName = info.GetValue(CommandNameName, typeof(XName)) as XName;
        //}

        public XName CommandName { get; private set; }

        //[Fx.Tag.SecurityNote(Critical = "Overrides critical inherited method")]
        //[SecurityCritical]
        ////[SuppressMessage(FxCop.Category.Security, FxCop.Rule.SecureGetObjectDataOverrides,
        //    //Justification = "Method is SecurityCritical")]
        //public override void GetObjectData(SerializationInfo info, StreamingContext context)
        //{
        //    base.GetObjectData(info, context);
        //    info.AddValue(CommandNameName, CommandName, typeof(XName));
        //}

        private static string ToMessage(XName commandName)
        {
            return commandName == null ? SRCore.GenericInstanceCommandNull : SRCore.GenericInstanceCommand(commandName);
        }
    }
}
