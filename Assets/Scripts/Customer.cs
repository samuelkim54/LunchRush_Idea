using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Customer : MonoBehaviour
{
	public String id;
	public FoodMenu.Recipe recipe;
	public Vector2 startSpriteVector = new Vector2(-1f, 0.5f);
	public float gap = 0.05f;
	public float fulfilledRaised = 0.02f;

	public Sprite breadSprite;
	public Sprite candySprite;
	public Sprite cheeseSprite;
	public Sprite eggSprite;
	public Sprite steakSprite;
	public Boolean isFulfilled = false;

	// Start is called before the first frame update
	void Start()
	{
		//TODO this is bad
		Dictionary<FoodTypeEnum, Sprite> foodTypeToSpriteMap = new Dictionary<FoodTypeEnum, Sprite>
		{
			{ FoodTypeEnum.BREAD, breadSprite },
			{ FoodTypeEnum.CANDY, candySprite },
			{ FoodTypeEnum.CHEESE, cheeseSprite },
			{ FoodTypeEnum.EGG, eggSprite },
			{ FoodTypeEnum.STEAK, steakSprite }
		};

		Vector3 lastPosition = new Vector3(startSpriteVector.x, startSpriteVector.y, 0);
		int foodPosition = 0;
		//render recipe
		foreach (FoodTypeEnum foodType in recipe.ingredientSet)
		{
			GameObject foodTypeObject = new GameObject(foodType.ToString());
			foodTypeObject.transform.SetParent(this.gameObject.transform);
			SpriteRenderer foodSpriteRenderer = foodTypeObject.AddComponent<SpriteRenderer>();
			Sprite foodSprite = foodTypeToSpriteMap[foodType];
			foodSpriteRenderer.sprite = foodSprite;
			
			print("### adding food item " + foodType.ToString());
			print(foodSprite.name); 

			if (foodPosition > 0){
				//float pixelPerUnit = foodSprite.pixelsPerUnit;
				Vector2 spriteSize = foodSprite.bounds.size;
				lastPosition.x += spriteSize.x + gap;
			}
			foodTypeObject.transform.localPosition = lastPosition;
			foodPosition++;
		}
	}

	// Update is called once per frame
	void Update()
	{
		
	}

	public void toggleOrderFulfilled(){
		isFulfilled = !isFulfilled;
		if (isFulfilled){
			Vector3 currentLocation = this.transform.position;
			currentLocation.y += fulfilledRaised;
			this.transform.position = currentLocation; 
		} else {
			Vector3 currentLocation = this.transform.position;
			currentLocation.y -= fulfilledRaised;
			this.transform.position = currentLocation; 
		}
	}

	public void orderFulfilled(){
		if (!isFulfilled){
			isFulfilled = true;
			this.transform.position = new Vector3(
				this.transform.position.x,
				this.transform.position.y + fulfilledRaised,
				this.transform.position.z
			); 
		}
	}
}
