using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	public int gridSize = 8;

	public new Camera camera;
	public Vector2 startingPoint = new Vector2( 0f, 0f); //TODO compute this
	public float xySpan = 5;
	
	public float coordinateMax = 4f; //TODEPRECATE
	public float coordinateMin = -3f; //TODEPRECATE
	public int minSelectSize = 3;
	public Text scoreDisplay;
	public List<GameObject> prefabList = new List<GameObject>();
	public ScoreEnum scoreSystemType;

	private IScore scoreSystem;
	private float coordinateZ = 5; //doesn't matter
	private float rightPixelPadding = -50f;

	private Dictionary<int, GameObject> prefabMap = new Dictionary<int, GameObject>();
	private List<GameObject> selectedFoodList = new List<GameObject>();
	private List<String> selectedFoodIdList = new List<String>();
	private Dictionary<String, GameObject> foodMap = new Dictionary<string, GameObject>(); //id->food
	private Dictionary<String, FoodItem> foodItemMap = new Dictionary<string, FoodItem>(); //id->food
	private int score = 0;

	// Start is called before the first frame update
	void Start()
	{
		GetVisibleWorldSize();
		setPrefabMap();
		setScoreSystem();
		computeStartingPoint();
		generateIntitalGrid();
	}

	// Update is called once per frame
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space)){
			if(selectedFoodIdList.Count >= minSelectSize){
				int gainedScore = scoreSystem.checkScore();
				score += gainedScore;
				if (gainedScore > 0){
					replaceSelectedFoodItem();
				} else {
					clearSelection();
				}
			} else {
				clearSelection();
			}
			displayScore();
		}
	}

	//DEBUG ONLY
	private void GetVisibleWorldSize()
	{
		Camera cam = Camera.main;
		float height = cam.orthographicSize * 2;
		float width = height * cam.aspect;

		Debug.Log($"Visible World Size → Width: {width}, Height: {height}");
	}

	//DEBUG ONLY
	private  float getWorldUnitWidth(){
		Camera cam = Camera.main;
		float height = cam.orthographicSize * 2;
		float width = height * cam.aspect;
		return width;
	}

	private void setPrefabMap(){
		for (int i = 0; i < prefabList.Count; i++){
			prefabMap.Add(i, prefabList[i]);
		}
	}

	private void setScoreSystem(){
		switch (scoreSystemType)
		{
			case ScoreEnum.BASIC:
				scoreSystem = new BasicScore();
				break;
			default:
				scoreSystem = new BasicScore();
				break;
		}
		Dictionary<String, System.Object> propertiesMap = new Dictionary<String, System.Object>{
			{"minSelectSize", minSelectSize},
			{"foodItemMap", foodItemMap},
			{"selectedFoodIdList", selectedFoodIdList}
		};
		scoreSystem.setProperties(propertiesMap);
	}
	
	private void generateIntitalGrid(){
		//IDEA improved generation perhaps!
		for (int x = 0; x < gridSize; x++){
			for (int y = 0; y < gridSize; y++){
				GameObject randomizedPrefab = prefabMap[UnityEngine.Random.Range(0, prefabList.Count)];
				addItemInGrid(x, y, randomizedPrefab);
			}
		}
	}

	private void replaceSelectedFoodItem(){
		List<String> clonedSelectedFoodIdList = new List<String>(selectedFoodIdList);
		foreach (String foodId in clonedSelectedFoodIdList)
		{
			FoodItem foodItemToDelete = foodItemMap[foodId];
			GameObject gameObjectToDelete = foodMap[foodId];
			foodItemMap.Remove(foodId);
			foodMap.Remove(foodId);

			int x = foodItemToDelete.x;
			int y = foodItemToDelete.y;
			Destroy(gameObjectToDelete);
			GameObject randomizedPrefab = prefabMap[UnityEngine.Random.Range(0, prefabList.Count)];
			addItemInGrid(x, y, randomizedPrefab);
		}
		selectedFoodIdList.Clear();
		selectedFoodList.Clear();
	}

	private void addItemInGrid(int gridX, int gridY, GameObject inPrefab){
		GameObject food = Instantiate(
			inPrefab, 
			convertGridToVector3Position(gridX, gridY), 
			inPrefab.transform.rotation //Quaternion.identity
		);
		food.name = inPrefab.name + "_" + gridX.ToString() + "_" + gridY.ToString();
		FoodItem foodItemScript = food.GetComponent<FoodItem>();
		foodItemScript.id = Guid.NewGuid().ToString();
		foodItemScript.x = gridX;
		foodItemScript.y = gridY;
		//IDEA refactor perhaps
		foodMap.Add(foodItemScript.id, food);
		foodItemMap.Add(foodItemScript.id, foodItemScript);
	}

	private float pixelsToWorldUnits(float pixels)
	{
		Camera cam = Camera.main;
		float worldHeight = cam.orthographicSize * 2f; // Total world units visible in height
		float pixelsPerUnit = worldHeight / Screen.height; // Convert pixels to world units

		return pixels * pixelsPerUnit;
	}


	private void computeStartingPoint(){
		float y = -xySpan/2f;
		startingPoint.y = y;
		startingPoint.x = pixelsToWorldUnits(rightPixelPadding);
	}

	private Vector3 convertGridToVector3Position(int gridX, int gridY){
		float x = startingPoint.x + ((float)gridX/(float)(gridSize-1) * xySpan);
		float y = startingPoint.y + ((float)gridY/(float)(gridSize-1) * xySpan);
		return new Vector3(x, y, coordinateZ);
	}

	public void selectFood(GameObject food){
		selectedFoodList.Add(food);
		print(selectedFoodList.Count);
	}
	public void selectFood(String foodId){
		selectedFoodIdList.Add(foodId);
		print(selectedFoodIdList.Count);
	}

	public void deselectFood(GameObject food){
		selectedFoodList.Remove(food);
		print(selectedFoodList.Count);
	}
	public void deselectFood(String foodId){
		selectedFoodIdList.Remove(foodId);
		print(selectedFoodIdList.Count);
	}

	private void displayScore(){
		scoreDisplay.text = score.ToString();
	}

	private void clearSelection(){
		List<String> clonedSelectedFoodIdList = new List<String>(selectedFoodIdList);
		foreach (String foodId in clonedSelectedFoodIdList)
		{
			foodItemMap[foodId].toggleSelection();
		}
	}

}
