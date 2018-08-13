using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class Test : MonoBehaviour 
{

	// Use this for initialization
	void Start () 
	{	
		LeanTween.move(gameObject, Vector2.down, 0.5f).setEaseInOutSine().setLoopPingPong();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
