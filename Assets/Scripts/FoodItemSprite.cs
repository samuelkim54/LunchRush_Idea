using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FoodItemSprite 
{
	public static Dictionary<FoodTypeEnum, Sprite> foodTypeToSpriteMap;

	static FoodItemSprite(){
		foodTypeToSpriteMap = new Dictionary<FoodTypeEnum, Sprite>
		{
			{ FoodTypeEnum.BREAD, Resources.Load<Sprite>("Sprites/bread") },
			{ FoodTypeEnum.CANDY, Resources.Load<Sprite>("Sprites/candy") },
			{ FoodTypeEnum.CHEESE, Resources.Load<Sprite>("Sprites/cheese") },
			{ FoodTypeEnum.EGG, Resources.Load<Sprite>("Sprites/egg") },
			{ FoodTypeEnum.STEAK, Resources.Load<Sprite>("Sprites/steak") }
		};
	}
}
