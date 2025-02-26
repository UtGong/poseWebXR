using UnityEngine;
using UnityEditor;

public class BatchAnimationSpeedModifier : MonoBehaviour
{
    [MenuItem("Tools/Batch Adjust Animation Speed")]
    public static void AdjustAnimationSpeedBatch()
    {
        // Prompt the user to select multiple AnimationClips
        Object[] selectedObjects = Selection.objects;

        if (selectedObjects.Length == 0)
        {
            Debug.LogError("Please select one or more AnimationClips in the Project window.");
            return;
        }

        foreach (Object obj in selectedObjects)
        {
            AnimationClip originalClip = obj as AnimationClip;
            if (originalClip == null)
            {
                Debug.LogWarning($"The selected object '{obj.name}' is not an AnimationClip. Skipping...");
                continue;
            }

            // Clone the original animation clip
            AnimationClip newClip = new AnimationClip();
            EditorUtility.CopySerialized(originalClip, newClip);

            // Scale the keyframe times to slow down the animation
            AnimationClipCurveData[] curveDatas = AnimationUtility.GetAllCurves(originalClip, true);

            foreach (AnimationClipCurveData curveData in curveDatas)
            {
                AnimationCurve curve = new AnimationCurve();
                foreach (Keyframe key in curveData.curve.keys)
                {
                    // Double the time for each keyframe
                    Keyframe newKey = new Keyframe(key.time * 2, key.value, key.inTangent, key.outTangent)
                    {
                        tangentMode = key.tangentMode
                    };
                    curve.AddKey(newKey);
                }

                newClip.SetCurve(curveData.path, curveData.type, curveData.propertyName, curve);
            }

            // Save the new animation clip in the project
            string originalPath = AssetDatabase.GetAssetPath(originalClip);
            string newPath = originalPath.Replace(".anim", "_Slowed.anim");
            AssetDatabase.CreateAsset(newClip, newPath);

            Debug.Log($"Created slowed-down animation clip: {newPath}");
        }

        // Refresh the AssetDatabase to show new clips in the Project window
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}