using UnityEngine;
using System.Collections;

public class edge_info {

	public float dist;
	public int v1;
	public int v2;
	
	public edge_info(int i1, int i2, Vector3 v31, Vector3 v32) {
		v1 = i1;
		v2 = i2;
		dist = Mathf.Sqrt ( (v31.x - v32.x)*(v31.x-v32.x) + (v31.y-v32.y)*(v31.y-v32.y));
	}
	
	
}
