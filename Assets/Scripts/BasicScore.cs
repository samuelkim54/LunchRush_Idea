using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicScore : IScore
{
	private List<String> selectedFoodIdList;
	private Dictionary<String, FoodItem> foodItemMap;
	private int minSelectSize;
	// public BasicScore(int minSelectSize, List<String> selectedFoodIdList, Dictionary<String, FoodItem> foodItemMap){
	// 	this.minSelectSize = minSelectSize;
	// 	this.selectedFoodIdList = selectedFoodIdList;
	// 	this.foodItemMap = foodItemMap;
	// }

	public void setProperties(Dictionary<String, System.Object> propertiesMap){
		this.minSelectSize = (int)propertiesMap["minSelectSize"];
		this.selectedFoodIdList = (List<String>)propertiesMap["selectedFoodIdList"];
		this.foodItemMap = (Dictionary<String, FoodItem>)propertiesMap["foodItemMap"];
	}

	public int checkScore(){
		int ret = 0;
		List<Vector2> selectedFoodItemLocations = getSelectedFoodItemLocations();
		if (isAllSelectedSameFoodType() && (
			isHorizontal(selectedFoodItemLocations) 
			|| isVertical(selectedFoodItemLocations) 
			|| isDiagonal(selectedFoodItemLocations))){
			ret = 10 + (selectedFoodIdList.Count - minSelectSize) * 5;
		}
		return ret;
	}

	private List<Vector2> getSelectedFoodItemLocations(){
		List<Vector2> selectedFoodItemLocations = new List<Vector2>();
		foreach (String foodId in selectedFoodIdList)
		{
			FoodItem foodItem = foodItemMap[foodId];
			selectedFoodItemLocations.Add(new Vector2(foodItem.x, foodItem.y));
		}
		return selectedFoodItemLocations;
	}

	private bool isAllSelectedSameFoodType(){
		bool ret = true;
		FoodTypeEnum selectedType = foodItemMap[selectedFoodIdList[0]].type;
		foreach (String foodId in selectedFoodIdList)
		{
			if (selectedType != foodItemMap[foodId].type){
				ret = false;
				break;
			}
		}
		return ret;
	}

	private bool isHorizontal(List<Vector2> itemLocations){
		itemLocations.Sort((a, b) => a.x.CompareTo(b.x));

		bool ret = false;
		bool isOnHorizonal = true;
		float firstY = itemLocations[0].y;
		foreach(Vector2 itemLocation in itemLocations){
			if (itemLocation.y != firstY){
				isOnHorizonal = false;
				break;
			}
		}
		if (isOnHorizonal){
			ret = true;
			for (int i = 1; i < itemLocations.Count; i++){
				if ((itemLocations[i-1].x + 1) != itemLocations[i].x){
					ret = false;
					break;
				}
			}
		}
		return ret;
	}

	private bool isVertical(List<Vector2> itemLocations){
		itemLocations.Sort((a, b) => a.y.CompareTo(b.y));

		bool ret = false;
		bool isOnVertical = true;
		float firstX = itemLocations[0].x;
		foreach(Vector2 itemLocation in itemLocations){
			if (itemLocation.x != firstX){
				isOnVertical = false;
				break;
			}
		}
		if (isOnVertical){
			ret = true;
			for (int i = 1; i < itemLocations.Count; i++){
				if ((itemLocations[i-1].y + 1) != itemLocations[i].y){
					ret = false;
					break;
				}
			}
		}
		return ret;
	}

	private bool isDiagonal(List<Vector2> itemLocations){
		bool ret = true;

		itemLocations.Sort((a, b) => {
			int compareX = a.x.CompareTo(b.x);
			return (compareX != 0) ? compareX : a.y.CompareTo(b.y);
		});

		for (int i = 1; i < itemLocations.Count; i++){
			if ((itemLocations[i-1].x + 1) != itemLocations[i].x 
				|| ((itemLocations[i-1].y + 1) != itemLocations[i].y && (itemLocations[i-1].y - 1) != itemLocations[i].y)){
				ret = false;
				break;
			}
		}

		return ret;
	}
	
}
