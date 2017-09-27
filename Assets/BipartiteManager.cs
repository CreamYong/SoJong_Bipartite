using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Vectrosity;

public class BipartiteManager : MonoBehaviour {

	public Transform[] vert;	//assume odd is x, even is y
	public Transform[] disk;	//disk either
	public Color color1, color2;
	public Color t_color, m_color;
	
	public List<edge_info> Tight_edges;
	public List<edge_info> Match_edges;
	public List<edge_info> new_Path;
	public bool isPathExist;
	public bool[] tree_vert;
	public bool[] match_vert;
	public bool[] tight_vert;

	public int[] uf_index;
	public int phase=0;
	public float radius=0;
	private List<edge_info> edges;
	private List<edge_info> selected_edges;
	
	public List<VectorLine> tight_path;
	public List<VectorLine> match_path;


	public Material linematerial;
	public float width = 1.5f;
	public float m_width = 2.5f;
	public int VertNum = 6;
	public UILabel inputLabel;
	public UILabel inputBox;
	public GameObject[] Butt;
	public UILabel PhaseText;

	private int select_odd;
	private int select_even;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.Q)) {
			shake();
		}
		if(Input.GetKeyDown(KeyCode.A)) {
			initialization();
		}
		if(Input.GetKeyDown(KeyCode.S)) {
			StartCoroutine(Expanding_even_new());
		}
		if(Input.GetKeyDown(KeyCode.D)) {
			StartCoroutine(next_Phase());
		}
		if(Input.GetKeyDown(KeyCode.F)) {
			//StartCoroutine(moveEdge());
			for(int i=0; i<Match_edges.Count; ++i) {
				Debug.Log("M_E : "+Match_edges[i].v1+" "+Match_edges[i].v2);
			}
			for(int i=0; i<Tight_edges.Count; ++i) {
				Debug.Log("T_E : "+Tight_edges[i].v1+" "+Tight_edges[i].v2);
			}
		}
	}
	public void shake() {
		for(int i=0; i< vert.Length;++i) {
			int ccc=0;
			int[] check;
			check = new int[3];
			float min_dist=100000;
			while(true) {
				ccc=0;
				vert[i].position = new Vector3(Random.Range(-5f,45f), Random.Range(-5f,45f),0);
				for(int j=0; j<i; ++j) {
					if(ccc>20) break;
					ccc++;
					if(Vector3.Distance(vert[i].position, vert[j].position) < 2f) continue;
				}
				disk[i].position = vert[i].position;
				break;
			}
		}
		/*for(int i=0; i<VertNum; i+=2) {
			min_dist=100000000;
			for(int j=1; j<VertNum; j+=2) {
				if(Vector3.Distance(vert[i],vert[j]) <min_dist) {
					check[i/2]=
				}
			}	
		}*/
	}

	public void initialization() {
		this.edges = new List<edge_info>();
		this.Tight_edges = new List<edge_info>();
		this.Match_edges = new List<edge_info>();
		this.tight_path  = new List<VectorLine>();
		this.match_path  = new List<VectorLine>();
		
		for(int i=0; i<VertNum; i+=2) {
			for(int j=1; j<VertNum; j+=2) {
				edge_info t = new edge_info(i,j,vert[i].position, vert[j].position);
				edges.Add(t);
			}
		}

		edges.Sort( delegate(edge_info x, edge_info y){
			return x.dist.CompareTo(y.dist);
		});

		for(int i=0; i<vert.Length; ++i) {
			uf_index[i] = -1;
			match_vert[i] = false;
			tight_vert[i] = false;
			tree_vert[i] = false;
		}
	}



	public IEnumerator next_Phase() {
		Debug.Log("Phase : "+phase);
		int k=0;
		while(true) {
			k++;
			if(k>20) break;
			Debug.Log("Step : Adjustment");
			yield return StartCoroutine(adjustment_step());
			Debug.Log("Step : End Adjustment");
			
			yield return new WaitForSeconds(2f);

			Debug.Log("Select : "+select_even+ " " +select_odd);
			if(match_vert[select_odd]) { //odd가 프리가 아님!!
				Debug.Log("Step : TreeGrowing");
				yield return StartCoroutine(TreeGrowing());
				Debug.Log("Step : End TreeGrowing");
				continue;
			}
			else { //odd가 프리임!!
				Debug.Log("Step : Augmenting");
				yield return StartCoroutine(AUG_Path());
				Debug.Log("Step : End Augmenting");
				
				break;
			}
			yield return new WaitForSeconds(2f);

		}
		++phase;
		yield return null;
	}

	public IEnumerator Expanding_even_new() {
		//가장 짧은 edge부터 하나씩 연결함.
		//하나의 odd에 여러개의 even이 연결됐을경우 가장 짧은걸 Match, 나머지는 그냥 tight.
		//

		int done = 0;
		for(int i=0; i<edges.Count; ++i) {
			if(tight_vert[edges[i].v1]==false) { // if even vert is not tight
				Tight_edges.Add(edges[i]);	//add edges to tight
				tight_vert[edges[i].v1]=true; //marking tight vert
				tight_vert[edges[i].v2]=true;
				if(match_vert[edges[i].v2]==false) {//if odd vert is no matching
					Match_edges.Add(edges[i]);
					match_vert[edges[i].v1] = true;
					match_vert[edges[i].v2] = true;
				}
				done++;			
			}
			if(done==VertNum/2) break;
		}
		done = 0;
		bool[] job_done = new bool[VertNum];
		for(int i=0; i<VertNum; ++i) {
			job_done[i] = false;
		}
		for(int i=0; i<VertNum; i+=2) {
			tree_vert[i] = !match_vert[i];
		}
		

		while(done!=VertNum/2) {
			yield return new WaitForSeconds(0.02f);
			for(int i=0; i<Tight_edges.Count; ++i) {
				if(job_done[Tight_edges[i].v1]) continue;
				int v_e= Tight_edges[i].v1;
				disk[v_e].localScale = disk[v_e].localScale + new Vector3(0.16f,0.16f,0);
				if(disk[v_e].localScale.x*0.5f > Tight_edges[i].dist) {
					disk[v_e].localScale = new Vector3(Tight_edges[i].dist*2,Tight_edges[i].dist*2,0);
					job_done[Tight_edges[i].v1]=true;
					++done;
					Vector3[] t_V = { vert[Tight_edges[i].v1].position, vert[Tight_edges[i].v2].position };
					VectorLine VL = new VectorLine("tight_edges_"+(Tight_edges.Count+1), t_V, linematerial, width, LineType.Continuous);
					VL.textureScale = 2f;
					VL.SetColor(t_color);
					VL.Draw();
					tight_path.Add(VL);

					if(match_vert[Tight_edges[i].v1]) {
						Vector3[] t_V2 = { vert[Tight_edges[i].v1].position, vert[Tight_edges[i].v2].position };
						VectorLine VL2 = new VectorLine("match_edges_"+(Tight_edges.Count+1), t_V2, linematerial, m_width, LineType.Continuous);
						VL2.textureScale = 2f;
						VL2.SetColor(m_color);
						VL2.Draw();
						match_path.Add(VL2);
					}
				}
			}
		}
		Debug.Log(" :: Expanding_even done");
		yield return null;	
	}

	public IEnumerator adjustment_step() {
		//min delta를 찾자!
		//delta = cost(x,y) - px-py
		float delta_min=1000000000;
		int delta_even=-1;
		int delta_odd=-1;

		for(int i=0; i<VertNum; i+=2) {
			for(int j=1; j<VertNum; j+=2) {
				if(tree_vert[i]&&(!tree_vert[j])) { //if even is in T and odd is not in T
					float tt = Vector3.Distance(vert[i].position,vert[j].position)-disk[i].localScale.x*0.5f+disk[j].localScale.x*0.5f;
					//tt = dist(i,j) - evendual + odddual
					if(tt < delta_min) {
						//if is min
						delta_min	= tt;
						delta_even	= i;
						delta_odd	= j;
					}
				}
			}
		}
		float obj_dual = disk[delta_even].localScale.x*0.5f + delta_min;
		while(disk[delta_even].localScale.x*0.5f < obj_dual) {
			yield return new WaitForSeconds(0.02f);
			for(int i=0; i<VertNum; i+=2) {
				if(tree_vert[i]) {
					disk[i].localScale = disk[i].localScale + new Vector3(0.16f,0.16f,0);
				}
				if(tree_vert[i+1]) {
					disk[i+1].localScale = disk[i+1].localScale + new Vector3(0.16f,0.16f,0);
				}
			}
		}
		float error = obj_dual - disk[delta_even].localScale.x*0.5f;
		Vector3 err = new Vector3(error,error,0);
		for(int i=0; i<VertNum;++i) {
			if(tree_vert[i]) disk[i].localScale += err;
		}
		tight_vert[delta_even] = true;
		tight_vert[delta_odd] = true;
		select_odd = delta_odd;
		select_even = delta_even;
		
		if(delta_min > 0.0005f) {
			for(int i=0; i<edges.Count; ++i) {
				if(edges[i].v1==select_even && edges[i].v2==select_odd) {
					Tight_edges.Add(edges[i]);
					break;
				}
			}
		}
		yield return null;
	}

	public IEnumerator TreeGrowing() {
		//odd가 freeVertex가 아니였던거시다!
		//그러면 even이랑 odd가 tight가 됬으니 두 트리를 합치자!
		//이 경우 even이랑 odd는 사실 이미 tight인 상태!
		edge_info tt = new edge_info(select_even,select_odd,vert[select_even].position,vert[select_odd].position);
		//merge(select_even, select_odd);

		//odd가 freeVertex가 아니였다.
		//x는 이미 T에 속하니까 y랑 x`을 T에 추가하자.
		for(int i=0; i<Match_edges.Count; ++i) {
			if(Match_edges[i].v2==select_odd) {
				tree_vert[select_odd] = true;
				tree_vert[Match_edges[i].v1] = true;
				break;
			}
		}




		//선긋는 애니메이션 추가

		Vector3[] t_V = { vert[select_even].position, vert[select_odd].position };
		VectorLine VL = new VectorLine("tight_edges_"+(Tight_edges.Count+1), t_V, linematerial, width, LineType.Continuous);
		VL.textureScale = 2f;
		VL.SetColor(t_color);
		VL.Draw();
		tight_path.Add(VL);
		yield return null;
	}

	public IEnumerator AUG_Path() {
		// odd가 freeVertex였던 거시다. 매칭을 뚜쉬뚜쉬 추가하자
		// 1. even의 트리에서 freeVertex인 even'를 찾...자
		// 2. even'에서 even까지의 길을 찾자... (같은 T안에서의 tight edge의 집합)
		// 3. 기존의 M이랑 XOR을 갈기자
		// 4. even과 odd를 연결한 edge를 tight랑 match 양쪽다 추가하자
		// 5. 까먹지말고 트리도 합치자.
		///////////수정본
		// 1. T에서 free vertex인 even`을 찾자
		// 2. even`에서 even까지의 길을 찾자 (Path) + 이번 매칭을 추가하자
		// 3. M이랑 XOR을 하자
		// 4. match_vert를 수정하자


		//1
		/*
		int even_treeNum = find(select_even);
		int even_prime=-1;
		for(int i=0; i<VertNum; i+=2) {
			if(find(i)==even_treeNum) {
				Debug.Log(find(i) + " " +even_treeNum);
				if(!match_vert[i]) {
					even_prime = i;
					break;
				}
			}
		}*/
		/// new version
		int even_prime=-10;
		for(int i=0; i<VertNum; i+=2) {
			if(tree_vert[i] && !match_vert[i]) { //T에 속하면서 매칭은 아님
				even_prime = i;
				break;
			}
		}
		if(!match_vert[select_even]) even_prime = select_even;
		if(even_prime==-10) {
			Debug.Log("sSSIBBAL");
		}

		//2
		new_Path = new List<edge_info>();
		isPathExist = false;
		if (select_even != even_prime) {
			Debug.Log("find path from "+even_prime+" to "+select_even);
			findPath(even_prime,even_prime);
		}
		for(int i=0; i<Tight_edges.Count; ++i) {
			if(Tight_edges[i].v1==select_even && Tight_edges[i].v2==select_odd) {
				new_Path.Add(Tight_edges[i]);
				break;
			}
		}
		for(int i=0; i<new_Path.Count; ++i) {
			Debug.Log("asdfasdf : "+new_Path[i].v1+" "+new_Path[i].v2);
		}
				Debug.Log("prev Match is");
		for(int i=0; i<Match_edges.Count; ++i) {
			Debug.Log(Match_edges[i].v1+" "+Match_edges[i].v2);
		}
		Debug.Log("new Path is");
		for(int i=0; i<new_Path.Count; ++i) {
			Debug.Log(new_Path[i].v1+" "+new_Path[i].v2);
		}
		//3
		List<edge_info> XOR_Path = new List<edge_info>();
		for(int i=0; i<Match_edges.Count; ++i) {
			XOR_Path.Add(Match_edges[i]);
			for(int j=0; j<new_Path.Count; ++j) {
				if(new_Path[j].v1==Match_edges[i].v1 && new_Path[j].v2==Match_edges[i].v2) {
					new_Path.RemoveAt(j);
					XOR_Path.RemoveAt(XOR_Path.Count-1);
					break;
				}
			}
		}
		for(int i=0; i<new_Path.Count; ++i) {
			XOR_Path.Add(new_Path[i]);
		}
		//4
		match_vert[select_odd] = true;
		match_vert[select_even] = true;

		/*
		List<edge_info> new_match = new List<edge_info>();
		bool swit = true;
		if(even_prime==-1) {//no free vertex -> 같은 트리에 있는거 전부 뒤집어!
			Debug.Log("Tree do not have free even vertex!");
			for(int i=0; i<Tight_edges.Count; ++i) {
				if(find(Tight_edges[i].v1)==even_treeNum) {
					for(int j=0; j<Match_edges.Count;++j) {
						if((Tight_edges[i].v1==Match_edges[j].v1 && Tight_edges[i].v2==Match_edges[j].v2)) {
							swit=false; //Match인 edge체크
							break;
						}
					}
					if(swit) { //Match가 아니드라
						match_vert[Tight_edges[i].v1]=!match_vert[Tight_edges[i].v1];
						match_vert[Tight_edges[i].v2]=!match_vert[Tight_edges[i].v2];
						new_match.Add(Tight_edges[i]);
						Debug.Log("free vertex가 없었당");
					}
				}
			}
			if(new_match.Count==0) {
				//트리에 타이트엣지가 하나밖에 없었던 특수케이수!
				for(int i=0; i<VertNum;++i) {
					if(find(i)==even_treeNum) {
						match_vert[i]=!match_vert[i];
					}
				}
			}
		}
		else {	//yes freevertex -> even_prime부터 시작하는 path를 찾자
			Debug.Log("Yes Free VerteX!!!!");
			if( even_prime==select_even ) {
				//예외처리
				Debug.Log("free vertex가 select_even이였넹");
				for(int i=0; i<edges.Count; ++i) {
					if(edges[i].v1 == select_even && edges[i].v2 == select_odd) {
						Tight_edges.Add(edges[i]); //tight 추가
						match_vert[select_even] = true;
						match_vert[select_odd] = true;
						tight_vert[select_even] = true;
						tight_vert[select_odd] = true;
					}	
				}
				for(int i=0; i<Match_edges.Count; ++i) {
					new_match.Add(Match_edges[i]);
				}
			}
			else {
			int now=even_prime;
			match_vert[even_prime]=!match_vert[even_prime];
			int check=0;
			edge_info onEdge;
			int q=0;
			while(true) {
				q++;
				if(q<1000) break;
				if(now==select_even) break;
				if(check%2==0) {  // match가 아닌 tight를 찾자 (한쪽점이 now. 이때 now는 even
					for(int i=0; i<Tight_edges.Count; ++i) {
						if(Tight_edges[i].v1==now) { //한쪽이 now인 타이트를 찾았다
							for(int j=0; j<Match_edges.Count;++j) { 
								if(!(Tight_edges[i].v1==Match_edges[j].v1 && Tight_edges[i].v2==Match_edges[j].v2)) {
									//한쪽이 now이면서 Match가 아닌거 찾았당
									new_match.Add(Tight_edges[i]);
									Debug.Log("free vertex가 있었당");
									match_vert[now] = !match_vert[now];
									now = Tight_edges[i].v2;
									++check;
									break;
								}
							}
							if(check%2==1) break;
						}
					}
				}
				else { //이번엔 한쪽이 now인 match를 찾자. 이때 now는 odd
					for(int i=0; i<Match_edges.Count; ++i) {
						if(Match_edges[i].v2==now) {//찾았땅
							now=Match_edges[i].v1;
							++check;
							break;
						}
					}
				}
			}
			}
		}
		if(even_prime!=select_even) {
			for(int i=0; i<Match_edges.Count;++i) { //여기는 T가 아닌애들 추가
				if(find(Match_edges[i].v1)!=even_treeNum) {
					new_match.Add(Match_edges[i]);
					Debug.Log("add other tree matching");
				}
			}
		}
		*/
		/*
		//4
		for(int i=0; i<edges.Count;++i) {
			if(edges[i].v1 == select_even && edges[i].v2 == select_odd) {
				new_match.Add(edges[i]);
			}
		}
		//5
		match_vert[select_even]=true;
		match_vert[select_odd]=true;
		merge(select_even, select_odd);
*/
		//애니메이션 (줄이 사라지고, 생기고)

		Debug.Log("XOR result");
		for(int i=0; i<XOR_Path.Count; ++i) {
			Debug.Log(XOR_Path[i].v1+" "+XOR_Path[i].v2);
		}
		Debug.Log("end XOR result");
		List<VectorLine> tVL = new List<VectorLine>();
		for(int i=0; i<XOR_Path.Count; ++i) {
			Vector3[] t_V = { vert[XOR_Path[i].v1].position, vert[XOR_Path[i].v2].position };
			VectorLine VL = new VectorLine("match_edges_"+(XOR_Path.Count+1), t_V, linematerial, m_width, LineType.Continuous);
			VL.textureScale = 2f;
			VL.SetColor(m_color);
			VL.Draw();
			tVL.Add(VL);
		}
		for(int i=0; i<match_path.Count;++i) {
			VectorLine.Destroy(match_path);
		}
		match_path.Clear();
		match_path = tVL;
		//
		Match_edges = XOR_Path;
		yield return null;
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
//		Debug.Log(a+" "+b);
		if(a==b) return false;
		uf_index[b] = a;
		return true;
	}

	public void findPath(int prev, int curr) {
		if(curr==select_even) {
			Debug.Log(" reach to destination");
			isPathExist = true;
			return;
		}

		if(curr%2==0) {
			for(int i=0; i<Tight_edges.Count; ++i) {
				if(Tight_edges[i].v1 == curr && Tight_edges[i].v2!=prev && tree_vert[Tight_edges[i].v2]) {
					new_Path.Add(Tight_edges[i]);
					findPath(Tight_edges[i].v1, Tight_edges[i].v2);
					if(isPathExist) return;
				}
			}
		}
		else {
			for(int i=0; i<Tight_edges.Count; ++i) {
				if(Tight_edges[i].v2 == curr && Tight_edges[i].v1!=prev && tree_vert[Tight_edges[i].v1]) {
					new_Path.Add(Tight_edges[i]);
					findPath(Tight_edges[i].v2, Tight_edges[i].v1);
					if(isPathExist) return;
				}
			}
		}
		new_Path.RemoveAt(new_Path.Count-1);
		return;
	}

}
