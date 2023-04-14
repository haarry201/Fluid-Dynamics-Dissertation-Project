using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Painter : MonoBehaviour
{
    private Texture2D Image;
    private int progression;
    private Fluid fluid;
    float scale;
    public GameObject plane;
    public int N = 64;
    Vector3 lastpos;
    Vector3 delta;
    public int dAmount;

    public int iterations;






    void Start()
    {
        scale = (N/2f) / 4.97f;
        fluid = new Fluid(0.000008f, 0.000001f, 0.2f, N, iterations);
        this.Image = new Texture2D(N, N, TextureFormat.RGBA32, false);
        GetComponent<Renderer>().material.SetTexture("_BaseMap", this.Image);
        lastpos = Input.mousePosition;

        fluid.TestSolver();


        
    }


    void Update()
    {

        Vector3 mouse = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Input.mousePosition.z);
        Ray ray;
        ray = Camera.main.ScreenPointToRay(mouse);
        RaycastHit hit;

        delta = Input.mousePosition - lastpos;

        float centre = (0.5f * N);
        int centreInt = (int)centre;
        fluid.AddDensity(centreInt, centreInt, UnityEngine.Random.Range(1, 10));

        float amtX = UnityEngine.Random.Range(-15,15);
        float amtY = UnityEngine.Random.Range(-15,15);
        amtX *= 0.1f;
        amtY *= 0.1f;
        fluid.AddVelocity(centreInt, centreInt, amtX, amtY);



        // if (Physics.Raycast(ray, out hit, 10))
        // {
        //     Vector3 localPoint = plane.transform.InverseTransformPoint(hit.point);

        //     localPoint.x *= scale;
        //     localPoint.z *= scale;

        //     localPoint.x = (int)localPoint.x;
        //     localPoint.z = (int)localPoint.z;

        //     localPoint.x += N/2;
        //     localPoint.z += N/2;

        //     if (localPoint.x >= N) localPoint.x = N;
        //     if (localPoint.z >= N) localPoint.z = N;

        //     if (localPoint.x <= 0) localPoint.x = 0;
        //     if (localPoint.z <= 0) localPoint.z = 0;

        //     localPoint.x = N - localPoint.x;
        //     localPoint.z = N - localPoint.z;

            



        //     //fluid.AddDensity((int)localPoint.x, (int)localPoint.z, dAmount);



        //     if (delta.x <= -N) delta.x = -N + 1;
        //     if (delta.y <= -N) delta.y = -N + 1;

        //     if (delta.x >= N) delta.x = N - 1;
        //     if (delta.y >= N) delta.y = N - 1;

        //     delta.x = 0 - delta.x;
        //     delta.y = 0 - delta.y;

        //     delta.x /= 15;
        //     delta.y /= 15;

            
            
        //     //fluid.AddVelocity((int)localPoint.x, (int)localPoint.z, delta.x, delta.y);

        // }


        fluid.Step();
        fluid.RenderD(this.Image);
        //fluid.fade(40);
        lastpos = Input.mousePosition;

        /*
        for (int i = 0; i < ival; i++)
        {
 
            var x = Random.Range(0, res);
            var y = Random.Range(0, res);

            this.Image.SetPixel(x, y, Random.ColorHSV(0f, 1f, 0f, 0f, 0f, 1f, 0f, 1f));
        }
       */
    }

    // void FixedUpdate()
    // {
    //     fluid.Step();
    //     fluid.RenderD(this.Image);
    // }



}
