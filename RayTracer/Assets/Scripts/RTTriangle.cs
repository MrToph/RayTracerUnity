using UnityEngine;
using System.Collections;

public class RTTriangle : RTObject
{
    protected Vector3 v0, v1, v2;
    protected Vector3 normal;
    protected Vector3 u, v;
    protected Vector3 uPerp, vPerp;
    protected float denominatorST;

    public override void Awake()
    {
        base.Awake();
        UnityTriangle ut = GetComponent<UnityTriangle>();
        v0 = transform.rotation * ut.v0 + transform.position;
        v1 = transform.rotation * ut.v1 + transform.position;
        v2 = transform.rotation * ut.v2 + transform.position;
        Init(v0, v1, v2, true);
        normal = transform.rotation * normal;
        ULog.Log(gameObject.name, "v0:", v0, "v1:", v1, "v2:", v2);
        ULog.Log("Triangle Color", shading.diffuse, "Normal", normal);
    }

    public RTTriangle(Vector3 v0, Vector3 v1, Vector3 v2, bool clockwise = false)
    {
        Init(v0, v1, v2, clockwise);
    }
    
    protected void Init(Vector3 v0, Vector3 v1, Vector3 v2, bool clockwise = false)
    {
        this.v0 = v0;
        this.v1 = v1;
        this.v2 = v2;
        u = v1 - v0;
        v = v2 - v0;

        // Unity uses clockwise winding order to determine front-facing triangles
        // Unity uses a left-handed coordinate system
        // the normal will face the front
        normal = (clockwise ? 1 : -1) * Vector3.Cross(u, v).normalized;

        uPerp = Vector3.Cross(normal, u);
        vPerp = Vector3.Cross(normal, v);
        denominatorST = Vector3.Dot(u, vPerp);
        if (Mathf.Abs(denominatorST) < Mathf.Epsilon)
        {
            ULog.LogError("Triangle is broken");
            return;
        }

        //Vector3 center = (v0 + v1 + v2) / 3;
        //Vector3 lightPos = FindObjectOfType<RTLight>().position;
        //Vector3 toLightDir = (lightPos - center).normalized;
        //Color col = Vector3.Dot(toLightDir, normal) < 0 ? Color.red : Color.green;
        //Debug.DrawLine(center, center + normal * Mathf.Abs(Vector3.Dot(toLightDir, normal)), col, 100.0f);
        //if (Vector3.Dot(toLightDir, normal) < 0) Debug.DrawLine(center, center + 0.1f * toLightDir, Color.white, 100.0f);
    }

    public override RayTracer.HitInfo Intersect(Ray ray)
    {
        RayTracer.HitInfo info = new RayTracer.HitInfo(shading);

        Vector3 d = ray.direction;
        float denominator = Vector3.Dot(d, normal);

        if (Mathf.Abs(denominator) < Mathf.Epsilon) return info;      // direction and plane parallel, no intersection

        float tHit = Vector3.Dot(v0 - ray.origin, normal) / denominator;
        if (tHit < 0) return info;    // plane behind ray's origin

        Vector3 w = ray.GetPoint(tHit) - v0;

        float s = Vector3.Dot(w, vPerp) / denominatorST;
        if (s < 0 || s > 1) return info;    // won't be inside triangle

        float t = Vector3.Dot(w, uPerp) / -denominatorST;
        if (t >= 0 && (s + t) <= 1)
        {
            info.time = tHit;
            info.hitPoint = ray.GetPoint(tHit);
            info.normal = normal;
        }

        return info;
    }

    // http://geomalgorithms.com/a06-_intersect-2.html
    //public override RayTracer.HitInfo Intersect(Ray ray)
    //{
    //    RayTracer.HitInfo info = new RayTracer.HitInfo(shading);

    //    Vector3 d = ray.direction;
    //    float denominator = Vector3.Dot(d, normal);

    //    if (Mathf.Abs(denominator) < Mathf.Epsilon) return info;      // direction and plane parallel, no intersection

    //    float tHit = Vector3.Dot(v0 - ray.origin, normal) / denominator;
    //    if (tHit < 0) return info;    // plane behind ray's origin

    //    Vector3 w = ray.GetPoint(tHit) - v0;

    //    float wu = Vector3.Dot(w, u), wv = Vector3.Dot(w, v);
    //    float s = (uv * wv - vv * wu) / denominatorST;
    //    float t = (uv * wu - uu * wv) / denominatorST;
    //    if( s >= 0 && t >= 0 && (s + t) <= 1)
    //    {
    //        info.time = tHit;
    //        info.hitPoint = ray.GetPoint(tHit);
    //        info.normal = normal;
    //    }

    //    return info;
    //}

}
