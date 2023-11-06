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
