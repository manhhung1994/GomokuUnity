using UnityEngine;
using System.Collections;

public class SoundMenu : MonoBehaviour {
    private static SoundMenu _instance;
    public static SoundMenu instance {
        get
        {
            if (_instance == null)
            {
                GameObject obj = Resources.Load("SoundMenu") as GameObject;
                GameObject obj2 = Instantiate(obj) as GameObject;
                _instance = obj2.GetComponent<SoundMenu>();
            }
            return _instance;
        }
    }
    public AudioSource soundMenu;
    public AudioClip bgMenuClip;
    public AudioSource soundButton;
    public AudioClip clipButton;
    //public bool isOnMusic;
    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
        
    }
    
	void Start () {
        PlayBgClip();
    }
    public void PlaySFX()
    {

    }
    public void PlayBgClip()
    {
        
            soundMenu.clip = bgMenuClip;
            soundMenu.Play();
       
    }
    public void PlaySoundButton()
    {
       
            soundButton.clip = clipButton;
            soundButton.Play();
        
    }
}
