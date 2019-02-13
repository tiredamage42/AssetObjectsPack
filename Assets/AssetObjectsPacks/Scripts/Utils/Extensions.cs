using System.Collections.Generic;
//using UnityEngine;
using System.Linq;

namespace AssetObjectsPacks {
    public static class Extensions 
    {
        public delegate T GetMethod<T> (int index);
      
        
        static IEnumerable<T> Generate<T>(GetMethod<T> get_obj_method, int count) {
            for (int i = 0; i < count; i++) yield return get_obj_method(i);
        }
        /*
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> a) {
            HashSet<T> r = new HashSet<T>();
            foreach (T o in a) r.Add(o);
            return r;
        }
         */

        public delegate T TFromO<T, O> (O other);
        public static HashSet<T> Generate<T, O> (this HashSet<T> h, HashSet<O> other, TFromO<T,O> t_from_o) {
            h.Clear();
            foreach (O o in other) h.Add( t_from_o(o) );
            return h;
        }
        public static HashSet<T> Generate<T, O> (this HashSet<T> h, IEnumerable<O> other, TFromO<T,O> t_from_o) {
            h.Clear();
            foreach (O o in other) h.Add( t_from_o(o) );
            return h;
        }
        
        public static HashSet<T> Generate<T>(this HashSet<T> h, int count, GetMethod<T> get_method) {
            h.Clear();
            for (int i = 0; i < count; i++) h.Add(get_method(i));
            return h;   
        }





        static void CheckArraySize<T> (ref T[] a, int c) {
            if (a.Length != c) {
                System.Array.Resize(ref a, c);
            }
        }
        public static T[] Generate<T, O> (this T[] h, HashSet<O> other, TFromO<T,O> t_from_o) {

            CheckArraySize(ref h, other.Count);
            int i = 0;
            foreach (O o in other){
                h[i] = t_from_o(o);
                i++;
            } 
            return h;
        }
        public static T[] Generate<T, O> (this T[] h, IList<O> other, TFromO<T,O> t_from_o) {
            int c = other.Count;
            CheckArraySize(ref h, c);
            
            for (int i = 0; i < c; i++) h[i] = t_from_o( other[i] );
            return h;
        }
        
        public static T[] Generate<T>(this T[] h, int count, GetMethod<T> get_method) {
            CheckArraySize(ref h, count);
            for (int i = 0; i < count; i++) h[i] = get_method(i);
            return h;   
        }
        public static T[] Generate<T>(this T[] h, GetMethod<T> get_method) {
            for (int i = 0; i < h.Length; i++) h[i] = get_method(i);
            return h;   
        }
        
        
        
        //public static HashSet<T> GenerateHashset<T>(this HashSet<T> a, GetMethod<T> get_obj_method, int count) {
        //    return Generate(get_obj_method, count).ToHashSet();
        //}
        //public static T[] GenerateArray<T>(this T[] a, GetMethod<T> get_obj_method, int count) {
        //    return Generate(get_obj_method, count).ToArray();
        //}
/*
        public static List<T> ToList<T>(this T[] a) {
            int l = a.Length;
            List<T> r = new List<T>(l);
            for (int i = 0; i < l; i++) r.Add(a[i]);
            return r;
        }
 */

    public static T Last<T> (this IList<T> a) {
        return a[a.Count - 1];
    }
    public static T[] ToArray<T> (this ISet<T> s) {
        T[] r = new T[s.Count];
        int u = 0;
        foreach (T t in s) {
            r[u] = t;
            u++;
        }
        return r;
    }
    public static IList<T> Slice<T>(this IList<T> x, int a=0, int b=-1) {
        if (b < 0) {
            b = x.Count + b;
        }
        //int l = (b < 0) ? x.Count - a : b;
        List<T> r = new List<T>();
        for (int i = a; i <= b; i++) r.Add( x[i] ); 
        return r;
    }

    public static bool Contains(this int[] a, int e) {
        int l = a.Length;
        for (int i = 0; i < l; i++) {
            if (a[i] == e) {
                return true;
            }
        }
        return false;
    }
        

    public static T RandomChoice<T>(this IList<T> l) {
        return l[UnityEngine.Random.Range(0, l.Count)];
    }
    }

}


