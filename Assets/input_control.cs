using UnityEngine;
using System.Collections;

public class input_control : MonoBehaviour {

	public scene_manager sm;
	private int select_vert;
	private bool onDrag = false;
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown (0) && !onDrag) {
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if(Physics.Raycast(ray,out hit, 100.0f)) {
				int nearest_p=-1;
				float nearest_dist = 1000000;
				for(int i=0; i<sm.VertNum; ++i) {
					float dist = Vector3.Distance(hit.point, sm.vert[i].position);
					if(dist < 0.5f) {
						if(nearest_dist > dist) {
							nearest_dist = dist;
							nearest_p = i;
						}
					}
				}
				if(nearest_p!=-1) {
					select_vert=nearest_p;
					sm.vert[select_vert].localScale = new Vector3(2f,2f,2f);
					onDrag=true;
					sm.vert[select_vert].GetComponent<SpriteRenderer>().color = new Color(210f/255f,210f/255f,210f/255f);
				}
				else select_vert=-1;
			}
		}
		else if(Input.GetMouseButtonUp(0) && onDrag) {
			sm.vert[select_vert].localScale = new Vector3(0.5f,0.5f,0.5f);
			sm.vert[select_vert].GetComponent<SpriteRenderer>().color = new Color(154f/255f,154f/255f,154f/255f);
			onDrag = false;
		}
		else if(onDrag) {
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			Physics.Raycast(ray,out hit, 100.0f);
			sm.vert[select_vert].position = new Vector3(hit.point.x, hit.point.y,0);
		}


	
	}
}
