using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Frustum : MonoBehaviour {
    [SerializeField]
    private Camera mainCamera;

    [SerializeField] private List<GameObject> objectsToCull;

    private void CullObjects() {
        foreach (GameObject obj in objectsToCull) {
            BoxCollider boundingBox = obj.GetComponent<BoxCollider>();
            if (!boundingBox) {
                Debug.LogError("Object " + obj.name + " does not have a BoxCollider component.");
                continue;
            }

            Debug.Log("["+ obj.name + "] Position: " + obj.transform.position);

            // Si una de las aristas de la boundingBox se encuentra dentro del frustum.

            Vector3[] vertices = GetBoundingBoxVertices(boundingBox);

            if (IsAnyVertexInFrustum(vertices))
            {
                Debug.Log("[" + obj.name + "] esta adentro.");
            }

        }
    }

    private Vector3[] GetBoundingBoxVertices(BoxCollider collider)
    {
        Vector3[] vertices = new Vector3[8];

        Vector3 center = collider.transform.TransformPoint(collider.center);
        Vector3 extents = collider.size / 2f;

        // Calculate the vertices of the bounding box
        vertices[0] = center + new Vector3(extents.x, extents.y, extents.z);
        vertices[1] = center + new Vector3(-extents.x, extents.y, extents.z);
        vertices[2] = center + new Vector3(extents.x, -extents.y, extents.z);
        vertices[3] = center + new Vector3(-extents.x, -extents.y, extents.z);
        vertices[4] = center + new Vector3(extents.x, extents.y, -extents.z);
        vertices[5] = center + new Vector3(-extents.x, extents.y, -extents.z);
        vertices[6] = center + new Vector3(extents.x, -extents.y, -extents.z);
        vertices[7] = center + new Vector3(-extents.x, -extents.y, -extents.z);

        return vertices;
    }

    private bool IsAnyVertexInFrustum(Vector3[] vertices)
    {
        Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(mainCamera);

        foreach (Vector3 vertex in vertices)
        {
            bool isInsideFrustum = true;

            foreach (Plane plane in frustumPlanes)
            {
                if (plane.GetDistanceToPoint(vertex) < 0)
                {
                    isInsideFrustum = false;
                    break;
                }
            }

            if (isInsideFrustum)
            {
                return true;
            }
        }

        return false;
    }

    private void Awake()
    {
        mainCamera = GetComponent<Camera>();
    }

    private void Update()
    {
        CullObjects();
    }
}
