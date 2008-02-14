using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace System.Common.Logging
{
    /// <summary>
    /// Represents a configured sink in the applications configuration file
    /// </summary>
    internal class SinkConfigurationElement
    {
        #region Member Variables
        /// <summary>
        /// The type and the assembly containing the type.
        /// </summary>
        private string mType;
        /// <summary>
        /// The logging level threshold 
        /// </summary>
        private string  mLogLevel;
        /// <summary>
        /// The name of the sink
        /// </summary>
        private string mName;

        /// <summary>
        /// Contains the dynamic properites according to the derived sink type
        /// </summary>
        private Dictionary<string, string> mProperties = new Dictionary<string, string>();

        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new instance of the <see cref="SinkConfigurationElement"/> class.
        /// </summary>
        /// <param name="sinkNode">The <see cref="System.Xml.XmlNode"/> representing the configured sink</param>
        internal SinkConfigurationElement(XmlNode sinkNode)
        {
            mType = sinkNode.Attributes["type"].Value;
            mLogLevel = sinkNode.Attributes["loglevel"].Value;
            mName = sinkNode.Attributes["name"].Value;
        }
        #endregion

        #region Internal Properties
        /// <summary>
        /// Gets the name of the configured sink
        /// </summary>
        internal string Name
        {
            get { return mName; }
            
        }
	
        /// <summary>
        /// Gets the log level threshold of the configured sink
        /// </summary>
        internal string LogLevel
        {
            get { return mLogLevel; }
        }
	
        /// <summary>
        /// Gets the type and the assembly containging the type of the configured sink
        /// </summary>
        internal string Type
        {
            get { return mType; }
        }
        
        /// <summary>
        /// Returns a key/value based collection of dynamic properties of the configured sink
        /// </summary>
        internal Dictionary<string, string> Properties
        {
            get
            {
                return mProperties;
            }
        }
        #endregion
    }
	
}
