using System;
using System.Collections.Generic;
using System.Core.Collections;
using System.Text;

namespace System.Core.LateBinding
{
    public static class LateBinderFactory
    {
        private static HybridCollection<Type, ILateBinder> _latebinders = new HybridCollection<Type, ILateBinder>();
        
        public static ILateBinder GetLateBinder(Type type)
        {
            if (!_latebinders.Contains(type))
                _latebinders.Add(type,new LateBinder(type));

            return _latebinders[type];
        }
    }
}
