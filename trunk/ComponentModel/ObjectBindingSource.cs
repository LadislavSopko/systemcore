using System.Collections.Generic;
using System.ComponentModel;
using System.Core.Design;
using System.Core.Helpers;
using System.Drawing;
using System.Windows.Forms;

namespace System.Core.ComponentModel
{   
    /// <summary>
    /// Extends the standard <see cref="BindingSource"/> to provide
    /// support for nested property accessors.
    /// </summary>
    [ToolboxBitmap(typeof(ResFinder), "System.Core.Bitmaps.ObjectDataSource.png")]
    [ToolboxItem(typeof(ObjectBindingSourceToolBoxItem))]
    public partial class ObjectBindingSource : BindingSource
    {
       
        #region Private Member Variables

        private readonly BindingList<BindableProperty> _bindableProperties = new BindingList<BindableProperty>();        

        private readonly List<PropertyDescriptor> _propertyDescriptors = new List<PropertyDescriptor>();

        private bool _createProperties = true;

        private bool _autoCreateObjects = false;


        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="ObjectBindingSource"/> component class.
        /// </summary>
        public ObjectBindingSource()
        {
            InitializeComponent();
            ((ISupportInitializeNotification)this).Initialized += ObjectBindingSource_Initialized;            
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ObjectBindingSource"/> component class.
        /// </summary>
        /// <param name="container"></param>
        public ObjectBindingSource(IContainer container)
        {
            container.Add(this);
            InitializeComponent();
            ((ISupportInitializeNotification)this).Initialized += ObjectBindingSource_Initialized;
        }
        
        #endregion


        private void CreatePropertyDescriptors()
        {            
            //Clear the previous list of property descriptors
            _propertyDescriptors.Clear();
            
            //Get the type of object that this bindingsource is bound to
            
            Type itemType = ListBindingHelper.GetListItemType(DataSource, DataMember);

            PropertyDescriptorCollection originalProperites = TypeDescriptor.GetProperties(itemType);            

            //Check to see if there are any bindable properties defined.
            if (BindableProperties.Count == 0)
            {
                foreach (PropertyDescriptor propertyDescriptor in originalProperites)
                {
                    Attribute[] attributes = new Attribute[propertyDescriptor.Attributes.Count];
                    propertyDescriptor.Attributes.CopyTo(attributes,0);
                    
                    _propertyDescriptors.Add(
                        new CustomPropertyDescriptor(propertyDescriptor.Name, ReflectionHelper.GetPropertyDescriptorFromPath(itemType, propertyDescriptor.Name), attributes,false));
                }
            }
            else
            {
                try
                {
                    foreach (BindableProperty bindableProperty in _bindableProperties)
                    {
                        //Get the original propertydescriptor based on the property path in bindableProperty
                        PropertyDescriptor propertyDescriptor = ReflectionHelper.GetPropertyDescriptorFromPath(itemType, bindableProperty.Name);
                        //Create a attribute array and make room for one more attribute 
                        Attribute[] attributes = new Attribute[propertyDescriptor.Attributes.Count + 1];
                        //Copy the original attributes to the custom descriptor
                        propertyDescriptor.Attributes.CopyTo(attributes, 0);
                        //Create a new attrute preserving information about the original property.
                        attributes[attributes.Length - 1] = new CustomPropertyAttribute(itemType, bindableProperty.Name, propertyDescriptor);
                        //Finally add the new custom property descriptor to the list of property descriptors
                       _propertyDescriptors.Add(new CustomPropertyDescriptor(bindableProperty.Name, propertyDescriptor, attributes,_autoCreateObjects));
                    }
                }
                catch
                {
                    //Something is wrong in the property path of one or more properties
                    _bindableProperties.Clear();                                      
                }
            }

            _createProperties = false;
            ResetBindings(true);
        }


        void ObjectBindingSource_Initialized(object sender, EventArgs e)
        {
            _bindableProperties.ListChanged += _bindableProperties_ListChanged;
            _createProperties = true;
        }



        void _bindableProperties_ListChanged(object sender, ListChangedEventArgs e)
        {
            _createProperties = true;
        }


        #region Protected Methods
        
        /// <summary>
        /// Raises the <see cref="BindingSource.DataMemberChanged"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> containing the event data.</param>
        protected override void OnDataMemberChanged(EventArgs e)
        {                        
            base.OnDataMemberChanged(e);
            _createProperties = true;
            ResetBindings(true);
        }

        /// <summary>
        /// Raises the <see cref="BindingSource.DataSourceChanged"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> containing the event data.</param>
        protected override void OnDataSourceChanged(EventArgs e)
        {                        
            base.OnDataSourceChanged(e);
            _createProperties = true;
            ResetBindings(true);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets a list containing the bindable properties.
        /// </summary>
        [Category("Data")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]        
        public BindingList<BindableProperty> BindableProperties
        {
            get { return _bindableProperties; }
        }

        /// <summary>
        /// Indicates if objects should be automatically created when setting property path values.
        /// </summary>
        [Category("Data")]        
        public bool AutoCreateObjects
        {
            get { return _autoCreateObjects; }
            set { _autoCreateObjects = value; }
        }

        #endregion 

        #region Public Methods

        /// <summary>
        /// Retrieves an array of PropertyDescriptor objects representing the bindable properties of the data source list type. 
        /// </summary>
        /// <param name="listAccessors">An array of PropertyDescriptor objects to find in the list as bindable.</param>
        /// <returns>An array of PropertyDescriptor objects that represents the properties on this list type used to bind data. </returns>
        public override PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[] listAccessors)
        {            
            //Check to see if the descriptors should be recreated
            if (_createProperties)
                CreatePropertyDescriptors();
            //Check to see if we have a list of descriptors
            if (_propertyDescriptors.Count > 0)
                return new PropertyDescriptorCollection(_propertyDescriptors.ToArray());
            else
                //If not for some reason, we just revert to the default implementation
                return base.GetItemProperties(listAccessors);
        }

        #endregion

        

        
    }
}
