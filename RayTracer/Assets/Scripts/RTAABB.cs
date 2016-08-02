using UnityEngine;
using System.Collections;

public class RTAABB : RTObject
{
    protected Vector3 min, max;

    public RTAABB(Vector3 min, Vector3 max)
    {
        Init(min, max);
    }

    protected void Init(Vector3 min, Vector3 max)
    {
        this.min = min;
        this.max = max;

        //Vector3 d = max - min;
        //Debug.DrawLine(min, min + new Vector3(d.x, 0, 0), Color.green, 100.0f);
        //Debug.DrawLine(min, min + new Vector3(0, d.y, 0), Color.green, 100.0f);
        //Debug.DrawLine(min, min + new Vector3(0, 0, d.z), Color.green, 100.0f);
        //Debug.DrawLine(min + new Vector3(d.x, d.y, 0), max, Color.blue, 100.0f);
        //Debug.DrawLine(min + new Vector3(d.x, 0, d.z), max, Color.blue, 100.0f);
        //Debug.DrawLine(min + new Vector3(0, d.y, d.z), max, Color.blue, 100.0f);
    }

    // http://www.scratchapixel.com/lessons/3d-basic-rendering/minimal-ray-tracer-rendering-simple-shapes/ray-box-intersection
    public override RayTracer.HitInfo Intersect(Ray ray)
    {
        RayTracer.HitInfo info = new RayTracer.HitInfo(shading);

        float tmin = (min.x - ray.origin.x) / ray.direction.x;
        float tmax = (max.x - ray.origin.x) / ray.direction.x;

        if (tmin > tmax) swap(ref tmin, ref tmax);

        float tymin = (min.y - ray.origin.y) / ray.direction.y;
        float tymax = (max.y - ray.origin.y) / ray.direction.y;

        if (tymin > tymax) swap(ref tymin, ref tymax);

        if ((tmin > tymax) || (tymin > tmax))
            return info;

        if (tymin > tmin)
            tmin = tymin;

        if (tymax < tmax)
            tmax = tymax;

        float tzmin = (min.z - ray.origin.z) / ray.direction.z;
        float tzmax = (max.z - ray.origin.z) / ray.direction.z;

        if (tzmin > tzmax) swap(ref tzmin, ref tzmax);

        if ((tmin > tzmax) || (tzmin > tmax))
            return info;

        if (tzmin > tmin)
            tmin = tzmin;

        if (tzmax < tmax)
            tmax = tzmax;

        info.time = tmin;
        info.hitPoint = ray.GetPoint(tmin);
        //Debug.DrawLine(info.hitPoint, info.hitPoint + new Vector3(0, 0, 0.01f), Color.green, 100.0f);
        return info;
    } 
    
    private void swap(ref float a, ref float b)
    {
        float tmp = a;
        a = b;
        b = tmp;
    }
}