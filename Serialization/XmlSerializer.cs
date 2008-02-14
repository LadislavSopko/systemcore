using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace System.Common.Serialization
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

        

        public void Serialize(Stream stream, TValue value)
        {
            XmlWriter writer = XmlWriter.Create(stream);
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



        private static TValue Deserialize(XmlReader reader)
        {
            return (TValue)CreateSerializer().Deserialize(reader);
        }


        private static void Serialize(XmlWriter writer, TValue value)
        {            
            CreateSerializer().Serialize(writer,value);
        }

        private static XmlSerializer CreateSerializer()
        {
            return new XmlSerializer(typeof(TValue));
        }


    }
}
