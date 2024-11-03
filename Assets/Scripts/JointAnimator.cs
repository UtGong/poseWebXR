using UnityEngine;

public class JointAnimator : MonoBehaviour
{
    [SerializeField] private CSVDataLoader csvDataLoader;
    [SerializeField] private JointMapper jointMapper;

    void Start()
    {
        Debug.Log("JointAnimator Start method called.");
        
        if (csvDataLoader == null || jointMapper == null)
        {
            Debug.LogError("CSVDataLoader or JointMapper reference is missing.");
            return;
        }

        Vector3[] frameData = csvDataLoader.GetSingleFrameData();
        if (frameData == null || frameData.Length == 0)
        {
            Debug.LogError("Frame data is null or empty in JointAnimator.");
        }
        else
        {
            Debug.Log("Frame data loaded in JointAnimator with " + frameData.Length + " joints.");
            ApplySingleFrameData(frameData);
        }
    }

    private void ApplySingleFrameData(Vector3[] frameData)
    {
        Debug.Log("Applying single frame data.");
        for (int i = 0; i < frameData.Length; i++)
        {
            Transform jointTransform = jointMapper.GetJointTransform(i);
            if (jointTransform != null)
            {
                Debug.Log($"Setting position for Joint {i}: Old Position: {jointTransform.localPosition}, New Position: {frameData[i]}");
                jointTransform.localPosition = frameData[i];
            }
            else
            {
                Debug.LogWarning($"No transform found for joint index: {i}");
            }
        }
    }
}
