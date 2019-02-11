

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace AssetObjectsPacks {
    public static class Extensions 
    {
        public delegate T GetMethod<T> (int index);
        public static IEnumerable<T> Generate<T>(GetMethod<T> get_obj_method, int count) {
            int i = 0;
            while (i < count){
                yield return get_obj_method(i);
                i++;
            }
            
        }
        public static T[] GenerateArray<T>(this T[] a, GetMethod<T> get_obj_method, int count) {
            return Generate(get_obj_method, count).ToArray();
        }

        public static List<T> ToList<T>(this T[] a) {
            int l = a.Length;
            List<T> r = new List<T>(l);
            for (int i = 0; i < l; i++) r.Add(a[i]);
            return r;
        }

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


