using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Vectrosity;

public class scene_manager : MonoBehaviour {
	
	public Transform[] vert;
	public Transform[] disk;
	public bool[] tight_vert;
	public Color[] select_color;
	public Color color1,color2;
	public int[] uf_index;
	public int phase=0;
	public float radius=0;
	private List<edge_info> edges;  
	private List<edge_info> selected_edges;
	public VectorLine[] path;
	public Material linematerial;
	public float width = 5;
	public VectorLine[] colorStick;
	public VectorLine[] colorStick2;
	public int VertNum = 6;
	public UILabel inputLabel;
	public UIInput inputBox;
	public GameObject[] Butt;
	public UILabel PhaseText;
	void Start () {
		//shaking();

	}

	void Update () {
		if(Input.GetKeyDown(KeyCode.A)) {
			Debug.Log ("step : "+phase);
			StartCoroutine(kruskal());
		}
		if(Input.GetKeyDown(KeyCode.S)) {
			//shaking();
		//	setP();
			StartCoroutine(Expanding_even());
		}
		if(phase==0 && Input.GetKeyDown(KeyCode.D)) {
			initialization();
		}
		if(Input.GetKeyDown(KeyCode.F)) {
			StartCoroutine(moveEdge());
		}
	}
	public void StepUp() {
		PhaseText.text = "phase\n"+(phase+1);
		StartCoroutine(kruskal());
	}
	public void Ready() {
		Butt[1].SetActive(false);
		Butt[2].SetActive(false);
		Butt[3].SetActive(true);
		initialization();
	}
	public void setVertNum(){
		VertNum = int.Parse(inputLabel.text);
		setting();
		Butt[0].SetActive(false);
		Butt[1].SetActive(true);
		Butt[2].SetActive(true);
	}
	public void setting() {
		if(VertNum<6){
			VertNum = 6;
		}
		tight_vert = new bool[VertNum];
		Transform[] t_vert = new Transform[VertNum];
		Transform[] t_disk = new Transform[VertNum];
		for(int i=0; i<6; ++i) {
			t_vert[i] = vert[i];
			t_disk[i] = disk[i];
		}
		disk = t_disk;
		vert = t_vert;
		if(VertNum>6) {
			for(int i=6; i<VertNum; ++i) {
				vert[i] = Instantiate(vert[0].gameObject).transform;
				disk[i] = Instantiate(vert[i%2].gameObject).transform;
			}
		}
		VectorLine[] t_path = new VectorLine[VertNum];
		for(int i=0; i<6; ++i) {
			t_path[i] = path[i];
		}
		path = t_path;
		int[] t_uf_index = new int[VertNum];
		for(int i=0; i<6; ++i) {
			t_uf_index[i] = uf_index[i];
		}
		uf_index= t_uf_index;

		VectorLine[] t_colorStick = new VectorLine[VertNum*(VertNum+1)];
		colorStick = t_colorStick;

		VectorLine[] t_colorStick2 = new VectorLine[VertNum+2];
		colorStick2 = t_colorStick2;
		shaking();
	}

	public IEnumerator Expanding_even() {
		Vector3[] near_odd_p = new Vector3[VertNum/2];
		for(int i=0; i<VertNum/2; ++i) {
			int near_vert=-1;
			near_odd_p[i] = new Vector3(1000000f,1000000f,100000f);
			for(int j=0; j<VertNum/2;++j) {
				if(Vector3.Distance(vert[i*2].position,near_odd_p[i]) > Vector3.Distance(vert[i*2].position,vert[j*2+1].position)){
					near_odd_p[i] = vert[j*2+1].position;
					near_vert = j*2+1;
				}
			}
			merge(i*2,near_vert);
			Debug.Log("merge : "+i*2+" "+near_vert);
		}

		bool[] job_done = new bool[VertNum/2];
		int done = 0;
		for(int i=0; i<VertNum/2;++i) job_done[i] = false;
		while(done!=VertNum/2) {
			yield return new WaitForSeconds(0.02f);
			for(int i=0; i<VertNum/2; ++i) {
				if(job_done[i]) continue;
				disk[i*2].localScale = disk[i*2].localScale + new Vector3(0.08f,0.08f,0);
				if(disk[i*2].localScale.x*0.5f > Vector3.Distance(vert[i*2].position, near_odd_p[i])) {
					float t = Vector3.Distance(vert[i*2].position, near_odd_p[i]);
					disk[i*2].localScale = new Vector3(t*2,t*2,0);
					job_done[i]=true;
					done++;
				}
			}	
		}
		yield return null;
	}

	public IEnumerator adjustment() {
		//find smallest delta, and merge, and adjust arguments. (red +, blue -(값은 플러스))
		 yield return null;
	}


	public IEnumerator kruskal () {
		if(phase==0) {
			while(true) {
				if(merge(edges[0].v1, edges[0].v2)) {
				Debug.Log ("connect : " + edges[0].v1 + " " + edges[0].v2);
				selected_edges.Add(edges[0]);
				edges.RemoveAt(0);
				break;
				}
				edges.RemoveAt(0);
			}
			StartCoroutine(makeColorStick2(selected_edges[0].dist));
			yield return StartCoroutine(expansion(0,0,selected_edges[0].dist));
			StartCoroutine(makeColorStick(selected_edges[0].dist));
			++phase;
			Vector3[] tt = {vert[selected_edges[0].v1].position, vert[selected_edges[0].v2].position };
			path[phase-1] = new VectorLine("edge_"+(phase-1), tt,linematerial, width, LineType.Continuous);
			path[phase-1].textureScale = 2f;
			path[phase-1].SetColor(Color.white);
			path[phase-1].Draw3DAuto();
		}
		else {
			while(true) {
				if(merge(edges[0].v1, edges[0].v2)) {
					Debug.Log ("connect : " + edges[0].v1 + " " + edges[0].v2);
					selected_edges.Add(edges[0]);
					edges.RemoveAt(0);
					break;
				}
				edges.RemoveAt(0);
			}
			List<GameObject> gogo = new List<GameObject>();
			for(int i=0; i<vert.Length; ++i) {
				gogo.Add( Instantiate(vert[i].gameObject) );
			}
			for(int i=0; i<vert.Length; ++i) {
				gogo[i].transform.position = new Vector3(vert[i].position.x, vert[i].position.y, phase);
				gogo[i].GetComponent<SpriteRenderer>().color = select_color[phase%8];
			}
		//	selected_edges.Add(edges[0]);
		//	edges.RemoveAt(0);
			StartCoroutine(makeColorStick2(selected_edges[phase].dist));
			yield return StartCoroutine(expansion2 (gogo, 1, selected_edges[phase-1].dist, selected_edges[phase].dist));
			
			StartCoroutine(makeColorStick(selected_edges[phase].dist));

			++phase;

			Vector3[] tt = {vert[selected_edges[phase-1].v1].position, vert[selected_edges[phase-1].v2].position };
			path[phase-1] = new VectorLine("edge_"+(phase-1), tt,linematerial, width, LineType.Continuous);
			path[phase-1].textureScale = 2f;
			path[phase-1].SetColor(Color.white);
			path[phase-1].Draw3DAuto();
		}
	}
	
	public IEnumerator expansion (int index, float min_r, float max_r) {
		float ff=0;
		Vector3 start = new Vector3(min_r,min_r,1f);
		Vector3 end = new Vector3(max_r,max_r,1f);
		while(ff<1) {
			Vector3 t_scale = Vector3.Lerp(start,end,ff);
			for(int i=0;i<vert.Length;++i) {
				vert[i].localScale = t_scale;
			}
			ff+=0.01f;
			yield return new WaitForSeconds(0.02f);
		}
		for(int i=0; i<vert.Length;++i){
			vert[i].localScale = end;
		}
	}
	
	public IEnumerator expansion2 (List<GameObject> go, int index, float min_r, float max_r) {
		float ff=0;
		Vector3 start = new Vector3(min_r,min_r,1f);
		Vector3 end = new Vector3(max_r,max_r,1f);
		while(ff<1) {
			Vector3 t_scale = Vector3.Lerp(start,end,ff);
			for(int i=0;i<vert.Length;++i) {
				go[i].transform.localScale = t_scale;
			}
			ff+=0.01f;
			yield return new WaitForSeconds(0.02f);
		}
		for(int i=0; i<vert.Length;++i){
			go[i].transform.localScale = end;
		}
	}
	
	public int find(int a) {
		if(uf_index[a]<0) return a;
		uf_index[a] = find (uf_index[a]);
		return uf_index[a];
	}
	
	//if parent is different, return true
	public bool merge(int a, int b){
		a = find (a);
		b = find (b);
		Debug.Log(a+" "+b);
		if(a==b) return false;
		uf_index[b] = a;
		return true;
	}

	public void shaking() {
		for(int i=0; i< vert.Length;++i) {
			int ccc=0;
			while(true) {
				ccc=0;
				vert[i].position = new Vector3(Random.Range(-5f,45f), Random.Range(-5f,45f),0);
				for(int j=0; j<i; ++j) {
					if(ccc>20) break;
					ccc++;
					if(Vector3.Distance(vert[i].position, vert[j].position) < 2f) continue;
				}
				break;
			}
		}
		this.edges = new List<edge_info>();
		this.selected_edges = new List<edge_info>();
		
		for(int i=0; i<VertNum/2;++i) {
			for(int j=0; j<VertNum/2;++j) {
				edge_info t = new edge_info(i*2,j*2+1,vert[i*2].position,vert[j*2+1].position);
				edges.Add(t);
			}
		}
	}
	
	public void initialization() {
		for(int i=0; i<vert.Length; ++i) {
			Instantiate(vert[i].gameObject, vert[i].transform.position+ new Vector3(0,0,-1), vert[i].transform.rotation);
			vert[i].GetComponent<SpriteRenderer>().color = select_color[0];
		}
		edges.Sort ( delegate(edge_info x, edge_info y) {
			return x.dist.CompareTo(y.dist);
		});
		Debug.Log (edges.Count);
		for(int i=0; i<edges.Count;++i) {
			Debug.Log (edges[i].dist);
		}		
		for(int i=0; i<vert.Length; ++i ) {
			uf_index[i] = -1;
		}
	}

	IEnumerator moveEdge() {
		float EdgeSum=0;
		float edgeLength;
		for(int i=0; i<path.Length-1; ++i) {
			Vector3 tp1,tp2;
			if(path[i].points3[0].x < path[i].points3[1].x) {
				tp1= path[i].points3[0];
				tp2= path[i].points3[1];
			}
			else {
				tp1= path[i].points3[1];
				tp2= path[i].points3[0];
			}
			edgeLength = Vector3.Distance(tp1,tp2);
			Vector3 dist1 = new Vector3(EdgeSum-35,-10,0);
			Vector3 dist2 = new Vector3(EdgeSum+edgeLength-35,-10,0);

			for(float j=0; j<1; j+=0.04f) {
				path[i].points3[0] = Vector3.Lerp(tp1,dist1,j);
				path[i].points3[1] = Vector3.Lerp(tp2,dist2,j);
				yield return new WaitForSeconds(0.02f);
			}

			path[i].points3[0] = dist1;
			path[i].points3[1] = dist2;
			EdgeSum += edgeLength;
		}
	}

	void setP() {
		//colorStick[0].rectTransform.parent = colorStick[1].rectTransform;
		//colorStick[0].points3[0] = new Vector3(10f,10f,0f);
		//colorStick[0].points3[1] = new Vector3(20f,10f,0f);
		//colorStick[0].drawTransform.position = new Vector3(10f,10f,10f);
		//colorStick[0].SetColor(Color.white);
		//colorStick[0].Draw3D();
		//Debug.Log("asdf");
	}

	/*IEnumerator moveEdge2(int i) {
		int[] n_edge = new int[VertNum];
		float[] n_dist = new float[VertNum];
		Vector3[] tail_pts = new Vector3[i+2];
		int first = VertNum-2-i;
		Vector3 head_pts = colorStick[first].points3[0];
		for(int j=0; j<i+1; ++j) {
			n_edge[j] = first + (VertNum-1)*j;
			n_dist[j] = Vector3.Distance(colorStick[n_edge[j]].points3[0],colorStick[n_edge[j]].points3[1]);
			tail_pts[j] = colorStick[n_edge[j]].points3[1];
		}
		Vector3 dest = new Vector3(-35f,8f,0);

		for(float j=0; j<1; j+=0.04f) {
			colorStick[first].points3[0] = Vector3.Lerp(head_pts,dest,j);
			colorStick[first].points3[1] = Vector3.Lerp(tail_pts[0],dest+n_dist[0],j);
			for(int k=1; k<i+1; ++k) {
				colorStick[n_edge[k]].points3[0] = Vector3.Lerp(tail_pts[k-1],dest+n_dist[k-1],j );
				colorStick[n_edge[k]].points3[1] = Vector3.Lerp(tail_pts[k],dest+n_dist[k],j );
			}
		}
		yield return null;
	}*/

	IEnumerator makeColorStick(float disttt) {
		Vector3 tp = new Vector3(-25f,50f,0f);
		for(int i=0; i<vert.Length-1-phase; ++i) {
			Vector3[] tt = { new Vector3(-25,50-1f*i,0), new Vector3(-25,50-1f*i,0)};
			colorStick[(phase)*(vert.Length-1) + i] = new VectorLine("stick_"+(phase)+"_"+(i), tt,linematerial, width, LineType.Continuous);
			colorStick[(phase)*(vert.Length-1) + i].textureScale = 2f;
			colorStick[(phase)*(vert.Length-1) + i].SetColor(select_color[phase%8]);
			//colorStick[(phase)*(vert.Length-1) + i].Draw3DAuto();
		}	
		Debug.Log("make colorstick : "+ (phase*(vert.Length-1)+0)+" to "+(phase*(vert.Length-1)+(vert.Length-2-phase)));
		for(int j=0; j<vert.Length-1-phase; ++j) {
			if(phase!=0) colorStick[(phase)*(vert.Length-1) + j].points3[0] = colorStick[(phase-1)*(vert.Length-1) + j].points3[1];
			colorStick[(phase)*(vert.Length-1) + j].points3[1] = new Vector3(colorStick[(phase)*(vert.Length-1) + j].points3[1].x+disttt,colorStick[(phase)*(vert.Length-1) + j].points3[0].y,0);
			colorStick[(phase)*(vert.Length-1)+j].Draw3D();
		}
		for(int i=0; i<vert.Length-1-phase; ++i) {
			if(phase==0) continue;
			colorStick[(phase)*(vert.Length-1)+i].rectTransform.parent = colorStick[i].rectTransform;
		}

		yield return null;
	}

	IEnumerator makeColorStick2(float disttt) {
		float prev_dist;
		if(phase==0) prev_dist=0;
		else prev_dist = selected_edges[phase-1].dist;
		float stick_length = (VertNum-1-phase)*(disttt-prev_dist);
		float ff=0;
		Vector3 head,tail;
		if(phase==0) head = new Vector3(-35f,-12f,0);
		else head = colorStick2[phase-1].points3[1];
		tail = head + new Vector3(stick_length,0,0);
		Vector3[] tt = { head, head};
		colorStick2[phase]= new VectorLine("stick2_"+(phase),tt,linematerial,width,LineType.Continuous );
		colorStick2[phase].textureScale=2f;
		colorStick2[phase].SetColor(select_color[phase%8]);
		while(ff<1) {
			colorStick2[phase].points3[1] = Vector3.Lerp(head,tail,ff);
			ff+=0.01f;
			colorStick2[phase].Draw3D();
			yield return new WaitForSeconds(0.02f);
		}
		yield return null;
	}
}
