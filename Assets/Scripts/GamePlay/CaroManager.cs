using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaroManager : MonoBehaviour {

    public float startX;
    public float startY;

    public Transform titles;
    public Camera mainCam;
    public static int BOARD_SIZE = 15;
    
	void Start () {
        CreateBoardOfLine();
	}
    void CreateBoard()
    {

    }
    void CreateBoardOfLine()
    {
        float titleX = titles.GetComponent<SpriteRenderer>().bounds.size.y;
        float titleY = titles.GetComponent<SpriteRenderer>().bounds.size.x;
        float yCor = mainCam.ScreenToWorldPoint(new Vector3(0, startY, 0)).y;

        for (int i = 0;i  < BOARD_SIZE;i ++)
        {
            float xCor = mainCam.ScreenToWorldPoint(new Vector3(startX, 0, 0)).x;
            for (int j=0;j<BOARD_SIZE;j++)
            {
                Vector3 tilesPos = new Vector3(xCor, yCor, 0);
                var obj = Instantiate(titles, tilesPos, Quaternion.identity);
                obj.SetParent(transform);
                xCor += titleX;
            }
            yCor -= titleY;
        }
    }
}
