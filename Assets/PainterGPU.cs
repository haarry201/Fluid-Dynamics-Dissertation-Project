using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PainterGPU : MonoBehaviour
{
    private Texture2D Image;
    private int progression;
    private FluidGPU fluid;
    float scale;
    public GameObject plane;
    public int N;
    Vector3 lastpos;
    Vector3 delta;
    public int dAmount;

    public int densityWidth;

    public int iterations;


    //GPU STUFF
    public ComputeShader shader;
    public RenderTexture tex;




    void Start()
    {
        scale = (N/2f) / 4.97f;
        fluid = new FluidGPU(0.000008f, 0.000001f, 0.2f, N, iterations);
        this.Image = new Texture2D(N, N, TextureFormat.RGBA32, false);
        GetComponent<Renderer>().material.SetTexture("_BaseMap", this.Image);
        lastpos = Input.mousePosition;


        //GPU STUFF
        tex = new RenderTexture(N, N, 24);
        tex.enableRandomWrite = true;
        tex.Create();

        shader.SetFloat("resolution", N);
        shader.SetInt("N", N);

        //fluid.TestSolver(shader);


    }


    void Update()
    {

        Vector3 mouse = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Input.mousePosition.z);
        Ray ray;
        ray = Camera.main.ScreenPointToRay(mouse);
        RaycastHit hit;

        delta = Input.mousePosition - lastpos;

        if (Physics.Raycast(ray, out hit, 10))
        {
            Vector3 localPoint = plane.transform.InverseTransformPoint(hit.point);

            localPoint.x *= scale;
            localPoint.z *= scale;

            localPoint.x = (int)localPoint.x;
            localPoint.z = (int)localPoint.z;

            localPoint.x += N/2;
            localPoint.z += N/2;

            if (localPoint.x >= N) localPoint.x = N;
            if (localPoint.z >= N) localPoint.z = N;

            if (localPoint.x <= 0) localPoint.x = 0;
            if (localPoint.z <= 0) localPoint.z = 0;

            localPoint.x = N - localPoint.x;
            localPoint.z = N - localPoint.z;

            fluid.AddDensity((int)localPoint.x, (int)localPoint.z, dAmount, densityWidth);



            if (delta.x <= -N) delta.x = -N + 1;
            if (delta.y <= -N) delta.y = -N + 1;

            if (delta.x >= N) delta.x = N - 1;
            if (delta.y >= N) delta.y = N - 1;

            delta.x = 0 - delta.x;
            delta.y = 0 - delta.y;

            delta.x /= 15;
            delta.y /= 15;
            
            fluid.AddVelocity((int)localPoint.x, (int)localPoint.z, delta.x, delta.y);

        }

        fluid.Step(shader);
        fluid.RenderD(this.Image, shader, tex);
        
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

    void FixedUpdate()
    {
        // fluid.Step(shader);
        // fluid.RenderD(this.Image, shader, tex);
    }



}


