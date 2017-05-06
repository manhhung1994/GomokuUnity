using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;

public class SaveHostAddress : MonoBehaviour {

    public  static HostAddress hostAdd;
    public Text host;
    public Text port;
    public  Text datapath;
    public string hostText;
    public int portNum;
     void Start()
    {
        Load();
        hostText = hostAdd.Host;
        portNum = hostAdd.PORT;
    }
    public  void Save()
    {
        if (host.text != "" && int.Parse(port.text) != null)
        {
            hostAdd.Host = host.text;
            hostAdd.PORT = int.Parse(port.text);
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Create(Application.persistentDataPath + "/friend.gd");
            Debug.Log(Application.persistentDataPath);
            //datapath.text = Application.persistentDataPath;
            bf.Serialize(file, hostAdd);
            file.Close();
        }

    }
    public static void Load()
    {
        if (File.Exists(Application.persistentDataPath + "/friend.gd"))
        {
            BinaryFormatter bf = new BinaryFormatter();

            FileStream file = File.Open(Application.persistentDataPath + "/friend.gd", FileMode.Open);

            hostAdd = (HostAddress)bf.Deserialize(file);
            file.Close();
        }
    }
}
