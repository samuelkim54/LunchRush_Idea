using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.TerrainTools;
using UnityEngine;

public class FoodItem : MonoBehaviour
{
	public FoodTypeEnum type;
	public String id;
	public int x; //read/write from gameManager
	public int y; //red/write from gameManager
	public int gridSize; //necessary to know the edge of grid
	public Sprite idleSprite;
	public Sprite selectedSprite;
	public Sprite deleteSprite;
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

	public int ascKey {
		get { return y-x; }
	}

	public int ascArrayIndex {
		get {
			int index = x; //cover y >= x
			if (x > y){
				index = y;
			}
			return index;
		}
	}

	public int descKey {
		get { return x+y; }
	}

	public int descArrayIndex {
		get {
			int index = x; //if x+y < gridSize
			if ((x+y) >= gridSize){
				index = gridSize-(y+1);
			}
			return index;
		}
	}

	public void toggleSelection(){
		isSelected = !isSelected;
		//print(this.gameObject.name + " isSelected is " + isSelected.ToString());
		
		//workaround TODO revisit check for race condition
		if (renderer == null){
			renderer = this.GetComponent<SpriteRenderer>();
		}
		
		if (isSelected){
			renderer.sprite = selectedSprite;
			if (gameManager != null){
				gameManager.selectFood(gameObject);
				gameManager.selectFood(this.id);
			}
		} else {
			renderer.sprite = idleSprite;
			if (gameManager != null){
				gameManager.deselectFood(gameObject);
				gameManager.deselectFood(this.id);
			}
		}
	}

	public void deselect(){
		if (isSelected == true){
			toggleSelection();
		}
	}

	public void select(){
		if (isSelected == false){
			toggleSelection();
		}
	}

	public void markForDeletion(){
		if (renderer != null){ //workaround, TODO figure out why run into null pointer
			renderer.sprite = deleteSprite;
		}
	}
}
