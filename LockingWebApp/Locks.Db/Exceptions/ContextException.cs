using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace LockingWebApp.Locks.Db.Exceptions
{
    [Serializable]
    public class ContextException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public ContextException()
        {
        }

        public ContextException(string message)
            : base(message)
        {
        }

        public ContextException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected ContextException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }
}