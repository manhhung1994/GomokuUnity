
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

    };
    public GameStatus GameState;
    public AccStatus AccState;
    [SerializeField]
    String HostAddress;
    [SerializeField]
    int PORT;

    [Header("Game data")]
    public GameObject player1;
    public GameObject player2;
    public Text winnerText;
    public int winCondition;

    public GameObject currentPlayer;
    GameController gameController;
    int[][] flag;
    int maxRow;
    int maxCol;

    [Header("------GUI-------")]
    [Header("TEXT")]
    public Text mess;
    public Text inputId;
    public Text inputPass;
    public Text inputTest;
    public Text inputHost;
    public Text inputPort;

    [Header("GAME_OBJECT")]
    public GameObject panelLogin;
    public GameObject panelChooseRoom;
    public GameObject caroBoard;
    public GameObject panelCaro;
    public GameObject panelConnect;
    public GameObject panelReconnect;
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
    private byte[] _recieveBuffer = new byte[8142];

    public readonly static Queue<Action> ExecuteOnMainThread = new Queue<Action>();

    private EventGame ev;
    private byte[] reply;

    private bool isConnected = false;
    private bool isJoinRoom = false;
    public bool isRoot;
    public bool isMyTurn;
    public bool isReady;
    public int room;
    UnitySynchronizeInvoke synchronizeInvoke;
    public IAsyncResult abc;
    

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
        if (Input.GetMouseButtonDown(0) && GameState == GameStatus.MYTURN )
        {           
            Interact();
        }
        synchronizeInvoke.ProcessQueue();       
    }
    private void OnApplicationQuit()
    {
        SendJson(newEventGame(EventType.LOG_OUT, ""));
        _clientSocket.Close();
    }


    private void SetupServer(string hostAddr, int port)
    {
        try
        {
            //panelConnect.transform.SetAsLastSibling();
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
        if (inputHost.text == "" || Int32.Parse(inputPort.text) == null)
            SetupServer("127.0.0.1", 9696);
        else 
            SetupServer(inputHost.text, Int32.Parse(inputPort.text));
    }

    public void onClick_Back()
    {
        caroBoard.SetActive(false);
        areYouSure.transform.SetAsLastSibling();
    }
    public void onClickOKBack(bool ok)
    {
        if(ok)
        {

            if (AccState == AccStatus.LOGIN)
            {
                SendJson(newEventGame(EventType.DISCONNECT, ""));
                _clientSocket.Close();
                
                //Application.Quit();
            }
           
            if (AccState == AccStatus.ON_GAMEROOMCHOOSE)
            {
                panelLogin.transform.SetAsLastSibling();
               
                SendJson(newEventGame(EventType.DISCONNECT, ""));
            }
            if( 
                (GameState == GameStatus.READY ||
                GameState == GameStatus.ENEMY_TURN || 
                GameState == GameStatus.MYTURN))
            {
                SendJson(newEventGame(EventType.GAME_ROOM_LEAVE, "lose"));
            }
            if(GameState == GameStatus.WAITING_PLAYER || GameState == GameStatus.PLAYING )
            {
                SendJson(newEventGame(EventType.GAME_ROOM_LEAVE, ""));
            }
        }
        else
        {
            
        }
        areYouSure.transform.SetAsFirstSibling();

    }
    public void onClick_Room(int id)
    {
        SendJson(newEventGame(EventType.GAME_ROOM_JOIN, id.ToString()));
    }
    public void onClick_Login()
    {
        CheckLogin(inputId.text, inputPass.text);
        
        SendJson(newEventGame(EventType.LOG_IN, inputId.text + "-" + inputPass.text));
        
    }
    private void CheckLogin(string id, string pass)
    {
        Debug.Log(id + pass) ;
    }
    public void on_ClickReady()
    {
        if (GameState == GameStatus.PLAYING)
        {
            SendJson(newEventGame(EventType.READY, ""));
            panelReady.transform.SetAsFirstSibling();
            panelReady.SetActive(false);
        }  
        
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
                onConnectSuccess(eve);
                break;
            case EventType.LOG_IN_SUCCESS:
                onLoginSuccess(eve);
                break;
            case EventType.GAME_ROOM_JOIN_SUCCESS:
                onJoinRoomSuccess(eve);
                break;
            case EventType.READY_SUCCESS:
                onReadySuccess(eve);
                break;
            case EventType.ENEMY_JOIN_ROOM:
                onEnemyJoinRoom(eve);
                break;
            case EventType.ENEMY_LEAVE:
                onEnemyLeave(eve);
                break;
            case EventType.ENEMYDATA:
                onEnemyData(eve);
                break;
            case EventType.ROOM_LEAVE_SUCCESS:
                onRoomLeaveSuccess(eve);
                break;
            case EventType.PLAYER2_READY:
                onPlayer2Ready(eve);
                break;
            case EventType.POSSITION:
                onPossition(eve);
                break;
            case EventType.CHECKWIN:
                onCheckWin(eve);
                break;
            
            default: break;
        }
       
    }
    private void onConnectSuccess(EventGame eve)
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
    private void onLoginSuccess(EventGame eve)
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
    private void onJoinRoomSuccess(EventGame eve)
    {
        AccState = AccStatus.ON_ROOM;
        GameState = GameStatus.WAITING_PLAYER;
        Debug.Log("onJoinRoomSuccess");
        var retObj = synchronizeInvoke.Invoke((System.Func<string>)(() =>
        {
            //panelCaro.transform.SetAsLastSibling();
            panelCaro.transform.SetAsLastSibling();
            caroBoard.SetActive(false);
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
   
    private void onRoomLeaveSuccess(EventGame eve)
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
    private void onEnemyJoinRoom(EventGame eve)
    {
        
        Debug.Log("onEnemyJoinRoom");
        
        var retObj = synchronizeInvoke.Invoke((System.Func<string>)(() =>
        {
            GameState = GameStatus.PLAYING;
            if(isRoot)
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
    private void onEnemyData(EventGame eve)
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
    private void onEnemyLeave(EventGame eve)
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
    private void onReadySuccess(EventGame eve)
    {
        Debug.Log("onReadySuccess");
        GameState = GameStatus.READY;

        var retObj = synchronizeInvoke.Invoke((System.Func<string>)(() =>
        {
            panelReady.SetActive(false);
            caroBoard.SetActive(true);
            if(isRoot)
                playerLeft.transform.GetChild(1).gameObject.SetActive(true);
            else
                playerRight.transform.GetChild(1).gameObject.SetActive(true);

            return this.gameObject.name;
        }), null);
    }
    private void onPlayer2Ready(EventGame eve)
    {
        GameState = GameStatus.MYTURN;
        var retObj = synchronizeInvoke.Invoke((System.Func<string>)(() =>
        {

            playerRight.transform.GetChild(1).gameObject.SetActive(true);
            return this.gameObject.name;
        }), null);
    }
    private void onPossition(EventGame eve)
    {
        string[] pos = eve.data.Split('-');
        int posX = Int32.Parse(pos[1]);
        int posY = Int32.Parse(pos[0]);
        Debug.Log(posX + "||" + posY);

        GameState = GameStatus.MYTURN;
        var retObj = synchronizeInvoke.Invoke((System.Func<string>)(() =>
        {
            if (PlayerCanMark(posX, posY))
            {
                if (currentPlayer == player1)
                      Instantiate(player2, new Vector3(posX, posY, 0), Quaternion.identity).transform.SetParent(caroBoard.transform);                    
                else
                      Instantiate(player1, new Vector3(posX, posY, 0), Quaternion.identity).transform.SetParent(caroBoard.transform);
                
            }
           
            return this.gameObject.name;
        }), null);
    }
    private void onCheckWin(EventGame eve)
    {
        Debug.Log("onCheckWin");
        var retObj = synchronizeInvoke.Invoke((System.Func<string>)(() =>
        {
            caroBoard.SetActive(false);
            if (eve.data.Equals("true"))
            {
                Debug.Log("Ban da Win");
                panelWin.SetActive(true);
                
                panelWin.transform.GetChild(1).gameObject.GetComponent<Text>().text = "Win";
                GameState = GameStatus.WIN;
            }
            else
            {
                panelWin.SetActive(true);
                panelWin.transform.GetChild(1).gameObject.GetComponent<Text>().text = "Lose";
                Debug.Log("Ban da Lose");
                Stars.SetActive(false);
                GameState = GameStatus.GAME_OVER;
            }

            return this.gameObject.name;
        }), null);
        
    }
    public void SendJson(EventGame even)
    {
        Send(JsonUtility.ToJson(even));
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
            //Mark
            Vector3 position = new Vector3(Mathf.Round(mousePosition.x), Mathf.Round(mousePosition.y), 0);
            Instantiate(currentPlayer, position, Quaternion.identity).transform.SetParent(caroBoard.transform);
            GameState = GameStatus.ENEMY_TURN;
            //SendJson(newEventGame(EventType.POSSITION, row + "-" + col));
            Debug.Log(row + "|" + col);
            // set flag 
            /*
            int markValue = currentPlayer == player1 ? 1 : 2;

            flag[row][col] = markValue;

            int winValue = CheckRow(markValue, row, col);

            CheckWinner(winValue, markValue);

            winValue = CheckColumn(markValue, row, col);
            CheckWinner(winValue, markValue);

            winValue = CheckLeftDiagonal(markValue, row, col);
            CheckWinner(winValue, markValue);

            winValue = CheckRightDiagonal(markValue, row, col);
            CheckWinner(winValue, markValue);
            */
        }
        else
        {
            Debug.Log(row);
            Debug.Log(col);
        }

    }

    bool PlayerCanMark(int currentRow, int currentCol)
    {

        if (OutOfRange(currentRow, 0, maxRow - 1)) return false;
        if (OutOfRange(currentCol, 0, maxCol - 1)) return false;

        if (flag[currentRow][currentCol] != 0) return false;

        return true;
    }

    bool OutOfRange(int value, int min, int max)
    {

        return (value < min) || (value > max);
    }

    bool CheckWinner(int value, int player)
    {

        if (value >= winCondition)
        {
            String message = "Player " + player.ToString() + " win!!!!";
            DisplayMessage(message);
            return true;
        }
        return false;
    }

    void DisplayMessage(String message)
    {
        winnerText.text = message;
        winnerText.enabled = true;
    }
    int CheckRow(int checkValue, int row, int col)
    {

        int count = 1;
        int stepTry = winCondition - 1;
        int limit = maxCol - 1;

        //determine number of square that can be checked
        int numberOfStep = CalculatePossibeSteps(col, stepTry, limit);
        //check square on the right
        Step step = new Step(0, 1, numberOfStep);
        count += NumberOfMatch(row, col, step, checkValue);

        stepTry = -(winCondition - 1);
        limit = 0;
        //determine number of square that can be checked
        numberOfStep = CalculatePossibeSteps(col, stepTry, limit);
        //check square on the left
        step.UpdateValues(0, -1, numberOfStep);

        count += NumberOfMatch(row, col, step, checkValue);
        return count;


    }

    int CheckColumn(int checkValue, int row, int col)
    {

        int count = 1;
        int stepTry = winCondition - 1;
        int limit = maxRow - 1;

        //determine number of square that can be checked
        int numberOfStep = CalculatePossibeSteps(row, stepTry, limit);
        //check square on below
        Step step = new Step(1, 0, numberOfStep);

        count += NumberOfMatch(row, col, step, checkValue);

        stepTry = -(winCondition - 1);
        limit = 0;
        //determine number of square that can be checked
        numberOfStep = CalculatePossibeSteps(row, stepTry, limit);

        //check square on the left
        step.UpdateValues(-1, 0, numberOfStep);

        count += NumberOfMatch(row, col, step, checkValue);
        return count;


    }

    int CheckLeftDiagonal(int checkValue, int row, int col)
    {

        int count = 1;
        int stepTry = winCondition - 1;
        int limit = maxRow - 1;

        //determine number of square that can be checked
        int numberOfStep = CalculatePossibeSteps(row, stepTry, limit);

        limit = maxCol - 1;
        int tmp = CalculatePossibeSteps(col, stepTry, limit);
        numberOfStep = numberOfStep < tmp ? numberOfStep : tmp;

        //check square on down right
        Step step = new Step(1, 1, numberOfStep);

        count += NumberOfMatch(row, col, step, checkValue);

        stepTry = -(winCondition - 1);
        limit = 0;
        //determine number of square that can be checked
        numberOfStep = CalculatePossibeSteps(row, stepTry, limit);

        limit = 0;
        tmp = CalculatePossibeSteps(col, stepTry, limit);
        numberOfStep = numberOfStep < tmp ? numberOfStep : tmp;

        //check square on the up right
        step.UpdateValues(-1, -1, numberOfStep);

        count += NumberOfMatch(row, col, step, checkValue);
        return count;
    }

    int CheckRightDiagonal(int checkValue, int row, int col)
    {

        int count = 1;
        int stepTry = -(winCondition - 1);
        int limit = 0;

        //determine number of square that can be checked
        int numberOfStep = CalculatePossibeSteps(row, stepTry, limit);

        stepTry = winCondition - 1;
        limit = maxCol - 1;
        int tmp = CalculatePossibeSteps(col, stepTry, limit);
        numberOfStep = numberOfStep < tmp ? numberOfStep : tmp;

        //check square on up right
        Step step = new Step(-1, 1, numberOfStep);

        count += NumberOfMatch(row, col, step, checkValue);

        stepTry = winCondition - 1;
        limit = maxRow - 1;
        //determine number of square that can be checked
        numberOfStep = CalculatePossibeSteps(row, stepTry, limit);

        stepTry = -(winCondition - 1);
        limit = 0;
        tmp = CalculatePossibeSteps(col, stepTry, limit);
        numberOfStep = numberOfStep < tmp ? numberOfStep : tmp;

        //check square on the down left
        step.UpdateValues(1, -1, numberOfStep);

        count += NumberOfMatch(row, col, step, checkValue);
        return count;
    }

    int NumberOfMatch(int row, int col, Step step, int checkValue)
    {

        int rStep = step.rowStep;
        int cStep = step.colStep;
        int maxStep = step.maxStep;

        int count = 0;

        for (int i = 1; i <= maxStep; ++i)
        {

            int value = flag[row + i * rStep][col + i * cStep];

            if (value != checkValue) return count;

            ++count;
        }

        return count;
    }
    int CalculatePossibeSteps(int initial, int steps, int limit)
    {

        int result = Mathf.Abs(steps);
        // upper limit
        if (steps > 0)
        {

            if (initial + steps > limit)
            {
                result = limit - initial;
            }
        }
        else
        {
            if (initial + steps < limit)
            {
                result = initial - limit;
            }
        }

        return result;
    }

    int CheckRange(int value, int min, int max)
    {

        if (value > max)
        {
            value = max;
        }
        else if (value < min)
        {
            value = min;
        }

        return value;
    }
}
#endregion