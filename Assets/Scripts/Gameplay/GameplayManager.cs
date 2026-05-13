using UnityEngine;
using TMPro;

public class GameplayManager : MonoBehaviour
{
    [Header("References")]
    public UDPReceiver udpReceiver;
    public GameObject circlePrefab;

    [Header("Settings")]
    public float circleSize = 2f;
    public float spawnInterval = 2f;
    public float stageDuration = 60f;   // ← เวลาต่อด่าน

    [Header("UI")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;   // ← เพิ่ม Timer Text

    // ตัวแปรภายใน
    private int score = 0;
    private float timeLeft;             // ← เวลาที่เหลือ
    private CircleZone currentCircle;
    private float spawnTimer = 0f;
    private bool gameEnded = false;     // ← ป้องกันจบซ้ำ

    void Start()
    {
        timeLeft = stageDuration;       // เซ็ตเวลาเริ่มต้น
        SpawnCircle();
    }

    void Update()
    {
        // ถ้าเกมจบแล้วไม่ต้องทำอะไรอีก
        if (gameEnded) return;

        // ตรวจ Hit
        if (currentCircle != null)
            CheckHit();

        // นับเวลา Spawn
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
            SpawnCircle();

        // ===== ส่วนที่เพิ่มใหม่ =====

        // ลดเวลา
        timeLeft -= Time.deltaTime;

        // อัปเดต UI Timer (แสดงเป็นเลขเต็ม ไม่มีทศนิยม)
        timerText.text = "Time: " + Mathf.CeilToInt(timeLeft);

        // ถ้าเวลาหมด → จบด่าน
        if (timeLeft <= 0)
            EndStage();
    }

    void CheckHit()
    {
        Vector3 pos = GetBodyPartWorldPos(currentCircle.targetPart);
        float dist = Vector3.Distance(currentCircle.transform.position, pos);

        if (currentCircle.IsHit(pos))
        {
            score += 1000;
            scoreText.text = "Score: " + score;
            SpawnCircle();
        }
    }

    void SpawnCircle()
    {
        spawnTimer = 0f;
        if (currentCircle != null)
            Destroy(currentCircle.gameObject);

        CircleZone.BodyPart part = (CircleZone.BodyPart)Random.Range(0, 4);

        float minY, maxY;
        if (part == CircleZone.BodyPart.LeftFoot || part == CircleZone.BodyPart.RightFoot)
        {
            minY = 0.15f;
            maxY = 0.45f;
        }
        else
        {
            minY = 0.50f;
            maxY = 0.85f;
        }

        float randX = Random.Range(0.15f, 0.85f);
        float randY = Random.Range(minY, maxY);

        Vector3 spawnPos = Camera.main.ViewportToWorldPoint(
            new Vector3(randX, randY, 10f)
        );

        GameObject obj = Instantiate(circlePrefab, spawnPos, Quaternion.identity);
        currentCircle = obj.GetComponent<CircleZone>();
        currentCircle.Setup(part, circleSize);
    }

    // ===== ส่วนที่เพิ่มใหม่ =====
    void EndStage()
    {
        gameEnded = true;
        if (currentCircle != null) Destroy(currentCircle.gameObject);

        int stage = PlayerPrefs.GetInt("SelectedStage", 1);
        int playerId = PlayerPrefs.GetInt("PlayerId", -1);

        // บันทึกคะแนนถ้า login อยู่
        if (playerId != -1)
            DatabaseManager.Instance.SaveScore(playerId, stage, score);

        SceneLoader.GoToResult(score, stage);
    }
    Vector3 GetBodyPartWorldPos(CircleZone.BodyPart part)
    {
        Vector2 norm = Vector2.zero;

        if (part == CircleZone.BodyPart.LeftHand) norm = udpReceiver.leftHand;
        else if (part == CircleZone.BodyPart.RightHand) norm = udpReceiver.rightHand;
        else if (part == CircleZone.BodyPart.LeftFoot) norm = udpReceiver.leftFoot;
        else if (part == CircleZone.BodyPart.RightFoot) norm = udpReceiver.rightFoot;

        return Camera.main.ViewportToWorldPoint(
            new Vector3(norm.x, 1f - norm.y, 10f)
        );
    }
}