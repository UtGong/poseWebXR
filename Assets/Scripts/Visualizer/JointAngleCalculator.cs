using UnityEngine;

public class JointAngleCalculator : MonoBehaviour
{
    public Transform mainJoint; // The main joint
    public Transform connectedJoint1; // First connected joint
    public Transform connectedJoint2; // Second connected joint

    void Update()
    {
        if (mainJoint != null && connectedJoint1 != null && connectedJoint2 != null)
        {
            // Calculate the vectors from the main joint to the connected joints
            Vector3 vector1 = connectedJoint1.position - mainJoint.position;
            Vector3 vector2 = connectedJoint2.position - mainJoint.position;

            // Calculate the angle between the two vectors
            float angle = Vector3.Angle(vector1, vector2);

            // Log the calculated angle
            Debug.Log($"Angle at {mainJoint.name}: {angle} degrees");
        }
    }
}
