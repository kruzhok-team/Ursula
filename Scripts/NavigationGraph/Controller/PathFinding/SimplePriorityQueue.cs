using System;
using System.Collections.Generic;

namespace bearloga.addons.Ursula.Scripts.NavigationGraph.Controller.PathFinding
{
    // Binary min-heap
    public class SimplePriorityQueue<T>
    {
        // All items should be unique
        private List<(T item, float priority)> heap = new List<(T, float)>();
        private Dictionary<T, int> indexMap = new Dictionary<T, int>();

        public int Count => heap.Count;

        public bool Contains(T item) => indexMap.ContainsKey(item);

        public void Enqueue(T item, float priority)
        {
            heap.Add((item, priority));
            int i = heap.Count - 1;
            indexMap[item] = i;
            HeapifyUp(i);
        }

        public T Dequeue()
        {
            if (heap.Count == 0) 
                throw new InvalidOperationException("Queue is empty");
            T root = heap[0].item;
            Swap(0, heap.Count - 1);
            heap.RemoveAt(heap.Count - 1);
            indexMap.Remove(root);
            if (heap.Count > 0) 
                HeapifyDown(0);
            return root;
        }

        public void UpdatePriority(T item, float newPriority)
        {
            if (!indexMap.TryGetValue(item, out int i))
                throw new KeyNotFoundException(nameof(item));
            float old = heap[i].priority;
            heap[i] = (item, newPriority);
            if (newPriority == old) 
                return;
            if (newPriority < old) 
                HeapifyUp(i);
            else 
                HeapifyDown(i);
        }

        private void HeapifyUp(int i)
        {
            while (i > 0)
            {
                int parent = (i - 1) / 2;
                if (heap[i].priority < heap[parent].priority)
                {
                    Swap(i, parent);
                    i = parent;
                }
                else 
                    break;
            }
        }

        private void HeapifyDown(int i)
        {
            int n = heap.Count;
            while (true)
            {
                int left = 2 * i + 1;
                int right = 2 * i + 2;
                int smallest = i;

                if (left < n && heap[left].priority < heap[smallest].priority) 
                    smallest = left;
                if (right < n && heap[right].priority < heap[smallest].priority) 
                    smallest = right;

                if (smallest != i)
                {
                    Swap(i, smallest);
                    i = smallest;
                }
                else 
                    break;
            }
        }

        private void Swap(int i, int j)
        {
            (T, float) tmp = heap[i];
            heap[i] = heap[j];
            heap[j] = tmp;
            indexMap[heap[i].item] = i;
            indexMap[heap[j].item] = j;
        }
    }
}
