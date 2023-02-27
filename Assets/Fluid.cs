using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Fluid {

    private Texture2D Image;
    public static int res = 1024;
    public static int ival = 256;

    int size;
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

    public Fluid(float diffusion, float viscosity, float dt, int size, int iterations)
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

    public void Step() 
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

        Diffuse(1, Vx0, Vx, visc, dt, iter);
        Diffuse(2, Vy0, Vy, visc, dt, iter);

        Project(Vx0, Vy0, Vx, Vy, iter);

        Advect(1, Vx, Vx0, Vx0, Vy0, dt);
        Advect(2, Vy, Vy0, Vx0, Vy0, dt);

        Project(Vx, Vy, Vx0, Vy0, iter);
        Diffuse(0, s, density, diff, dt, iter);
        Advect(0, density, s, Vx, Vy, dt);
    }

    public void RenderD(Texture2D Image)
    {
        for (int i = 0; i < N; i++)
        {
            for (int j = 0; j < N; j++)
            {   
                int x = (int)i;
                int y = (int)j;
                float d = this.density[IX(i, j)];
                Color color = new Color(d, d, d, d);
                Image.SetPixel(x, y, color);
            }
        }
        Image.Apply();
    }


    public void AddDensity(int x, int y, float amount)
    {
        int index = IX(x, y);
        this.density[index] += amount;
    }

    public void AddVelocity(int x, int y, float amountX, float amountY)
    {
        int index = IX(x, y);
        this.Vx[index] += amountX;
        this.Vy[index] += amountY;
    }

    void Diffuse(int b, float[] x, float[] x0, float diff, float dt, int iter)
    {
        float a = dt * diff * (N - 2) * (N - 2);
        lin_solve(b, x, x0, a, 1 + 6 * a, iter);
    }


        public void lin_solve(int b, float[] x, float[] x0, float a, float c, int iter)
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
        set_bnd(b, x);
    }



    void Project(float[] velocX, float[] velocY, float[] p, float[] div, int iter)
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
        
        set_bnd(0, div);
        set_bnd(0, p);
        lin_solve(0, p, div, 1, 6, iter);

        for (int j = 1; j < N - 1; j++)
        {
            for (int i = 1; i < N - 1; i++)
            {
                velocX[IX(i, j)] -= 0.5f * (p[IX(i + 1, j)] - p[IX(i - 1, j)]) * N;
                velocY[IX(i, j)] -= 0.5f * (p[IX(i, j + 1)] - p[IX(i, j - 1)]) * N;
            }
        }

        set_bnd(1, velocX);
        set_bnd(2, velocY);

    }


    public void Advect(int b, float[] d, float[] d0, float[] velocX, float[] velocY, float dt)
    {
        float i0, i1, j0, j1;

        float dtx = dt * (N - 2);
        float dty = dt * (N - 2);

        float s0, s1, t0, t1;
        float tmp1, tmp2, x, y;

        float NXfloat = N;
        float NYfloat = N;
        float ifloat, jfloat;
        int i, j;

        for (j = 1, jfloat = 1; j < N - 1; j++, jfloat++)
        {
            for (i = 1, ifloat = 1; i < N - 1; i++, ifloat++)
            {
                tmp1 = dtx * velocX[IX(i, j)];
                tmp2 = dty * velocY[IX(i, j)];
                x = ifloat - tmp1;
                y = jfloat - tmp2;

                if (x < 0.5f) x = 0.5f;
                if (x > NXfloat + 0.5f) x = NXfloat + 0.5f;
                i0 = (float)Math.Floor(x);
                i1 = i0 + 1.0f;
                if (y < 0.5f) y = 0.5f;
                if (y > NYfloat + 0.5f) y = NYfloat + 0.5f;
                j0 = (float)Math.Floor(y);
                j1 = j0 + 1.0f;

                s1 = x - i0;
                s0 = 1.0f - s1;
                t1 = y - j0;
                t0 = 1.0f - t1;

                int i0i = (int)(i0);
                int i1i = (int)(i1);
                int j0i = (int)(j0);
                int j1i = (int)(j1);

                d[IX(i, j)] =
                  s0 * (t0 * d0[IX(i0i, j0i)] + t1 * d0[IX(i0i, j1i)]) +
                  s1 * (t0 * d0[IX(i1i, j0i)] + t1 * d0[IX(i1i, j1i)]);
            }
        }

        set_bnd(b, d);
    }


    /*public void fade(int amount)
    {
        for (int i = 0; i < density.Length; i++)
        {
            density[i] -= density[i] / amount;
        }
    }
*/



    void set_bnd(int b, float[] x)
    {
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
