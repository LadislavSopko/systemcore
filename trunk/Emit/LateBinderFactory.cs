using System;
using System.Collections.Generic;
using System.Core.Collections;
using System.Text;

namespace System.Core.LateBinding
{
    /// <summary>
    /// Factory class used to create new instances of the <seealso cref="LateBinder"/> class.
    /// </summary>
    /// <remarks>
    /// The factory class will only create a latebinder once for each type.
    /// If the request for a type is made twice, the cached Latebinder will be returned.
    /// </remarks>
    public static class LateBinderFactory
    {
        /// <summary>
        /// A collection of latebinders indexed by type
        /// </summary>
        private static HybridCollection<Type, ILateBinder> _latebinders = new HybridCollection<Type, ILateBinder>();

        /// <summary>
        /// Returns a reference to a Latebinder for the requested type represented by <seealso cref="ILateBinder"/>.
        /// </summary>
        /// <param name="type">The for to get the Latebinder</param>
        /// <returns><see cref="LateBinder"/></returns>
        public static ILateBinder GetLateBinder(Type type)
        {
            if (!_latebinders.Contains(type))
                _latebinders.Add(type,new LateBinder(type));

            return _latebinders[type];
        }
    }
}
