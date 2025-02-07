using System;
using System.Net.Sockets;
using UnityEngine;

public class DebugServer : MonoBehaviour
{
    private RINLBody rINLBody;

    public string serverIP = "127.0.0.1";
    public int serverPort = 8764;
    private Socket socket;
    private readonly byte[] buffer = new byte[8192];

    void Start()
    {
        // Check if we are on the editor, if not, destroy this script
        if (!Application.isEditor)
        {
            Destroy(this);
            return;
        }

        rINLBody = gameObject.GetComponent<RINLBody>();

        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Bind(new System.Net.IPEndPoint(System.Net.IPAddress.Parse(serverIP), serverPort));
        Debug.Log("Server started on " + serverIP + ":" + serverPort);
    }

    void Update()
    {
        if (socket.Poll(0, SelectMode.SelectRead))
        {
            int bytesRead = socket.Receive(buffer);
            string data = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);

            try {
                rINLBody.SetBodyPosition(data);
            } catch (Exception e) {
                Debug.LogWarning(e);
            }
        }
    }
}
