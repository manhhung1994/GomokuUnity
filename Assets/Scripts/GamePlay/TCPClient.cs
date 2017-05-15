
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
    public enum GameStatus
    {
        MYTURN,
        ENEMY_TURN,
        WAITING_PLAYER,
        READY,
        PLAYING,
        WIN,
        GAME_OVER,    
        NULL,
    };
    public enum AccStatus
    {
        ON_GAMEROOMCHOOSE,
        LOGIN,
        ON_ROOM,
        RECONNECT,
        ON_REGISTER,

    };
    [Header("Game Status")]
    public GameStatus GameState;
    public AccStatus AccState;
    [SerializeField]
    String HostAddress;
    [SerializeField]

    private bool isConnected = false;
    private bool isJoinRoom = false;
    public bool isRoot;
    public bool isMyTurn;
    public bool isReady;
    public bool isEnemyReady;
    public int room;
    int PORT;

    [Header("Game data")]
    public GameObject player1;
    public GameObject player2;
    public Sprite caro_X;
    public Sprite caro_Y;
    public Sprite caro_null;
    public Text winnerText;
    public int winCondition;

    public GameObject currentPlayer;
    GameController gameController;
    public  int[][] flag;
    int maxRow;
    int maxCol;

    [Header("------GUI-------")]
    [Header("TEXT")]
    public Text matrix;
    [Header("LOGIN")]
    public Text login_inputId;
    public Text login_inputPass;
    public Text login_status;
    [Header("RECONNECT")]
    public Text recon_inputHost;
    public Text recon_inputPort;
    [Header("REGISTER")]
    public Text regis_username;
    public Text regis_Pass;
    public Text regis_nickName;
    public Text regis_status;
    [Header("GAME_OBJECT")]
    public GameObject panelLogin;
    public GameObject panelChooseRoom;
    public GameObject caroBoard;
    public GameObject panelCaro;
    public GameObject panelConnect;
    public GameObject panelReconnect;
    public GameObject panelRegister;
    public GameObject areYouSure;
    public GameObject panelWin;
    public GameObject txtConnecting;
    public GameObject panelReady;
    public GameObject playerLeft;
    public GameObject playerRight;
    public GameObject Stars;

    [Header("CLIENT")]

    static ManualResetEvent _clientDone = new ManualResetEvent(false);

    private static Socket _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    private byte[] _recieveBuffer = new byte[99999];

    public readonly static Queue<Action> ExecuteOnMainThread = new Queue<Action>();

    private EventGame ev;
    private byte[] reply;
    UnitySynchronizeInvoke synchronizeInvoke;
    private IAsyncResult abc;
    

    private void Start()
    {
        AccState = AccStatus.LOGIN;
        GameState = GameStatus.NULL;
        SaveHostAddress.Load();
        //if(SaveHostAddress.hostAdd.Host!= "" && SaveHostAddress.hostAdd.PORT != null)
        //SetupServer(SaveHostAddress.hostAdd.Host, SaveHostAddress.hostAdd.PORT);
        SetupServer("127.0.0.1"  , 9696);
        {
            gameController = GetComponent<GameController>();

            maxRow = gameController.maxRow;
            maxCol = gameController.maxCol;
            flag = new int[maxRow][];

            for (int i = 0; i < maxRow; ++i)
            {
                flag[i] = new int[maxCol];
            }
        }
        synchronizeInvoke = new UnitySynchronizeInvoke();
    }
    void Update()
    {
        //
        if (Input.GetMouseButtonDown(0)
             && GameState == GameStatus.MYTURN)
        {           
            Interact();
        }
       
        synchronizeInvoke.ProcessQueue();       
    }
    private void OnApplicationQuit()
    {
        SendJson(newEventGame(EventType.DISCONNECT, ""));
        _clientSocket.Close();
    }


    private void SetupServer(string hostAddr, int port)
    {
        try
        {
            //panelConnect.transform.SetAsLastSibling();
            _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            txtConnecting.SetActive(true);
            _clientSocket.Connect(new IPEndPoint(IPAddress.Parse(hostAddr), port));
            
        }
        catch (SocketException ex)
        {
            Debug.Log(ex.Message);
            Reconnect();
        }

         abc =_clientSocket.BeginReceive(_recieveBuffer, 0, _recieveBuffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), null);
        
    }
    private void Reconnect()
    {
        AccState = AccStatus.RECONNECT;
        Debug.Log("CONNECT_FAILED");
        //panelReconnect.SetActive(true);
        panelReconnect.transform.SetAsLastSibling();
        txtConnecting.SetActive(false);
    }
    public void onClick_Reconnect()
    {
        if (recon_inputHost.text == "" || Int32.Parse(recon_inputPort.text) == null)
            SetupServer("127.0.0.1", 9696);
        else 
            SetupServer(recon_inputHost.text, Int32.Parse(recon_inputPort.text));
    }
    public void onPopupRegister()
    {
        panelRegister.SetActive(true);
        AccState = AccStatus.ON_REGISTER;
    }
    public void onClick_Register()
    {
        if(regis_username.text !="" && regis_Pass.text !="" && regis_nickName.text !="")
        SendJson(newEventGame(EventType.REGISTER, regis_username.text +"-" + regis_Pass.text +"-" + regis_nickName.text));
    }
    public void onClick_Back()
    {
        caroBoard.SetActive(false);

        areYouSure.transform.SetAsLastSibling();
    }
    public void onClick_OKBack(bool ok)
    {
        if (ok)
        {

            if (AccState == AccStatus.ON_REGISTER)
            {
                panelRegister.SetActive(false);
                AccState = AccStatus.LOGIN;
            }
            if (AccState == AccStatus.LOGIN)
            {
                SendJson(newEventGame(EventType.DISCONNECT, ""));


                //Application.Quit();
            }

            if (AccState == AccStatus.ON_GAMEROOMCHOOSE)
            {
                panelLogin.transform.SetAsLastSibling();

                SendJson(newEventGame(EventType.LOG_OUT, ""));
                AccState = AccStatus.LOGIN;
            }
            if (
                (GameState == GameStatus.READY ||
                GameState == GameStatus.ENEMY_TURN ||
                GameState == GameStatus.MYTURN))
            {
                SendJson(newEventGame(EventType.GAME_ROOM_LEAVE, "lose"));
            }
            if (GameState == GameStatus.WAITING_PLAYER || GameState == GameStatus.PLAYING)
            {
                SendJson(newEventGame(EventType.GAME_ROOM_LEAVE, ""));
            }
        }
        else
        {
            if (GameState == GameStatus.ENEMY_TURN || GameState == GameStatus.MYTURN)
            {
                caroBoard.SetActive(true);
            }
        }
        areYouSure.transform.SetAsFirstSibling();

    }
    public void onClick_Room(int id)
    {
        SendJson(newEventGame(EventType.GAME_ROOM_JOIN, id.ToString()));
    }
    public void onClick_Login()
    {

        if (login_inputId.text != "" && login_inputPass.text != "")
            SendJson(newEventGame(EventType.LOG_IN, login_inputId.text + "-" + login_inputPass.text));
        
    }
    private void CheckLogin(string id, string pass)
    {
        Debug.Log(id + pass) ;
    }
    public void onClick_Ready()
    {
        if (GameState == GameStatus.PLAYING)
        {
            SendJson(newEventGame(EventType.READY, ""));
            panelReady.transform.SetAsFirstSibling();
            panelReady.SetActive(false);
        }  
        
    }
    public void onClick_Home()
    {
        SendJson(newEventGame(EventType.GAME_ROOM_LEAVE, ""));

        panelWin.SetActive(false);
    }
    public void onClick_Replay()
    {
        SendJson(newEventGame(EventType.REPLAY, ""));
        panelWin.SetActive(false);
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
        IAsyncResult x = _clientSocket.BeginReceive(_recieveBuffer, 0, _recieveBuffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), null);


        //Log(Encoding.UTF8.GetString(recData), false);
        ProcessData(Encoding.UTF8.GetString(recData));
        Debug.Log(Encoding.UTF8.GetString(recData));
       
    }
    private void ProcessData(string json)
    {
        EventGame eve = JsonUtility.FromJson<EventGame>(json);


        switch(eve.eventType)
        {
            case EventType.CONNECT_SUCCESS:
                onConnect_Success(eve);
                break;
            case EventType.LOG_IN_SUCCESS:
                onLogin_Success(eve);
                break;
            case EventType.LOG_IN_FAILURE:
                onLogin_Failure(eve);
                break;
            case EventType.REGISTER_SUCCESS:
                onRegister_Success(eve);
                break;
            case EventType.REGISTER_FAILURE:
                onRegister_Failure(eve);
                break;
            case EventType.GAME_ROOM_JOIN_SUCCESS:
                onJoinRoom_Success(eve);
                break;
            case EventType.READY_SUCCESS:
                onReady_Success(eve);
                break;
            case EventType.ENEMY_JOIN_ROOM:
                onEnemy_JoinRoom(eve);
                break;
            case EventType.ENEMY_LEAVE:
                onEnemy_Leave(eve);
                break;
            case EventType.ENEMYDATA:
                onEnemy_Data(eve);
                break;
            case EventType.ENEMY_READY:
                onEnemy_Ready(eve);
                break;
            case EventType.ROOM_LEAVE_SUCCESS:
                onRoomLeave_Success(eve);
                break;
            case EventType.PLAYER2_READY:
                //onPlayer2Ready(eve);
                break;
            case EventType.POSSITION:
                onPossition(eve);
                break;
            case EventType.WIN:
                on_Win(eve);
                break;
            case EventType.GAME_OVER:
                on_GameOver(eve);
                break;
            case EventType.REPLAY_SUCCESS:
                on_ReplaySuccess(eve);
                break;
            default: break;
        }
       
    }
    private void onConnect_Success(EventGame eve)
    {
        Debug.Log("onConnectSuccess");
        AccState = AccStatus.LOGIN;
        var retObj = synchronizeInvoke.Invoke((System.Func<string>)(() =>
        {
            //panelLogin.transform.SetAsLastSibling();
            panelLogin.transform.SetAsLastSibling();
            isConnected = true;
            return this.gameObject.name;
            
        }), null);
    }
    private void onRegister_Success(EventGame eve)
    {
        var retObj = synchronizeInvoke.Invoke((System.Func<string>)(() =>
        {
            regis_status.text = "Register success";
            return this.gameObject.name;

        }), null);
        
    }
    private void onRegister_Failure(EventGame eve)
    {
        var retObj = synchronizeInvoke.Invoke((System.Func<string>)(() =>
        {
            regis_status.text = "Register failure";
            return this.gameObject.name;

        }), null);
    }
    private void onLogin_Success(EventGame eve)
    {
        AccState = AccStatus.ON_GAMEROOMCHOOSE;
        Debug.Log("onLoginSuccess");

        var retObj = synchronizeInvoke.Invoke((System.Func<string>)(() =>
        {
            panelChooseRoom.transform.SetAsLastSibling();
            isConnected = true;
            return this.gameObject.name;

        }), null);
    }
    private void onLogin_Failure(EventGame eve)
    {
        
        Debug.Log("onLogin_Failure");
        var retObj = synchronizeInvoke.Invoke((System.Func<string>)(() =>
        {
            if(eve.data.Equals("online"))
                login_status.text = "Tài khoản đang được sử dụng ";
            if (eve.data.Equals("notexit"))
                login_status.text = "Tài khoản hoặc mật khẩu không đúng ";
            return this.gameObject.name;

        }), null);
    }
    public void resetCaroBoard()
    {
        Debug.Log("on reset caro");
        for(int i=0;i<15;i++)
        {
            for(int j=0;j<15;j++)
            {
                flag[i][j] = 0;
            }
        }
        var retObj = synchronizeInvoke.Invoke((System.Func<string>)(() =>
        {
            for (int i = 0; i < caroBoard.transform.childCount; i++)
            {
                caroBoard.transform.GetChild(i).gameObject.GetComponent<SpriteRenderer>().sprite = caro_null;
            }
            return this.gameObject.name;

        }), null);
       
    }
    private void onJoinRoom_Success(EventGame eve)
    {
        AccState = AccStatus.ON_ROOM;
        GameState = GameStatus.WAITING_PLAYER;
        //resetCaroBoard();
        Debug.Log("onJoinRoomSuccess");
        var retObj = synchronizeInvoke.Invoke((System.Func<string>)(() =>
        {
            //panelCaro.transform.SetAsLastSibling();
            panelCaro.transform.SetAsLastSibling();
            caroBoard.SetActive(false);
            panelWin.SetActive(false);
            panelReady.SetActive(true);
            isJoinRoom = true;


            string[] tokens = eve.data.Split(new[] { "-" }, StringSplitOptions.None);

            int flag = Int32.Parse(tokens[1]);
            
            if (flag == 1)
            {
                playerLeft.transform.GetChild(0).GetComponent<Text>().text = tokens[0];
                playerLeft.transform.GetChild(0).GetComponent<Text>().color = Color.green;
                playerLeft.SetActive(true);
                playerRight.SetActive(false);
                currentPlayer = player1;
                isRoot = true;
                if (tokens[2].Equals("exitedEnemy"))
                {
                    playerRight.SetActive(true);
                    playerRight.transform.GetChild(0).GetComponent<Text>().color = Color.red;
                    GameState = GameStatus.PLAYING;
                }
                else
                {
                    Debug.Log("chua co enemy");
                }
            }
            else {

                playerRight.transform.GetChild(0).GetComponent<Text>().text = tokens[0];
                playerRight.transform.GetChild(0).GetComponent<Text>().color = Color.green;
                playerLeft.SetActive(true);
                playerRight.SetActive(true);
                currentPlayer = player2;
                isRoot = false;
                if (tokens[2].Equals("exitedEnemy"))
                {
                    playerLeft.SetActive(true);
                    playerLeft.transform.GetChild(0).GetComponent<Text>().color = Color.red;

                    GameState = GameStatus.PLAYING;
                }
                else
                {
                    Debug.Log("chua co enemy");
                }
            }

        
            


            return this.gameObject.name;

        }), null);
        
    }
   
    private void onRoomLeave_Success(EventGame eve)
    {
        Debug.Log("onRoomLeaveSuccess");
        GameState = GameStatus.NULL;
        AccState = AccStatus.ON_GAMEROOMCHOOSE;
        var retObj = synchronizeInvoke.Invoke((System.Func<string>)(() =>
        {
            panelChooseRoom.transform.SetAsLastSibling();
            
            caroBoard.SetActive(false);


            return this.gameObject.name;
        }), null);
    }
    private void onEnemy_JoinRoom(EventGame eve)
    {
        
        Debug.Log("onEnemyJoinRoom" + eve.data);

        string[] tokens = eve.data.Split(new[] { "-" }, StringSplitOptions.None);

        var retObj = synchronizeInvoke.Invoke((System.Func<string>)(() =>
        {

            GameState = GameStatus.PLAYING;
            if(isRoot)
            {
                playerRight.transform.GetChild(0).GetComponent<Text>().text = tokens[0];
                playerRight.transform.GetChild(0).GetComponent<Text>().color = Color.red;
                if(tokens[1].Equals("true"))
                {
                    playerRight.transform.GetChild(1).gameObject.SetActive(true);
                }
                else
                    playerRight.transform.GetChild(1).gameObject.SetActive(false);
                playerRight.SetActive(true);              
            }
            else
            {
                playerLeft.transform.GetChild(0).GetComponent<Text>().text = tokens[0];
                playerLeft.transform.GetChild(0).GetComponent<Text>().color = Color.red;
                if (tokens[1].Equals("true"))
                {
                    playerLeft.transform.GetChild(1).gameObject.SetActive(true);
                }
                else
                    playerLeft.transform.GetChild(1).gameObject.SetActive(false);
                playerLeft.SetActive(true);               
            }
            return this.gameObject.name;
        }), null);
        
    }
    private void onEnemy_Data(EventGame eve)
    {
        Debug.Log("onEnemyData");
        if(eve.data.Equals("exitedEnemy"))
        {
            var retObj = synchronizeInvoke.Invoke((System.Func<string>)(() =>
            {
                if (isRoot)
                {
                    playerRight.SetActive(true);
                }
                else
                {
                    playerLeft.SetActive(true);
                }
                return this.gameObject.name;
            }), null);
        }
    }
    private void onEnemy_Leave(EventGame eve)
    {

        Debug.Log("onEnemyLeave" + eve.data);
        var retObj = synchronizeInvoke.Invoke((System.Func<string>)(() =>
        {
            GameState = GameStatus.WAITING_PLAYER;
            if (isRoot)
                playerRight.SetActive(false);           
            else
                playerLeft.SetActive(false);
            if(eve.data.Equals("win"))
            {
                panelWin.SetActive(true);
                caroBoard.SetActive(false);
            }
            return this.gameObject.name;
        }), null);
        
    }
    private void onEnemy_Ready(EventGame eve)
    {
        Debug.Log("onEnemy_Ready" + eve.data);
        var retObj = synchronizeInvoke.Invoke((System.Func<string>)(() =>
        {
            isEnemyReady = true;
            if (isRoot)
            {
                if (GameState == GameStatus.READY)
                {
                    GameState = GameStatus.MYTURN;
                    playerLeft.transform.GetChild(2).gameObject.SetActive(true);
                    playerRight.transform.GetChild(2).gameObject.SetActive(false);
                }
                playerRight.transform.GetChild(1).gameObject.SetActive(true);
            }
            else
            {
                if (GameState == GameStatus.READY)
                {
                    GameState = GameStatus.ENEMY_TURN;
                    playerLeft.transform.GetChild(2).gameObject.SetActive(true);
                    playerRight.transform.GetChild(2).gameObject.SetActive(false);
                }
                playerLeft.transform.GetChild(1).gameObject.SetActive(true);
            }
            
            return this.gameObject.name;
        }), null);
    }
    private void onReady_Success(EventGame eve)
    {
        Debug.Log("onReadySuccess");
        GameState = GameStatus.READY;
        if (isEnemyReady)
        {
            if (isRoot)
            {
                GameState = GameStatus.MYTURN;

            }
            else
            {
                GameState = GameStatus.ENEMY_TURN;

            }
            
        }
        var retObj = synchronizeInvoke.Invoke((System.Func<string>)(() =>
        {
            panelReady.SetActive(false);
            caroBoard.SetActive(true);

            resetCaroBoard();

            if (isRoot)
                playerLeft.transform.GetChild(1).gameObject.SetActive(true);
            else
                playerRight.transform.GetChild(1).gameObject.SetActive(true);

            return this.gameObject.name;
        }), null);
    }
    /*
    private void onPlayer2Ready(EventGame eve)
    {
        GameState = GameStatus.MYTURN;
        var retObj = synchronizeInvoke.Invoke((System.Func<string>)(() =>
        {

            playerRight.transform.GetChild(1).gameObject.SetActive(true);
            return this.gameObject.name;
        }), null);
    }*/
    private void onPossition(EventGame eve)
    {
        string[] pos = eve.data.Split('-');

        int row = Int32.Parse(pos[0]);
        int col = Int32.Parse(pos[1]);
        bool isGameOver = bool.Parse( pos[2]);
        Debug.Log("nhan duoc  " + row + "-" + col + "-" + isGameOver);
        
        flag[row][col] = 1;
        GameState = GameStatus.MYTURN;

        var retObj = synchronizeInvoke.Invoke((System.Func<string>)(() =>
        {
            if (isRoot)
            {
                caroBoard.transform.GetChild((row * 15) + col).GetComponent<SpriteRenderer>().sprite = caro_Y;
                playerLeft.transform.GetChild(2).gameObject.SetActive(true);
                playerRight.transform.GetChild(2).gameObject.SetActive(false);
            }
            else
            {
                caroBoard.transform.GetChild((row * 15) + col).GetComponent<SpriteRenderer>().sprite = caro_X;
                playerLeft.transform.GetChild(2).gameObject.SetActive(false);
                playerRight.transform.GetChild(2).gameObject.SetActive(true);
            }

            if(isGameOver)
            {
                on_GameOver(eve);
            }
            return this.gameObject.name;
        }), null);
    }
    private void on_Win(EventGame eve)
    {

        var retObj = synchronizeInvoke.Invoke((System.Func<string>)(() =>
        {
            caroBoard.SetActive(false);

            Debug.Log("Ban da Win");
            panelWin.SetActive(true);
            panelWin.transform.GetChild(1).gameObject.GetComponent<Text>().text = "Win";
            GameState = GameStatus.WIN;
            
           

            return this.gameObject.name;
        }), null);
    }
    private void on_GameOver(EventGame eve)
    {
        Debug.Log("GameOver");
        var retObj = synchronizeInvoke.Invoke((System.Func<string>)(() =>
        {
            caroBoard.SetActive(false);

            panelWin.SetActive(true);
            panelWin.transform.GetChild(1).gameObject.GetComponent<Text>().text = "Lose";
            Debug.Log("Ban da Lose");
            Stars.SetActive(false);
            GameState = GameStatus.GAME_OVER;


            return this.gameObject.name;
        }), null);
    }
    public void on_ReplaySuccess(EventGame eve)
    {
        Debug.Log("on_ReplaySuccess");
        var retObj = synchronizeInvoke.Invoke((System.Func<string>)(() =>
        {
            resetCaroBoard();
            panelReady.SetActive(true);
            caroBoard.SetActive(false);
            GameState = GameStatus.PLAYING;


            return this.gameObject.name;
        }), null);
    }
    public void SendJson(EventGame even)
    {
        string result = Send(JsonUtility.ToJson(even));
        Debug.Log(result);
        if(result.Equals( "Operation Timeout"))
        {
            _clientSocket.Close();
            panelReconnect.transform.SetAsLastSibling();
           
        }
    }
    private void SendData(byte[] data)
    {
        SocketAsyncEventArgs socketAsyncData = new SocketAsyncEventArgs();
        socketAsyncData.SetBuffer(data, 0, data.Length);
        
        _clientSocket.SendAsync(socketAsyncData);
        
    }
    public static string Send(string data)
    {
        string response = "Operation Timeout";
        
        
        if (_clientSocket != null)
        {
            SocketAsyncEventArgs socketEventArg = new SocketAsyncEventArgs();

            socketEventArg.RemoteEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"),9696);
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
            Debug.Log("Socket is not initialized");
        }

        return response;
    }
   

    public EventGame newEventGame(int eventType,String data)
    {
        EventGame even = new EventGame();
        even.data = data;
        even.eventType = eventType;

        return even;
    }

    public static implicit operator TcpClient(TCPClient v)
    {
        throw new NotImplementedException();
    }
    #region game play
    void Interact()
    {

        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);       
        int row = Mathf.RoundToInt(mousePosition.y);
        int col = Mathf.RoundToInt(mousePosition.x);

        if (PlayerCanMark(row, col) )
        {

            Vector3 position = new Vector3(Mathf.Round(mousePosition.x), Mathf.Round(mousePosition.y), 0);
            if(isRoot)
                caroBoard.transform.GetChild((row * 15 )+ col).GetComponent<SpriteRenderer>().sprite = caro_X;
            else
                caroBoard.transform.GetChild((row * 15) + col).GetComponent<SpriteRenderer>().sprite = caro_Y;
            GameState = GameStatus.ENEMY_TURN;
           SendJson(newEventGame(EventType.POSSITION, row + "-" + col));
            
            if(isRoot)
            {
                playerRight.transform.GetChild(2).gameObject.SetActive(true);
                playerLeft.transform.GetChild(2).gameObject.SetActive(false);
            }
            else
            {
                playerRight.transform.GetChild(2).gameObject.SetActive(false);
                playerLeft.transform.GetChild(2).gameObject.SetActive(true);
            }
            // set flag 
            
            flag[row][col] = 1;
            
        }
        else
        {

            Debug.Log("toa do khong dung");
        }

    }

    bool PlayerCanMark(int currentRow, int currentCol)
    {

        Debug.Log("row " + currentRow + "col " + currentCol);
        if (currentCol < 15 && currentRow < 15)
            if (flag[currentRow][currentCol] != 0) return false;
        if (currentCol >= 15 || currentRow >= 15) return false;

        return true;
    }

  

    
}
#endregion