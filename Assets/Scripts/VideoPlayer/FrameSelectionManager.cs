using System.Collections.Generic;
using UnityEngine;
using ProjektSumperk;

public class FrameSelectionManager : MonoBehaviour
{
    public static FrameSelectionManager Instance;

    [Header("Visualizer Receivers")]
    public StaticPoseVisualizer visualizer1; // Visualizer for the first video
    public StaticPoseVisualizer visualizer2; // Visualizer for the second video

    [Header("Graph Parent")]
    public GameObject graphParent; // Reference to the GraphParent GameObject
    private DualBarChart dualBarChart; // Reference to the DualBarChart script
    private LineGraphDual lineGraphDual; // Reference to the LineGraphDual script

    private bool video1FramesReady = false;
    private bool video2FramesReady = false;

    private string video1Name = null, video2Name = null;
    private int video1StartFrame = -1, video1EndFrame = -1, video2StartFrame = -1, video2EndFrame = -1;
    private float video1StartTime = -1, video1EndTime = -1, video2StartTime = -1, video2EndTime = -1;

    // Frame containers for each video
    private Transform video1FrameContainer = null;
    private Transform video2FrameContainer = null;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Find the DualBarChart script attached to the GraphParent GameObject
        if (graphParent != null)
        {
            dualBarChart = graphParent.GetComponent<DualBarChart>();
            lineGraphDual = graphParent.GetComponent<LineGraphDual>(); // Correct variable name here
        }
        else
        {
            Debug.LogError("GraphParent GameObject is not assigned.");
        }
    }

    // Method to receive frame data from VideoController
    public void SetFrameData(string videoName, int startFrame, int endFrame, float startTime, float endTime, Transform frameContainer)
    {
        // If video1Name is null, assign this video to video1
        if (video1Name == null)
        {
            video1Name = videoName;
            video1StartFrame = startFrame;
            video1EndFrame = endFrame;
            video1StartTime = startTime;
            video1EndTime = endTime;
            video1FrameContainer = frameContainer; // Store the frame container
            video1FramesReady = true;
        }
        // If this video matches video1Name, update its data
        else if (videoName == video1Name)
        {
            video1StartFrame = startFrame;
            video1EndFrame = endFrame;
            video1StartTime = startTime;
            video1EndTime = endTime;
            video1FrameContainer = frameContainer; // Update the frame container if needed
        }
        // If video2Name is null and this is a different video, assign it to video2
        else if (video2Name == null)
        {
            video2Name = videoName;
            video2StartFrame = startFrame;
            video2EndFrame = endFrame;
            video2StartTime = startTime;
            video2EndTime = endTime;
            video2FrameContainer = frameContainer; // Store the frame container
            video2FramesReady = true;
        }
        // If this video matches video2Name, update its data
        else if (videoName == video2Name)
        {
            video2StartFrame = startFrame;
            video2EndFrame = endFrame;
            video2StartTime = startTime;
            video2EndTime = endTime;
            video2FrameContainer = frameContainer; // Update the frame container if needed
        }
        // If a third unique video appears, ignore it
        else
        {
            Debug.LogWarning($"Ignored frame data for {videoName}. Only two videos can be processed.");
            return;
        }

        Debug.Log($"Checking for video data completion...");
        // Check if both videos have selected their frames
        if (video1FramesReady && video2FramesReady)
        {
            Debug.Log("Both videos have selected their frames!");
            SendData();
        }
    }

    private void SendData()
    {
        Debug.Log("Sending frame data to visualizers...");

        // Send frame data to visualizer1
        if (visualizer1 != null && video1FramesReady)
        {
            List<float> frameTimes = CalculateFrameTimes(video1StartFrame, video1EndFrame, video1StartTime, video1EndTime);
            visualizer1.ReceiveFrameData(frameTimes, video1FrameContainer, video1StartFrame, video1EndFrame);
            Debug.Log($"Data sent to Visualizer1: Frames {video1StartFrame} to {video1EndFrame}");
        }

        // Send frame data to visualizer2
        if (visualizer2 != null && video2FramesReady)
        {
            List<float> frameTimes = CalculateFrameTimes(video2StartFrame, video2EndFrame, video2StartTime, video2EndTime);
            visualizer2.ReceiveFrameData(frameTimes, video2FrameContainer, video2StartFrame, video2EndFrame);
            Debug.Log($"Data sent to Visualizer2: Frames {video2StartFrame} to {video2EndFrame}");
        }
        dualBarChart.StartPlotting();
        lineGraphDual.StartPlotting();
        // Reset flags for the next selection
        ResetSelections();
    }

    private List<float> CalculateFrameTimes(int startFrame, int endFrame, float startTime, float endTime)
    {
        List<float> frameTimes = new List<float>();
        int totalFrames = endFrame - startFrame + 1;
        float interval = (endTime - startTime) / (totalFrames - 1);

        for (int i = 0; i < totalFrames; i++)
        {
            frameTimes.Add(startTime + (i * interval));
        }

        return frameTimes;
    }

    private void ResetSelections()
    {
        video1FramesReady = false;
        video2FramesReady = false;
    }
}