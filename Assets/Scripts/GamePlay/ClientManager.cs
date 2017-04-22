using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class ClientManager : MonoBehaviour {

    // Use this for initialization
    public Text message;
    public Text inputText; 
    

    MemoryStream stream1 = new MemoryStream();

    private const int portNum = 8080;
    delegate void SetTextCallback(string text);

    TcpClient client;
    NetworkStream ns;
    Thread t;
    private const string hostName = "localhost";
    Socket socket;
    private bool isActive;
     void Awake()
    {
        if(t!= null)
        t.Abort();
    }
    void Start () {

        ServerConnect();
       
    }
    public void ServerConnect()
    {
        while(!isActive)
        {
            if (hostName != "")
            {
                client = new TcpClient(hostName, portNum);
                ns = client.GetStream();
                String s = "Connected";
                byte[] byteTime = Encoding.ASCII.GetBytes(s);
                ns.Write(byteTime, 0, byteTime.Length);
                t = new Thread(DoListen);
                t.Start();
                isActive = true;
            }
            else
            {
                isActive = false;
            }
            Thread.Sleep(100);
        }
        
    }
    public void onClick_StopListen()
    {
        if (t.IsAlive == true)
        {
            client.Close();
            ns.Close();
            t.Abort();
            Debug.Log("stop thread");
        }
        else Debug.Log("thread da null");
    }

    void OnApplicationQuit()
    {
        try
        {
            // kill the network
            Debug.Log("Shutting Down Network Interface");
            
            // kill the tcpip thread
            Debug.Log("Shutting Down TCP Thread");
            t.Abort();
           
        }
        catch { }
    }



    public void btnSend_Click()
    {
        String s = inputText.text;
        byte[] byteTime = Encoding.ASCII.GetBytes(s);
        ns.Write(byteTime, 0, byteTime.Length);
        
    }

    // This is run as a thread
    public void DoListen()
    {
        try
        {
            byte[] bytes = new byte[1024];
            do
            {
                int bytesRead = ns.Read(bytes, 0, bytes.Length);
                Debug.Log(bytesRead);
                Thread.Sleep(1);
            } while (isActive);
        }
        catch
        {
            isActive = false;
        }
    }
    public void DoWork()
    {
        byte[] bytes = new byte[1024];
        while (true)
        {
            int bytesRead = ns.Read(bytes, 0, bytes.Length);
            //this.SetText(Encoding.ASCII.GetString(bytes, 0, bytesRead));
            Debug.Log(bytesRead);
        }
    }
    private void SetText(string text)
    {
        message.text = text;
    }
}
