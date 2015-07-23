namespace CloudFoundry.VisualStudio
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;
    using CloudFoundry.CloudController.V2.Client.Data;

    public class XmlUri : IXmlSerializable
    {
        private Uri value;

        public XmlUri() 
        { 
        }

        public XmlUri(Uri source) 
        { 
            this.value = source; 
        }

        public static implicit operator Uri(XmlUri uriInfo)
        {
            return uriInfo == null ? null : uriInfo.value;
        }

        public static implicit operator XmlUri(Uri uriInfo)
        {
            return uriInfo == null ? null : new XmlUri(uriInfo);
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            if (reader != null)
            {
                this.value = new Uri(reader.ReadElementContentAsString());
            }
            else
            {
                throw new InvalidOperationException("Xml reader cannot be null");
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            if (writer != null)
            {
                writer.WriteValue(this.value.ToString());
            }
            else
            {
                throw new InvalidOperationException("Xml writer cannot be null");
            }
        }

        public override string ToString()
        {
            return this.value.ToString();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Validated")]
        public override bool Equals(object obj)
        {
            if (this == null && obj == null)
            {
                return true;
            }

            return this.ToString() == obj.ToString();
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
