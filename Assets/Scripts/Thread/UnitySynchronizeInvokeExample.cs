using UnityEngine;
using System.Threading;

public class UnitySynchronizeInvokeExample : MonoBehaviour
{

    UnitySynchronizeInvoke synchronizeInvoke;
    public GameObject player;
    void Start()
    {
        synchronizeInvoke = new UnitySynchronizeInvoke();
        (new Thread(ThreadMain)).Start();
    }
    void ThreadMain()
    {
        while (true)
        {
            var retObj = synchronizeInvoke.Invoke((System.Func<string>)(() =>
            {
                //this.transform.localScale = Vector3.one * Random.Range(1.0f, 10.0f);
                Instantiate(player);
                return this.gameObject.name;
            }), null);
            Debug.Log("Waited for the end of synchronizeInvoke and it synchronously returned me: " + (retObj as string));
            Thread.Sleep(1 * 1000);
        }
    }
    void Update()
    {
        synchronizeInvoke.ProcessQueue();
    }
}