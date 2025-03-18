using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting.Antlr3.Runtime;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.Experimental.AI;
using UnityEngine.UI;

public class MenuGameManager : MonoBehaviour
{
	public int gridSize = 8;

	public new Camera camera;
	public Vector2 startingPoint = new Vector2( 0f, 0f); //TODO compute this
	public float xySpan = 5;
	
	public float coordinateMax = 4f; //TODEPRECATE
	public float coordinateMin = -3f; //TODEPRECATE
	public int minSelectSize = 3;
	public Text scoreDisplay;
	public float timeLimitInSeconds = 90f;
	public Text timeDispay;
	public Text gameOverDisplay;
	public List<GameObject> prefabList = new List<GameObject>();
	public GameObject leftButtonPrefab;
	public GameObject rightButtonPrefab;
	public ScoreEnum scoreSystemType;
	public float rightPixelPadding = -50f;
	public float secondsDelay = 1f;
	public float orderGeneratorSecondsDelay = 1f;
	public GameObject orderManagerInstance;
	private OrderManager orderManager;

	private float orderGeneratorTimer = 2f;
	private IScore scoreSystem;
	private FoodMenu foodMenu = new FoodMenu();
	private float coordinateZ = 5; //doesn't matter
	private HashSet<int> columnToReplenishSet = new HashSet<int>();

	private Dictionary<int, GameObject> prefabMap = new Dictionary<int, GameObject>();
	private List<GameObject> selectedFoodList = new List<GameObject>();
	private List<String> selectedFoodIdList = new List<String>();
	private Dictionary<String, GameObject> foodMap = new Dictionary<string, GameObject>(); //id->food
	private Dictionary<String, FoodItem> foodItemMap = new Dictionary<string, FoodItem>(); //id->food

	private Dictionary<int, List<FoodItem>> foodItemByYRowMap = new Dictionary<int, List<FoodItem>>();
	private Dictionary<int, List<FoodItem>> foodItemByXColumnMap = new Dictionary<int, List<FoodItem>>();
	private Dictionary<int, FoodItem[]> foodItemArrayByXColumMap = new Dictionary<int, FoodItem[]>();
	private Dictionary<int, List<FoodItem>> foodItemByAscendingDiagonalMap = new Dictionary<int, List<FoodItem>>();
	private Dictionary<int, FoodItem[]> foodItemArrayByAscendingDiagonalMap = new Dictionary<int, FoodItem[]>();
	private Dictionary<int, List<FoodItem>> foodItemByDescendingDiagonalMap = new Dictionary<int, List<FoodItem>>();
	private Dictionary<int, FoodItem[]> foodItemArrayByDescendingDiagonalMap = new Dictionary<int, FoodItem[]>();
	
	//collection of all column/both diagonals of matched list
	private List<List<FoodItem>> allLongestMatchList = new List<List<FoodItem>>();
	
	private int score = 0;
	private float elapsedTime = 0f;
	private bool isGameOver = false;

	// Start is called before the first frame update
	void Start()
	{
		GetVisibleWorldSize();
		setPrefabMap();
		initFoodItemLocationMap();
		initOrderManager();
		setScoreSystem();
		computeStartingPoint();
		generateIntitalGrid();
		generateShiftButton();

		//DEBUG
		printFoodItemArrays("Ascending after start", foodItemArrayByAscendingDiagonalMap);
		printFoodItemArrays("Descending after start", foodItemArrayByDescendingDiagonalMap);

		print(FoodItemSprite.foodTypeToSpriteMap[FoodTypeEnum.BREAD]);
	}

	// Update is called once per frame
	void Update()
	{
		orderGeneratorTimer += Time.deltaTime;
		if (orderGeneratorTimer >= orderGeneratorSecondsDelay){
			int generateNow = UnityEngine.Random.Range(0, 5);
			if (generateNow > 1){ //50% chance
				FoodMenu.Recipe recipe = foodMenu.recipeList[UnityEngine.Random.Range(0, foodMenu.recipeList.Count)];
				orderManager.addOrderItem(recipe);
			}
			orderGeneratorTimer = 0f;
		}

		if (hasTimeRunOut()){
			isGameOver = true;
			gameOverDisplay.enabled = isGameOver;
			Time.timeScale = 0f;
		}
		//not doing scoring
		/*
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
		*/
	}

	private bool hasTimeRunOut(){
		bool hasTimeRunOut = false;
		elapsedTime += Time.deltaTime;
		float remainingTime = timeLimitInSeconds - elapsedTime;
		String minute = ((int)remainingTime / 60).ToString("D2");
		String seconds = ((int)remainingTime % 60).ToString("D2");
		displayTime(minute, seconds);
		if (remainingTime <= 0){
			hasTimeRunOut = true;
		}
		return hasTimeRunOut;
	}

	private void displayTime(String minute, String seconds){
		timeDispay.text = minute + ":" + seconds; 
	}

	
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

	private void initOrderManager(){
		orderManager = orderManagerInstance.GetComponent<OrderManager>();
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

	private void generateShiftButton(){
		for (int y = 0; y < gridSize; y++){
			//generate left button
			addButtonInGrid(-1, y, leftButtonPrefab);
			//generate right button
			addButtonInGrid(gridSize, y, rightButtonPrefab);
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

	private void addButtonInGrid(int gridX, int gridY, GameObject inPrefab){
		GameObject button = Instantiate(
			inPrefab, 
			convertGridToVector3Position(gridX, gridY), 
			inPrefab.transform.rotation //Quaternion.identity
		);
		button.name = inPrefab.name + "_" + gridX.ToString() + "_" + gridY.ToString();
		ShiftButton buttonScript = button.GetComponent<ShiftButton>();
		buttonScript.id = Guid.NewGuid().ToString();
		buttonScript.x = gridX;
		buttonScript.y = gridY;
	}

	private void addItemInGrid(int gridX, int gridY, GameObject inPrefab){
		GameObject food = Instantiate(
			inPrefab, 
			convertGridToVector3Position(gridX, gridY), 
			inPrefab.transform.rotation //Quaternion.identity
		);
		food.name = inPrefab.name + "_" + gridX.ToString() + "_" + gridY.ToString();
		food.GetComponent<BoxCollider2D>().enabled = false;
		FoodItem foodItem = food.GetComponent<FoodItem>();
		foodItem.id = Guid.NewGuid().ToString();
		foodItem.x = gridX;
		foodItem.y = gridY;
		foodItem.gridSize = gridSize;
		//IDEA refactor perhaps
		foodMap.Add(foodItem.id, food);
		foodItemMap.Add(foodItem.id, foodItem);
		addFoodItemToLocationMap(foodItem);
	}

	private float pixelsToWorldUnits(float pixels)
	{
		Camera cam = Camera.main;
		float worldHeight = cam.orthographicSize * 2f; // Total world units visible in height
		float pixelsPerUnit = worldHeight / Screen.height; // Convert pixels to world units

		return pixels * pixelsPerUnit;
	}

	private void initFoodItemLocationMap(){
		for (int x = 0; x < gridSize; x++){
			foodItemByXColumnMap.Add(x, new List<FoodItem>());
			foodItemArrayByXColumMap.Add(x, new FoodItem[gridSize]);
		}
		for (int y = 0; y < gridSize; y++){
			foodItemByYRowMap.Add(y, new List<FoodItem>());
		}

		Dictionary<int, int> ascendingArrayCounter = new Dictionary<int, int>();
		Dictionary<int, int> descendingArrayCounter = new Dictionary<int, int>();
		for (int x = 0; x < gridSize; x++){
			for (int y = 0; y < gridSize; y++){
				int ascendingDiagonal = y-x;
				int descendingDiagonal = x+y;
				if (!foodItemByAscendingDiagonalMap.ContainsKey(ascendingDiagonal)){
					foodItemByAscendingDiagonalMap.Add(ascendingDiagonal, new List<FoodItem>());
				}
				if (!foodItemByDescendingDiagonalMap.ContainsKey(descendingDiagonal)){
					foodItemByDescendingDiagonalMap.Add(descendingDiagonal, new List<FoodItem>());
				}
				if (!ascendingArrayCounter.ContainsKey(ascendingDiagonal)){
					ascendingArrayCounter.Add(ascendingDiagonal, 0);
				}
				ascendingArrayCounter[ascendingDiagonal]++;
				if (!descendingArrayCounter.ContainsKey(descendingDiagonal)){
					descendingArrayCounter.Add(descendingDiagonal, 0);
				}
				descendingArrayCounter[descendingDiagonal]++;
			}
		}

		foreach (int ascendingDiagonal in ascendingArrayCounter.Keys)
		{
			int arraySize = ascendingArrayCounter[ascendingDiagonal];
			foodItemArrayByAscendingDiagonalMap.Add(ascendingDiagonal, new FoodItem[arraySize]);
			print("#### ascending key["+ascendingDiagonal+"] size is " + arraySize);
		}
		foreach (int descendingDiagonal in descendingArrayCounter.Keys)
		{
			int arraySize = descendingArrayCounter[descendingDiagonal];
			foodItemArrayByDescendingDiagonalMap.Add(descendingDiagonal, new FoodItem[arraySize]);
			print("#### ascending key["+descendingDiagonal+"] size is " + arraySize);
		}
	}

	private void addFoodItemToLocationMap(FoodItem foodItem){
		foodItemByYRowMap[foodItem.y].Add(foodItem);
		foodItemByXColumnMap[foodItem.x].Add(foodItem);
		foodItemArrayByXColumMap[foodItem.x][foodItem.y] = foodItem;
		foodItemByAscendingDiagonalMap[foodItem.ascKey].Add(foodItem);
		try{
			foodItemArrayByAscendingDiagonalMap[foodItem.ascKey][foodItem.ascArrayIndex] = foodItem;
		} catch (Exception ex){
			print("### failed to add ascending array ["+foodItem.ascKey+"]["+foodItem.ascArrayIndex+"] cause : \n" + ex.Message);
		}
		foodItemByDescendingDiagonalMap[foodItem.descKey].Add(foodItem);
		try{
			foodItemArrayByDescendingDiagonalMap[foodItem.descKey][foodItem.descArrayIndex] = foodItem;
		} catch (Exception ex){
			print("### failed to add to descending array ["+foodItem.descKey+"]["+foodItem.descArrayIndex+"] cause : \n" + ex.Message);
			Debug.Break();
		}
	}
	private void removeFoodItemToLocationMap(FoodItem foodItem, bool removeFromItemArray = true){
		foodItemByYRowMap[foodItem.y].Remove(foodItem);
		foodItemByXColumnMap[foodItem.x].Remove(foodItem);
		foodItemByAscendingDiagonalMap[foodItem.ascKey].Remove(foodItem);
		foodItemByDescendingDiagonalMap[foodItem.descKey].Remove(foodItem);
		if (removeFromItemArray){ //TODO reveisit, this is a hack!
			foodItemArrayByXColumMap[foodItem.x][foodItem.y] = null;
			foodItemArrayByAscendingDiagonalMap[foodItem.ascKey][foodItem.ascArrayIndex] = null;
			foodItemArrayByDescendingDiagonalMap[foodItem.descKey][foodItem.descArrayIndex] = null;
		}
	}
	
	public void shiftRow(int rowY, int direction){ 
		//TODO there is error here that impact foodItemArray

		//to enable interrupt
		CancelInvoke("highlightScoreable");

		//TOREMOVE, POC only
		clearHighlight();
		//POC END
		List<FoodItem> foodItemToMove = new List<FoodItem>();
		foreach (FoodItem foodItem in foodItemByYRowMap[rowY])
		{
			foodItemToMove.Add(foodItem);
		}
		foreach (FoodItem foodItem in foodItemToMove)
		{
			int oldX = foodItem.x;
			int newX = foodItem.x + direction;
			if (newX >= gridSize){
				newX = 0;
			}
			if (newX < 0){
				newX = gridSize-1;
			}

			removeFoodItemToLocationMap(foodItem, false);
			//wrap around, TODO refactor
			foodItem.x = newX;
			GameObject gameObject = foodMap[foodItem.id];
			gameObject.transform.position = convertGridToVector3Position(foodItem.x, foodItem.y);
			addFoodItemToLocationMap(foodItem);
		}
		Invoke("highlightScoreable", secondsDelay);
	}

	private void clearHighlight(){
		foreach (FoodItem foodItem in foodItemMap.Values)
		{
			foodItem.deselect();
		}
	}

	private void highlightScoreable(){
		print("### called highlightScoreable");
		allLongestMatchList.Clear();
		allLongestMatchList.AddRange(highlightColumnsByOrders());
		//allLongestMatchList.AddRange(highlightColumns());
		//allLongestMatchList.AddRange(highlightAscendingDiagonal());
		//allLongestMatchList.AddRange(highlightDescendingDiagonal());
		printListList("vertical hightlight", allLongestMatchList);
		//Debug.Break();
		if (allLongestMatchList.Count > 0){
			highlightLongestMatch(allLongestMatchList);
			Invoke("scoreAndMarkForDeletion", secondsDelay);
		}
	}

	private List<List<FoodItem>> highlightColumnsByOrders(){
		List<List<FoodItem>> longestMatchList = new List<List<FoodItem>>();
		for (int x = 0; x < gridSize; x++){
			longestMatchList.AddRange(orderManager.getRecipeFromFoodItemArray(foodItemArrayByXColumMap[x]));
		}
		return longestMatchList;
	}

	private List<List<FoodItem>> highlightColumnsByFoodMenu(){
		List<List<FoodItem>> longestMatchList = new List<List<FoodItem>>();
		for (int x = 0; x < gridSize; x++){
			var recipeResult = foodMenu.getRecipeFromFoodItemArray(
				foodItemArrayByXColumMap[x]
			);
			longestMatchList.AddRange(recipeResult.Item2);
		}
		return longestMatchList;
	}

	private List<List<FoodItem>> highlightColumns(){
		List<List<FoodItem>> longestMatchList = new List<List<FoodItem>>();
		for (int x = 0; x < gridSize; x++){
			List<FoodItem> sortedFoodItems = foodItemByXColumnMap[x];
			sortedFoodItems.Sort((a, b) => a.y.CompareTo(b.y));
			longestMatchList.AddRange(getLongestMatch(sortedFoodItems));
		}
		return longestMatchList;
	}

	private List<List<FoodItem>> highlightAscendingDiagonal(){
		List<List<FoodItem>> longestMatchList = new List<List<FoodItem>>();
		foreach (int diagonalKey in foodItemByAscendingDiagonalMap.Keys)
		{
			List<FoodItem> sortedFoodItems = foodItemByAscendingDiagonalMap[diagonalKey];
			sortedFoodItems.Sort((a, b) => a.y.CompareTo(b.y));
			longestMatchList.AddRange(getLongestMatch(sortedFoodItems));
		}
		return longestMatchList;
	}

	private List<List<FoodItem>> highlightDescendingDiagonal(){
		List<List<FoodItem>> longestMatchList = new List<List<FoodItem>>();
		foreach (int diagonalKey in foodItemByDescendingDiagonalMap.Keys)
		{
			List<FoodItem> sortedFoodItems = foodItemByDescendingDiagonalMap[diagonalKey];
			sortedFoodItems.Sort((a, b) => a.x.CompareTo(b.x));
			longestMatchList.AddRange(getLongestMatch(sortedFoodItems));
		}
		return longestMatchList;
	}

	private List<List<FoodItem>> getLongestMatch(List<FoodItem> sortedFoodItemList){
		List<List<FoodItem>> longestMatchList = new List<List<FoodItem>>();
		
		FoodTypeEnum latestType = sortedFoodItemList[0].type;
		List<FoodItem> currentLongestMatch = new List<FoodItem>();
		foreach (FoodItem foodItem in sortedFoodItemList)
		{
			if (latestType == foodItem.type){
				currentLongestMatch.Add(foodItem);
			} else if (latestType != foodItem.type){
				if (currentLongestMatch.Count >= minSelectSize){
					longestMatchList.Add(currentLongestMatch);
				}
				latestType = foodItem.type;
				currentLongestMatch = new List<FoodItem>{foodItem};
			}
		}
		if (currentLongestMatch.Count >= minSelectSize){
			longestMatchList.Add(currentLongestMatch);
		}

		allLongestMatchList.AddRange(longestMatchList);
		return longestMatchList;
	}

	private void highlightLongestMatch(List<List<FoodItem>> longestMatchList){
		foreach (List<FoodItem> longestMatch in longestMatchList)
		{
			foreach (FoodItem foodItem in longestMatch)
			{
				try{
					foodItem.select();
				} catch (Exception ex){
					print("### foodItem uuid : " + foodItem.id);
					print("### ex message : " + ex.Message);
					print("### ex stack trace : " + ex.StackTrace);
					Debug.Break();
				}
			}
		}
	}

	private void scoreAndMarkForDeletion(){
		foreach (List<FoodItem> longestMatch in allLongestMatchList)
		{
			score += 30 + (longestMatch.Count-3)*5;
			foreach (FoodItem foodItem in longestMatch)
			{
				foodItem.markForDeletion();
			}
		}
		displayScore();
		Invoke("destroyMatchedItem", secondsDelay);
		Invoke("destroyFulfilledAndReorderOrder", secondsDelay);
	}

	private void destroyMatchedItem(){ //TODO refactor
		List<FoodItem> foodItemToDestroy = new List<FoodItem>();
		foreach (List<FoodItem> longestMatch in allLongestMatchList)
		{
			foodItemToDestroy.AddRange(longestMatch);
		}

		foodItemToDestroy = foodItemToDestroy.Distinct().ToList();
		foreach (FoodItem foodItem in foodItemToDestroy)
		{
			columnToReplenishSet.Add(foodItem.x);
			removeFoodItemToLocationMap(foodItem);
			String foodId = foodItem.id;
			GameObject food = foodMap[foodId]; 
			foodItemMap.Remove(foodId);
			foodMap.Remove(foodId);
			Destroy(food);
		}

		Invoke("replenishFoodItem", secondsDelay);
	}

	private void destroyFulfilledAndReorderOrder(){
		orderManager.destroyFulfilledOrders();
		orderManager.reorderOrders();
	}

	private void replenishFoodItem(){
		List<int> columnToReplenishList = new List<int>();
		columnToReplenishList.AddRange(columnToReplenishSet);
		columnToReplenishList.Sort();
		foreach (int x in columnToReplenishList)
		{
			bool isColumnFull = replenishColumn(x);
			if (isColumnFull){
				columnToReplenishSet.Remove(x);
			}
		}

		if (columnToReplenishSet.Count > 0){
			Invoke("replenishFoodItem", secondsDelay);
		} else {
			highlightScoreable();
		}
	}

	private bool replenishColumn(int x){
		bool isColumnFull;

		for (int y = 0; y < gridSize; y++)
		{
			if (foodItemArrayByXColumMap[x][y] == null){
				if (y == gridSize - 1){
					//print("### add new item to top");
					GameObject randomizedPrefab = prefabMap[UnityEngine.Random.Range(0, prefabList.Count)];
					addItemInGrid(x, y, randomizedPrefab);
				} else if (foodItemArrayByXColumMap[x][y+1] != null) {
					//print("### move item ["+x+"]["+(y+1)+"] down to ["+x+"]["+y+"]");
					FoodItem aboveFoodItem = foodItemArrayByXColumMap[x][y+1];
					moveItemToLocation(aboveFoodItem, x, y);
				}
			}
		}
		
		isColumnFull = foodItemByXColumnMap[x].Count() == gridSize;
		return isColumnFull;
	}

	private void moveItemToLocation(FoodItem foodItem, int destinationX, int destinationY){
		removeFoodItemToLocationMap(foodItem);
		foodItem.x = destinationX;
		foodItem.y = destinationY;
		GameObject gameObject = foodMap[foodItem.id];
		gameObject.transform.position = convertGridToVector3Position(foodItem.x, foodItem.y);
		addFoodItemToLocationMap(foodItem);
	}

	private void computeStartingPoint(){
		float y = -xySpan/2f;
		startingPoint.y = y;
		startingPoint.x = pixelsToWorldUnits(rightPixelPadding); //TODO centering
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

	//DEBUG FUNCTION ONLY
	private void GetVisibleWorldSize()
	{
		Camera cam = Camera.main;
		float height = cam.orthographicSize * 2;
		float width = height * cam.aspect;

		Debug.Log($"Visible World Size → Width: {width}, Height: {height}");
	}

	private void printFoodColumnArrayX(String prefix, int x){
		String columnArray = "";
		for (int y = 0; y < gridSize; y++)
		{
			if (foodItemArrayByXColumMap[x][y] == null){
				columnArray += "null\n";	
			} else {
				columnArray += foodItemArrayByXColumMap[x][y].id + ":" + foodItemArrayByXColumMap[x][y].type.ToString() + "\n";
			}
		}
		print("### "+prefix+" columnArray["+x+"] : \n" + columnArray);
	}

	private void printFoodItemArrays(String prefix, Dictionary<int, FoodItem[]>foodArrayDictionary){
		foreach (int key in foodArrayDictionary.Keys)
		{
			String columnArray = "";
			for (int i = 0; i < foodArrayDictionary[key].Length; i++)
			{
				if (foodArrayDictionary[key][i] == null){
					columnArray += "null\n";	
				} else {
					columnArray += foodArrayDictionary[key][i].id + ":" + foodArrayDictionary[key][i].type.ToString() + "\n";
				}
			}
			print("### "+prefix+" columnArray["+key+"] : \n" + columnArray);
		}
	}

	private void printListList(String prefix, List<List<FoodItem>> foodItemListList){
		if (foodItemListList.Count == 0){
			print("### "+prefix+" is empty");
		} else {
			foreach (List<FoodItem> foodItemList in foodItemListList)
			{
				String columnArray = "";
				foreach (FoodItem foodItem in foodItemList)
				{
					columnArray += foodItem.id + ":" + foodItem.type.ToString() + "\n";
				}
				print("### "+prefix+" : \n" + columnArray);
			}
		}
	}
	//DEBUG FUNCTION ONLY
}
