using CloudFoundry.CloudController.V2.Client.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace CloudFoundry.VisualStudio
{
    public class XmlUri : IXmlSerializable
    {
        private Uri value;

        public XmlUri() { }

        public XmlUri(Uri source) { value = source; }

        public static implicit operator Uri(XmlUri o)
        {
            return o == null ? null : o.value;
        }

        public static implicit operator XmlUri(Uri o)
        {
            return o == null ? null : new XmlUri(o);
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            value = new Uri(reader.ReadElementContentAsString());
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteValue(value.ToString());
        }

        public override string ToString()
        {
            return this.value.ToString();
        }

        public override bool Equals(object obj)
        {
            if (this == null && obj == null)
            {
                return true;
            }

            return this.ToString() == obj.ToString();
        }
    }
}
