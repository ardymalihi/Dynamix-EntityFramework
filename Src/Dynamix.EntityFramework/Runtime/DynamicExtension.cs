using System.Collections.Generic;
using System.Collections;

namespace Dynamix.EntityFramework.Runtime
{
    public static class DynamicExtensions
    {
        public static IEnumerable<dynamic> AsDynamic(this IEnumerable list)
        {
            foreach (var obj in list) yield return obj;
        }
    }
}