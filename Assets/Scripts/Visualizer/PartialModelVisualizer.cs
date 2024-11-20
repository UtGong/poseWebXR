using System.Collections.Generic;
using UnityEngine;

public class PartialModelVisualizer : MonoBehaviour
{
    public GameObject fullCharacterPrefab; // Full character model prefab
    public List<string> bonesToClone = new List<string>(); // List of bone names to clone
    public Vector3 clonePosition = Vector3.zero; // Position for the cloned partial model

    void Start()
    {
        if (fullCharacterPrefab == null)
        {
            Debug.LogError("Full character prefab is not assigned!");
            return;
        }

        if (bonesToClone.Count == 0)
        {
            Debug.LogError("No bones specified to clone!");
            return;
        }

        CreatePartialModel();
    }

    private void CreatePartialModel()
    {
        // Instantiate the full character prefab
        GameObject fullCharacter = Instantiate(fullCharacterPrefab);
        
        // Create a new GameObject for the partial model
        GameObject partialModel = new GameObject("PartialModel");

        // Get the SkinnedMeshRenderer from the full character
        SkinnedMeshRenderer fullMeshRenderer = fullCharacter.GetComponentInChildren<SkinnedMeshRenderer>();
        if (fullMeshRenderer == null)
        {
            Debug.LogError("No SkinnedMeshRenderer found on the full character!");
            Destroy(fullCharacter);
            return;
        }

        // Clone specified bones
        List<Transform> clonedBones = new List<Transform>();
        foreach (string boneName in bonesToClone)
        {
            Transform bone = FindChildRecursive(fullCharacter.transform, boneName);
            if (bone != null)
            {
                Transform clonedBone = Instantiate(bone, partialModel.transform);
                clonedBone.name = bone.name; // Ensure the cloned bone has the same name
                clonedBones.Add(clonedBone);
            }
            else
            {
                Debug.LogError($"Bone {boneName} not found in full character!");
            }
        }

        // Create a SkinnedMeshRenderer for the partial model
        SkinnedMeshRenderer partialMeshRenderer = partialModel.AddComponent<SkinnedMeshRenderer>();
        partialMeshRenderer.sharedMesh = fullMeshRenderer.sharedMesh;
        partialMeshRenderer.materials = fullMeshRenderer.materials;

        // Set the cloned bones as the bones for the SkinnedMeshRenderer
        partialMeshRenderer.bones = clonedBones.ToArray();

        // Set the root bone for the SkinnedMeshRenderer
        partialMeshRenderer.rootBone = FindChildRecursive(partialModel.transform, "mixamorig:Hips");

        // Set the position of the partial model
        partialModel.transform.position = clonePosition;

        // Destroy the full character instance
        Destroy(fullCharacter);

        Debug.Log("Cloning and visualization complete!");
    }

    private Transform FindChildRecursive(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
            {
                return child;
            }

            Transform found = FindChildRecursive(child, name);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }
}
