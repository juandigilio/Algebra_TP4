using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class Frustum : MonoBehaviour {
    [SerializeField]
    private Camera mainCamera;

    [SerializeField] 
    private List<GameObject> objectsToCull;

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

    Bounds GetMeshBounds(Mesh mesh)
    {
        Vector3[] vertices = mesh.vertices;

        if (vertices.Length == 0)
        {
            Debug.LogWarning("The mesh has no vertices!");
            return new Bounds(Vector3.zero, Vector3.zero);
        }

        Vector3 min = vertices[0];
        Vector3 max = vertices[0];

        for (int i = 1; i < vertices.Length; i++)
        {
            min = Vector3.Min(min, vertices[i]);
            max = Vector3.Max(max, vertices[i]);
        }

        Bounds bounds = new Bounds((max + min) / 2f, max - min);
        return bounds;
    }

    /// <summary>
    /// Devuelve los vertices de la bounding box de una mesh transformados.
    /// </summary>
    /// <param name="mesh"></param>
    /// <param name="objTransform"></param>
    /// <returns></returns>
    Vector3[] GetMeshBoundsVertex(Mesh mesh, Transform objTransform)
    {
        Bounds bounds = GetMeshBounds(mesh);
        Vector3 center = bounds.center;
        Vector3 extents = bounds.extents;

        // Nuestro frustum trabaja con vertices Vector3, por lo tanto debemos pasar nuestra variable Bounds a 8 vertices.
        Vector3[] corners = new Vector3[8];

        corners[0] = center + new Vector3(-extents.x, -extents.y, -extents.z);
        corners[1] = center + new Vector3(extents.x, -extents.y, -extents.z);
        corners[2] = center + new Vector3(-extents.x, -extents.y, extents.z);
        corners[3] = center + new Vector3(extents.x, -extents.y, extents.z);
        corners[4] = center + new Vector3(-extents.x, extents.y, -extents.z);
        corners[5] = center + new Vector3(extents.x, extents.y, -extents.z);
        corners[6] = center + new Vector3(-extents.x, extents.y, extents.z);
        corners[7] = center + new Vector3(extents.x, extents.y, extents.z);

        // Ya tenemos todas los vertices de la caja, pero tambien debemos transformar la posicion de estos a su posicion en el mundo.
        for (int i = 0; i < corners.Length; i++)
        {
            corners[i] = objTransform.TransformPoint(corners[i]);
        }

        return corners;
    }

    /// <summary>
    /// Transforma todos los vertices de una mesh y los devuelve en un array.
    /// </summary>
    /// <param name="mesh"></param>
    /// <param name="objTransform"></param>
    /// <returns></returns>
    Vector3[] GetMeshVertex(Mesh mesh, Transform objTransform)
    {
        Vector3[] meshVertex = new Vector3[mesh.vertexCount];

        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            meshVertex[i] = objTransform.TransformPoint(mesh.vertices[i]);
        }

        return meshVertex;
    }

    private void CullObjects()
    {
        foreach (GameObject obj in objectsToCull)
        {
            MeshFilter mf = obj.GetComponent<MeshFilter>();
            if (!mf) 
            {
                Debug.LogWarning("Object " + obj.name + " does not have a mesh."); // El objeto no tiene ningun modelo.
                continue;
            }
            MeshRenderer mr = obj.GetComponent<MeshRenderer>();
            if (!mr)
            {
                Debug.LogWarning("Object " + obj.name + " does not have a mesh renderer."); // El objeto no se esta dibujando, por lo tanto no es necesario hacer cull.
                continue;
            }

            Vector3[] boundingBoxVertex = GetMeshBoundsVertex(mf.mesh, obj.transform); // Sacamos la bounding box de la mesh.

            if (IsAnyVertexInFrustum(boundingBoxVertex)) // Verificamos si alguno de los verices de la bounding box se encuentra dentro del frustum.
            {
                // Ahora, antes de activar el renderer debemos verificar que uno de los vertices del modelo se encuentre dentro del frustum.
                Vector3[] meshVertex = GetMeshVertex(mf.mesh, obj.transform); // Sacamos los vertices de la mesh.
                // (Cabe destacar que mesh tiene mesh.vertices, pero nosotros los necesitamos transformados al transform de nuestro gameObject)

                // Hacemos el segundo checkeo.
                if (IsAnyVertexInFrustum(meshVertex))
                {
                    mr.enabled = true; // Al menos un vertice de la mesh se encuentra dentro del frustum, activamos el mesh renderer para que se dibuje el modelo.
                }
                else
                {
                    mr.enabled = false; // Si ningun vertice de la mesh se encuentra dentro del frustum entonces no lo dibujamos.
                }

            }
            else
            {
                mr.enabled = false; // Si la bounding box esta fuera del frustum, no dibujamos el modelo.
            }

        }
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
