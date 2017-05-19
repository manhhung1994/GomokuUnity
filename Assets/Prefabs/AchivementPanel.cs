using UnityEngine;
using System.Collections;

public class AchivementPanel : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    public void Open()
    {
        gameObject.SetActive(true);
    }
    public void Close()
    {
        SoundMenu.instance.PlaySoundButton();
        GetComponent<Animator>().SetTrigger("Close");
    }
    public void OnAnimationEnd()
    {
        gameObject.SetActive(false);
    }
}
