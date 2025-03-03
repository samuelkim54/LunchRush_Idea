using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodItem : MonoBehaviour
{
	public FoodTypeEnum type;
	public String id;
	public int x; //read/write from gameManager
	public int y; //red/write from gameManager
	public Sprite idleSprite;
	public Sprite selectedSprite;
	private GameManager gameManager;

	private Boolean isSelected = false;
	private new SpriteRenderer renderer;

	// Start is called before the first frame update
	void Start()
	{
		renderer = this.GetComponent<SpriteRenderer>();
		gameManager = GameObject.FindObjectOfType<GameManager>();
	}

	// Update is called once per frame
	void Update()
	{
		
	}

	void OnMouseDown() //this is when clicked over collider (which is the object itself)
	{
		toggleSelection();
	}

	public void toggleSelection(){
		isSelected = !isSelected;
		//print(this.gameObject.name + " isSelected is " + isSelected.ToString());
		if (isSelected){
			renderer.sprite = selectedSprite;
			gameManager.selectFood(gameObject);
			gameManager.selectFood(this.id);
		} else {
			renderer.sprite = idleSprite;
			gameManager.deselectFood(gameObject);
			gameManager.deselectFood(this.id);
		}
	}
}
