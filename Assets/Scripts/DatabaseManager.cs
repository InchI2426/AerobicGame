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
        if (_instance != null) { Destroy(gameObject); return; }
        _instance = this;
        DontDestroyOnLoad(gameObject);

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
                cmd.CommandText = @"CREATE TABLE IF NOT EXISTS players (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    username TEXT NOT NULL UNIQUE,
                    password TEXT NOT NULL);";
                cmd.ExecuteNonQuery();

                cmd.CommandText = @"CREATE TABLE IF NOT EXISTS high_scores (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    player_id INTEGER NOT NULL,
                    stage INTEGER NOT NULL,
                    score INTEGER NOT NULL,
                    UNIQUE(player_id, stage));";
                cmd.ExecuteNonQuery();

                cmd.CommandText = @"CREATE TABLE IF NOT EXISTS play_history (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    player_id INTEGER NOT NULL,
                    stage INTEGER NOT NULL,
                    score INTEGER NOT NULL,
                    played_at TEXT NOT NULL,
                    accuracy REAL,
                    avg_reaction_time REAL,
                    movement_amount REAL,
                    is_completed INTEGER);";
                cmd.ExecuteNonQuery();
            }

            AddColumnIfMissing(conn, "play_history", "accuracy", "REAL");
            AddColumnIfMissing(conn, "play_history", "avg_reaction_time", "REAL");
            AddColumnIfMissing(conn, "play_history", "movement_amount", "REAL");
            AddColumnIfMissing(conn, "play_history", "is_completed", "INTEGER");
        }
    }

    void AddColumnIfMissing(SqliteConnection conn, string table, string column, string type)
    {
        bool exists = false;

        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "PRAGMA table_info(" + table + ")";
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    string name = reader["name"].ToString();
                    if (name == column) { exists = true; break; }
                }
            }
        }

        if (!exists)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "ALTER TABLE " + table + " ADD COLUMN " + column + " " + type;
                cmd.ExecuteNonQuery();
            }
        }
    }

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
        catch { return false; }
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

    public void SaveScore(int playerId, int stage, int score, float accuracy,
        float avgReactionTime, float movementAmount, bool isCompleted)
    {
        using (var conn = new SqliteConnection(dbPath))
        {
            conn.Open();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"INSERT INTO play_history
                    (player_id, stage, score, played_at, accuracy, avg_reaction_time, movement_amount, is_completed)
                    VALUES (@pid, @s, @sc, @d, @acc, @rt, @mv, @comp)";
                cmd.Parameters.AddWithValue("@pid", playerId);
                cmd.Parameters.AddWithValue("@s", stage);
                cmd.Parameters.AddWithValue("@sc", score);
                cmd.Parameters.AddWithValue("@d", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.Parameters.AddWithValue("@acc", accuracy);
                cmd.Parameters.AddWithValue("@rt", avgReactionTime);
                cmd.Parameters.AddWithValue("@mv", movementAmount);
                cmd.Parameters.AddWithValue("@comp", isCompleted ? 1 : 0);
                cmd.ExecuteNonQuery();

                cmd.Parameters.Clear();
                cmd.CommandText = @"INSERT INTO high_scores (player_id, stage, score)
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

    public float GetCompletionRate(int playerId, int totalStages = 10)
    {
        using (var conn = new SqliteConnection(dbPath))
        {
            conn.Open();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"SELECT COUNT(DISTINCT stage) FROM play_history
                    WHERE player_id=@pid AND is_completed=1";
                cmd.Parameters.AddWithValue("@pid", playerId);
                var result = cmd.ExecuteScalar();
                int completedStages = result != null ? Convert.ToInt32(result) : 0;

                float rate = (float)completedStages / totalStages * 100f;
                return Mathf.Round(rate * 10f) / 10f;
            }
        }
    }

    public bool IsStageCompleted(int playerId, int stage)
    {
        using (var conn = new SqliteConnection(dbPath))
        {
            conn.Open();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"SELECT COUNT(*) FROM play_history
                    WHERE player_id=@pid AND stage=@s AND is_completed=1";
                cmd.Parameters.AddWithValue("@pid", playerId);
                cmd.Parameters.AddWithValue("@s", stage);
                var result = cmd.ExecuteScalar();
                int count = result != null ? Convert.ToInt32(result) : 0;
                return count > 0;
            }
        }
    }
}