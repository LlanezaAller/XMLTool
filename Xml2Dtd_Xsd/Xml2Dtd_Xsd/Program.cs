using System;
using System.Collections.Generic;
using System.Linq;
using System.IO; 
using System.Xml;
using Xml2Dtd_Xsd.Model;
using System.Xml.Schema;

/// <summary>
/// Autor: Iñigo Llaneza Aller
/// Master de Ingenieria Web : Universidad de Oviedo
/// </summary>
namespace Xml2Dtd_Xsd
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string funcion = "";
#if DEBUG
                string file = "libros.xml";
                string validate = "file.xsd";
                XmlReader xml = XmlReader.Create(file);
#else
                if (args.Length < 2)
                    throw (new ArgumentNullException());

                String file = args[0];
                funcion = args[1];
#endif 

                if (!funcion.Contains('.'))
                    throw (new ArgumentException("Parametros: \"nameFile.xml validation.xsd\" o \"nameFile.xml validation.dtd\" "));

                if(!funcion.Split('.')[1].Contains("xsd") && !funcion.Split('.')[1].Contains("dtd"))
                    throw (new ArgumentException("Parametros: \"nameFile.xml validation.xsd\" o \"nameFile.xml validation.dtd\" "));

#if !DEBUG
                XmlReader xml = XmlReader.Create(file); 
#endif
                
                funcion = funcion.Split('.')[1];

                switch (funcion)
                {
                    case "dtd":
                        CreateDTD(CreateTree(xml), file.Split('.')[0] + ".dtd");
                        Console.WriteLine("Archivo {0} creado", file.Split('.')[0] + ".dtd");
                        break;
                    case "xsd":
                        CreateXSD(xml, file.Split('.')[0] + ".xsd");
                        Console.WriteLine("Archivo {0} creado", file.Split('.')[0] + ".xsd");
                        break;
                    default:
                        throw (new ArgumentException("Argumentos incorrectos"));
                }

            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Error: Archivo {0} no encontrado", args[0]);
            }
            catch (ArgumentNullException)
            {
                Console.WriteLine("Formato correcto de uso:");
                Console.WriteLine("\n\t TraductorXML <archivo>");
            }
            catch (Exception e)
            {
                Console.WriteLine("Error no documentado: " + e);
            }
            finally
            {
                Console.WriteLine("\nHerramienta con fines educativos");
                Console.WriteLine("Versión 1.0, 29-Septiembre-2017");
                Console.WriteLine("Autor: Iñigo Llaneza Aller");
                Console.WriteLine("Master de Ingeniería Web - Uniovi");
                Console.ReadLine();
            }

        }
        
        private static IDictionary<string, XmlDictionaryElement> CreateTree(XmlReader xml)
        {
            IDictionary<string, XmlDictionaryElement> xmlMap = new Dictionary<string, XmlDictionaryElement>();
            string parent = "";
            string actual = "";
            int actualDepth = 0;
            int parentDepth = 0;
            while (xml.Read())
            {
                switch (xml.NodeType)
                {
                    case XmlNodeType.Element:
                        if (xml.Depth == 0)
                        {
                            parent = xml.Name;
                            actual = xml.Name;
                            xmlMap.Add(xml.Name, new XmlDictionaryElement(xml.IsEmptyElement));
                        }
                        else
                        {
                            if (actualDepth < xml.Depth)
                            {
                                parent = actual;
                                parentDepth = xml.Depth - 1;
                            }
                            if (!xmlMap.ContainsKey(xml.Name))
                                xmlMap.Add(xml.Name, new XmlDictionaryElement(xml.IsEmptyElement));
                            actual = xml.Name;
                            xmlMap[parent].addProperty(xml.Name);
                            actualDepth = xml.Depth;
                            if (xml.HasAttributes)
                            {
                                while (xml.MoveToNextAttribute())
                                {
                                    xmlMap[actual].addAttribute(xml.Name);
                                }
                            }
                        }
                        break;
                    case XmlNodeType.EndElement:
                        for(int i = xmlMap.Keys.Count -1; i >= 0; i--)
                        {
                            if (xmlMap[xmlMap.Keys.ElementAt(i)].properties.Contains(xml.Name))
                                parent = xmlMap.Keys.ElementAt(i);
                        }
                        break;
                    case XmlNodeType.Text:
                        break;
                    case XmlNodeType.XmlDeclaration:
                        break;
                    case XmlNodeType.ProcessingInstruction:
                        break;
                    case XmlNodeType.Comment:
                        break;
                    case XmlNodeType.Whitespace:
                        break;
                    default:
                        return null;
                }//fin del switch
            }
            return xmlMap;
        }

        private static void CreateDTD(IDictionary<string, XmlDictionaryElement> xml, string fileName)
        {
            IList<string> dtdLines = new List<string>();
            XmlDictionaryElement xel;
            string prop = "";
            foreach (string key in xml.Keys)
            {
                xel = xml[key];
                if (xel.properties.Count > 0)
                {
                    foreach (string at in xel.properties)
                    {
                        if(xel.properties.ElementAt(0).Equals(at))
                            prop += string.Format("{0}*", at);
                        else
                            prop += string.Format(", {0}*", at);
                    }
                    dtdLines.Add(string.Format("<!ELEMENT {0} ({1})>", key, prop));
                    prop = "";
                }
                else
                {
                    if(xel.isEmptyElement)
                        dtdLines.Add(string.Format("<!ELEMENT {0} EMPTY>", key));
                    else
                       dtdLines.Add(string.Format("<!ELEMENT {0} (#PCDATA)>", key));
                }
            }

            dtdLines.Add("");
            dtdLines.Add("");

            foreach (string key in xml.Keys)
            {
                xel = xml[key];
                if (xel.attributes.Count > 0)
                {
                    foreach (string at in xel.attributes)
                        dtdLines.Add(string.Format("<!ATTLIST {0} {1} CDATA \"\">", key, at));

                }
            }

            try
            {
              System.IO.File.WriteAllLines(AppDomain.CurrentDomain.BaseDirectory + fileName, dtdLines);
                
            }
            catch (Exception f)
            {
                System.Diagnostics.Debug.Write(f);
            }
        }

        private static void CreateXSD(XmlReader xml, string fileName)
        {
            XmlSchemaSet schemaSet = new XmlSchemaSet();
            XmlSchemaInference schema = new XmlSchemaInference();
            while (xml.Read())
            {
                schemaSet = schema.InferSchema(xml);

                foreach (XmlSchema s in schemaSet.Schemas())
                {
                    XmlWriter writer;
                    foreach (XmlSchema t in schemaSet.Schemas())
                    {
                        writer = XmlWriter.Create(fileName);
                        t.Write(writer);
                        writer.Close();
                    }
                }
            }
            xml.Close();
        }
    }
}
