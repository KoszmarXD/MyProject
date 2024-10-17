using System;
using System.Collections.Generic;

public class PriorityQueue<T>
{
    private SortedSet<(float priority, int index, T item)> sortedSet;
    private int count = 0;

    public PriorityQueue()
    {
        sortedSet = new SortedSet<(float priority, int index, T item)>(Comparer<(float priority, int index, T item)>.Create((a, b) =>
        {
            int compare = a.priority.CompareTo(b.priority);
            if (compare == 0)
            {
                compare = a.index.CompareTo(b.index);
                if (compare == 0)
                {
                    compare = 1; // 確保不同元素可以存在於 SortedSet 中
                }
            }
            return compare;
        }));
    }

    public void Enqueue(T item, float priority)
    {
        sortedSet.Add((priority, count++, item));
    }

    public T Dequeue()
    {
        if (sortedSet.Count == 0)
            throw new InvalidOperationException("Heap is empty");
        var min = sortedSet.Min;
        sortedSet.Remove(min);
        return min.item;
    }

    public bool IsEmpty()
    {
        return sortedSet.Count == 0;
    }

    public bool Contains(T item)
    {
        foreach (var element in sortedSet)
        {
            if (element.item.Equals(item))
                return true;
        }
        return false;
    }

    // 選擇性：提供可枚舉的方式訪問元素（僅供調試或其他用途）
    public IEnumerable<(float priority, int index, T item)> Elements => sortedSet;
}
