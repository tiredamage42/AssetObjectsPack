
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetObjectsPacks {
    public static class StringUtils {
        public const string empty = "";
    }
    public class Pool<T> where T : new() {
        List<T> pool = new List<T>();
        Queue<int> available_indicies = new Queue<int>();
        public int GetNewObject () {
            if (available_indicies.Count == 0) {
                pool.Add(new T());
                return pool.Count - 1;
            }
            return available_indicies.Dequeue();
        }
        public void ReturnToPool (int key) {
            if (!available_indicies.Contains(key)) 
                available_indicies.Enqueue(key);
        }
        public T this[int key] { get { return pool[key]; } }
    }

}


