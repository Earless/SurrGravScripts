using UnityEngine;
using System.Collections;

public class MaterialSelect : MonoBehaviour {
	public Material[] MaterialList;
	
	// Randomly select a material from the list when instantiated
	void Start ()
	{
		renderer.material = MaterialList [(int) (Random.Range (0, MaterialList.Length-0.0001f))];
	}
}
