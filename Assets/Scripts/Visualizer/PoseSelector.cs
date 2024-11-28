using UnityEngine;

public class PoseSelector : MonoBehaviour
{
    private GameObject pose; // Reference to the pose
    private StaticPoseSelector poseSelector; // Reference to the StaticPoseSelector

    // Initialize the selector with the pose and selector
    public void Initialize(GameObject selectedPose, StaticPoseSelector selector)
    {
        pose = selectedPose;
        poseSelector = selector;
    }

    void OnMouseDown()
    {
        // Select the pose when clicked
        if (!poseSelector.selectedPose1 || !poseSelector.selectedPose2)
        {
            if (poseSelector.selectedPose1 == null || pose != poseSelector.selectedPose1)
            {
                poseSelector.OnPoseSelected(pose);
            }
            else if (poseSelector.selectedPose2 == null || pose != poseSelector.selectedPose2)
            {
                poseSelector.OnPoseSelected(pose);
            }
        }
    }

    void OnMouseUp()
    {
        // Unselect the pose if clicked again
        if (poseSelector.selectedPose1 == pose)
        {
            poseSelector.OnPoseUnselected(pose);
        }
        else if (poseSelector.selectedPose2 == pose)
        {
            poseSelector.OnPoseUnselected(pose);
        }
    }
}
