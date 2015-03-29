using Assisticant.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RoverMob
{
    public static class CollectionExtensions
    {
        public static void RemoveAll<T>(
            this ObservableList<T> collection,
            Predicate<T> predicate)
        {
            int index = 0;
            while (index < collection.Count)
            {
                if (predicate(collection[index]))
                    collection.RemoveAt(index);
                else
                    index++;
            }
        }

        public static void AddRange<T>(
            this ObservableList<T> collection,
            IEnumerable<T> items)
        {
            foreach (var item in items)
                collection.Add(item);
        }
    }
}
