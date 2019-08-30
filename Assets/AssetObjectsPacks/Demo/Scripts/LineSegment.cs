using System.Collections.Generic;
using UnityEngine;

public struct LineSegment {

    public static HashSet<LineSegment> SplitSegments (HashSet<LineSegment> orig, float maxSize) {
        HashSet<LineSegment> r = new HashSet<LineSegment>();
        foreach (var o in orig) {
            Vector3 a2b = (o.b-o.a).normalized;
            float dist = a2b.magnitude;
            if (dist > maxSize) {
                int divs = (int)(dist / maxSize);
                for (int i = 0; i < divs; i++) {
                    r.Add(new LineSegment(o.a + a2b * (maxSize * i), i == divs - 1 ? o.b : o.a + a2b * (maxSize * (i + 1)), o.normal));
                }
            }
            else {
                r.Add(o);
            }
        }
        return r;
    }
    public Vector3 a, b, normal, midPoint;
    public static LineSegment empty { get { return new LineSegment(Vector3.zero); } }
    LineSegment (Vector3 s) {
        a = b = normal = midPoint = s;
    }
    public LineSegment (Vector3 a, Vector3 b){
        this.a = a;
        this.b = b;
        normal = Vector3.Cross((b - a).normalized, Vector3.up);
        midPoint = (a + b) * .5f;
    }
    LineSegment (Vector3 a, Vector3 b, Vector3 normal) {
        this.a = a;
        this.b = b;
        this.normal = normal;
        midPoint = (a + b) * .5f;
    }
}

public class ValueTracker {
    object lastValue;
    System.Func<object> getValue;
    string displayName;

    public ValueTracker (System.Func<object> getValue, object initValue, string displayName) {
        lastValue = initValue;
        this.getValue = getValue;
        this.displayName = displayName;
    }
    // public ValueTracker (System.Func<object> getValue, string displayName) {
    //     lastValue = getValue();
    //     this.getValue = getValue;
    // }
    public void UpdateLastValue () {
        lastValue = getValue();
    }
    public bool CheckValueChange (bool debug) {
        object v = getValue();
        bool changed = lastValue == null ? v != null : !v.Equals(lastValue);
        lastValue = v;

        if (debug && changed) {
            Debug.Log("Changed:: " + displayName);
        } 
        return changed;
    }
}

