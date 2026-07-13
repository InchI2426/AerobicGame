using UnityEngine;
using UnityEngine.UI;

public class BodyMarker : MonoBehaviour
{
    public RectTransform leftHandMarker;
    public RectTransform rightHandMarker;
    public RectTransform leftFootMarker;
    public RectTransform rightFootMarker;

    private RectTransform canvasRect;
    private UDPReceiver udpReceiver;

    void Start()
    {
        // ดึงอัตโนมัติจาก GestureManager แทนการลาก Inspector
        if (GestureManager.Instance != null)
        {
            udpReceiver = GestureManager.Instance.Receiver;
            SetCanvas(GestureManager.Instance.markerCanvas);
        }
    }

    public void SetCanvas(Canvas canvas)
    {
        if (canvas == null) { canvasRect = null; return; }
        canvasRect = canvas.GetComponent<RectTransform>();
    }

    void Update()
    {
        if (udpReceiver == null || canvasRect == null) return;

        MoveMarker(leftHandMarker, udpReceiver.leftHand);
        MoveMarker(rightHandMarker, udpReceiver.rightHand);
        MoveMarker(leftFootMarker, udpReceiver.leftFoot);
        MoveMarker(rightFootMarker, udpReceiver.rightFoot);
    }

    void MoveMarker(RectTransform marker, Vector2 normalizedPos)
    {
        if (marker == null) return;

        marker.anchoredPosition = new Vector2(
            (normalizedPos.x - 0.5f) * canvasRect.rect.width,
            (0.5f - normalizedPos.y) * canvasRect.rect.height
        );
    }
}