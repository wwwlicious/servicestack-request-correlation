// This Source Code Form is subject to the terms of the Mozilla Public 
// License, v. 2.0. If a copy of the MPL was not distributed with this 
// file, You can obtain one at http://mozilla.org/MPL/2.0/. 
namespace ServiceStack.Request.Correlation.Extensions
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