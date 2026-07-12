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

    public Vector2 leftHand;
    public Vector2 rightHand;
    public Vector2 leftFoot;
    public Vector2 rightFoot;
    public Vector2 leftKnee;
    public Vector2 rightKnee;
    public Vector2 leftHip;
    public Vector2 rightHip;

    [System.Serializable]
    private class Point { public float x; public float y; }

    [System.Serializable]
    private class PoseData
    {
        public Point left_hand;
        public Point right_hand;
        public Point left_foot;
        public Point right_foot;
        public Point left_knee;
        public Point right_knee;
        public Point left_hip;
        public Point right_hip;
    }

    void Start()
    {
        udpClient = new UdpClient(port);
        receiveThread = new Thread(ReceiveLoop);
        receiveThread.IsBackground = true;
        receiveThread.Start();
        Debug.Log("UDP started on port " + port);
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
        leftKnee  = new Vector2(pose.left_knee.x,  pose.left_knee.y);
        rightKnee = new Vector2(pose.right_knee.x, pose.right_knee.y);

        if (pose.left_hip != null)
            leftHip = new Vector2(pose.left_hip.x, pose.left_hip.y);

        if (pose.right_hip != null)
            rightHip = new Vector2(pose.right_hip.x, pose.right_hip.y);
    }

    void OnDestroy()
    {
        receiveThread?.Abort();
        udpClient?.Close();
    }
}
