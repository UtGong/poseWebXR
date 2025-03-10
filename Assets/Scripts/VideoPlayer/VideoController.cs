using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using Nobi.UiRoundedCorners;

public class VideoController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Video Playback Controls")]
    public VideoPlayer videoPlayer;
    public Slider timeSlider;
    public Button playPauseButton;
    public TMP_Text playPauseButtonText;

    [Header("Frame Extraction")]
    public float timeInterval = 0.3f;
    public Canvas targetCanvas;

    private bool isDragging = false;
    private bool hasPlayedOnce = false;
    private Transform frameContainer;
    private GameObject framePrefab;
    public RectTransform videoScreen;

    private int startFrame = -1, endFrame = -1;
    private bool framesReady = false;
    private string videoName;

    private GameObject startBoundingBox;  // Bounding box for start frame
    private GameObject endBoundingBox;    // Bounding box for end frame

    void Start()
    {
        if (targetCanvas == null)
        {
            Debug.LogError("Please assign a Canvas in the Inspector!");
            return;
        }

        if (videoPlayer == null || timeSlider == null || playPauseButton == null || playPauseButtonText == null)
        {
            Debug.LogError("Please assign all required components in the Inspector!");
            return;
        }

        videoPlayer.prepareCompleted += OnVideoPrepared;
        videoPlayer.loopPointReached += OnVideoEnded;

        playPauseButton.onClick.AddListener(TogglePlayPause);
        timeSlider.onValueChanged.AddListener(OnSliderValueChanged);

        videoPlayer.Prepare();
        playPauseButtonText.text = "Loading...";

        // Initialize the bounding boxes
        CreateBoundingBoxes();
    }

    void Update()
    {
        if (videoPlayer.isPrepared && videoPlayer.isPlaying && !isDragging)
        {
            timeSlider.value = (float)(videoPlayer.time / videoPlayer.length);
        }
    }

    private void TogglePlayPause()
    {
        if (!videoPlayer.isPrepared)
        {
            Debug.LogWarning("Video is not ready yet.");
            return;
        }

        if (videoPlayer.isPlaying)
        {
            videoPlayer.Pause();
            playPauseButtonText.text = "Play";
        }
        else
        {
            videoPlayer.Play();
            playPauseButtonText.text = "Pause";
        }
    }

    private void OnSliderValueChanged(float value)
    {
        if (videoPlayer.isPrepared && isDragging)
        {
            float newTime = value * (float)videoPlayer.length;
            videoPlayer.time = newTime;
            Debug.Log($"Video time set to: {newTime} seconds.");
        }
    }

    private void OnVideoPrepared(VideoPlayer vp)
    {
        Debug.Log("Video prepared successfully.");
        playPauseButtonText.text = "Pause";
        timeSlider.value = 0f;
        timeSlider.interactable = true;

        videoPlayer.Play();
    }

    private void OnVideoEnded(VideoPlayer vp)
    {
        Debug.Log("Video playback completed.");
        playPauseButtonText.text = "Play";

        if (!hasPlayedOnce)
        {
            StartCoroutine(ExtractFrames());
            hasPlayedOnce = true;
        }

        videoPlayer.Pause();
        videoPlayer.time = 0;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.pointerEnter == timeSlider.gameObject)
        {
            isDragging = true;
            videoPlayer.Pause();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.pointerEnter == timeSlider.gameObject)
        {
            isDragging = false;

            float newTime = timeSlider.value * (float)videoPlayer.length;
            videoPlayer.time = newTime;
            videoPlayer.Play();
            playPauseButtonText.text = "Pause";

            Debug.Log($"Video resumed at {newTime} seconds.");
        }
    }

    IEnumerator ExtractFrames()
    {
        Debug.Log("Extracting frames...");
        float totalTime = (float)videoPlayer.length;
        int frameCount = Mathf.FloorToInt(totalTime / timeInterval);
        Debug.Log($"Total frames to extract: {frameCount}");

        if (framePrefab == null)
        {
            framePrefab = CreateFramePrefab();
        }

        GameObject videoCloneObj = new GameObject("VideoPlayerClone");
        VideoPlayer videoClone = videoCloneObj.AddComponent<VideoPlayer>();
        videoClone.clip = videoPlayer.clip;
        videoClone.renderMode = VideoRenderMode.RenderTexture;

        RenderTexture tempRenderTexture = new RenderTexture(2880, 1620, 24);
        // RenderTexture tempRenderTexture = new RenderTexture(1920, 1080, 24);
        videoClone.targetTexture = tempRenderTexture;

        videoClone.Prepare();
        yield return new WaitUntil(() => videoClone.isPrepared);

        if (frameContainer == null)
        {
            frameContainer = new GameObject("FrameContainer").transform;
            frameContainer.name = "FrameContainer_" + videoPlayer.clip.name;
            frameContainer.SetParent(videoScreen.parent, false);
            // frameContainer.localPosition = Vector3.zero;
            frameContainer.localPosition = new Vector3(-120, frameContainer.localPosition.y, frameContainer.localPosition.z);

        }

        foreach (Transform child in frameContainer)
            Destroy(child.gameObject);

        float startX = videoScreen.anchoredPosition.x + videoScreen.rect.width + 10;
        float startY = videoScreen.anchoredPosition.y;
        float currentX = startX;
        float spacing = 10f;

        for (int i = 1; i < frameCount; i++)
        {
            float timestamp = i * timeInterval;
            videoClone.time = timestamp;

            videoClone.Play();
            yield return new WaitForSeconds(0.1f);
            videoClone.Pause();

            RenderTexture.active = tempRenderTexture;
            Texture2D frameTexture = new Texture2D(tempRenderTexture.width, tempRenderTexture.height, TextureFormat.RGB24, false);
            frameTexture.ReadPixels(new Rect(0, 0, tempRenderTexture.width, tempRenderTexture.height), 0, 0);
            frameTexture.Apply();
            RenderTexture.active = null;            

            GameObject frameObj = Instantiate(framePrefab, frameContainer);
            frameObj.name = $"Frame_{i}";
            frameObj.GetComponent<Image>().sprite = Sprite.Create(frameTexture, new Rect(0, 0, frameTexture.width, frameTexture.height), Vector2.zero);

            // make the frame rounded by adding "image with rounded corners" script and set the radius to 22
            ImageWithRoundedCorners imageWithRoundedCorners = frameObj.AddComponent<ImageWithRoundedCorners>();
            imageWithRoundedCorners.radius = 22;

            Button frameButton = frameObj.GetComponent<Button>();
            if (frameButton != null)
            {
                int index = i;
                frameButton.onClick.AddListener(() => OnFrameSelected(index, frameObj.GetComponent<RectTransform>()));
            }

            RectTransform frameRect = frameObj.GetComponent<RectTransform>();
            frameRect.anchorMin = new Vector2(0, 0.5f);
            frameRect.anchorMax = new Vector2(0, 0.5f);
            frameRect.pivot = new Vector2(0, 0.5f);
            frameRect.anchoredPosition = new Vector2(currentX, startY);

            currentX += frameRect.rect.width + spacing;
            frameObj.SetActive(true);
        }

        Destroy(videoCloneObj);
        tempRenderTexture.Release();
        Destroy(tempRenderTexture);
    }

    private void OnFrameSelected(int index, RectTransform frameRect)
    {
        if (startFrame == -1)
        {
            startFrame = index;
            HighlightFrame(startBoundingBox, frameRect);
        }
        else if (endFrame == -1)
        {
            endFrame = index;
            HighlightFrame(endBoundingBox, frameRect);
            framesReady = true;
            SendFrameData();
        }
        else
        {
            startFrame = index;
            endFrame = -1;
            HighlightFrame(startBoundingBox, frameRect);
        }
    }

    private void HighlightFrame(GameObject boundingBox, RectTransform frameRect)
    {
        // Parent bounding box to the same parent as the frame
        boundingBox.transform.SetParent(frameRect.parent, false);

        // Match the frame's size and position
        RectTransform boxRect = boundingBox.GetComponent<RectTransform>();
        boxRect.sizeDelta = frameRect.sizeDelta; // Match size
        boxRect.anchoredPosition = frameRect.anchoredPosition; // Match position
        // boundingbox's anchored min max should be the same as the frame's anchored min max
        boxRect.anchorMin = frameRect.anchorMin;
        boxRect.anchorMax = frameRect.anchorMax;
        boxRect.pivot = frameRect.pivot;

        // boundingBox's rect transform position should be the same as the frame's rect transform position
        boundingBox.transform.position = frameRect.position;

        // Ensure the bounding box is above other elements
        boundingBox.transform.SetAsLastSibling();
        boundingBox.SetActive(true);
    }

    private void SendFrameData()
    {
        if (framesReady)
        {
            string videoName = videoPlayer.clip != null ? videoPlayer.clip.name : "UnknownVideo";
            float startTime = startFrame * timeInterval;
            float endTime = endFrame * timeInterval;

            // Pass the frameContainer along with other data
            FrameSelectionManager.Instance.SetFrameData(videoName, startFrame, endFrame, startTime, endTime, frameContainer);
        }
    }

    private void CreateBoundingBoxes()
    {
        startBoundingBox = CreateBoundingBox("StartBoundingBox", Color.white);
        endBoundingBox = CreateBoundingBox("EndBoundingBox", Color.white);
    }

    private GameObject CreateBoundingBox(string name, Color color)
    {
        GameObject box = new GameObject(name, typeof(Image));
        RectTransform rect = box.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(192, 108);
        Image image = box.GetComponent<Image>();
        image.color = new Color(color.r, color.g, color.b, 0.5f);
        box.SetActive(false);
        ImageWithRoundedCorners imageWithRoundedCorners = box.AddComponent<ImageWithRoundedCorners>();
        imageWithRoundedCorners.radius = 22;
        return box;
    }

    private GameObject CreateFramePrefab()
    {
        GameObject frameObj = new GameObject("FramePrefab", typeof(RectTransform), typeof(Button), typeof(Image));
        RectTransform rect = frameObj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(192, 108) * 1.5f;
        Image image = frameObj.GetComponent<Image>();
        image.preserveAspect = true;
        frameObj.SetActive(false);
        return frameObj;
    }
}