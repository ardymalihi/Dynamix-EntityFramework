using System.Collections.Generic;

namespace Dynamix.EntityFramework
{
    public class CustomList<T> : List<T>
    {
        public IEnumerable<dynamic> AsDynamic()
        {
            foreach (var obj in this) yield return obj;
        }
    }
}
