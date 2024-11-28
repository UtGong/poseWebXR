using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PoseRenderer : MonoBehaviour
{
    // Define the body parts to modify
    string[] _bodyToModify = new string[] { "Pelvis", "Left_Hip", "Right_Hip", "Spine1", "Left_Knee", "Right_Knee", "Spine2", "Left_Ankle", "Right_Ankle", "Spine3", "Left_Foot", "Right_Foot", "Neck", "Left_Collar", "Right_Collar", "Head", "Left_Shoulder", "Right_Shoulder", "Left_Elbow", "Right_Elbow", "Left_Wrist", "Right_Wrist", "Left_Index1", "Right_Index1" };

    // Data structure to hold the 3D position for each body part
    [System.Serializable]
    public class BodyPartData
    {
        public Vector3 position;
        public Quaternion rotation; // Rotation can be optional if your data includes it
    }

    // Structure to hold the pose data for each frame
    [System.Serializable]
    public class PoseFrameData
    {
        public Dictionary<string, BodyPartData> bodyParts = new Dictionary<string, BodyPartData>();
    }

    // List to hold the frames of pose data
    public List<PoseFrameData> allPoseData = new List<PoseFrameData>();

    // Prefab for the players
    public GameObject playerPrefab;

    // Instantiate player objects
    private GameObject[] players;
    public int numPlayers = 10;  // Number of players

    // For player body part transformations
    private Dictionary<string, Transform>[] _transformFromNamePlayers;

    void Start()
    {
        // Initialize player objects
        players = new GameObject[numPlayers];
        _transformFromNamePlayers = new Dictionary<string, Transform>[numPlayers];

        // Initialize each player prefab
        for (int i = 0; i < numPlayers; i++)
        {
            players[i] = Instantiate(playerPrefab, new Vector3(i * 2, 0, 0), Quaternion.identity);
            _transformFromNamePlayers[i] = new Dictionary<string, Transform>();

            // Store body part transforms (you must assign these based on your prefab structure)
            // Example: Find each transform for the player's body parts
            _transformFromNamePlayers[i]["Pelvis"] = players[i].transform.Find("Pelvis");
            _transformFromNamePlayers[i]["Left_Hip"] = players[i].transform.Find("Left_Hip");
            // Repeat for other body parts...
        }

        // Load CSV and parse data
        LoadPoseData("Assets/Resources/pose_data.csv");
    }

    // Method to load and parse CSV data
    void LoadPoseData(string path)
    {
        string[] lines = File.ReadAllLines(path);
        int frameIndex = 0;

        // Skip header and process each frame
        for (int i = 1; i < lines.Length; i++)
        {
            string[] entries = lines[i].Split(',');

            // Create a new PoseFrameData for each frame
            PoseFrameData frameData = new PoseFrameData();

            // Read the data for each player and their body parts
            for (int playerIndex = 0; playerIndex < numPlayers; playerIndex++)
            {
                string prefix = $"Player{playerIndex + 1}"; // Player identifier

                // For each body part, we get the x, y, z coordinates
                foreach (string bodyPart in _bodyToModify)
                {
                    int idx = Array.IndexOf(_bodyToModify, bodyPart);
                    Vector3 position = new Vector3(
                        float.Parse(entries[3 * idx + 2]), // x position
                        float.Parse(entries[3 * idx + 3]), // y position
                        float.Parse(entries[3 * idx + 4])  // z position
                    );

                    Quaternion rotation = Quaternion.identity;  // Optional if you have rotation data

                    // Add the body part data to the frame
                    if (!frameData.bodyParts.ContainsKey(bodyPart))
                        frameData.bodyParts[bodyPart] = new BodyPartData();

                    frameData.bodyParts[bodyPart].position = position;
                    frameData.bodyParts[bodyPart].rotation = rotation;
                }
            }

            allPoseData.Add(frameData);
            frameIndex++;
        }
    }

    // Update poses based on the loaded data (e.g., in Update or a Coroutine)
    void UpdatePose(int frameIndex)
    {
        // Ensure the frameIndex is valid
        if (frameIndex >= 0 && frameIndex < allPoseData.Count)
        {
            PoseFrameData currentFrame = allPoseData[frameIndex];

            for (int playerIndex = 0; playerIndex < numPlayers; playerIndex++)
            {
                // For each body part, update the transform position
                foreach (string bodyPart in _bodyToModify)
                {
                    if (currentFrame.bodyParts.ContainsKey(bodyPart))
                    {
                        Transform bodyPartTransform = _transformFromNamePlayers[playerIndex][bodyPart];
                        if (bodyPartTransform != null)
                        {
                            bodyPartTransform.position = currentFrame.bodyParts[bodyPart].position;
                            bodyPartTransform.rotation = currentFrame.bodyParts[bodyPart].rotation;
                        }
                    }
                }
            }
        }
    }
}
