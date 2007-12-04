using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace System.Core.Serialization
{
    
    public class XmlSerializer<TValue>  :IXmlSerializer<TValue>
    {
        public void Serialize(string fileName,TValue value)
        {
            XmlWriter writer = XmlWriter.Create(fileName);
            try
            {
                Serialize(writer, value);    
            }
            finally
            {
                writer.Close();
            }
            
        }

        public TValue Deserialize(string fileName)
        {
            XmlReader reader = XmlReader.Create(fileName);
            try
            {
                return Deserialize(reader);
            }
            finally
            {
                reader.Close();
            }
        }



        private TValue Deserialize(XmlReader reader)
        {
            return (TValue)CreateSerializer().Deserialize(reader);
        }


        private void Serialize(XmlWriter writer, TValue value)
        {            
            CreateSerializer().Serialize(writer,value);
        }

        private XmlSerializer CreateSerializer()
        {
            return new XmlSerializer(typeof(TValue));
        }


    }
}
