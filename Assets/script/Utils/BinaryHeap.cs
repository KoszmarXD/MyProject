using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BinaryHeap<T>
{
    private List<(float priority, T item)> heap = new List<(float, T)>();

    public int Count => heap.Count;

    public void Enqueue(T item, float priority)
    {
        heap.Add((priority, item));
        HeapifyUp(heap.Count - 1);
    }

    public T Dequeue()
    {
        if (heap.Count == 0)
            throw new InvalidOperationException("Heap is empty");

        T item = heap[0].item;
        heap[0] = heap[heap.Count - 1];
        heap.RemoveAt(heap.Count - 1);
        HeapifyDown(0);
        return item;
    }

    public bool IsEmpty()
    {
        return heap.Count == 0;
    }

    private void HeapifyUp(int index)
    {
        while (index > 0)
        {
            int parent = (index - 1) / 2;
            if (heap[index].priority < heap[parent].priority)
            {
                Swap(index, parent);
                index = parent;
            }
            else
            {
                break;
            }
        }
    }

    private void HeapifyDown(int index)
    {
        int lastIndex = heap.Count - 1;
        while (true)
        {
            int left = 2 * index + 1;
            int right = 2 * index + 2;
            int smallest = index;

            if (left <= lastIndex && heap[left].priority < heap[smallest].priority)
            {
                smallest = left;
            }

            if (right <= lastIndex && heap[right].priority < heap[smallest].priority)
            {
                smallest = right;
            }

            if (smallest != index)
            {
                Swap(index, smallest);
                index = smallest;
            }
            else
            {
                break;
            }
        }
    }

    private void Swap(int i, int j)
    {
        var temp = heap[i];
        heap[i] = heap[j];
        heap[j] = temp;
    }
}