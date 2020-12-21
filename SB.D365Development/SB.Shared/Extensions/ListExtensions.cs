using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SB.Shared.Extensions
{
    public static class ListExtensions
    {
        public static List<List<T>> Chunk<T>(this List<T> list, int size)
        {
            var chunks = new List<List<T>>();
            var chunkCount = list.Count() / size;

            if (list.Count % size > 0)
            { chunkCount++; }

            for (var i = 0; i < chunkCount; i++)
            {
                chunks.Add(list.Skip(i * size).Take(size).ToList());
            }

            return chunks;
        }
    }
}
