using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Unity.VisualScripting;
using UnityEngine;

public class FoodMenu
{
	//TODO
	//create a map of menu and ingredients set
	//scoring is itterating through a row and match against a set menu

	// Start is called before the first frame update
	private Dictionary<FoodMenuEnum, Recipe> recipeMap = new Dictionary<FoodMenuEnum, Recipe>();
	private Dictionary<FoodMenuEnum, int> recipeLengthMap = new Dictionary<FoodMenuEnum, int>();
	
	public FoodMenu(){
		//TODO have another class that have a definition of recipe
		//or maybe a text file that contain definition
		recipeMap.Add(FoodMenuEnum.EGG_CHEESE_SANDWICH,
			new Recipe(new HashSet<FoodTypeEnum>{
				FoodTypeEnum.BREAD,
				FoodTypeEnum.CHEESE,
				FoodTypeEnum.EGG
		}));
		recipeLengthMap.Add(FoodMenuEnum.EGG_CHEESE_SANDWICH, 
			recipeMap[FoodMenuEnum.EGG_CHEESE_SANDWICH].ingredientSet.Count);

		recipeMap.Add(FoodMenuEnum.STEAK_EGG_SANDWICH,
			new Recipe(new HashSet<FoodTypeEnum>{
				FoodTypeEnum.BREAD,
				FoodTypeEnum.STEAK,
				FoodTypeEnum.EGG
		}));
		recipeLengthMap.Add(FoodMenuEnum.STEAK_EGG_SANDWICH, 
			recipeMap[FoodMenuEnum.STEAK_EGG_SANDWICH].ingredientSet.Count);

		recipeMap.Add(FoodMenuEnum.STEAK_EGG_CHEESE_SANDWICH,
			new Recipe(new HashSet<FoodTypeEnum>{
				FoodTypeEnum.BREAD,
				FoodTypeEnum.CHEESE,
				FoodTypeEnum.EGG,
				FoodTypeEnum.STEAK
		}));
		recipeLengthMap.Add(FoodMenuEnum.STEAK_EGG_CHEESE_SANDWICH, 
			recipeMap[FoodMenuEnum.STEAK_EGG_CHEESE_SANDWICH].ingredientSet.Count);
	}
	
	// void Start()
	// {

	// }

	// // Update is called once per frame
	// void Update()
	// {
		
	// }

	public (Dictionary<int, FoodMenuEnum>, List<List<FoodItem>>) getRecipeFromFoodItemArray(FoodItem[] foodItemArray){
		Dictionary<int, FoodMenuEnum> recipeBeginIndexMap = new Dictionary<int, FoodMenuEnum>();
		List<List<FoodItem>> ingredientsForRecipeList = new List<List<FoodItem>>();

		int minIndexSteakEggCheeseSandwich = foodItemArray.Length - recipeLengthMap[FoodMenuEnum.STEAK_EGG_CHEESE_SANDWICH];
		int minIndexEggCheeseSandwich = foodItemArray.Length - recipeLengthMap[FoodMenuEnum.EGG_CHEESE_SANDWICH];
		for (int i = 0; i < foodItemArray.Length; i++)
		{
			//TODO make this dynamic, right mow is harcoded :(
			HashSet<FoodTypeEnum> ingredientSet = new HashSet<FoodTypeEnum>();
			if (i <= minIndexSteakEggCheeseSandwich){
				ingredientSet.Add(foodItemArray[i].type);
				ingredientSet.Add(foodItemArray[i+1].type);
				ingredientSet.Add(foodItemArray[i+2].type);
				ingredientSet.Add(foodItemArray[i+3].type);
				if (isIngredientSetMatchIngredients(ingredientSet, FoodMenuEnum.STEAK_EGG_CHEESE_SANDWICH)){
					recipeBeginIndexMap.Add(i, FoodMenuEnum.STEAK_EGG_CHEESE_SANDWICH);
					ingredientsForRecipeList.Add(new List<FoodItem>{
						foodItemArray[i],
						foodItemArray[i+1],
						foodItemArray[i+2],
						foodItemArray[i+3]
					});
					i += recipeLengthMap[FoodMenuEnum.STEAK_EGG_CHEESE_SANDWICH];
					break;
				}
			}
			ingredientSet.Clear();
			if (i <= minIndexEggCheeseSandwich){
				ingredientSet.Add(foodItemArray[i].type);
				ingredientSet.Add(foodItemArray[i+1].type);
				ingredientSet.Add(foodItemArray[i+2].type);
				bool matched = false;
				if (isIngredientSetMatchIngredients(ingredientSet, FoodMenuEnum.EGG_CHEESE_SANDWICH)){
					recipeBeginIndexMap.Add(i, FoodMenuEnum.EGG_CHEESE_SANDWICH);
					matched = true;
				} else if (isIngredientSetMatchIngredients(ingredientSet, FoodMenuEnum.STEAK_EGG_SANDWICH)){
					//i += recipeLengthMap[FoodMenuEnum.STEAK_EGG_SANDWICH];
					recipeBeginIndexMap.Add(i, FoodMenuEnum.STEAK_EGG_SANDWICH);
					matched = true;
				}
				if (matched){
					ingredientsForRecipeList.Add(new List<FoodItem>{
						foodItemArray[i],
						foodItemArray[i+1],
						foodItemArray[i+2]
					});
					i += recipeLengthMap[FoodMenuEnum.EGG_CHEESE_SANDWICH];
					break;
				}
			}
		}

		return (recipeBeginIndexMap, ingredientsForRecipeList);
	}

	public bool isIngredientSetMatchIngredients(HashSet<FoodTypeEnum> ingredientSet, FoodMenuEnum foodMenu){
		return ingredientSet.SetEquals(recipeMap[foodMenu].ingredientSet);
	}

	public class Recipe {
		public HashSet<FoodTypeEnum> ingredientSet;
		public Recipe(HashSet<FoodTypeEnum> ingredientSetIn){
			this.ingredientSet = ingredientSetIn;
		}
	}

	//DEBUG
}
