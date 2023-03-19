using System;
using System.Collections.Generic;
using System.Linq;

namespace DiffDemo.LCS
{
    public static class Diff
    {
        public static IEnumerable<LCS<T>.DiffResut<T>> RunLCS<T>(T[] oldState, T[] newState, Func<T,T,bool>? comparer = null)
        {
            var lcsDiff = new LCS<T>(oldState, newState, comparer);
            return lcsDiff.RunDiff() ?
                lcsDiff.DiffResult :
                Enumerable.Empty<LCS<T>.DiffResut<T>>();
        }
    }
}
