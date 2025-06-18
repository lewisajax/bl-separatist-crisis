using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeparatistCrisis.Extensions
{
    /// <summary>
    ///     Extension methods for IEnumerable. 
    ///     Mainly need this to remove game models in OnGameStart, instead of using a transpiler or something
    ///     By hunharibo: https://discord.com/channels/411286129317249035/677511186295685150/942002793651253258
    /// </summary>
    public static class IEnumerableExtensions
    {   
        /// <summary>
        ///     Removes any matching items in a collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="item"></param>
        public static void RemoveIfExists<T>(this IEnumerable<T> collection, T item) where T : class
        {
            collection = collection.Where(x => x != item);
        }

        /// <summary>
        ///     Removes all occurrences of a type in a collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="type"></param>
        public static void RemoveAllOfType<T>(this IEnumerable<T> collection, Type type) where T : class
        {
            collection = collection.Where(x => x.GetType() != type);
        }
    }
}
