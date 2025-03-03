using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public interface IScore { 
	public void setProperties(Dictionary<String, System.Object> propertiesMap);
	public int checkScore();
}
