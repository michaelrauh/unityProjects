using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class cloth_motion: MonoBehaviour
{

    float         t;              // The time step
    Vector3[]     velocities;     // The velocity array
    float     damping;        // The damping multiplier coefficient
    int[]         edge_list;      // The edge list
    float[]   L0;             // The edge rest length list


    // Use this for initialization
    void Start()
    {
        t = 0.075f;
        damping = 0.99f;

        Mesh mesh = GetComponent<MeshFilter>().mesh;
        int[] triangles = mesh.triangles;
        Vector3[] vertices = mesh.vertices;

        //Construct the original edge list
        int[] original_edge_list = new int[triangles.Length * 2];
        for (int i=0; i<triangles.Length; i+=3)
        {
            original_edge_list [i * 2 + 0] = triangles [i + 0];
            original_edge_list [i * 2 + 1] = triangles [i + 1];
            original_edge_list [i * 2 + 2] = triangles [i + 1];
            original_edge_list [i * 2 + 3] = triangles [i + 2];
            original_edge_list [i * 2 + 4] = triangles [i + 2];
            original_edge_list [i * 2 + 5] = triangles [i + 0];
        }
        //Reorder the original edge list
        for (int i=0; i<original_edge_list.Length; i+=2)
            if (original_edge_list [i] > original_edge_list [i + 1]) 
                Swap(ref original_edge_list [i], ref original_edge_list [i + 1]);
        //Sort the original edge list using quicksort
        Quick_Sort(ref original_edge_list, 0, original_edge_list.Length / 2 - 1);

        int count = 0;
        for (int i=0; i<original_edge_list.Length; i+=2)
            if (i == 0 || 
                original_edge_list [i + 0] != original_edge_list [i - 2] ||
                original_edge_list [i + 1] != original_edge_list [i - 1]) 
                count++;

        edge_list = new int[count * 2];
        int r_count = 0;
        for (int i=0; i<original_edge_list.Length; i+=2)
            if (i == 0 || 
                original_edge_list [i + 0] != original_edge_list [i - 2] ||
                original_edge_list [i + 1] != original_edge_list [i - 1])
            {
                edge_list [r_count * 2 + 0] = original_edge_list [i + 0];
                edge_list [r_count * 2 + 1] = original_edge_list [i + 1];
                r_count++;
            }


        L0 = new float[edge_list.Length / 2];
        for (int e=0; e<edge_list.Length/2; e++)
        {
            int v0 = edge_list [e * 2 + 0];
            int v1 = edge_list [e * 2 + 1];
            L0 [e] = (vertices [v0] - vertices [v1]).magnitude;
        }

        velocities = new Vector3[vertices.Length];
        for (int v=0; v<vertices.Length; v++)
            velocities [v] = new Vector3(0, 0, 0);
    }

    void Quick_Sort(ref int[] a, int l, int r)
    {
        int j;
        if (l < r)
        {
            j = Quick_Sort_Partition(ref a, l, r);
            Quick_Sort(ref a, l, j - 1);
            Quick_Sort(ref a, j + 1, r);
        }
    }

    int  Quick_Sort_Partition(ref int[] a, int l, int r)
    {
        int pivot_0, pivot_1, i, j;
        pivot_0 = a [l * 2 + 0];
        pivot_1 = a [l * 2 + 1];
        i = l;
        j = r + 1;
        while (true)
        {
            do
                ++i; while( (a[i*2]<pivot_0 || a[i*2]==pivot_0 && a[i*2+1]<=pivot_1) && i<=r);
            do
                --j; while(  a[j*2]>pivot_0 || a[j*2]==pivot_0 && a[j*2+1]> pivot_1);
            if (i >= j)
                break;
            Swap(ref a [i * 2], ref a [j * 2]);
            Swap(ref a [i * 2 + 1], ref a [j * 2 + 1]);
        }
        Swap(ref a [l * 2 + 0], ref a [j * 2 + 0]);
        Swap(ref a [l * 2 + 1], ref a [j * 2 + 1]);
        return j;
    }

    void Swap(ref int a, ref int b)
    {
        int temp = a;
        a = b;
        b = temp;
    }

    // Update is called once per frame
    void Update()
    {
        // Use t as the time step (Delta t).
        // Edge_list and L0 have both been created already.
        // Your job is to finish the rest of the code.

        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;
        
        var gravityVector = new Vector3(0f, -9.8f, 0f);
        //Step 1: Formulate the simulation as a basic particle system with the gravity acceleration only.
        for (int current = 1; current<vertices.Length; current++)
        {
            if (current != 10)
            {
                velocities [current] += gravityVector * t;
                velocities [current] *= damping;
                vertices [current] += (velocities [current] * t);
            }
        }
    
        //Step 2: Apply the strain limiting method as a constraint
        //You may apply it multiple times here. (64 times, for example)
        for (int n = 0; n<100; n++)
        {
            //create temp_x, temp_n, fill them with zeros
            List<Vector3> temp_x = new List<Vector3>();
            var temp_n = new List<int>(edge_list.Length);

            for (int k = 0;k<edge_list.Length;k++)
            {
                temp_x.Add(new Vector3());
            }

            for (int k = 0;k<edge_list.Length;k++)
            {
                temp_n.Add(0);
            }

            //For each edge
            for (int j = 0; j<edge_list.Length; j+=2)
            {
                //compute xinew,xjnew
                Vector3 xi = vertices [edge_list [j]];
                Vector3 xj = vertices [edge_list [j + 1]];
                float Lo = L0 [j / 2];
                Vector3 xinew = ((xi + xj) + (Lo * (xi - xj) / (xi - xj).magnitude)) / 2f;
                Vector3 xjnew = ((xi + xj) + (Lo * (xj - xi) / (xi - xj).magnitude)) / 2f;

                //Add into temp x
                temp_x [edge_list[j]]+=xinew;
                temp_n [edge_list[j]]++;
                temp_x [edge_list[j + 1]]+=xjnew;
                temp_n [edge_list[j + 1]]++;
            }
            //apply changes
            for (int i =1; i<vertices.Length;i++) //foreach vertex i not fixed
            {
                if (i != 10)
                {
                    Vector3 xinew = ((.2f * vertices[i] + temp_x[i])/(.2f + temp_n[i]));
                    velocities[i] = velocities[i] + (xinew - vertices[i])/t;
                    vertices[i] = xinew;
                }
            }
        }
    
        //Step 3: Apply sphere-vertex collision as a constraint
        Vector3 c = GameObject.Find("Sphere").transform.position;
        float r = 2.7f;
    
        for (int current = 1; current<vertices.Length; current++)
        {
            if (current != 10)
            {
                Vector3 p = vertices [current];
                if ((p - c).magnitude < r)
                {
                    p = c + r * ((p - c) / (p - c).magnitude);
                    vertices [current] = p;
                    velocities [current] = Vector3.zero;
                }
            }
        }
    
        mesh.vertices = vertices;
        mesh.RecalculateNormals();
    }
}