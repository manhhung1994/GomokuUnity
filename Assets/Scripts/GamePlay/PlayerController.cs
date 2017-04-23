using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	public enum PlayerTurn{
        PLAYER1,
        PLAYER2,
        BOT
    }
    public PlayerTurn playerTurn;
    public Sprite xSprite;
    public Sprite oSprite;
	void Start () {
        playerTurn = PlayerTurn.PLAYER1;
	}
    private void Update()
    {
        Play();
    }
    void Play()
    {
        if(Input.GetMouseButtonDown(0))
        {
            Debug.Log("getmoust0");
            float xCor = Camera.main.ScreenToWorldPoint(Input.mousePosition).x;
            float yCor = Camera.main.ScreenToWorldPoint(Input.mousePosition).y;

            Vector2 origin = new Vector2(xCor, yCor);
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.zero, 0);

            if(hit.collider != null && hit.transform.gameObject.tag.Equals("Title"))
            {
                Debug.Log("hit");
            }
                

        }
    }
}
