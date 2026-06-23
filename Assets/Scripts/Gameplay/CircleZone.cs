using UnityEngine;
using TMPro;

public class CircleZone : MonoBehaviour
{
    public enum BodyPart { LeftHand, RightHand, LeftFoot, RightFoot }
    public BodyPart targetPart;

    public TextMeshPro countdownText;

    private float radius;
    private float timeLeft;

    static Color colorLeftHand = new Color(0f, 1f, 0f, 0.7f);
    static Color colorRightHand = new Color(1f, 0f, 0f, 0.7f);
    static Color colorLeftFoot = new Color(1f, 1f, 0f, 0.7f);
    static Color colorRightFoot = new Color(1f, 0f, 1f, 0.7f);

    public void Setup(BodyPart part, float circleSize, float lifetime)
    {
        targetPart = part;
        transform.localScale = new Vector3(circleSize, circleSize, 1f);
        radius = circleSize * 0.5f;
        timeLeft = lifetime;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (part == BodyPart.LeftHand) sr.color = colorLeftHand;
        else if (part == BodyPart.RightHand) sr.color = colorRightHand;
        else if (part == BodyPart.LeftFoot) sr.color = colorLeftFoot;
        else if (part == BodyPart.RightFoot) sr.color = colorRightFoot;

        if (countdownText != null)
            countdownText.gameObject.SetActive(false);
    }

    void Update()
    {
        timeLeft -= Time.deltaTime;

        // แสดง countdown เมื่อเหลือ 3 วินาที
        if (countdownText != null)
        {
            bool show = timeLeft <= 3f && timeLeft > 0f;
            countdownText.gameObject.SetActive(show);
            if (show)
                countdownText.text = Mathf.CeilToInt(timeLeft).ToString();
        }

        if (timeLeft <= 0f)
            Destroy(gameObject);
    }

    public bool IsHit(Vector3 worldPos)
    {
        return Vector3.Distance(transform.position, worldPos) < radius;
    }

    public float GetRadius() => radius;
}