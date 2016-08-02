using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
public class RTModel : RTObject
{
    protected List<RTTriangle> triangles;
    protected RTAABB aabb;

    public override void Awake()
    {
        base.Awake();
        triangles = new List<RTTriangle>();
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3 pos = transform.position;
        Quaternion rot = transform.rotation;
        ULog.Log(gameObject.name + ": Amount of triangles: " + mesh.triangles.Length / 3 + " normals: " + mesh.normals.Length + " " + mesh.vertexCount);
        for(int i = 0, j = 1, k = 2; i < mesh.triangles.Length; i += 3, j +=3, k += 3)
        {
            Vector3 v0 = rot * mesh.vertices[mesh.triangles[i]];
            Vector3 v1 = rot * mesh.vertices[mesh.triangles[j]];
            Vector3 v2 = rot * mesh.vertices[mesh.triangles[k]];
            //Debug.DrawLine(pos + v0, pos + v1, Color.green, 100.0f);
            //Debug.DrawLine(pos + v1, pos + v2, Color.green, 100.0f);
            //Debug.DrawLine(pos + v2, pos + v0, Color.green, 100.0f);
            triangles.Add(new RTTriangle(pos + v0, pos + v1, pos + v2, true));
        }
        Bounds b = GetComponent<Renderer>().bounds;
        aabb = new RTAABB(b.center - b.extents, b.center + b.extents);
    }

    // http://geomalgorithms.com/a06-_intersect-2.html
    public override RayTracer.HitInfo Intersect(Ray ray)
    {
        RayTracer.HitInfo aabbInfo = aabb.Intersect(ray);
        if (aabbInfo.time < 0) return aabbInfo; // didn't even hit the bounding box of the model

        RayTracer.HitInfo info = new RayTracer.HitInfo(shading);

        foreach (RTTriangle triangle in triangles)   // check collision for each individual object
        {
            RayTracer.HitInfo tmpInfo = triangle.Intersect(ray);
            if (tmpInfo.time >= 0)     // we hit it
            {
                if (info.time == -1.0f) // first hit
                {
                    info = tmpInfo;
                }
                else if (tmpInfo.time < info.time)  // there has already been a hit => check if this hit is closer to the camera
                    info = tmpInfo;
            }
        }

        // needs to be done here as it was overwritten in triangle shading
        info.shading = shading;
        //if(info.time < 0)   // only hit AABB
        //{
        //    info = aabbInfo;
        //    info.color = Color.yellow;
        //}
        return info;
    }
    
}
