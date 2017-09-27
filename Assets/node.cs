using UnityEngine;
using System.Collections;

public class node {

	public int index;
	public int x;
	public int y;
	public node parent;
	
	
	public node(int i, int xx, int yy) {
		index = i;
		x = xx;
		y = yy;
		parent = null;
	}
	
	public node(int i, int xx, int yy, node pp) {
		index = i;
		x = xx;
		y = yy;
		parent = pp;
	}
	
	public node find() {
		if(parent==null) return this;
		node pp = parent.find ();
		return pp;	
	}

}
