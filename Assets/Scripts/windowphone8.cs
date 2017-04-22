using UnityEngine;
using System.Collections;
using System.Threading;
using System.Net.Sockets;
using System;
using System.Text;
using System.Net;

public class windowphone8 : MonoBehaviour {
    Socket _socket = null;

    static ManualResetEvent _clientDone = new ManualResetEvent(false);

   
    const int TIMEOUT_MILLISECONDS = 5000;
    
    const int MAX_BUFFER_SIZE = 2048;

    void Start () {
        Connect("127.0.0.1", 8080);
        string s = Receive();
        Debug.Log(s);
	}
    public string Connect(string hostName, int portNumber)
    {
        string result = string.Empty;


        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        SocketAsyncEventArgs socketEventArg = new SocketAsyncEventArgs();
        socketEventArg.RemoteEndPoint = new  IPEndPoint(IPAddress.Parse("127.0.0.1"), 8080); 

        socketEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(delegate (object s, SocketAsyncEventArgs e)
        {
            result = e.SocketError.ToString();

            _clientDone.Set();
        });

        _clientDone.Reset();

        // Make an asynchronous Connect request over the socket
        _socket.ConnectAsync(socketEventArg);

        // Block the UI thread for a maximum of TIMEOUT_MILLISECONDS milliseconds.
        // If no response comes back within this time then proceed
        _clientDone.WaitOne(TIMEOUT_MILLISECONDS);

        return result;
    }
    public void onClick_Send()
    {
        Send("xin chao csaca ban");
    }
    public string Send(string data)
    {
        string response = "Operation Timeout";

        // We are re-using the _socket object initialized in the Connect method
        if (_socket != null)
        {

            SocketAsyncEventArgs socketEventArg = new SocketAsyncEventArgs();

            socketEventArg.RemoteEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8080);
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
            _socket.SendAsync(socketEventArg);

            // Block the UI thread for a maximum of TIMEOUT_MILLISECONDS milliseconds.
            // If no response comes back within this time then proceed
            _clientDone.WaitOne(TIMEOUT_MILLISECONDS);
        }
        else
        {
            response = "Socket is not initialized";
        }

        return response;
    }
    public string Receive()
    {
        string response = "Operation Timeout";

        // We are receiving over an established socket connection
        if (_socket != null)
        {
            // Create SocketAsyncEventArgs context object
            SocketAsyncEventArgs socketEventArg = new SocketAsyncEventArgs();
            socketEventArg.RemoteEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8080);

            // Setup the buffer to receive the data
            socketEventArg.SetBuffer(new Byte[MAX_BUFFER_SIZE], 0, MAX_BUFFER_SIZE);

            // Inline event handler for the Completed event.
            // Note: This even handler was implemented inline in order to make 
            // this method self-contained.
            socketEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(delegate (object s, SocketAsyncEventArgs e)
            {
                if (e.SocketError == SocketError.Success)
                {
                    // Retrieve the data from the buffer
                    response = Encoding.UTF8.GetString(e.Buffer, e.Offset, e.BytesTransferred);
                    response = response.Trim('\0');
                }
                else
                {
                    response = e.SocketError.ToString();
                }

                _clientDone.Set();
            });

            // Sets the state of the event to nonsignaled, causing threads to block
            _clientDone.Reset();

            // Make an asynchronous Receive request over the socket
            _socket.ReceiveAsync(socketEventArg);

            // Block the UI thread for a maximum of TIMEOUT_MILLISECONDS milliseconds.
            // If no response comes back within this time then proceed
            _clientDone.WaitOne(TIMEOUT_MILLISECONDS);
        }
        else
        {
            response = "Socket is not initialized";
        }

        return response;
    }

    /// <summary>
    /// Closes the Socket connection and releases all associated resources
    /// </summary>
    public void Close()
    {
        if (_socket != null)
        {
            _socket.Close();
        }
    }

}
