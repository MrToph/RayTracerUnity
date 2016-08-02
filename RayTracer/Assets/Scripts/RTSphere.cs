using UnityEngine;
using System.Collections;
using System;

public class RTSphere : RTObject {
    protected Vector3 center;
    protected float radius;

    public override void Awake()
    {
        base.Awake();
        center = gameObject.transform.position;
        radius = gameObject.transform.localScale.x / 2;
        ULog.Log(gameObject.name + " Center: " + center.ToString() + " " + radius.ToString());
    }

    public override RayTracer.HitInfo Intersect(Ray ray)
    {
        RayTracer.HitInfo info = new RayTracer.HitInfo(shading);

        Vector3 eMinusS = ray.origin - center;
        Vector3 d = ray.direction; 
        double discriminant = Math.Pow(2 * Vector3.Dot(d, eMinusS), 2) - 4 * Vector3.Dot(d, d) *
                    (Vector3.Dot(eMinusS, eMinusS) - Math.Pow(radius, 2.0f));

        if (discriminant < -Mathf.Epsilon)
        {   // 0 hits
            return info;
        }
        else {      // there will be one or two hits
            float front = -2.0f * Vector3.Dot(d, eMinusS);
            float denominator = 2.0f * Vector3.Dot(d, d);
            if (discriminant <= Mathf.Epsilon)
            {   // 1 hit
                info.time = (float)(front + Math.Sqrt(discriminant)) / denominator;  // does not matter if +- discriminant
            }
            else {  // 2 hits
                float t1 = (float)(front - Math.Sqrt(discriminant)) / denominator;  // smaller t value
                float t2 = (float)(front + Math.Sqrt(discriminant)) / denominator;  // larger t value
                if (t2 < 0) // sphere is "behind" start of ray
                {
                    return info;    // no hit
                }
                else {  // one of them is in front
                    if (t1 >= 0) info.time = t1; // return first intersection with sphere (usual case, smaller t)
                    else info.time = t2;        // return second hit (ray's origin is inside the sphere)
                }
            }
        }

        // if we are here, info.time has been set, otherwise the function would have returned false
        info.hitPoint = ray.GetPoint(info.time);
        info.normal = (info.hitPoint - center).normalized;
        return info;
    }
}
