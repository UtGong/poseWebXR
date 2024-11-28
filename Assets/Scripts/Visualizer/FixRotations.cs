using System.Collections.Generic;
using UnityEngine;

public class FixRotations : MonoBehaviour
{
    // List of characters (GameObjects) to apply rotation fix
    public List<GameObject> characters = new List<GameObject>();

    // The rotation offset to apply when the character is in the target animation
    public Vector3 rotationOffset = new Vector3(90, 0, 0); // Adjust this to correct the downward facing rotation

    void Start()
    {
        // For each character in the list, apply the rotation fix
        foreach (GameObject character in characters)
        {
            // Get the Animator component attached to the character
            Animator animator = character.GetComponent<Animator>();

            if (animator != null)
            {
                // Apply the rotation fix to the character (since only one animation is used)
                ApplyRotationFix(character);
            }
        }
    }

    // Function to apply the rotation fix to a character
    private void ApplyRotationFix(GameObject character)
    {
        // Apply the rotation to the character's transform
        character.transform.rotation = Quaternion.Euler(rotationOffset);
    }
}
