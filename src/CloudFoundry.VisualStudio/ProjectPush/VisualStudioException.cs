namespace CloudFoundry.VisualStudio.ProjectPush
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Threading.Tasks;

    [Serializable]
    public class VisualStudioException : Exception, ISerializable
    {
        public VisualStudioException()
        {
        }

        public VisualStudioException(string message) : base(message)
        {
        }

        public VisualStudioException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected VisualStudioException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
