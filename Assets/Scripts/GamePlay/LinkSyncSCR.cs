using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using UnityTCPIPLib;
using System.Threading;
using System.Net.Sockets;

public class LinkSyncSCR : MonoBehaviour
{
  // need to make sure some object is assigned to this
  public Transform PlayerCoord;
  // allow a name for our player
  public string PlayerName;

  private Connector test;
  private string lastMessage;
  private bool isActive = false;
  private Thread listenerThread;
  private delegate void AddListITem(string val);
  private AddListITem valDelegate;
  private AddListITem valPlayer;
  private delegate void RemoveListItem(string val);
  private RemoveListItem delDelegatePlayer;

  // methods used by the delegates
  // This one is to update our status messages
  // or just shows in Debug for now
  private void UpdatelstStatus(string statusMessage)
  {
    // this is a listview on a windows form
    // obviously can't use in Unity, so will use
    // a GUI call later, for now throw to DEBUG
    //lstStatus.Items.Add(statusMessage);
    //lstStatus.Update();
    Debug.Log(statusMessage);
  }
  // this one updates our player list GUI
  // or just shows in Debug for now
  private void UpdatelstPlayers(string player)
  {
    // this is a listview on a windows form
    // obviously can't use in Unity, so will use
    // a GUI call later, for now throw to DEBUG
    //lstPlayers.Items.Add(player);
    //lstPlayers.Update();
    Debug.Log(player);
  }
  // this one removes our players from the active list
  // or just shows in Debug for now
  private void RemovePlayer(string player)
  {
    //lstPlayers.Items.Remove(player);
    //lstPlayers.Update();
    Debug.Log("Player Dropped: " + player);
  }

  // start routine for the object
  void Start()
  {
    // setup our delegates
    valDelegate = new AddListITem(UpdatelstStatus);
    valPlayer = new AddListITem(UpdatelstPlayers);
    delDelegatePlayer = new RemoveListItem(RemovePlayer);

    serverConnect();
  }

  // routine to connect to server
  void serverConnect()
  {
    while(!isActive)
    {
      if (PlayerName != "")
      {
        test = new Connector();

        Debug.Log(test.fnConnectResult("localhost", 8080, PlayerName));
        if (test.res != "")
        {
          Debug.Log(test.res);
          //lstStatus.Items.Add(test.res);
        }
        listenerThread = new Thread(new ThreadStart(DoListen));
        listenerThread.Start();
        Debug.Log("Listener Started");
        //lstStatus.Invoke(valDelegate, "Listener started");
        test.fnListUsers();
        isActive = true;
      }
      else
      {
        isActive = false;
      }
      // sleep this thing because we will remain in infinite
      // loop until the person supplies a player name
      Thread.Sleep(100);
    }
  }

  private void DoListen()
  {
    try
    {
      do
      {
        switch (test.cmd)
        {
          case "JOIN":
            break;
          case "DROP":
            GetPlayerList();
            break;
          case "ADD":
            GetPlayerList();
            break;
          default:
            if (test.res != lastMessage)
            {
              if (test.res != "")
              {
                Debug.Log(test.res);
               // lstStatus.Invoke(valDelegate, test.res);
              }
              lastMessage = test.res;
            }
            break;
        }
        Thread.Sleep(1);
      } while (isActive);
    }
    catch
    {
      isActive = false;
    }
  }

  void Update()
  {    
   
  }

  // general use methods
  private void GetPlayerList()
  {
    test.fnListUsers();
    // clear our list of users from our listview
    // in Unity clear from a listbox or other GUI element

    //lstPlayers.Items.Clear();
    for (int i = 0; i < test.lstUsers.Count; i++)
    {
      string entry = test.lstUsers[i].ToString();
      Debug.Log("Player " + i + ":" + entry);
      // assign the list to the listview
      // in unity, to some listbox or output to DEBUG for now
      //lstPlayers.Invoke(valPlayer, entry);
    }
  }

  void OnApplicationQuit()
  {
    
    try {      
      // kill the network
      Debug.Log("Shutting Down Network Interface");
      test.fnDisconnect();
      // kill the tcpip thread
      Debug.Log("Shutting Down TCP Thread");
      listenerThread.Abort();
    }
    catch { }
  }
}
