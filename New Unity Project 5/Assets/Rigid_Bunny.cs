using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Rigid_Bunny: MonoBehaviour {
    
    public float gravity; //Gravity coefficient
    public float gravityForce; //Downward force
    public Vector3 x; //position
    public Vector3 v=new Vector3(0, 0, 0); //Velocity
    public Quaternion q = Quaternion.identity; //Quaternion
    public Vector3 w=new Vector3(2, 0, 0); //Angular velocity
    
    public Vector3 j = new Vector3 (); //Impulse
    
    public float m; //Mass per vertex
    
    public float mass; //Mass
    public float inv_mass; //1/mass
    public Matrix4x4 I_body; //Body inertia
    public Matrix4x4 inv_I_body; //(body inertia)^{-1}
    
    public float damping; //Abstract frictional force
    public float restitution; //collision

    public Vector3[] vertices; //All vertices
    bool hasCollision; //True if a vertex is below plane
    
    // Use this for initialization
    void Start () 
    {
        //Initialize coefficients
        gravity = -9.8f;
        w = new Vector3 (0, 0, 2);
        x = new Vector3 (0, 0.6f, 0);
        q = Quaternion.identity;
        damping = 0.96f;
        restitution = 0.5f;//0.5f;  //elastic collision
        m = 1;
        mass = 0; 
        
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;
        for (int i=0; i<vertices.Length; i++)
        {
            mass += m;
            float diag=m*vertices[i].sqrMagnitude;
            I_body[0, 0]+=diag;
            I_body[1, 1]+=diag;
            I_body[2, 2]+=diag;
            I_body[0, 0]-=m*vertices[i][0]*vertices[i][0];
            I_body[0, 1]-=m*vertices[i][0]*vertices[i][1];
            I_body[0, 2]-=m*vertices[i][0]*vertices[i][2];
            I_body[1, 0]-=m*vertices[i][1]*vertices[i][0];
            I_body[1, 1]-=m*vertices[i][1]*vertices[i][1];
            I_body[1, 2]-=m*vertices[i][1]*vertices[i][2];
            I_body[2, 0]-=m*vertices[i][2]*vertices[i][0];
            I_body[2, 1]-=m*vertices[i][2]*vertices[i][1];
            I_body[2, 2]-=m*vertices[i][2]*vertices[i][2];
        }
        I_body [3, 3] = 1;
        inv_I_body = I_body.inverse;
        inv_mass = 1 / mass;
        hasCollision = false;
    }
    
    Matrix4x4 Get_Cross_Matrix(Vector3 a)
    {
        //Get the cross product matrix of vector a
        Matrix4x4 A = Matrix4x4.zero;
        A [0, 0] = 0; 
        A [0, 1] = -a [2]; 
        A [0, 2] = a [1]; 
        A [1, 0] = a [2]; 
        A [1, 1] = 0; 
        A [1, 2] = -a [0]; 
        A [2, 0] = -a [1]; 
        A [2, 1] = a [0]; 
        A [2, 2] = 0; 
        A [3, 3] = 1;
        return A;
    }
    
    Matrix4x4 Get_Rotation_Matrix(Quaternion q)
    {
        //Get the rotation matrix R from quaternion q
        Matrix4x4 R = Matrix4x4.zero;
        R[0, 0]=q[3]*q[3]+q[0]*q[0]-q[1]*q[1]-q[2]*q[2];
        R[0, 1]=2*(q[0]*q[1]-q[3]*q[2]);
        R[0, 2]=2*(q[0]*q[2]+q[3]*q[1]);
        R[1, 0]=2*(q[0]*q[1]+q[3]*q[2]);
        R[1, 1]=q[3]*q[3]-q[0]*q[0]+q[1]*q[1]-q[2]*q[2];
        R[1, 2]=2*(q[1]*q[2]-q[3]*q[0]);
        R[2, 0]=2*(q[0]*q[2]-q[3]*q[1]);
        R[2, 1]=2*(q[1]*q[2]+q[3]*q[0]);
        R[2, 2]=q[3]*q[3]-q[0]*q[0]-q[1]*q[1]+q[2]*q[2];
        R[3, 3]=1;
        return R;
    }
    
    Quaternion mult (Quaternion a, Quaternion b)
    {
        //Multiply a and b
        Vector3 vone = new Vector3(a.x, a.y, a.z);
        Vector3 vtwo = new Vector3(b.x, b.y, b.z);
        float sone = a.w;
        float stwo = b.w;
        Vector3 vthree = sone * vtwo + stwo * vone + Vector3.Cross(vone,vtwo);
        float sthree = sone * stwo - Vector3.Dot(vone, vtwo); 
        
        return new Quaternion(vthree.x, vthree.y, vthree.z, sthree);
    }
    
    Quaternion add (Quaternion a, Quaternion b)
    {
        //Add a and b
        return new Quaternion(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w);
    }
    
    Quaternion scale (Quaternion a, float s)
    {
        //Multiply a by scalar s
        return new Quaternion(s * a.x, s * a.y, s * a.z, s * a.w);
    }

    Vector3 avgVertex (Vector3[] vertices)
    {
        //Find average position of colliding vertices
        Vector3 total = new Vector3(0, 0, 0);
        int count = 0;
        
        foreach (Vector3 vertex in vertices)
        {
            var transformed = transform.TransformPoint(vertex); // convert to x + r
            if (transformed.y < 0)
            {
                total += transformed - x; // subtract x off and add to total
                count++;
            }
        }

        //If more than 0 points fall below the plane, it is colliding
        if (count > 0)
        {
            hasCollision = true;
        } 
        else
        {
            hasCollision = false;
        }

        return total/count;
    }
    
    Matrix4x4 subtract (Matrix4x4 a,Matrix4x4 b)
    {
        //subtract a from b
        Matrix4x4 c = new Matrix4x4();
        c.m00 = a.m00 - b.m00;
        c.m01 = a.m01 - b.m01;
        c.m02 = a.m02 - b.m02;
        c.m03 = a.m03 - b.m03;
        c.m10 = a.m10 - b.m10;
        c.m11 = a.m11 - b.m11;
        c.m12 = a.m12 - b.m12;
        c.m13 = a.m13 - b.m13;
        c.m20 = a.m20 - b.m20;
        c.m21 = a.m21 - b.m21;
        c.m22 = a.m22 - b.m22;
        c.m23 = a.m23 - b.m23;
        c.m30 = a.m30 - b.m30;
        c.m31 = a.m31 - b.m31;
        c.m32 = a.m32 - b.m32;
        c.m33 = a.m33 - b.m33;
        
        return c;
    }
    
    // Get average, detect if it shows a collision, and respond
    void handleCollisions(Vector3[] vertices)
    {
        Vector3 avg = avgVertex(vertices); //Average colliding vector
        Vector3 vAvg = v + Vector3.Cross(w, avg); //Apply velocity to this

        if (hasCollision && vAvg.y < 0) //Colliding and heading downward
        {
            if (Mathf.Abs(vAvg.y) < 0.35f) 
            {
                restitution = 0;
            }

            var rStar = Get_Cross_Matrix(avg); //Given
            var R = Get_Rotation_Matrix(q); //Given
            
            var I = R * I_body * R.transpose; //Equation 16
            var identity = Matrix4x4.identity; //The identity matrix
            identity[3,3] = 0;
            var scalar = Matrix4x4.Scale(new Vector3 (inv_mass,inv_mass,inv_mass)); // Create the 1/m * 1 matrix
            var K = subtract(scalar * identity,rStar * I.inverse * rStar); // equation 25
            
            //Calculate impulse
            Vector3 rest_vector = new Vector3(0,-restitution * vAvg.y,0); //make vector with restitution
            j = K.inverse * (rest_vector - vAvg); // equation 28

            //Apply impulse to v, w
            var j3 = new Vector3(j.x,j.y,j.z);
            v += (inv_mass * j3);
            var imp = Vector3.Cross(avg,j3);
            var deltaw = I.inverse * imp;
            var deltaw3 = new Vector3(deltaw.x,deltaw.y,deltaw.z);
            w += deltaw3;
            
            restitution = 0.5f; //Reset restitution
        }
    }
    
    // Update is called once per frame
    void Update () 
    {
        float t = 0.02f;
        
        //Application of forces
        gravityForce = mass * gravity;
        
        if (Input.GetKey("a"))
        { 
            // If a is pressed, move it upward
            x.y += 0.05f;
        } else
        {
            //Apply gravity and damping
            v.y += (t / mass) * gravityForce;
            v *= damping;
            w *= damping;
            
            //Update based upon collisions
            handleCollisions(vertices);
            
            // Update based upon forces
            x += (v * t); // Update position with velocity
            q = add(q, (scale(mult(new Quaternion(w.x, w.y, w.z, 0), q), .5f * t))); // Update rotation based upon equation 19
        }
        
        //Apply final transformations
        transform.position = x; //apply to position
        transform.rotation = q; //apply to rotation
        
    }
}
