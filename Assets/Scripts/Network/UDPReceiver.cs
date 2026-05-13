using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class UDPReceiver : MonoBehaviour
{
    private int port = 5005;
    private UdpClient udpClient;
    private Thread receiveThread;
    private string latestData = "";
    private bool hasNewData = false;

    // ตำแหน่ง 4 จุด — script อื่นดึงค่าไปใช้ได้
    public Vector2 leftHand;
    public Vector2 rightHand;
    public Vector2 leftFoot;
    public Vector2 rightFoot;

    [System.Serializable]
    private class Point { public float x; public float y; }

    [System.Serializable]
    private class PoseData
    {
        public Point left_hand;
        public Point right_hand;
        public Point left_foot;
        public Point right_foot;
    }

    void Start()
    {
        udpClient = new UdpClient(port);
        receiveThread = new Thread(ReceiveLoop);
        receiveThread.IsBackground = true;
        receiveThread.Start();
        Debug.Log("✅ UDP รับข้อมูลที่ port " + port);
    }

    private void ReceiveLoop()
    {
        IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, port);
        while (true)
        {
            byte[] data = udpClient.Receive(ref endPoint);
            latestData = Encoding.UTF8.GetString(data);
            hasNewData = true;
        }
    }

    void Update()
    {
        if (!hasNewData) return;
        hasNewData = false;

        PoseData pose = JsonUtility.FromJson<PoseData>(latestData);
        if (pose == null) return;

        leftHand  = new Vector2(pose.left_hand.x,  pose.left_hand.y);
        rightHand = new Vector2(pose.right_hand.x, pose.right_hand.y);
        leftFoot  = new Vector2(pose.left_foot.x,  pose.left_foot.y);
        rightFoot = new Vector2(pose.right_foot.x, pose.right_foot.y);

        Debug.Log($"มือซ้าย: {leftHand} | มือขวา: {rightHand}");
    }

    void OnDestroy()
    {
        receiveThread?.Abort();
        udpClient?.Close();
    }
}