using System.Net.Sockets;
using UnityEngine;

public class DebugServer : MonoBehaviour
{
    private RINLBody rINLBody;

    public string serverIP = "127.0.0.1";
    public int serverPort = 8765;
    private Socket socket, connection = null;
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

        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Bind(new System.Net.IPEndPoint(System.Net.IPAddress.Parse(serverIP), serverPort));
        socket.Listen(20);
        Debug.Log("Server started on " + serverIP + ":" + serverPort);
    }

    void Update()
    {
        // If we dont have a connection, check if we have a new one
        connection ??= socket.Accept();

        // If we have a connection, check if we have data
        if (connection != null)
        {
            if (connection.Poll(0, SelectMode.SelectRead))
            {
                int bytesRead = connection.Receive(buffer);
                string data = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);

                try {
                    rINLBody.SetBodyPosition(data);
                } catch (System.Exception e) {
                    Debug.LogWarning(e);
                }
            }
        }
    }
}
