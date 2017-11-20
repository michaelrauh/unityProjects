using UnityEngine;
using System.Collections;

public class shallow_wave : MonoBehaviour {
	int size;
	float[,] old_h = new float[64,64];
	float[,] h = new float[64,64];
	float[,] new_h = new float[64,64];


	// Use this for initialization
	void Start () {
		size = 64;

	
		//Resize the mesh into a size*size grid
		Mesh mesh = GetComponent<MeshFilter> ().mesh;
		mesh.Clear ();
		Vector3[] vertices=new Vector3[size*size];
		for (int i=0; i<size; i++)
		for (int j=0; j<size; j++) 
		{
			vertices[i*size+j].x=i*0.2f-size*0.1f;
			vertices[i*size+j].y=0;
			vertices[i*size+j].z=j*0.2f-size*0.1f;
		}
		int[] triangles = new int[(size - 1) * (size - 1) * 6];
		int index = 0;
		for (int i=0; i<size-1; i++)
		for (int j=0; j<size-1; j++)
		{
			triangles[index*6+0]=(i+0)*size+(j+0);
			triangles[index*6+1]=(i+0)*size+(j+1);
			triangles[index*6+2]=(i+1)*size+(j+1);
			triangles[index*6+3]=(i+0)*size+(j+0);
			triangles[index*6+4]=(i+1)*size+(j+1);
			triangles[index*6+5]=(i+1)*size+(j+0);
			index++;
		}
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.RecalculateNormals ();
	


	}

	void Shallow_Wave()
	{	
				float rate = 0.005f;
				float damping = 0.999f;

				for (int i = 0; i<size; i++) {
						for (int j = 0; j< size; j++) {
								new_h [i, j] = h [i, j] + (h [i, j] - old_h [i, j]) * damping;

								if (i != 0) {
										new_h [i, j] += (h [i - 1, j] - h [i, j]) * rate;
								}
								if (j != 0) {
										new_h [i, j] += (h [i, j - 1] - h [i, j]) * rate;
								}
								if (i != size - 1) {
										new_h [i, j] += (h [i + 1, j] - h [i, j]) * rate;
								}
								if (j != size - 1) {
										new_h [i, j] += (h [i, j + 1] - h [i, j]) * rate;
								}

						}
				}
				for (int i = 0; i<size; i++) {
						for (int j = 0; j< size; j++) {
								old_h [i, j] = h [i, j];
								h [i, j] = new_h [i, j];
						}
				}
		}

	// Update is called once per frame
	void Update () 
	{
		Mesh mesh = GetComponent<MeshFilter> ().mesh;
		Vector3[] vertices = mesh.vertices;

		//Step 1: Copy vertices.y into h
		for (int i = 0; i < size; i++) {
			for (int j = 0 ;j<size;j++)
			{
				h[i,j] = vertices[i * size + j].y;
			}
				}

		//Step 2: User interaction
		if (Input.GetKeyDown (KeyCode.R)) {
			h[Random.Range(0,size-1),Random.Range (0,size-1)] += Random.Range(.05f,.1f);
				}
	
		//Step 3: Run Shallow Wave
		for (int iter = 0; iter<8; iter++) {
			Shallow_Wave();
				}

		//Step 4: Copy h back into mesh
		for (int i = 0; i < size; i++) {
			for (int j = 0 ;j<size;j++)
			{
				vertices[i * size + j].y = h[i,j];
			}
		}

		mesh.vertices = vertices;
		mesh.RecalculateNormals ();

	}
}
