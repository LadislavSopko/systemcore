using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace System.Common.Collections
{



    public class BindingListView<T> : BindingList<T>, IBindingListView,
       IRaiseItemChangedEvents
    {
        private bool m_Sorted = false;
        private bool m_Filtered = false;
        private string m_FilterString = null;
        private ListSortDirection m_SortDirection =
           ListSortDirection.Ascending;
        private PropertyDescriptor m_SortProperty = null;
        private ListSortDescriptionCollection m_SortDescriptions =
         new ListSortDescriptionCollection();
        private List<T> m_OriginalCollection = new List<T>();

        public BindingListView()           
        {
        }

        public BindingListView(IList<T> list)
            : base(list)
        {
        }

        protected override bool SupportsSearchingCore
        {
            get { return true; }
        }

        protected override int FindCore(PropertyDescriptor property,
        object key)
        {
            // Simple iteration:
            for (int i = 0; i < Count; i++)
            {
                T item = this[i];
                if (property.GetValue(item).Equals(key))
                {
                    return i;
                }
            }
            return -1; // Not found

            // Alternative search implementation
            // using List.FindIndex:
            //Predicate<T> pred = delegate(T item)
            //{
            // if (property.GetValue(item).Equals(key))
            // return true;
            // else
            // return false;
            //};
            //List<T> list = Items as List<T>;
            //if (list == null)
            // return -1;
            //return list.FindIndex(pred);
        }

        protected override bool SupportsSortingCore
        {
            get { return true; }
        }
        protected override bool IsSortedCore
        {
            get { return m_Sorted; }
        }

        protected override ListSortDirection SortDirectionCore
        {
            get { return m_SortDirection; }
        }

        protected override PropertyDescriptor SortPropertyCore
        {
            get { return m_SortProperty; }
        }

        protected override void ApplySortCore(PropertyDescriptor property,
          ListSortDirection direction)
        {
            m_SortDirection = direction;
            m_SortProperty = property;
            SortComparer<T> comparer =
               new SortComparer<T>(property, direction);
            ApplySortInternal(comparer);
        }

        private void ApplySortInternal(SortComparer<T> comparer)
        {
            if (m_OriginalCollection.Count == 0)
            {
                m_OriginalCollection.AddRange(this);
            }
            List<T> listRef = this.Items as List<T>;
            if (listRef == null)
                return;
            listRef.Sort(comparer);
            m_Sorted = true;
            OnListChanged(new ListChangedEventArgs(
              ListChangedType.Reset, -1));
        }

        protected override void RemoveSortCore()
        {
            if (!m_Sorted)
                return; Clear();
            foreach (T item in m_OriginalCollection)
            {
                Add(item);
            }
            m_OriginalCollection.Clear();
            m_SortProperty = null;
            m_SortDescriptions = null;
            m_Sorted = false;
        }

        void IBindingListView.ApplySort(ListSortDescriptionCollection sorts)
        {
            m_SortProperty = null;
            m_SortDescriptions = sorts;
            SortComparer<T> comparer = new SortComparer<T>(sorts);
            ApplySortInternal(comparer);
        }

        string IBindingListView.Filter
        {
            get
            {
                return m_FilterString;
            }
            set
            {
                m_FilterString = value;
                m_Filtered = true;
                UpdateFilter();
            }
        }

        void IBindingListView.RemoveFilter()
        {
            if (!m_Filtered)
                return;
            m_FilterString = null;
            m_Filtered = false;
            m_Sorted = false;
            m_SortDescriptions = null;
            m_SortProperty = null;
            Clear();
            foreach (T item in m_OriginalCollection)
            {
                Add(item);
            }
            m_OriginalCollection.Clear();
        }
        ListSortDescriptionCollection IBindingListView.SortDescriptions
        {
            get
            {
                return m_SortDescriptions;
            }
        }

        bool IBindingListView.SupportsAdvancedSorting
        {
            get
            {
                return true;
            }
        }

        bool IBindingListView.SupportsFiltering
        {
            get
            {
                return true;
            }
        }

        protected virtual void UpdateFilter()
        {
            int equalsPos = m_FilterString.IndexOf('=');
            // Get property name
            string propName = m_FilterString.Substring(0, equalsPos).Trim();
            // Get filter criteria
            string criteria = m_FilterString.Substring(equalsPos + 1,
               m_FilterString.Length - equalsPos - 1).Trim();
            // Strip leading and trailing quotes
            criteria = criteria.Substring(1, criteria.Length - 2);
            // Get a property descriptor for the filter property
            PropertyDescriptor propDesc =
               TypeDescriptor.GetProperties(typeof(T))[propName];
            if (m_OriginalCollection.Count == 0)
            {
                m_OriginalCollection.AddRange(this);
            }
            List<T> currentCollection = new List<T>(this);
            Clear();
            foreach (T item in currentCollection)
            {
                object value = propDesc.GetValue(item);
                if (value.ToString().StartsWith(criteria))
                {
                    Add(item);
                }
            }
        }

        bool IBindingList.AllowNew
        {
            get
            {
                return CheckReadOnly();
            }
        }

        bool IBindingList.AllowRemove
        {
            get
            {
                return CheckReadOnly();
            }
        }

        private bool CheckReadOnly()
        {
            if (m_Sorted || m_Filtered)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        protected override void InsertItem(int index, T item)
        {
            foreach (PropertyDescriptor propDesc in
               TypeDescriptor.GetProperties(item))
            {
                if (propDesc.SupportsChangeEvents)
                {
                    propDesc.AddValueChanged(item, OnItemChanged);
                }
            }
            base.InsertItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            T item = Items[index];
            PropertyDescriptorCollection propDescs = TypeDescriptor.GetProperties(item);
            foreach (PropertyDescriptor propDesc in propDescs)
            {
                if (propDesc.SupportsChangeEvents)
                {
                    propDesc.RemoveValueChanged(item, OnItemChanged);
                }
            }
            base.RemoveItem(index);
        }

        void OnItemChanged(object sender, EventArgs args)
        {
            int index = Items.IndexOf((T)sender);
            OnListChanged(new ListChangedEventArgs(
              ListChangedType.ItemChanged, index));
        }

        bool IRaiseItemChangedEvents.RaisesItemChangedEvents
        {
            get { return true; }
        }
    }







}
