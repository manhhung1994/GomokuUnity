
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

    [Header("GUI")]

    public Text mess;
    public Text inputId;
    public Text inputPass;
    public Text inputTest;
    public Text inputHost;
    public Text inputPort;

    public GameObject panelLogin;
    public GameObject panelChooseRoom;
    public GameObject caroBoard;
    public GameObject panelCaro;
    public GameObject panelConnect;
    public GameObject panelReconnect;
    public GameObject txtConnecting;
    [Header("CLIENT")]

    static ManualResetEvent _clientDone = new ManualResetEvent(false);

    private static Socket _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    private byte[] _recieveBuffer = new byte[8142];

    public readonly static Queue<Action> ExecuteOnMainThread = new Queue<Action>();

    private EventGame ev;
    private byte[] reply;

    private bool isConnected = false;
    private bool isJoinRoom = false;
    private bool isRoot;
    private bool isMyTurn;
    UnitySynchronizeInvoke synchronizeInvoke;
    public IAsyncResult abc;
    
    private void Start()
    {
        SetupServer(HostAddress, PORT);

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
        if (Input.GetMouseButtonDown(0) && isJoinRoom == true)
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
            panelReconnect.SetActive(false);
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
        Debug.Log("CONNECT_FAILED");
        panelReconnect.SetActive(true);
        txtConnecting.SetActive(false);
    }
    public void onClick_Reconnect()
    {
        SetupServer(inputHost.text, Int32.Parse(inputPort.text));
    }
    public void onClick_Test()
    {
        SendJson(newEventGame(EventType.ANY, inputTest.text));
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
            case EventType.POSSITION:
                onPossition(eve);

                break;
            default: break;
        }
       
    }
    private void onConnectSuccess(EventGame eve)
    {
        Debug.Log("onConnectSuccess");
        var retObj = synchronizeInvoke.Invoke((System.Func<string>)(() =>
        {
            panelLogin.SetActive(true);
            panelConnect.SetActive(false);
            isConnected = true;
            return this.gameObject.name;

        }), null);
    }
    private void onLoginSuccess(EventGame eve)
    {

        Debug.Log("onLoginSuccess");
        var retObj = synchronizeInvoke.Invoke((System.Func<string>)(() =>
        {
            panelLogin.SetActive(false);
            panelChooseRoom.SetActive(true);
            isConnected = true;
            return this.gameObject.name;

        }), null);
    }
    private void onJoinRoomSuccess(EventGame eve)
    {
        Debug.Log("onJoinRoomSuccess");
        var retObj = synchronizeInvoke.Invoke((System.Func<string>)(() =>
        {
            panelLogin.SetActive(false);
            panelChooseRoom.SetActive(false);
            panelCaro.SetActive(true);
            caroBoard.SetActive(true);
            isJoinRoom = true;
            int flag = Int32.Parse(eve.data);
            if (flag == 1)
            {
                currentPlayer = player1;
                isMyTurn = true;
            }
            else {
                currentPlayer = player2;
                isMyTurn = false;
            }
            return this.gameObject.name;

        }), null);
        
    }
    private void onPossition(EventGame eve)
    {
        string[] pos = eve.data.Split('-');
        int posX = Int32.Parse(pos[1]);
        int posY = Int32.Parse(pos[0]);
        Debug.Log(posX + "||" + posY);
        isMyTurn = true;
        var retObj = synchronizeInvoke.Invoke((System.Func<string>)(() =>
        {

            if (PlayerCanMark(posX, posY))
            {
                if (currentPlayer == player1)
                    Instantiate(player2, new Vector3(posX, posY, 0), Quaternion.identity);
                else
                    Instantiate(player1, new Vector3(posX, posY, 0), Quaternion.identity);
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

        if (PlayerCanMark(row, col) && isMyTurn == true)
        {
            //Mark
            Vector3 position = new Vector3(Mathf.Round(mousePosition.x), Mathf.Round(mousePosition.y), 0);
            Instantiate(currentPlayer, position, Quaternion.identity);
            isMyTurn = false;
            SendJson(newEventGame(EventType.POSSITION, row +"-" + col));
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