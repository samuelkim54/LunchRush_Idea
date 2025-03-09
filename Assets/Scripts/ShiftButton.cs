using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShiftButton : MonoBehaviour
{
	public String id;
	public int x;
	public int y;
	public DirectionEnum direction;
	private int yShift;
	private SliderGameManager gameManager;

	// Start is called before the first frame update
	void Start()
	{
		gameManager = GameObject.FindObjectOfType<SliderGameManager>();
		if (direction == DirectionEnum.LEFT){
			yShift = -1;
		} else if (direction == DirectionEnum.RIGHT){
			yShift = 1;
		}
	}

	// Update is called once per frame
	void Update()
	{
		
	}

	void OnMouseDown()
	{
		gameManager.shiftRow(y, yShift);
	}
}
