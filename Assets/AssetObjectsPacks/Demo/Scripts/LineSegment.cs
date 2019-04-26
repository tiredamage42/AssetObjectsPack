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

public class ValueTracker<T> {
    T lastValue;
    public ValueTracker (T initValue) {
        lastValue = initValue;
    }
    public void SetLastValue(T value) {
        lastValue = value;
    }
    public bool CheckValueChange (T checkValue) {
        bool changed = false;
        if (lastValue == null) {
            changed = checkValue != null;
            // Debug.LogWarning(changed);
        }
        else {
            changed = !checkValue.Equals(lastValue);
        }
        SetLastValue(checkValue);

        // if (changed) {
        //     Debug.LogError("changed" + checkValue);
        // }
        return changed;
    }
}


[System.Serializable] public class Smoother {
    public enum SmoothMethod { SmoothDamp, Lerp, MoveTowards }
    public SmoothMethod smoothMethod;
    float currentVelocity;
    Vector2 currentVelocity2;
    Vector3 currentVelocity3;
    public float speed = 1.0f;
    

    public float Smooth(float a, float b, float deltaTime) {
        if (smoothMethod == SmoothMethod.Lerp) {
            return Mathf.Lerp(a, b, speed * deltaTime);
        }
        else if (smoothMethod == SmoothMethod.SmoothDamp) {
            return Mathf.SmoothDamp(a, b, ref currentVelocity, speed);
        }
        else {
            return Mathf.MoveTowards(a, b, speed * deltaTime);
        }
    }
    public Vector2 Smooth(Vector2 a, Vector2 b, float deltaTime) {
        if (smoothMethod == SmoothMethod.Lerp) {
            return Vector2.Lerp(a, b, speed * deltaTime);
        }
        else if (smoothMethod == SmoothMethod.SmoothDamp) {
            return Vector2.SmoothDamp(a, b, ref currentVelocity2, speed);
        }
        else {
            return Vector2.MoveTowards(a, b, speed * deltaTime);
        }
    }
    public Vector3 Smooth(Vector3 a, Vector3 b, float deltaTime) {
        if (smoothMethod == SmoothMethod.Lerp) {
            return Vector3.Lerp(a, b, speed * deltaTime);
        }
        else if (smoothMethod == SmoothMethod.SmoothDamp) {
            return Vector3.SmoothDamp(a, b, ref currentVelocity3, speed);
        }
        else {
            return Vector3.MoveTowards(a, b, speed * deltaTime);
        }
    }
}