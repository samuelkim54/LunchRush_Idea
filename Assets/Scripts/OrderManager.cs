using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class OrderManager : MonoBehaviour
{

	public Vector2 startingPoint = new Vector2(-8f, -4f);
	//public float gap = 0.2f;
	public int maxOrder = 5;
	public GameObject customerPrefab;
	private List<GameObject> customerObjectList;
	public List<Customer> customerList{
		get;
		private set;
	}

	private Dictionary<String, GameObject> customerObjectMap;
	public Dictionary<String, Customer> customerMap{
		get;
		private set;
	}
	// Start is called before the first frame update
	void Start()
	{
		customerObjectList = new List<GameObject>();
		customerList = new List<Customer>();
		customerMap = new Dictionary<String, Customer>();
		customerObjectMap = new Dictionary<String, GameObject>();
	}

	// Update is called once per frame
	void Update()
	{
		
	}

	public List<List<FoodItem>> getRecipeFromFoodItemArray(FoodItem[] foodItemArray){
		//Dictionary<int, FoodMenuEnum> recipeBeginIndexMap = new Dictionary<int, FoodMenuEnum>();
		List<List<FoodItem>> ingredientsForRecipeList = new List<List<FoodItem>>();

		//int minIndexSteakEggCheeseSandwich = foodItemArray.Length - recipeLengthMap[FoodMenuEnum.STEAK_EGG_CHEESE_SANDWICH];
		//int minIndexEggCheeseSandwich = foodItemArray.Length - recipeLengthMap[FoodMenuEnum.EGG_CHEESE_SANDWICH];

		List<Customer> customerToAnalyzeList = new List<Customer>();
		customerToAnalyzeList.AddRange(customerList);
		for (int i = 0; i < foodItemArray.Length; i++)
		{
			HashSet<FoodTypeEnum> ingredientSet = new HashSet<FoodTypeEnum>();
			Customer customerSelected = null;
			foreach (Customer customer in customerToAnalyzeList)
			{
				ingredientSet.Clear();
				int ingredientsCount = customer.recipe.ingredientSet.Count;
				if ((i+ingredientsCount) < foodItemArray.Length){
					for (int j = i; j < i+ingredientsCount; j++)
					{
						ingredientSet.Add(foodItemArray[j].type);
					}
					if (ingredientSet.SetEquals(customer.recipe.ingredientSet)){
						List<FoodItem> selectedIngredientList = new List<FoodItem>();
						for (int j = i; j < i+ingredientsCount; j++)
						{
							selectedIngredientList.Add(foodItemArray[j]);
						}
						ingredientsForRecipeList.Add(selectedIngredientList);
						customerSelected = customer;
						break;
					}
				} else {
					continue;
				}
			}
			if (customerSelected != null){
				print("### fulfilled customer : " + customerSelected.id);
				customerToAnalyzeList.Remove(customerSelected);
				customerSelected.orderFulfilled();
			}
		}

		return ingredientsForRecipeList;
	}

	public void addOrderItem(FoodMenu.Recipe recipe){
		if (customerList.Count == maxOrder){
			return;
		}
		GameObject customerObject = Instantiate(
			customerPrefab, 
			getLocation(customerList.Count), 
			customerPrefab.transform.rotation
		);
		customerObject.name = recipe.foodMenu.ToString();
		Customer customer = customerObject.GetComponent<Customer>();
		customer.recipe = recipe;
		customer.id = Guid.NewGuid().ToString();

		customerObjectList.Add(customerObject);
		customerList.Add(customer);
		customerMap.Add(customer.id, customer);
		customerObjectMap.Add(customer.id, customerObject);
	}

	public Vector3 getLocation(int index){
		SpriteRenderer customerPrefabRenderer = customerPrefab.GetComponent<SpriteRenderer>();
		Vector2 customerSize = customerPrefabRenderer.bounds.size;
		Vector3 customerLocation = new Vector3(0, 0, 0);
		customerLocation.y = startingPoint.y;
		customerLocation.x = startingPoint.x + (index * customerSize.x);
		return customerLocation;
	}

	public void destroyFulfilledOrders(){
		List<Customer> customerToAnalyzeList = new List<Customer>();
		customerToAnalyzeList.AddRange(customerList);
		foreach (Customer customer in customerToAnalyzeList)
		{
			if(customer.isFulfilled){
				String id = customer.id;
				GameObject customerObject = customerObjectMap[id];
				customerObjectList.Remove(customerObject);
				customerList.Remove(customer);
				customerMap.Remove(id);
				customerObjectMap.Remove(id);
				Destroy(customerObject);
			}
		}
	}

	public void reorderOrders(){
		int index = 0;
		foreach (GameObject customerGameObject in customerObjectList)
		{
			customerGameObject.transform.position = getLocation(index);
			index++;
		}
	}
}
