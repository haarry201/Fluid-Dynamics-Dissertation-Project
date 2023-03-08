using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GPUPainter : MonoBehaviour
{
    private Texture2D Image;
    private int progression;
    float scale;
    public GameObject plane;
    public int N = 64;
    Vector3 lastpos;
    Vector3 delta;
    public int dAmount;




    void Start()
    {
        scale = (N / 2f) / 4.97f;
        this.Image = new Texture2D(N, N, TextureFormat.RGBA32, false);
        GetComponent<Renderer>().material.SetTexture("_BaseMap", this.Image);
        lastpos = Input.mousePosition;
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

            localPoint.x += N / 2;
            localPoint.z += N / 2;

            if (localPoint.x >= N) localPoint.x = N;
            if (localPoint.z >= N) localPoint.z = N;

            if (localPoint.x <= 0) localPoint.x = 0;
            if (localPoint.z <= 0) localPoint.z = 0;

            localPoint.x = N - localPoint.x;
            localPoint.z = N - localPoint.z;

            //fluid.AddDensity((int)localPoint.x, (int)localPoint.z, dAmount);


            if (delta.x <= -N) delta.x = -N + 1;
            if (delta.y <= -N) delta.y = -N + 1;

            if (delta.x >= N) delta.x = N - 1;
            if (delta.y >= N) delta.y = N - 1;

            delta.x = 0 - delta.x;
            delta.y = 0 - delta.y;

            delta.x /= 15;
            delta.y /= 15;

            //fluid.AddVelocity((int)localPoint.x, (int)localPoint.z, delta.x, delta.y);

        }


        //fluid.Step();
        //fluid.RenderD(this.Image);
        lastpos = Input.mousePosition;

        
    }



}
