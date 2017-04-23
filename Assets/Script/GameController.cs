using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {
    public Transform gameBoard;
	public GameObject square;
	public int maxRow;
	public int maxCol;

	public int size;
	// Use this for initialization
	void Start () {


		for(int r = 0; r <maxRow ; r++){

			for(int c = 0; c <maxCol ; c++){
				GameObject squareClone = Instantiate(square,new Vector3(r,c,0),Quaternion.identity) as GameObject;
                //squareClone.transform.parent = transform;
                squareClone.transform.parent = gameBoard;
			} 
		} 
	}
	
	
}
