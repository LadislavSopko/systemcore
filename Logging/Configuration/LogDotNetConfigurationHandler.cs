using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Xml;
using System.Collections;


namespace System.Core.Logging
{
    /// <summary>
    /// Reads the LogDotNet configuration section in the application configuration file.
    /// </summary>
    internal class LogDotNetConfigurationHandler : IConfigurationSectionHandler
    {

        #region IConfigurationSectionHandler Members

        /// <summary>
        /// Reads the LogDotnet configurstion section in the applications configuration file
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="configContext"></param>
        /// <param name="section"></param>
        /// <returns>Return a list of <see cref="SinkConfigurationElement"/></returns>
        public object Create(object parent, object configContext, System.Xml.XmlNode section)
        {

            List<SinkConfigurationElement> sinks = new List<SinkConfigurationElement>();

            if (section.HasChildNodes == true)
            {
                foreach (XmlNode sinkNode in section.ChildNodes)
                {
                    if ((sinkNode is XmlElement))
                    {
                        SinkConfigurationElement sink = new SinkConfigurationElement(sinkNode);

                        foreach (XmlNode properyNode in sinkNode.ChildNodes)
                        {
                            sink.Properties.Add(properyNode.Name, properyNode.Attributes[0].Value);
                        }
                        sinks.Add(sink);
                    }
                }
            }
            return sinks;
        }
        #endregion
    }
}
