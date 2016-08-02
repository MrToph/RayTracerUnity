using UnityEngine;

public abstract class RTObject : MonoBehaviour {

    protected RayTracer.ShadingInfo shading;

    public virtual void Awake()
    {
        shading = new RayTracer.ShadingInfo(gameObject.GetComponent<Renderer>().sharedMaterial.color);
    }

    public abstract RayTracer.HitInfo Intersect(Ray ray);
}
