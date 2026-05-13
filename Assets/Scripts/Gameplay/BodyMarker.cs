using UnityEngine;

public class BodyMarker : MonoBehaviour
{
    // ลาก UDPReceiver GameObject มาใส่ใน Inspector
    public UDPReceiver udpReceiver;

    // จุด 4 อัน (ลาก GameObject มาใส่)
    public Transform leftHandMarker;
    public Transform rightHandMarker;
    public Transform leftFootMarker;
    public Transform rightFootMarker;

    void Update()
    {
        if (udpReceiver == null) return;

        MoveMarker(leftHandMarker, udpReceiver.leftHand);
        MoveMarker(rightHandMarker, udpReceiver.rightHand);
        MoveMarker(leftFootMarker, udpReceiver.leftFoot);
        MoveMarker(rightFootMarker, udpReceiver.rightFoot);
    }

    void MoveMarker(Transform marker, Vector2 normalizedPos)
    {
        if (marker == null) return;

        // กลับ y เพราะ Mediapipe กับ Unity นับกลับกัน
        Vector3 viewportPos = new Vector3(
            normalizedPos.x,
            1f - normalizedPos.y,  // ← สำคัญมาก
            10f                    // ระยะห่างจากกล้อง
        );

        // แปลงจาก Viewport → World Position
        marker.position = Camera.main.ViewportToWorldPoint(viewportPos);
    }
}