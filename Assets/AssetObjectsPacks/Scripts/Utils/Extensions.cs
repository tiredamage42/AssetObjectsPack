using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

namespace AssetObjectsPacks {
    public static class Extensions  {

        public static bool Contains(this Vector2Int v, int i) {
            return i >= v.x && i <= v.y;
        }

        public static IEnumerable<T> Generate<T> (this int c, Func<int, T> g) {
            for (int i = 0; i < c; i++) yield return g(i);
        }
        
        public static IEnumerable<T> Generate<T> (this Vector2Int c, Func<int, T> g) {
            for (int i = c.x; i <= c.y; i++) yield return g(i);
        }
        
        
        public static IEnumerable<T> Generate<T, O> (this IEnumerable<O> c, Func<O, T> g) {
            foreach (var o in c) yield return g(o);
        }
        public static IEnumerable<int> Generate (this Vector2Int c) {
            for (int i = c.x; i <= c.y; i++) yield return i;
        }
        //public static IEnumerable<int> Generate (this int c) {
        //    return new Vector2Int(0, c - 1).Generate();
        //}
        public static List<T> ToList<T>(this IEnumerable<T> a) {
            List<T> r = new List<T>(a.Count());
            r.AddRange(a);
            return r;
        }
        public static HashSet<T> DeepCopy<T>(this HashSet<T> a) {
            return a.ToHashSet();
        }
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> a) {
            HashSet<T> r = new HashSet<T>();
            r.AddRange(a, true);
            return r;        
        }
        public static void AddRange<T> (this HashSet<T> e, IEnumerable<T> r, bool allowCopies) {
            foreach (var i in r) {
                if (allowCopies || !e.Contains(i)) {
                    e.Add(i);
                }
            }
        }
        public static T Last<T> (this IList<T> a) {
            return a[a.Count - 1];
        }
        public static IList<T> Slice<T>(this IList<T> x, int a=0, int b=-1) {
            if (b < 0) b = x.Count + b;
            List<T> r = new List<T>();
            for (int i = a; i <= b; i++) r.Add( x[i] ); 
            return r;
        }
        //public static void ToggleElement<T> (this IList<T> e, T o) {
        //    if (e.Contains(o)) e.Remove(o);
        //    else e.Add(o);
        //}
        public static void ToggleElement<T> (this HashSet<T> e, T o) {
            if (e.Contains(o)) e.Remove(o);
            else e.Add(o);
        }
        
        public static T RandomChoice<T>(this IList<T> l) {
            return l[UnityEngine.Random.Range(0, l.Count)];
        }
    }

}


