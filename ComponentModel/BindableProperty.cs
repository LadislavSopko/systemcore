using System;
using System.Collections.Generic;
using System.Text;

namespace System.Core.ComponentModel
{
    /// <summary>
    /// Represent a bindable propery 
    /// </summary>
    [Serializable]
    public class BindableProperty
    {
        private string _name;        

        /// <summary>
        /// Creates a new instance of the <see cref="BindableProperty"/> class.
        /// </summary>
        public BindableProperty()
        {
        }

        
        /// <summary>
        /// Creates a new instance of the <see cref="BindableProperty"/> class.
        /// </summary>
        /// <param name="name">The name/property path of the bindable property.
        /// <remarks>
        /// Nested properies can be described as property paths. Ex Person.Address.StreetAddress
        /// </remarks>
        /// </param>                
        /// </param>        
        public BindableProperty(string name)
        {
            _name = name;
        }


        /// <summary>
        /// Gets or sets the name of the property.
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        
    }
}
