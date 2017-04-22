
using UnityEngine;
using System.Collections;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Threading;
using UnityEngine.UI;
using System.Collections.Generic;

public class TCPClient: MonoBehaviour
{
    public Text mess;
    public Text inputString;
    public Text inputHost;
    public Text inputPort;
    public Text inputTest;

    static ManualResetEvent _clientDone = new ManualResetEvent(false);

    private Socket _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    private byte[] _recieveBuffer = new byte[8142];

    public readonly static Queue<Action> ExecuteOnMainThread = new Queue<Action>();

    private EventGame ev;
    private byte[] reply;

    public GameObject panelLogin;
    public GameObject panelChooseRoom;
    public GameObject caroBoard;
    public GameObject panelCaro;
    public bool isConnected = false;
    public bool isJoinRoom = false;
    private void Start()
    {
        
    }
    public virtual void Update()
    {
        if (isConnected == true)
        {
            panelChooseRoom.SetActive(true);
            panelLogin.SetActive(false);
            isConnected = false;
            
        }
        if(isJoinRoom)
        {
            caroBoard.SetActive(true);
            panelCaro.SetActive(true);
            panelChooseRoom.SetActive(false);
            isJoinRoom = false;
        }


    }
    public void Connect()
    {
        Debug.Log(inputHost.text + Int32.Parse(inputPort.text));
        SetupServer(inputHost.text, Int32.Parse(inputPort.text));
    }
    private void SetupServer(string hostAddr, int port)
    {
        try
        {
            _clientSocket.Connect(new IPEndPoint(IPAddress.Parse(hostAddr), port));
        }
        catch (SocketException ex)
        {
            Debug.Log(ex.Message);
        }

        _clientSocket.BeginReceive(_recieveBuffer, 0, _recieveBuffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), null);

    }
    public void onClick_Test()
    {
        SendJson(newEventGame(Events.ANY, inputTest.text));
    }
    public void onClick_Room(int id)
    {
        SendJson(newEventGame(Events.GAME_ROOM_JOIN, id.ToString()));
    }
    public void onClick_Login()
    {
        Connect();

        SendJson(newEventGame(Events.CONNECT, "tao la hung"));

    }
    private void ReceiveCallback(IAsyncResult AR)
    {
        //Check how much bytes are recieved and call EndRecieve to finalize handshake
        int recieved = _clientSocket.EndReceive(AR);

        if (recieved <= 0)
            return;

        //Copy the recieved data into new buffer , to avoid null bytes
        byte[] recData = new byte[recieved];
        Buffer.BlockCopy(_recieveBuffer, 0, recData, 0, recieved);

        //Process data here the way you want , all your bytes will be stored in recData

        //Start receiving again
        _clientSocket.BeginReceive(_recieveBuffer, 0, _recieveBuffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), null);


        //Log(Encoding.UTF8.GetString(recData), false);
        ProcessData(Encoding.UTF8.GetString(recData));
        Debug.Log(Encoding.UTF8.GetString(recData));
       
    }
    private void ProcessData(string json)
    {
        EventGame eve = JsonUtility.FromJson<EventGame>(json);


        switch(eve.eventType)
        {
            case Events.LOG_IN_SUCCESS:
                onLoginSuccess();
                break;
            case Events.GAME_ROOM_JOIN_SUCCESS:
                onJoinrRoomSuccess();
                break;
            case Events.ANY:
                Log(json, false);
                break;
            default: break;
        }
       
    }
    void onLoginSuccess()
    {
        Debug.Log("onLoginSuccess");
        isConnected = true;        
    }
    void onJoinrRoomSuccess()
    {
        Debug.Log("onJoinrRoomSuccess");
        isJoinRoom = true;
    }
    private void SendJson(EventGame even)
    {
        Send(JsonUtility.ToJson(even));
    }
    private void SendData(byte[] data)
    {
        SocketAsyncEventArgs socketAsyncData = new SocketAsyncEventArgs();
        socketAsyncData.SetBuffer(data, 0, data.Length);
        
        _clientSocket.SendAsync(socketAsyncData);
        
    }
    public string Send(string data)
    {
        string response = "Operation Timeout";

        if (_clientSocket != null)
        {
            SocketAsyncEventArgs socketEventArg = new SocketAsyncEventArgs();

            socketEventArg.RemoteEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"),8080);
            socketEventArg.UserToken = null;

            socketEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(delegate (object s, SocketAsyncEventArgs e)
            {
                response = e.SocketError.ToString();

                // Unblock the UI thread
                _clientDone.Set();
            });

            // Add the data to be sent into the buffer
            byte[] payload = Encoding.UTF8.GetBytes(data);
            socketEventArg.SetBuffer(payload, 0, payload.Length);

            // Sets the state of the event to nonsignaled, causing threads to block
            _clientDone.Reset();

            // Make an asynchronous Send request over the socket
            _clientSocket.SendAsync(socketEventArg);

            // Block the UI thread for a maximum of TIMEOUT_MILLISECONDS milliseconds.
            // If no response comes back within this time then proceed
            _clientDone.WaitOne(5000);
        }
        else
        {
            response = "Socket is not initialized";
        }

        return response;
    }
    private void Log(string message, bool isOutgoing)
    {
        string direction = (isOutgoing) ? ">> " : "<< ";
        mess.text += Environment.NewLine + direction + message;
    }


    private void ClearLog()
    {
        mess.text = String.Empty;
    }

    public EventGame newEventGame(int eventType,String data)
    {
        EventGame even = new EventGame();
        even.data = data;
        even.eventType = eventType;

        return even;
    }
}