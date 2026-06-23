using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class GameplayManager : MonoBehaviour
{
    [Header("References")]
    public UDPReceiver udpReceiver;
    public GameObject circlePrefab;

    [Header("UI")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;

    private struct StageData
    {
        public float circleSize;
        public float spawnInterval;
        public float stageDuration;
        public int circleCount;

        public StageData(float size, float interval, float duration, int count)
        {
            circleSize = size;
            spawnInterval = interval;
            stageDuration = duration;
            circleCount = count;
        }
    }

    private StageData[] stages = new StageData[]
    {
        new StageData(3.00f, 5.00f, 30f, 1),
        new StageData(3.00f, 5.00f, 30f, 1),
        new StageData(3.00f, 5.00f, 30f, 1),
        new StageData(2.66f, 4.75f, 30f, 1),
        new StageData(2.33f, 4.50f, 30f, 1),
        new StageData(2.00f, 4.25f, 30f, 1),
        new StageData(2.00f, 4.00f, 30f, 2),
        new StageData(1.75f, 3.75f, 30f, 2),
        new StageData(1.50f, 3.50f, 30f, 2),
        new StageData(1.00f, 3.00f, 30f, 3),
    };

    private int score = 0;
    private float timeLeft;
    private bool gameEnded = false;
    private CircleZone[] activeCircles;
    private StageData currentStage;
    private float kneeY = 0.6f;

    void Start()
    {
        int stageIndex = Mathf.Clamp(PlayerPrefs.GetInt("SelectedStage", 1) - 1, 0, 9);
        currentStage = stages[stageIndex];
        timeLeft = currentStage.stageDuration;
        activeCircles = new CircleZone[currentStage.circleCount];

        SpawnAllCircles();
    }

    void Update()
    {
        if (gameEnded) return;

        // อัปเดต kneeY
        if (udpReceiver != null)
        {
            float lk = udpReceiver.leftKnee.y;
            float rk = udpReceiver.rightKnee.y;
            if (lk > 0 || rk > 0)
                kneeY = (lk + rk) * 0.5f;
        }

        // ตรวจ Hit และ Respawn ถ้า circle หมดเวลา
        for (int i = 0; i < activeCircles.Length; i++)
        {
            if (activeCircles[i] == null)
            {
                SpawnAllCircles(); // respawn ทุก slot พร้อมกัน
                break;
            }
            CheckHit(i);
        }

        timeLeft -= Time.deltaTime;
        timerText.text = "Time: " + Mathf.CeilToInt(timeLeft);

        if (timeLeft <= 0f)
            EndStage();
    }

    void CheckHit(int index)
    {
        CircleZone circle = activeCircles[index];
        if (circle == null) return;

        Vector3 pos = GetBodyPartWorldPos(circle.targetPart);
        if (circle.IsHit(pos))
        {
            score += 1000;
            scoreText.text = "Score: " + score;
            Destroy(circle.gameObject);
            activeCircles[index] = null;
        }
    }

    void SpawnAllCircles()
    {
        // สร้าง list BodyPart แล้ว shuffle → ไม่ซ้ำกัน
        List<CircleZone.BodyPart> parts = new List<CircleZone.BodyPart>
        {
            CircleZone.BodyPart.LeftHand,
            CircleZone.BodyPart.RightHand,
            CircleZone.BodyPart.LeftFoot,
            CircleZone.BodyPart.RightFoot
        };

        for (int i = parts.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            CircleZone.BodyPart tmp = parts[i];
            parts[i] = parts[j];
            parts[j] = tmp;
        }

        for (int i = 0; i < activeCircles.Length; i++)
        {
            if (activeCircles[i] != null)
                Destroy(activeCircles[i].gameObject);
            activeCircles[i] = SpawnOneCircle(parts[i]);
        }
    }

    CircleZone SpawnOneCircle(CircleZone.BodyPart part)
    {
        float minY, maxY;
        bool isFoot = part == CircleZone.BodyPart.LeftFoot ||
                      part == CircleZone.BodyPart.RightFoot;

        if (isFoot)
        {
            float kneeViewportY = 1f - kneeY;
            maxY = Mathf.Clamp(kneeViewportY - 0.05f, 0.1f, 0.5f);
            minY = 0.05f;
        }
        else
        {
            minY = 0.50f;
            maxY = 0.90f;
        }

        float randX = Random.Range(0.15f, 0.85f);
        float randY = Random.Range(minY, maxY);

        Vector3 spawnPos = Camera.main.ViewportToWorldPoint(
            new Vector3(randX, randY, 10f)
        );

        GameObject obj = Instantiate(circlePrefab, spawnPos, Quaternion.identity);
        CircleZone zone = obj.GetComponent<CircleZone>();
        zone.Setup(part, currentStage.circleSize, currentStage.spawnInterval);
        return zone;
    }

    void EndStage()
    {
        gameEnded = true;
        foreach (var c in activeCircles)
            if (c != null) Destroy(c.gameObject);

        int stage = PlayerPrefs.GetInt("SelectedStage", 1);
        int playerId = PlayerPrefs.GetInt("PlayerId", -1);
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