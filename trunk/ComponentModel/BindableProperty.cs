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
        /// <param name="caption">When the property is displayed in ex. a grid control, the nested property name 
        /// will display as Person_Address_StreetAddress in order to make the property unique.        /// 
        /// <remarks>
        /// 
        /// </remarks>
        /// </param>
        
        public BindableProperty(string name, string caption)
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
