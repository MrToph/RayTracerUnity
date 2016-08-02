using UnityEngine;
using System.Collections;

public class RTPlane : RTObject {
    protected Vector3 center;
    protected Vector3 normal;

    public override void Awake()
    {
        base.Awake();
        shading.reflection = 0.3f;
        center = gameObject.transform.GetComponent<Renderer>().bounds.center; // gameObject.transform.position;
        normal = gameObject.transform.rotation * Vector3.up;
        ULog.Log(gameObject.name, "Center:", center, "Normal:", normal);
    }

    public override RayTracer.HitInfo Intersect(Ray ray)
    {
        RayTracer.HitInfo info = new RayTracer.HitInfo(shading);

        Vector3 d = ray.direction;
        float denominator = Vector3.Dot(d, normal);

        if (Mathf.Abs(denominator) < Mathf.Epsilon) return info;      // direction and plane parallel, no intersection

        float t = Vector3.Dot(center - ray.origin, normal) / denominator;
        if (t < 0) return info;    // plane behind ray's origin

        info.time = t;
        info.hitPoint = ray.GetPoint(t);
        info.normal = normal;
        return info;
    }

    //void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.yellow;
    //    Gizmos.DrawSphere(transform.position, 0.1f);  //center sphere
    //        Gizmos.DrawWireCube(center, gameObject.GetComponent<Renderer>().bounds.size);
    //}
}
