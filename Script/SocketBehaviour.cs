using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;

using System.Threading;
using System.Linq;

public class SocketBehaviour : MonoBehaviour
{
    private GameObject GameObj;
    private static SocketBehaviour _singleton;
    public static SocketBehaviour Singleton
    {
        get
        {
            if (_singleton == null)
            {
                _singleton = FindObjectOfType<SocketBehaviour>();
            }
            return _singleton;
        }
    }
    private const int BUFFER_SIZE = 128;

    public string host = "127.0.0.1";
    public int port = 8899;

    private byte[] buffer;

    private Socket socket;
    // Use this for initialization
    void Start()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        GameObj = this.gameObject;

        Connect();
    }

    private void Connect()
    {
        try
        {
            socket.Connect(host, port);
        }
        catch (Exception e)
        {
            print(e.Message);
        }

        if (socket.Connected)
        {
            print("Connected");
            //SocketBehaviour.Singleton.Send("Hwnd:" + TransparentWindow.hwnd.ToString() + ";");
            Receive();
        }
        else
        {
            print("Connect fail");
        }
    }

    private void Receive()
    {
        if (!socket.Connected)
            return;

        buffer = new byte[BUFFER_SIZE];

        try
        {
            socket.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, new AsyncCallback(Receive_Callback), socket);
        }
        catch (Exception e)
        {
            print(e.Message);
        }
    }

    private void Receive_Callback(IAsyncResult ar)
    {
        if (!socket.Connected)
        {
            return;
        }

        int read = socket.EndReceive(ar);

        if (read > 0)
        {
            //接收消息
            string receiveString = Encoding.UTF8.GetString(buffer);
            Debug.Log("接收到消息：" + receiveString);

            Loom.AddList(receiveString);
            Receive();

        }
    }

    public void Send(string message)
    {
        if (!socket.Connected)
            return;

        byte[] msg = Encoding.ASCII.GetBytes(message);
        socket.Send(msg);
    }

    private void OnDisable()
    {
        if (socket.Connected)
        {
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }
    }


}

