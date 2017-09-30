using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Xml2Dtd_Xsd.Model
{
    class XmlDictionaryElement
    {
        public IList<string> attributes = new List<string>();
        public IList<string> properties = new List<string>();
        public bool isEmptyElement {get;set;}

        public void addAttribute(string attribute)
        {
            if (!attributes.Contains(attribute))
                attributes.Add(attribute);
        }
        public void addProperty(string property)
        {
            if (!properties.Contains(property))
                properties.Add(property);
        }

        public XmlDictionaryElement(bool con)
        {
            this.isEmptyElement = con;
        }
    }
}
