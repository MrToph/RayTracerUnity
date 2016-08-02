using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class RayTracer : MonoBehaviour
{
    public int maxBounces;
    public int GUIUpdateRate;
    private float lastGUIUpdate;
    private Texture2D renderTexture;
    bool rendered = false;
    bool showRender = false;
    List<RTObject> collisionObjects;
    List<RTLight> lights;

    [SerializeField]
    private RawImage renderUI;


    // Use this for initialization
    void Awake()
    {
        Application.runInBackground = true;
        renderTexture = new Texture2D(Screen.width, Screen.height);
        ULog.Log("Screen width:", renderTexture.width, "- height", renderTexture.height);
        collisionObjects = new List<RTObject>(FindObjectsOfType<RTObject>());
        lights = new List<RTLight>(FindObjectsOfType<RTLight>());
    }

    Color PhongColor(HitInfo info, RTLight light)
    {
        Vector3 L = (light.position - info.hitPoint).normalized;    // vector from hitPoint to lightSource
        Vector3 N = info.normal.normalized;
        Vector3 V = (Camera.main.transform.position - info.hitPoint).normalized;  // vector from hitPoint to camera
        float cosTheta = Vector3.Dot(L, N);
        cosTheta = Mathf.Max(cosTheta, 0.0f);    // truncate negative values
        float cosAlpha = Vector3.Dot(2.0f * Vector3.Dot(L, N) * N - L, V);
        cosAlpha = Mathf.Max(cosAlpha, 0.0f);    // truncate negative values
        Color iDiff = cosTheta * info.shading.diffuse;
        Color iSpec = (float)Mathf.Pow(cosAlpha, info.shading.specularIntensity) * info.shading.specular;
        return light.intensity * (iDiff + iSpec);
    }

    HitInfo CheckIntersection(Ray ray)
    {
        HitInfo info = new HitInfo(-1.0f);
        foreach (RTObject obj in collisionObjects)   // check collision for each individual object
        {
            HitInfo tmpInfo = obj.Intersect(ray);
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

        return info;
    }

    Color CastRay(Ray ray)
    {
        return CastRay(ray, 0);
    }

    Color CastRay(Ray ray, int bounces)
    {
        Color color = RenderSettings.ambientLight;
        if (bounces >= maxBounces)
            return color;

        HitInfo info = CheckIntersection(ray);
        if (info.time >= 0)   // we hit something
        {
            // Now from each light source cast rays to this intersection point to compute soft edges
            for (int i = 0; i < lights.Count; i++)
            {
                Ray lightRay = new Ray(lights[i].position, (info.hitPoint - lights[i].position).normalized);
                HitInfo lightInfo = CheckIntersection(lightRay);
                if (lightInfo.time >= 0)
                {
                    // is the first hit of the light some other point than our point?
                    Vector3 difVector = info.hitPoint - lightInfo.hitPoint;
                    float difference = difVector.sqrMagnitude;
                    float threshold = 0.0001f;
                    if(difference < threshold)  // => same hitPoint => spot gets light
                        color += PhongColor(info, lights[i]);
                }
                else // this will never happen
                {
                    ULog.LogError("CastRay: AssertionError");
                    Debug.DrawLine(ray.origin, info.hitPoint, Color.white, 100.0f, false);
                    Debug.DrawLine(lightRay.origin, lightRay.origin + 100 * lightRay.direction, Color.green, 100.0f, false);
                }
            }

            // cast reflection ray by going into recursion. 
            if (info.shading.reflection > 0)
            {   // material has a reflection => send reflection ray
                float reflect = 2.0f * Vector3.Dot(ray.direction, info.normal);
                Vector3 reflectDir = (ray.direction - reflect * info.normal).normalized;// Direction from http://www.ics.uci.edu/~gopi/CS211B/RayTracing%20tutorial.pdf Page 9
                Ray reflectRay = new Ray(info.hitPoint + 0.001f * reflectDir, reflectDir);    // move ray an epsilon away from surface of object so it doesn't hit itself
                Color reflectColor = CastRay(reflectRay, bounces + 1);
                color += info.shading.reflection * reflectColor;
            }
            return color;
        }
        else {  // ray hits nothing
            return color;
        }
    }

    IEnumerator Render()
    {
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        Color[] colors = new Color[renderTexture.width];
        for (int y = renderTexture.height - 1; y >= 0; y--)
        {
            for (int x = 0; x < renderTexture.width; x++)
            {
                Ray ray = Camera.main.ScreenPointToRay(new Vector2(x, y));
                Color col = CastRay(ray);
                colors[x] = col;
            }
            renderTexture.SetPixels(0, y, renderTexture.width, 1, colors);
            // apply changes to the texture
            renderTexture.Apply();
            // update GUI every GUIUpdateRate rows
            if (sw.ElapsedMilliseconds > GUIUpdateRate)
            {
                yield return null;
                sw.Reset();
                sw.Start();
            }
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            showRender = !showRender;
            if (showRender)
            {
                renderUI.gameObject.SetActive(true);
                if (!rendered)
                {
                    rendered = true;
                    renderUI.texture = renderTexture;
                    StartCoroutine(Render());
                }
            }
            else
            {
                renderUI.gameObject.SetActive(false);
            }
        }
    }

    public struct HitInfo
    {
        public Vector3 hitPoint;    // hitpoint in World coordinates
        public float time;             // t parameter of hitpoint with regards to the ray used
        public Vector3 normal;
        public ShadingInfo shading;

        public HitInfo(ShadingInfo shading)
        {
            hitPoint = new Vector3();
            normal = new Vector3();
            this.time = -1.0f;
            this.shading = shading;
        }

        public HitInfo(float t)
        {
            hitPoint = new Vector3();
            normal = new Vector3();
            this.time = t;
            this.shading = new ShadingInfo(Color.black);
        }
    }

    public struct ShadingInfo
    {
        // Material properties
        public Color diffuse;
        public Color specular;
        public float specularIntensity;
        public float reflection;

        public ShadingInfo(Color diffuse)
        {
            this.diffuse = diffuse;
            specular = Color.white;
            specularIntensity = 10;
            reflection = 0;
        }
    }
}
