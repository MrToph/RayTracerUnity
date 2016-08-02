# RayTracer

An implementation of a ray tracer that renders spheres, planes, triangles, and arbitrary models consisting of triangles defined in an .obj file.
The implementation is written in C# independent of Unity's engine. We only use Unity's scene management capabilities to feed data to our objects:
You can drag and drop objects (prefabs) into Unity's scene, and the coordinates will automatically be transformed into a representation usable by our RayTracer, which then renders the scene.
![RayTracer Render](/README/raytracer3DModel.png?raw=true "RayTracer Render")