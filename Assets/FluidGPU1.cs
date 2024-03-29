using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FluidGPU1 {

    private Texture2D Image;

    float dt;
    float diff;
    float visc;
    float[] s;
    float[] density;
    float[] Vx;
    float[] Vy;
    float[] Vx0;
    float[] Vy0;

    int N, iter;





    int IX(int x, int y)
    {
        if (x >= N) x = N - 1;
        if (y >= N) y = N - 1;

        if (x <=0) x = 0;
        if (y <=0) y = 0;

        return x + y * N;
    }

    public FluidGPU1(float diffusion, float viscosity, float dt, int size, int iterations)
    {
        N = size;
        iter = iterations;
        this.dt = dt;
        this.diff = diffusion;
        this.visc = viscosity;
        this.s = new float[N * N];
        this.density = new float[N * N];
        this.Vx = new float[N * N];
        this.Vy = new float[N * N];
        this.Vx0 = new float[N * N];
        this.Vy0 = new float[N * N];
    }

    public void Step(ComputeShader shader) 
    {
        
        
        float visc = this.visc;
        float diff = this.diff;
        float dt = this.dt;
        float[] Vx = this.Vx;
        float[] Vy = this.Vy;
        float[] Vx0 = this.Vx0;
        float[] Vy0 = this.Vy0;
        float[] s = this.s;
        float[] density = this.density;
        Texture2D Image = this.Image;

        Diffuse(1, Vx0, Vx, visc, dt, iter, shader);
        Diffuse(2, Vy0, Vy, visc, dt, iter, shader);

        Project(Vx0, Vy0, Vx, Vy, iter, shader);

        Advect(1, Vx, Vx0, Vx0, Vy0, dt, shader);
        Advect(2, Vy, Vy0, Vx0, Vy0, dt, shader);

        Project(Vx, Vy, Vx0, Vy0, iter, shader);
        Diffuse(0, s, density, diff, dt, iter, shader);
        Advect(0, density, s, Vx, Vy, dt, shader);
    }




    public void RenderD(Texture2D Image, ComputeShader shader, RenderTexture tex)
    {

        for (int i = 0; i < N; i++)
        {
            for (int j = 0; j < N; j++)
            {   
                int x = (int)i;
                int y = (int)j;
                float d = this.density[IX(i, j)];
                //if(d>0f){
                    //Color color = new Color(1/(3*d), 1/(4*d), 1/d, d);
                    Color color = new Color(d, d, d, d);
                //}
                // else{
                //     Color color = new Color(0, 0, 0, 0);
                // }

                
                Image.SetPixel(x, y, color);
            }
        }
        Image.Apply();


    }


    public void AddDensity(int x, int y, float amount, int densityWidth)
    {
        
        int index = IX(x, y);
        this.density[index] += amount;
        // for (int i = 0; i < densityWidth; i++){
        //     this.density[IX(x+i, y)] += amount;
        //     this.density[IX(x-i, y)] += amount;
        //     this.density[IX(x, y+i)] += amount;
        //     this.density[IX(x, y-i)] += amount;

        //     this.density[IX(x+i, y-i)] += amount;
        //     this.density[IX(x+i, y+i)] += amount;
        //     this.density[IX(x-i, y-i)] += amount;
        //     this.density[IX(x-i, y+i)] += amount;
        // }
        
    }

    public void AddVelocity(int x, int y, float amountX, float amountY)
    {
        
        int index = IX(x, y);
        this.Vx[index] += amountX;
        this.Vy[index] += amountY;
        
    }

    void Diffuse(int b, float[] x, float[] x0, float diff, float dt, int iter, ComputeShader shader)
    {
        int kernelHandle = shader.FindKernel("Diffuse0");
        if (b == 1){ kernelHandle = shader.FindKernel("Diffuse1");}
        if (b == 2){ kernelHandle = shader.FindKernel("Diffuse2");}

        shader.SetInt("N", N);

        ComputeBuffer x0Buffer = new ComputeBuffer(x0.Length, 4);
        x0Buffer.SetData(x0);
        shader.SetBuffer(kernelHandle, "x0", x0Buffer);

        ComputeBuffer xBuffer = new ComputeBuffer(x.Length, 4);
        xBuffer.SetData(x);
        shader.SetBuffer(kernelHandle, "x", xBuffer);

        shader.SetInt("b", b);
        shader.SetFloat("diff", diff);
        shader.SetFloat("dt", dt);
        shader.SetInt("iter", iter);

        shader.Dispatch(kernelHandle, (N/16)+1, (N/16)+1, 1);

        x0Buffer.GetData(x0);
        xBuffer.GetData(x);

        x0Buffer.Dispose();
        xBuffer.Dispose();

        x[IX(0, 0)] = 0.5f * (x[IX(1, 0)] + x[IX(0, 1)]);
        x[IX(0, N - 1)] = 0.5f * (x[IX(1, N - 1)] + x[IX(0, N - 2)]);
        x[IX(N - 1, 0)] = 0.5f * (x[IX(N - 2, 0)] + x[IX(N - 1, 1)]);
        x[IX(N - 1, N - 1)] = 0.5f * (x[IX(N - 2, N - 1)] + x[IX(N - 1, N - 2)]);
    }


        public void lin_solve(int b, float[] x, float[] x0, float a, float c, int iter, ComputeShader shader)
    {

        float cRecip = (float)1f / c;
        for (int k = 0; k < iter; k++)
        {
            for (int j = 1; j < N - 1; j++)
            {
                for (int i = 1; i < N - 1; i++)
                {
                    x[IX(i, j)] =
                      (x0[IX(i, j)]
                      + a *
                      (x[IX(i + 1, j)]
                      + x[IX(i - 1, j)]
                      + x[IX(i, j + 1)]
                      + x[IX(i, j - 1)]
                      )) * cRecip;
                }
            }
        }
        set_bnd(b, x, shader);
    }



    void Project(float[] velocX, float[] velocY, float[] p, float[] div, int iter, ComputeShader shader)
    {

        
        for (int j = 1; j < N - 1; j++)
        {
            
            for (int i = 1; i < N - 1; i++)
            {
               
                
                div[IX(i, j)] =
                  (-0.5f *
                    (velocX[IX(i + 1, j)] -
                      velocX[IX(i - 1, j)] +
                      velocY[IX(i, j + 1)] -
                      velocY[IX(i, j - 1)])) /
                  N;
               
                p[IX(i, j)] = 0;
            }
        }
        
        set_bnd(0, div, shader);
        set_bnd(0, p, shader);
        lin_solve(0, p, div, 1, 6, iter, shader);

        


        for (int j = 1; j < N - 1; j++)
        {
            for (int i = 1; i < N - 1; i++)
            {
                velocX[IX(i, j)] -= 0.5f * (p[IX(i + 1, j)] - p[IX(i - 1, j)]) * N;
                velocY[IX(i, j)] -= 0.5f * (p[IX(i, j + 1)] - p[IX(i, j - 1)]) * N;
            }
        }

        set_bnd(1, velocX, shader);
        set_bnd(2, velocY, shader);

    }


    public void Advect(int b, float[] d, float[] d0, float[] velocX, float[] velocY, float dt, ComputeShader shader)
    {
        
        int kernelHandle = shader.FindKernel("Advect0");

        if (b == 1){ kernelHandle = shader.FindKernel("Advect1");}
        if (b == 2){ kernelHandle = shader.FindKernel("Advect2");}

        ComputeBuffer velocXBuffer = new ComputeBuffer(velocX.Length, 4);
        velocXBuffer.SetData(velocX);
        shader.SetBuffer(kernelHandle, "velocX", velocXBuffer);

        ComputeBuffer velocYBuffer = new ComputeBuffer(velocY.Length, 4);
        velocYBuffer.SetData(velocY);
        shader.SetBuffer(kernelHandle, "velocY", velocYBuffer);

        ComputeBuffer dBuffer = new ComputeBuffer(d.Length, 4);
        dBuffer.SetData(d);
        shader.SetBuffer(kernelHandle, "d", dBuffer);

        ComputeBuffer d0Buffer = new ComputeBuffer(d0.Length, 4);
        d0Buffer.SetData(d0);
        shader.SetBuffer(kernelHandle, "d0", d0Buffer);

        shader.SetInt("b", b);

        shader.SetFloat("dt", dt);
        shader.SetInt("N", N);

        shader.Dispatch(kernelHandle, (N/16)+1, (N/16)+1, 1);

        velocXBuffer.GetData(velocX);
        velocYBuffer.GetData(velocY);
        dBuffer.GetData(d);
        d0Buffer.GetData(d0);


        velocXBuffer.Dispose();
        velocYBuffer.Dispose();
        dBuffer.Dispose();
        d0Buffer.Dispose();



        d[IX(0, 0)] = 0.5f * (d[IX(1, 0)] + d[IX(0, 1)]);
        d[IX(0, N - 1)] = 0.5f * (d[IX(1, N - 1)] + d[IX(0, N - 2)]);
        d[IX(N - 1, 0)] = 0.5f * (d[IX(N - 2, 0)] + d[IX(N - 1, 1)]);
        d[IX(N - 1, N - 1)] = 0.5f * (d[IX(N - 2, N - 1)] + d[IX(N - 1, N - 2)]);








        // float i0, i1, j0, j1;

        // float dtx = dt * (N - 2);
        // float dty = dt * (N - 2);

        // float s0, s1, t0, t1;
        // float tmp1, tmp2, x, y;

        // float NXfloat = N;
        // float NYfloat = N;
        // float ifloat, jfloat;
        // int i, j;

        // for (j = 1, jfloat = 1; j < N - 1; j++, jfloat++)
        // {
        //     for (i = 1, ifloat = 1; i < N - 1; i++, ifloat++)
        //     {
        //         tmp1 = dtx * velocX[IX(i, j)];
        //         tmp2 = dty * velocY[IX(i, j)];
        //         x = ifloat - tmp1;
        //         y = jfloat - tmp2;

        //         if (x < 0.5f) x = 0.5f;
        //         if (x > NXfloat + 0.5f) x = NXfloat + 0.5f;
        //         i0 = (float)Math.Floor(x);
        //         i1 = i0 + 1.0f;
        //         if (y < 0.5f) y = 0.5f;
        //         if (y > NYfloat + 0.5f) y = NYfloat + 0.5f;
        //         j0 = (float)Math.Floor(y);
        //         j1 = j0 + 1.0f;

        //         s1 = x - i0;
        //         s0 = 1.0f - s1;
        //         t1 = y - j0;
        //         t0 = 1.0f - t1;

        //         int i0i = (int)(i0);
        //         int i1i = (int)(i1);
        //         int j0i = (int)(j0);
        //         int j1i = (int)(j1);

        //         d[IX(i, j)] =
        //           s0 * (t0 * d0[IX(i0i, j0i)] + t1 * d0[IX(i0i, j1i)]) +
        //           s1 * (t0 * d0[IX(i1i, j0i)] + t1 * d0[IX(i1i, j1i)]);
        //     }
        // }

        // set_bnd(b, d, shader);



    }


    /*public void fade(int amount)
    {
        for (int i = 0; i < density.Length; i++)
        {
            density[i] -= density[i] / amount;
        }
    }
*/



    void set_bnd(int b, float[] x, ComputeShader shader)
    {
        // int kernelHandle = b == 1 ? kernelHandle = shader.FindKernel("SetBounds1") : kernelHandle = shader.FindKernel("SetBounds0");
        // kernelHandle = b == 2 ? kernelHandle = shader.FindKernel("SetBounds2") : kernelHandle = shader.FindKernel("SetBounds0");

        // ComputeBuffer xBuffer = new ComputeBuffer(x.Length, 4);
        // xBuffer.SetData(x);
        // shader.SetBuffer(kernelHandle, "x", xBuffer);

        // shader.SetInt("b", b);

        // shader.Dispatch(kernelHandle, N/16, N/16, 1);

        // xBuffer.GetData(x);


        // xBuffer.Dispose();
        

        for (int i = 1; i < N - 1; i++)
        {
            x[IX(i, 0)] = b == 2 ? -x[IX(i, 1)] : x[IX(i, 1)];
            x[IX(i, N - 1)] = b == 2 ? -x[IX(i, N - 2)] : x[IX(i, N - 2)];
        }
        for (int j = 1; j < N - 1; j++)
        {
            x[IX(0, j)] = b == 1 ? -x[IX(1, j)] : x[IX(1, j)];
            x[IX(N - 1, j)] = b == 1 ? -x[IX(N - 2, j)] : x[IX(N - 2, j)];
        }
        x[IX(0, 0)] = 0.5f * (x[IX(1, 0)] + x[IX(0, 1)]);
        x[IX(0, N - 1)] = 0.5f * (x[IX(1, N - 1)] + x[IX(0, N - 2)]);
        x[IX(N - 1, 0)] = 0.5f * (x[IX(N - 2, 0)] + x[IX(N - 1, 1)]);
        x[IX(N - 1, N - 1)] = 0.5f * (x[IX(N - 2, N - 1)] + x[IX(N - 1, N - 2)]);
    }




}
