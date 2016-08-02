using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
[ExecuteInEditMode]
public class UnityTriangle : MonoBehaviour {
    public Vector3 v0, v1, v2;

	void Awake () {
        Mesh mesh = new Mesh();
        mesh.vertices = new Vector3[] {v0, v1, v2 };
        mesh.triangles = new int[] {0, 1, 2 };

        GetComponent<MeshFilter>().mesh = mesh;
    }
	
}
