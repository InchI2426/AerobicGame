using UnityEngine;
using TMPro;

public class CircleZone : MonoBehaviour
{
    // ประเภทอวัยวะที่ต้องแตะ
    public enum BodyPart { LeftHand, RightHand, LeftFoot, RightFoot }
    public BodyPart targetPart;

    private float radius; // รัศมีสำหรับตรวจ collision

    // สีของแต่ละอวัยวะ
    static Color colorLeftHand = new Color(0f, 1f, 0f, 0.7f);   // เขียว
    static Color colorRightHand = new Color(1f, 0f, 0f, 0.7f);   // แดง
    static Color colorLeftFoot = new Color(1f, 1f, 0f, 0.7f);   // เหลือง
    static Color colorRightFoot = new Color(1f, 0f, 1f, 0.7f);   // ม่วง

    public void Setup(BodyPart part, float circleSize)
    {
        targetPart = part;
        transform.localScale = new Vector3(circleSize, circleSize, 1f);
        radius = circleSize * 0.5f; // รัศมี = ครึ่งหนึ่งของขนาด

        // ตั้งสีตามอวัยวะ
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.color = part switch
        {
            BodyPart.LeftHand => colorLeftHand,
            BodyPart.RightHand => colorRightHand,
            BodyPart.LeftFoot => colorLeftFoot,
            BodyPart.RightFoot => colorRightFoot,
            _ => Color.white
        };
    }

    // ตรวจว่าตำแหน่งที่รับเข้ามาอยู่ใน Circle ไหม
    public bool IsHit(Vector3 worldPos)
    {
        return Vector3.Distance(transform.position, worldPos) < radius;
    }
}