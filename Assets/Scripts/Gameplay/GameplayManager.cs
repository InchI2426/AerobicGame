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
    private float radiusVP = 0.1f; // radius ใน viewport units

    void Start()
    {
        int stageIndex = Mathf.Clamp(PlayerPrefs.GetInt("SelectedStage", 1) - 1, 0, 9);
        currentStage = stages[stageIndex];
        timeLeft = currentStage.stageDuration;
        activeCircles = new CircleZone[currentStage.circleCount];

        // คำนวณ radius ใน viewport units จาก world units
        CalculateRadiusViewport();

        SpawnAllCircles();
    }

    void CalculateRadiusViewport()
    {
        // เปรียบเทียบจุด 2 จุดใน world space → viewport เพื่อหา scale
        Vector3 centerWorld = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 10f));
        Vector3 edgeWorld = centerWorld + new Vector3(currentStage.circleSize * 0.5f, 0f, 0f);
        Vector3 centerVP = Camera.main.WorldToViewportPoint(centerWorld);
        Vector3 edgeVP = Camera.main.WorldToViewportPoint(edgeWorld);
        radiusVP = Mathf.Abs(edgeVP.x - centerVP.x);
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

        // ถ้า circle ใดหมดเวลา (null) → spawn ชุดใหม่ทั้งหมด
        bool anyNull = false;
        for (int i = 0; i < activeCircles.Length; i++)
        {
            if (activeCircles[i] == null) { anyNull = true; break; }
        }
        if (anyNull) { SpawnAllCircles(); }
        else { CheckAllHit(); }

        timeLeft -= Time.deltaTime;
        timerText.text = "Time: " + Mathf.CeilToInt(timeLeft);

        if (timeLeft <= 0f) EndStage();
    }

    // ปัญหาที่ 3 — ต้องโดนทุก circle พร้อมกันถึงได้คะแนน
    void CheckAllHit()
    {
        bool allHit = true;
        for (int i = 0; i < activeCircles.Length; i++)
        {
            if (activeCircles[i] == null) { allHit = false; break; }
            Vector3 pos = GetBodyPartWorldPos(activeCircles[i].targetPart);
            if (!activeCircles[i].IsHit(pos)) { allHit = false; break; }
        }

        if (allHit)
        {
            score += 1000;
            scoreText.text = "Score: " + score;
            SpawnAllCircles();
        }
    }

    void SpawnAllCircles()
    {
        // Destroy ชุดเก่า
        foreach (var c in activeCircles)
            if (c != null) Destroy(c.gameObject);

        // Shuffle BodyPart → ไม่ซ้ำกัน
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

        // ปัญหาที่ 1 — Cluster: สุ่ม center X ร่วมกัน
        float pad = radiusVP + 0.03f;
        float centerX = Random.Range(pad, 1f - pad);

        for (int i = 0; i < activeCircles.Length; i++)
        {
            activeCircles[i] = SpawnOneCircle(parts[i], centerX, pad);
        }

        CalculateRadiusViewport();
    }

    bool IsTooCloseToLandmarks(float vpX, float vpY, float minDist = 0.20f)
    {
        if (udpReceiver == null) return false;

        // landmark ทั้ง 4 จุด (แปลง Y ให้ตรงกับ viewport)
        Vector2[] landmarks = new Vector2[]
        {
        new Vector2(udpReceiver.leftHand.x,  1f - udpReceiver.leftHand.y),
        new Vector2(udpReceiver.rightHand.x, 1f - udpReceiver.rightHand.y),
        new Vector2(udpReceiver.leftFoot.x,  1f - udpReceiver.leftFoot.y),
        new Vector2(udpReceiver.rightFoot.x, 1f - udpReceiver.rightFoot.y),
        };

        Vector2 spawnVP = new Vector2(vpX, vpY);
        foreach (var lm in landmarks)
        {
            if (Vector2.Distance(spawnVP, lm) < minDist)
                return true;
        }
        return false;
    }
    CircleZone SpawnOneCircle(CircleZone.BodyPart part, float centerX, float pad)
    {
        bool isFoot = part == CircleZone.BodyPart.LeftFoot ||
                      part == CircleZone.BodyPart.RightFoot;

        float minY, maxY;
        if (isFoot)
        {
            float kneeViewportY = 1f - kneeY;
            maxY = Mathf.Clamp(kneeViewportY - pad, pad, 0.5f);
            minY = pad;
        }
        else
        {
            minY = 0.5f;
            maxY = 1f - pad;
        }

        float spawnX = pad, spawnY = pad;
        bool found = false;

        // ลองสุ่มสูงสุด 10 ครั้ง หาตำแหน่งที่ห่าง landmark
        for (int attempt = 0; attempt < 10; attempt++)
        {
            float tryX = Mathf.Clamp(
                centerX + Random.Range(-0.12f, 0.12f),
                pad, 1f - pad
            );
            float tryY = Mathf.Clamp(
                Random.Range(minY, maxY),
                pad, 1f - pad
            );

            if (!IsTooCloseToLandmarks(tryX, tryY))
            {
                spawnX = tryX;
                spawnY = tryY;
                found = true;
                break;
            }
        }

        // ถ้าหาไม่ได้ใน 10 ครั้ง ใช้ position ล่าสุดไปก่อน
        if (!found)
        {
            spawnX = Mathf.Clamp(centerX + Random.Range(-0.12f, 0.12f), pad, 1f - pad);
            spawnY = Mathf.Clamp(Random.Range(minY, maxY), pad, 1f - pad);
        }

        Vector3 spawnPos = Camera.main.ViewportToWorldPoint(
            new Vector3(spawnX, spawnY, 10f)
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