using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

//--
public struct Step{
	public int rowStep;
	public int colStep;
	public int maxStep;

	
	public Step(int rStep, int cStep, int max){
		
		rowStep = rStep;
		colStep = cStep;
		maxStep = max;

	}
	
	public void UpdateValues(int rStep, int cStep, int max){
		
		rowStep = rStep;
		colStep = cStep;
		maxStep = max;

	}
};

//--
public class Interaction : MonoBehaviour {

	public GameObject player1;
	public GameObject player2;
	public Text winnerText;
	public int winCondition;

	public GameObject currentPlayer;

	GameController gameController;
	int [][] flag; 
	int maxRow;
	int maxCol;




	// Use this for initialization
	void Start () {

		gameController = GetComponent<GameController>();

		maxRow = gameController.maxRow;
		maxCol = gameController.maxCol;
		flag = new int[maxRow][];

		for(int i = 0 ; i < maxRow ; ++i){
			flag[i] = new int[maxCol];

		}

	}
	
	// Update is called once per frame
	void Update () {

		if( Input.GetMouseButtonDown(0)){

			currentPlayer = currentPlayer != player1 ? player1 : player2;
			Interact();
		}
	}

	void Interact(){

		Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);


		int row = Mathf.RoundToInt(mousePosition.y);
		int col = Mathf.RoundToInt(mousePosition.x);

		if( PlayerCanMark(row, col) ){
			//Mark
			Vector3 position = new Vector3(Mathf.Round(mousePosition.x), Mathf.Round(mousePosition.y),0);
			Instantiate(currentPlayer,position,Quaternion.identity);

			// set flag 
			int markValue = currentPlayer == player1 ? 1 : 2 ;

			flag[row][col] = markValue;

			int winValue = CheckRow(markValue,row,col);

			CheckWinner(winValue,markValue);

			winValue = CheckColumn(markValue,row,col);
			CheckWinner(winValue,markValue);

			winValue = CheckLeftDiagonal(markValue,row,col);
			CheckWinner(winValue,markValue);

			winValue = CheckRightDiagonal(markValue,row,col);
			CheckWinner(winValue,markValue);
		}
		else{
			Debug.Log(row);
			Debug.Log (col);
		}

	}

	bool PlayerCanMark(int currentRow, int currentCol){
			
		if( OutOfRange(currentRow, 0, maxRow-1) ) return false;
		if( OutOfRange(currentCol, 0, maxCol-1) ) return false;

		if( flag[currentRow][currentCol] !=0) return false;

		return true;
	}
	
	bool OutOfRange(int value, int min, int max){
		
			return (value < min) || (value > max);
	}

	bool CheckWinner(int value,int player){

		if(value >= winCondition){
			String message = "Player " + player.ToString() + " win!!!!";
			DisplayMessage(message);
			return true;
		}
		return false;
	}

	void DisplayMessage(String message){
		winnerText.text = message;
		winnerText.enabled = true;
	}
	int CheckRow(int checkValue, int row,int col){

		int count=1;
		int stepTry = winCondition -1;
		int limit = maxCol - 1;

		//determine number of square that can be checked
		int numberOfStep = CalculatePossibeSteps(col,stepTry,limit);
		//check square on the right
		Step step = new Step(0,1,numberOfStep);
		count += NumberOfMatch(row,col,step, checkValue);

		stepTry = -(winCondition -1);
		limit = 0;
		//determine number of square that can be checked
		numberOfStep = CalculatePossibeSteps(col,stepTry,limit);
		//check square on the left
		step.UpdateValues(0,-1,numberOfStep);

		count += NumberOfMatch(row,col,step,checkValue);
		return count;


	}

	int CheckColumn(int checkValue, int row,int col){
		
		int count=1;
		int stepTry = winCondition -1;
		int limit = maxRow -1;
		
		//determine number of square that can be checked
		int numberOfStep = CalculatePossibeSteps(row,stepTry,limit);
		//check square on below
		Step step = new Step(1,0,numberOfStep);

		count += NumberOfMatch(row,col,step, checkValue);
		
		stepTry = -(winCondition -1);
		limit = 0;
		//determine number of square that can be checked
		numberOfStep = CalculatePossibeSteps(row,stepTry,limit);

		//check square on the left
		step.UpdateValues(-1,0,numberOfStep);
		
		count += NumberOfMatch(row,col,step,checkValue);
		return count;
		
		
	}

	int CheckLeftDiagonal(int checkValue, int row,int col){

		int count=1;
		int stepTry = winCondition -1;
		int limit = maxRow -1;
	
		//determine number of square that can be checked
		int numberOfStep = CalculatePossibeSteps(row,stepTry,limit);

		limit = maxCol -1;
		int tmp = CalculatePossibeSteps(col,stepTry,limit);
		numberOfStep = numberOfStep < tmp? numberOfStep : tmp;

		//check square on down right
		Step step = new Step(1,1,numberOfStep);
		
		count += NumberOfMatch(row,col,step, checkValue);
		
		stepTry = -(winCondition -1);
		limit = 0;
		//determine number of square that can be checked
		numberOfStep = CalculatePossibeSteps(row,stepTry,limit);

		limit = 0;
		tmp = CalculatePossibeSteps(col,stepTry,limit);
		numberOfStep = numberOfStep < tmp? numberOfStep : tmp;

		//check square on the up right
		step.UpdateValues(-1,-1,numberOfStep);
		
		count += NumberOfMatch(row,col,step,checkValue);
		return count;
	}

	int CheckRightDiagonal(int checkValue, int row,int col){
		
		int count=1;
		int stepTry = -(winCondition-1);
		int limit = 0;
		
		//determine number of square that can be checked
		int numberOfStep = CalculatePossibeSteps(row,stepTry,limit);

		stepTry = winCondition-1;
		limit = maxCol -1 ;
		int tmp = CalculatePossibeSteps(col,stepTry,limit);
		numberOfStep = numberOfStep < tmp? numberOfStep : tmp;
		
		//check square on up right
		Step step = new Step(-1,1,numberOfStep);
		
		count += NumberOfMatch(row,col,step, checkValue);
		
		stepTry = winCondition-1;
		limit = maxRow - 1;
		//determine number of square that can be checked
		numberOfStep = CalculatePossibeSteps(row,stepTry,limit);

		stepTry = -(winCondition-1);
		limit = 0;
		tmp = CalculatePossibeSteps(col,stepTry,limit);
		numberOfStep = numberOfStep < tmp? numberOfStep : tmp;
		
		//check square on the down left
		step.UpdateValues(1,-1,numberOfStep);
		
		count += NumberOfMatch(row,col,step,checkValue);
		return count;
	}

	int NumberOfMatch(int row,int col, Step step,int checkValue){

		int rStep = step.rowStep;
		int cStep = step.colStep;
		int maxStep = step.maxStep;

		int count = 0;

		for(int i = 1 ; i<= maxStep; ++i){

			int value = flag[row + i*rStep][col + i*cStep];

			if( value != checkValue) return count;

			++count;
		}

		return count;
	}
	int CalculatePossibeSteps(int initial, int steps, int limit){

		int result = Mathf.Abs(steps);
		// upper limit
		if(steps >0){

			if( initial + steps > limit){
				result = limit - initial; 
			}
		}
		else{ 
			if(initial + steps < limit){
				result = initial - limit;
			}
		}

		return result;
	}

	int CheckRange(int value, int min, int max){

		if(value > max) {
			value = max;
		}
		else if ( value < min) {
			value = min;
		}

		return value;
	}

}
