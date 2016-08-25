// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.CoreWf
{
    //[Serializable]
    public class InvalidWorkflowException : Exception
    {
        public InvalidWorkflowException()
            : base(SR.DefaultInvalidWorkflowExceptionMessage)
        {
        }

        public InvalidWorkflowException(string message)
            : base(message)
        {
        }

        public InvalidWorkflowException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        //protected InvalidWorkflowException(SerializationInfo info, StreamingContext context)
        //    : base(info, context)
        //{
        //}
    }
}
