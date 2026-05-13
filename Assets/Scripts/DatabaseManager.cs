using UnityEngine;
using Mono.Data.Sqlite;
using System.Data;
using System;

public class DatabaseManager : MonoBehaviour
{
    private static DatabaseManager _instance;
    public static DatabaseManager Instance => _instance;

    private string dbPath;

    void Awake()
    {
        // Singleton — มีแค่ตัวเดียวตลอด
        if (_instance != null) { Destroy(gameObject); return; }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        // ไฟล์ .db เก็บใน persistent path (ไม่หายเมื่อปิดเกม)
        dbPath = "URI=file:" + Application.persistentDataPath + "/aerobic_game.db";
        InitDatabase();
    }

    void InitDatabase()
    {
        using (var conn = new SqliteConnection(dbPath))
        {
            conn.Open();
            using (var cmd = conn.CreateCommand())
            {
                // ตาราง players
                cmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS players (
                        id       INTEGER PRIMARY KEY AUTOINCREMENT,
                        username TEXT NOT NULL UNIQUE,
                        password TEXT NOT NULL
                    );";
                cmd.ExecuteNonQuery();

                // ตาราง high_scores
                cmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS high_scores (
                        id        INTEGER PRIMARY KEY AUTOINCREMENT,
                        player_id INTEGER NOT NULL,
                        stage     INTEGER NOT NULL,
                        score     INTEGER NOT NULL,
                        UNIQUE(player_id, stage)
                    );";
                cmd.ExecuteNonQuery();

                // ตาราง play_history
                cmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS play_history (
                        id        INTEGER PRIMARY KEY AUTOINCREMENT,
                        player_id INTEGER NOT NULL,
                        stage     INTEGER NOT NULL,
                        score     INTEGER NOT NULL,
                        played_at TEXT NOT NULL
                    );";
                cmd.ExecuteNonQuery();

                Debug.Log("✅ Database พร้อมใช้งาน: " + Application.persistentDataPath);
            }
        }
    }

    // ===== PLAYER =====
    public bool RegisterPlayer(string username, string password)
    {
        try
        {
            using (var conn = new SqliteConnection(dbPath))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO players (username, password) VALUES (@u, @p)";
                    cmd.Parameters.AddWithValue("@u", username);
                    cmd.Parameters.AddWithValue("@p", password);
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
        }
        catch { return false; } // username ซ้ำ
    }

    public int LoginPlayer(string username, string password)
    {
        using (var conn = new SqliteConnection(dbPath))
        {
            conn.Open();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT id FROM players WHERE username=@u AND password=@p";
                cmd.Parameters.AddWithValue("@u", username);
                cmd.Parameters.AddWithValue("@p", password);
                var result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : -1;
            }
        }
    }

    // ===== SCORE =====
    public void SaveScore(int playerId, int stage, int score)
    {
        using (var conn = new SqliteConnection(dbPath))
        {
            conn.Open();
            using (var cmd = conn.CreateCommand())
            {
                // บันทึกประวัติทุกครั้ง
                cmd.CommandText = @"
                    INSERT INTO play_history (player_id, stage, score, played_at)
                    VALUES (@pid, @s, @sc, @d)";
                cmd.Parameters.AddWithValue("@pid", playerId);
                cmd.Parameters.AddWithValue("@s", stage);
                cmd.Parameters.AddWithValue("@sc", score);
                cmd.Parameters.AddWithValue("@d", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.ExecuteNonQuery();

                // อัปเดต high score ถ้าดีกว่าเดิม
                cmd.Parameters.Clear();
                cmd.CommandText = @"
                    INSERT INTO high_scores (player_id, stage, score)
                    VALUES (@pid, @s, @sc)
                    ON CONFLICT(player_id, stage)
                    DO UPDATE SET score = MAX(score, @sc)";
                cmd.Parameters.AddWithValue("@pid", playerId);
                cmd.Parameters.AddWithValue("@s", stage);
                cmd.Parameters.AddWithValue("@sc", score);
                cmd.ExecuteNonQuery();
            }
        }
    }

    public int GetHighScore(int playerId, int stage)
    {
        using (var conn = new SqliteConnection(dbPath))
        {
            conn.Open();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT score FROM high_scores WHERE player_id=@pid AND stage=@s";
                cmd.Parameters.AddWithValue("@pid", playerId);
                cmd.Parameters.AddWithValue("@s", stage);
                var result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
        }
    }
}