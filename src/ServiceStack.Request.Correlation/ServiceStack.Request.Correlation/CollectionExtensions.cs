namespace ServiceStack.Request.Correlation
{
    using System.Collections.Generic;

    public static class CollectionExtensions
    {
        public static void InsertAsFirst<T>(this List<T> list, T item)
        {
            list?.Insert(0, item);
        }
    }
}