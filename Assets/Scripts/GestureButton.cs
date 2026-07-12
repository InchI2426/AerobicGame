using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class GestureButton : MonoBehaviour
{
    [Header("Progress Ring (assign this one manually)")]
    public Image progressRing;

    [Header("Settings")]
    [Tooltip("Time in seconds the hand must stay over the button to trigger it")]
    public float dwellTime = 1.5f;

    [Tooltip("Which hand(s) can trigger this button")]
    public bool useLeftHand = true;
    public bool useRightHand = true;

    [Header("Ring Colors")]
    public Color ringColorStart = Color.yellow;
    public Color ringColorComplete = Color.green;

    private Button button;
    private RectTransform buttonRect;
    private RectTransform canvasRect;
    private Camera uiCamera;
    private UDPReceiver udpReceiver;

    private float currentDwell = 0f;
    private bool isReady = false;

    void Awake()
    {
        button = GetComponent<Button>();
        buttonRect = GetComponent<RectTransform>();
    }

    void Start()
    {
        if (progressRing != null)
        {
            progressRing.type = Image.Type.Filled;
            progressRing.fillMethod = Image.FillMethod.Radial360;
            progressRing.fillAmount = 0f;
            progressRing.color = ringColorStart;
            progressRing.gameObject.SetActive(false);
        }

        TryResolveDependencies();
    }

    void TryResolveDependencies()
    {
        if (GestureManager.Instance == null) return;

        udpReceiver = GestureManager.Instance.Receiver;

        Canvas canvas = GetComponentInParent<Canvas>();

        if (canvas == null || udpReceiver == null) return;

        canvasRect = canvas.GetComponent<RectTransform>();
        uiCamera = canvas.worldCamera;
        isReady = true;
    }

    void Update()
    {
        if (!isReady)
        {
            TryResolveDependencies();
            if (!isReady) return;
        }

        bool handInside = CheckHandInsideButton();

        if (handInside)
        {
            currentDwell += Time.deltaTime;

            UpdateRingVisual();

            if (currentDwell >= dwellTime)
            {
                TriggerButton();
            }
        }
        else
        {
            ResetDwell();
        }
    }

    bool CheckHandInsideButton()
    {
        if (useLeftHand && IsPointOverButton(udpReceiver.leftHand))
            return true;

        if (useRightHand && IsPointOverButton(udpReceiver.rightHand))
            return true;

        return false;
    }

    bool IsPointOverButton(Vector2 normalizedPos)
    {
        if (normalizedPos == Vector2.zero) return false;

        Vector2 screenPoint = new Vector2(
            normalizedPos.x * Screen.width,
            (1f - normalizedPos.y) * Screen.height
        );

        return RectTransformUtility.RectangleContainsScreenPoint(buttonRect, screenPoint, uiCamera);
    }

    void UpdateRingVisual()
    {
        if (progressRing == null) return;

        if (!progressRing.gameObject.activeSelf)
            progressRing.gameObject.SetActive(true);

        float t = currentDwell / dwellTime;
        progressRing.fillAmount = t;
        progressRing.color = Color.Lerp(ringColorStart, ringColorComplete, t);
    }

    void TriggerButton()
    {
        ResetDwell();
        button.onClick.Invoke();
    }

    void ResetDwell()
    {
        currentDwell = 0f;

        if (progressRing != null)
        {
            progressRing.fillAmount = 0f;
            progressRing.gameObject.SetActive(false);
        }
    }

    void OnDisable()
    {
        ResetDwell();
    }
}