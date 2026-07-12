using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Serialization;

[RequireComponent(typeof(Canvas))]
public class GestureManager : MonoBehaviour
{
    private static GestureManager _instance;
    public static GestureManager Instance => _instance;

    [Header("Marker Canvas (this GameObject, always renders on top)")]
    [FormerlySerializedAs("ownCanvas")]
    public Canvas markerCanvas;

    [Header("Webcam Canvas (separate child, renders behind scene UI)")]
    public Canvas webcamCanvas;
    public RawImage webcamBackground;

    [Header("Markers")]
    public RectTransform leftHandMarker;
    public RectTransform rightHandMarker;
    public RectTransform leftFootMarker;
    public RectTransform rightFootMarker;

    [Header("Scenes without gesture display")]
    public string[] hiddenInScenes = new string[] { "MainMenu" };

    private BodyMarker bodyMarker;
    private UDPReceiver udpReceiver;

    public UDPReceiver Receiver => udpReceiver;
    public Canvas GetCurrentCanvas() => markerCanvas;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            if (webcamCanvas != null) Destroy(webcamCanvas.gameObject);
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        if (webcamCanvas != null)
            DontDestroyOnLoad(webcamCanvas.gameObject);

        bodyMarker = GetComponentInChildren<BodyMarker>(true);
        udpReceiver = GetComponentInChildren<UDPReceiver>(true);

        if (udpReceiver == null)
            Debug.LogError("GestureManager: no UDPReceiver found in children");

        SetupMarkerCanvas();

        if (webcamBackground != null)
            StretchToFullscreen(webcamBackground.rectTransform);

        if (bodyMarker != null)
            bodyMarker.SetCanvas(markerCanvas);
    }

    void SetupMarkerCanvas()
    {
        if (markerCanvas == null)
            markerCanvas = GetComponent<Canvas>();

        markerCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        markerCanvas.sortingOrder = 999;

        if (webcamBackground != null)
            webcamBackground.raycastTarget = false;
    }

    void StretchToFullscreen(RectTransform target)
    {
        target.anchorMin = Vector2.zero;
        target.anchorMax = Vector2.one;
        target.offsetMin = Vector2.zero;
        target.offsetMax = Vector2.zero;
    }

    void OnEnable()
    {
        if (_instance != this) return;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        if (_instance != this) return;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        AttachWebcamCanvasToSceneCamera();
        ApplySceneVisibility(SceneManager.GetActiveScene());
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AttachWebcamCanvasToSceneCamera();
        ApplySceneVisibility(scene);
    }

    void AttachWebcamCanvasToSceneCamera()
    {
        if (webcamCanvas == null) return;

        Camera sceneCamera = Camera.main;

        if (sceneCamera == null)
            Debug.LogWarning("GestureManager: no Main Camera found in this scene");

        webcamCanvas.renderMode = RenderMode.ScreenSpaceCamera;
        webcamCanvas.worldCamera = sceneCamera;
        webcamCanvas.planeDistance = 11f;
        webcamCanvas.sortingOrder = -10;
    }

    void ApplySceneVisibility(Scene scene)
    {
        bool shouldHide = System.Array.IndexOf(hiddenInScenes, scene.name) >= 0;
        SetDisplayActive(!shouldHide);
    }

    void SetDisplayActive(bool active)
    {
        if (webcamBackground != null) webcamBackground.gameObject.SetActive(active);
        if (leftHandMarker != null) leftHandMarker.gameObject.SetActive(active);
        if (rightHandMarker != null) rightHandMarker.gameObject.SetActive(active);
        if (leftFootMarker != null) leftFootMarker.gameObject.SetActive(active);
        if (rightFootMarker != null) rightFootMarker.gameObject.SetActive(active);
    }
}