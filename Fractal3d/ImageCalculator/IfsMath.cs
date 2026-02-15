using System.Numerics;

namespace ImageCalculator
{
    public static class IfsMath
    {
        public delegate float SierpinskiDelegate(Vector3 startPt, FractalParams fractalParams, Matrix4x4 mat1, Matrix4x4 mat2);
        public delegate Vector3 SierpinskiVectorDelegate(Vector3 startPt, FractalParams fractalParams, Matrix4x4 mat1, Matrix4x4 mat2);

        //scale=2
        //bailout=1000      originally
        // iterations=10    originally
        public static float Sierpinski3(Vector3 q, float bailout, int iterations)
        {
            float scale = 2.0f;
            var r = q.LengthSquared();
            float x1 = q.X;
            float y1 = q.Y;
            float z1 = q.Z;
            int i = 0;
            for (; i < iterations && r < bailout; i++)
            {
                //Folding... These are some of the symmetry planes of the tetrahedron
                if (q.X + q.Y < 0) { x1 = -q.Y; q.Y = -q.X; q.X = x1; }
                if (q.X + q.Z < 0) { x1 = -q.Z; q.Z = -q.X; q.X = x1; }
                if (q.Y + q.Z < 0) { y1 = -q.Z; q.Z = -q.Y; q.Y = y1; }

                //Stretche about the point [1,1,1]*(scale-1)/scale; The "(scale-1)/scale" is here in order to keep the size of the fractal constant wrt scale
                q.X = scale * q.X - (scale - 1);//equivalent to: x=scale*(x-cx); where cx=(scale-1)/scale;
                q.Y = scale * q.Y - (scale - 1);
                q.Z = scale * q.Z - (scale - 1);
                r = q.LengthSquared();
            }
            // return (float)((Math.Sqrt(r) - 2.0) * Math.Pow(scale, -i));//the estimated distance
            return (float)((Math.Sqrt(r)) * Math.Pow(scale, -i));//the estimated distance
        }

        public static Vector3 Sierpinski3_vector(Vector3 q, float bailout, int iterations)
        {
            float scale = 2.0f;
            var r = q.LengthSquared();
            float x1 = q.X;
            float y1 = q.Y;
            float z1 = q.Z;
            int i = 0;
            for (; i < iterations && r < bailout; i++)
            {
                //Folding... These are some of the symmetry planes of the tetrahedron
                if (q.X + q.Y < 0) { x1 = -q.Y; q.Y = -q.X; q.X = x1; }
                if (q.X + q.Z < 0) { x1 = -q.Z; q.Z = -q.X; q.X = x1; }
                if (q.Y + q.Z < 0) { y1 = -q.Z; q.Z = -q.Y; q.Y = y1; }

                //Stretche about the point [1,1,1]*(scale-1)/scale; The "(scale-1)/scale" is here in order to keep the size of the fractal constant wrt scale
                q.X = scale * q.X - (scale - 1);//equivalent to: x=scale*(x-cx); where cx=(scale-1)/scale;
                q.Y = scale * q.Y - (scale - 1);
                q.Z = scale * q.Z - (scale - 1);
                r = q.LengthSquared();
            }
            return q;
        }

        public static float Sierpinski3_alt(Vector3 q, float bailout, int iterations)
        {
            float scale = 2.0f;
            var a1 = new Vector3(1, 1, 1);
            var a2 = new Vector3(-1, -1, 1);
            var a3 = new Vector3(1, -1, -1);
            var a4 = new Vector3(-1, 1, -1);
            var c = new Vector3();
            int n = 0;
            float dist = 0.0f; 
            float d = 0;

            while( n < iterations)
            {
                var a = a1;
                d = Vector3.Distance(q, a1);
                if (Vector3.Distance(q, a2) < d) { a = a2; d = Vector3.Distance(q, a2); }
                if (Vector3.Distance(q, a3) < d) { a = a3; d = Vector3.Distance(q, a3); }
                if (Vector3.Distance(q, a4) < d) { a = a4; d = Vector3.Distance(q, a4); }
                c = a;
                q = scale * (q - c) + c;
                n++;
                dist = d * (float)Math.Pow(scale, -n);
            }
            return dist;
        }

        public static Vector3 Sierpinski3_alt_vector(Vector3 q, float bailout, int iterations)
        {
            float scale = 2.0f;
            var a1 = new Vector3(1, 1, 1);
            var a2 = new Vector3(-1, -1, 1);
            var a3 = new Vector3(1, -1, -1);
            var a4 = new Vector3(-1, 1, -1);
            var c = new Vector3();
            int n = 0;
            float dist = 0.0f;
            float d = 0;

            while (n < iterations)
            {
                var a = a1;
                d = Vector3.Distance(q, a1);
                if (Vector3.Distance(q, a2) < d) { a = a2; d = Vector3.Distance(q, a2); }
                if (Vector3.Distance(q, a3) < d) { a = a3; d = Vector3.Distance(q, a3); }
                if (Vector3.Distance(q, a4) < d) { a = a4; d = Vector3.Distance(q, a4); }
                c = a;
                q = scale * (q - c) + c;
                n++;
                dist = d * (float)Math.Pow(scale, -n);
            }
            return q;
        }

        public static float Sierpinski3_alt2(Vector3 q, float bailout, int iterations)
        {
            float scale = 2f;
            var a1 = new Vector3(1, 1, 1);
            var a2 = new Vector3(-1, -1, 1);
            var a3 = new Vector3(1, -1, -1);
            var a4 = new Vector3(-1, 1, -1);
            var c = new Vector3();
            int n = 0;
            float dist = 0.0f;
            float d = 0;

            while (n < iterations)
            {
                c = a1;
                dist = (q- a1).Length();
                d = (q - a2).Length();
                if (d < dist)
                {
                    c = a2;
                    dist = d;
                }
                d = (q - a3).Length();
                if (d < dist)
                {
                    c = a3;
                    dist = d;
                }
                d = (q - a4).Length();
                if (d < dist)
                {
                    c = a4;
                    dist = d;
                }
                q = scale * q - c*(scale-1);
                n++;
                
            }
            return q.Length() * (float)Math.Pow(scale, -n);
        }


        public static Vector3 Sierpinski3_alt2_vector(Vector3 q, float bailout, int iterations)
        {
            float scale = 2f;
            var a1 = new Vector3(1, 1, 1);
            var a2 = new Vector3(-1, -1, 1);
            var a3 = new Vector3(1, -1, -1);
            var a4 = new Vector3(-1, 1, -1);
            var c = new Vector3();
            int n = 0;
            float dist = 0.0f;
            float d = 0;

            while (n < iterations)
            {
                c = a1;
                dist = (q - a1).Length();
                d = (q - a2).Length();
                if (d < dist)
                {
                    c = a2;
                    dist = d;
                }
                d = (q - a3).Length();
                if (d < dist)
                {
                    c = a3;
                    dist = d;
                }
                d = (q - a4).Length();
                if (d < dist)
                {
                    c = a4;
                    dist = d;
                }
                q = scale * q - c * (scale - 1);
                n++;

            }
            return q;
        }

        // Based on Mandelbulb websites/Distance Estimated 3D Fractals (III) Folding Space%20 Syntopia.htm
        // but with the addition of transformations before the folding and stretching steps. The transformations can be used to create different variations of the Sierpinski gasket.
        public static float Sierpinski3_alt3(Vector3 q, FractalParams fractalParams, Matrix4x4 mat1, Matrix4x4 mat2)
        {
            float scale = fractalParams.IfsScale;
            var a1 = new Vector3(1, 1, 1);
            var a2 = new Vector3(-1, -1, 1);
            var a3 = new Vector3(1, -1, -1);
            var a4 = new Vector3(-1, 1, -1);
            var c = new Vector3();
            int n = 0;
            float dist = 0.0f;
            float d = 0;
            var r = q.LengthSquared();

            while (n < fractalParams.Iterations && r < fractalParams.Bailout)
            {
                q = TransformationCalculator.Transform(mat1, q);
                c = a1;
                dist = (q - a1).Length();
                d = (q - a2).Length();
                if (d < dist)
                {
                    c = a2;
                    dist = d;
                }
                d = (q - a3).Length();
                if (d < dist)
                {
                    c = a3;
                    dist = d;
                }
                d = (q - a4).Length();
                if (d < dist)
                {
                    c = a4;
                    dist = d;
                }
                q = TransformationCalculator.Transform(mat2, q);
                q = scale * q - c * (scale - 1);
                n++;
                r = q.LengthSquared();
            }
            return q.Length() * (float)Math.Pow(scale, -n);
        }

        public static Vector3 Sierpinski3_alt3_vector(Vector3 q, FractalParams fractalParams, Matrix4x4 mat1, Matrix4x4 mat2)
        {
            float scale = fractalParams.IfsScale;
            var a1 = new Vector3(1, 1, 1);
            var a2 = new Vector3(-1, -1, 1);
            var a3 = new Vector3(1, -1, -1);
            var a4 = new Vector3(-1, 1, -1);
            var c = new Vector3();
            int n = 0;
            float dist = 0.0f;
            float d = 0;
            var r = q.LengthSquared();

            while (n < fractalParams.Iterations && r < fractalParams.Bailout)
            {
                q = TransformationCalculator.Transform(mat1, q);
                c = a1;
                dist = (q - a1).Length();
                d = (q - a2).Length();
                if (d < dist)
                {
                    c = a2;
                    dist = d;
                }
                d = (q - a3).Length();
                if (d < dist)
                {
                    c = a3;
                    dist = d;
                }
                d = (q - a4).Length();
                if (d < dist)
                {
                    c = a4;
                    dist = d;
                }
                q = TransformationCalculator.Transform(mat2, q);
                q = scale * q - c * (scale - 1);
                n++;
                r = q.LengthSquared();
            }
            return q;
        }

        // Sierpinski3_alt3 but with the addition of a center point for the stretching step.
        // So form is from Mandelbulb websites but extras from www.fractalforums.com/sierpinski-gasket/kaleidoscopic
        public static float Sierpinski3_center_stretch(Vector3 q, FractalParams fractalParams, Matrix4x4 mat1, Matrix4x4 mat2)
        {
            float scale = fractalParams.IfsScale;
            var a1 = new Vector3(1, 1, 1);
            var a2 = new Vector3(-1, -1, 1);
            var a3 = new Vector3(1, -1, -1);
            var a4 = new Vector3(-1, 1, -1);
            var c = new Vector3();
            var center = fractalParams.IfsC;
            int n = 0;
            float dist = 0.0f;
            float d = 0;
            var r = q.LengthSquared();

            while (n < fractalParams.Iterations && r < fractalParams.Bailout)
            {
                q = TransformationCalculator.Transform(mat1, q);
                c = a1;
                dist = (q - a1).Length();
                d = (q - a2).Length();
                if (d < dist)
                {
                    c = a2;
                    dist = d;
                }
                d = (q - a3).Length();
                if (d < dist)
                {
                    c = a3;
                    dist = d;
                }
                d = (q - a4).Length();
                if (d < dist)
                {
                    c = a4;
                    dist = d;
                }
                q = TransformationCalculator.Transform(mat2, q);
                var centerStretch = c * center;
                q = scale * q - centerStretch * (scale - 1);
                n++;
                r = q.LengthSquared();
            }
            return q.Length() * (float)Math.Pow(scale, -n);
        }

        public static Vector3 Sierpinski3_center_stretch_vector(Vector3 q, FractalParams fractalParams, Matrix4x4 mat1, Matrix4x4 mat2)
        {
            float scale = fractalParams.IfsScale;
            var a1 = new Vector3(1, 1, 1);
            var a2 = new Vector3(-1, -1, 1);
            var a3 = new Vector3(1, -1, -1);
            var a4 = new Vector3(-1, 1, -1);
            var c = new Vector3();
            var center = fractalParams.IfsC;
            int n = 0;
            float dist = 0.0f;
            float d = 0;
            var r = q.LengthSquared();

            while (n < fractalParams.Iterations && r < fractalParams.Bailout)
            {
                q = TransformationCalculator.Transform(mat1, q);
                c = a1;
                dist = (q - a1).Length();
                d = (q - a2).Length();
                if (d < dist)
                {
                    c = a2;
                    dist = d;
                }
                d = (q - a3).Length();
                if (d < dist)
                {
                    c = a3;
                    dist = d;
                }
                d = (q - a4).Length();
                if (d < dist)
                {
                    c = a4;
                    dist = d;
                }
                q = TransformationCalculator.Transform(mat2, q);
                var centerStretch = c * center;
                q = scale * q - centerStretch * (scale - 1);
                n++;
                r = q.LengthSquared();
            }
            return q;
        }

        // Sierpinski3_alt3 but with the addition of a center point for the stretching step.
        // So form is from Mandelbulb websites but extras from www.fractalforums.com/sierpinski-gasket/kaleidoscopic
        public static float Sierpinski3_center_stretch_no_bailout(Vector3 q, FractalParams fractalParams, Matrix4x4 mat1, Matrix4x4 mat2)
        {
            float scale = fractalParams.IfsScale;
            var a1 = new Vector3(1, 1, 1);
            var a2 = new Vector3(-1, -1, 1);
            var a3 = new Vector3(1, -1, -1);
            var a4 = new Vector3(-1, 1, -1);
            var c = new Vector3();
            var center = fractalParams.IfsC;
            int n = 0;
            float dist = 0.0f;
            float d = 0;
            var r = q.LengthSquared();

            while (n < fractalParams.Iterations)
            {
                q = TransformationCalculator.Transform(mat1, q);
                c = a1;
                dist = (q - a1).Length();
                d = (q - a2).Length();
                if (d < dist)
                {
                    c = a2;
                    dist = d;
                }
                d = (q - a3).Length();
                if (d < dist)
                {
                    c = a3;
                    dist = d;
                }
                d = (q - a4).Length();
                if (d < dist)
                {
                    c = a4;
                    dist = d;
                }
                q = TransformationCalculator.Transform(mat2, q);
                var centerStretch = c * center;
                q = scale * q - centerStretch * (scale - 1);
                n++;
                r = q.LengthSquared();
            }
            return q.Length() * (float)Math.Pow(scale, -n);
        }

        public static Vector3 Sierpinski3_center_stretch_no_bailout_vector(Vector3 q, FractalParams fractalParams, Matrix4x4 mat1, Matrix4x4 mat2)
        {
            float scale = fractalParams.IfsScale;
            var a1 = new Vector3(1, 1, 1);
            var a2 = new Vector3(-1, -1, 1);
            var a3 = new Vector3(1, -1, -1);
            var a4 = new Vector3(-1, 1, -1);
            var c = new Vector3();
            var center = fractalParams.IfsC;
            int n = 0;
            float dist = 0.0f;
            float d = 0;
            var r = q.LengthSquared();

            while (n < fractalParams.Iterations)
            {
                q = TransformationCalculator.Transform(mat1, q);
                c = a1;
                dist = (q - a1).Length();
                d = (q - a2).Length();
                if (d < dist)
                {
                    c = a2;
                    dist = d;
                }
                d = (q - a3).Length();
                if (d < dist)
                {
                    c = a3;
                    dist = d;
                }
                d = (q - a4).Length();
                if (d < dist)
                {
                    c = a4;
                    dist = d;
                }
                q = TransformationCalculator.Transform(mat2, q);
                var centerStretch = c * center;
                q = scale * q - centerStretch * (scale - 1);
                n++;
                r = q.LengthSquared();
            }
            return q;
        }

        // https://www.fractalforums.com/sierpinski-gasket/kaleidoscopic-(escape-time-ifs)/?PHPSESSID=1daa670e46af931712e2c96a11f5002e
        public static float Knighty(Vector3 q, FractalParams fractalParams, Matrix4x4 mat1, Matrix4x4 mat2)
        {
            float scale = fractalParams.IfsScale;
            var center = fractalParams.IfsC;
            var r = q.LengthSquared();
            float x1 = q.X;
            float y1 = q.Y;
            float z1 = q.Z;
            int i = 0;
            for (; i < fractalParams.Iterations && r < fractalParams.Bailout; i++)
            {
                q = TransformationCalculator.Transform(mat1, q);
                //Folding... These are some of the symmetry planes of the tetrahedron
                if (q.X + q.Y < 0) { x1 = -q.Y; q.Y = -q.X; q.X = x1; }
                if (q.X + q.Z < 0) { x1 = -q.Z; q.Z = -q.X; q.X = x1; }
                if (q.Y + q.Z < 0) { y1 = -q.Z; q.Z = -q.Y; q.Y = y1; }

                q = TransformationCalculator.Transform(mat2, q);
                //Stretche about the point [1,1,1]*(scale-1)/scale; The "(scale-1)/scale" is here in order to keep the size of the fractal constant wrt scale
                q.X = scale * q.X - center.X * (scale - 1);//equivalent to: x=scale*(x-cx); where cx=(scale-1)/scale;
                q.Y = scale * q.Y - center.Y * (scale - 1);
                q.Z = scale * q.Z - center.Z * (scale - 1);
                r = q.LengthSquared();
            }
            // return (float)((Math.Sqrt(r) - 2.0) * Math.Pow(scale, -i));//the estimated distance
            return (float)((Math.Sqrt(r)-2.0) * Math.Pow(scale, -i));//the estimated distance
        }

        // https://www.fractalforums.com/sierpinski-gasket/kaleidoscopic-(escape-time-ifs)/?PHPSESSID=1daa670e46af931712e2c96a11f5002e
        public static Vector3 Knighty_vector(Vector3 q, FractalParams fractalParams, Matrix4x4 mat1, Matrix4x4 mat2)
        {
            float scale = fractalParams.IfsScale;
            var center = fractalParams.IfsC;
            var r = q.LengthSquared();
            float x1 = q.X;
            float y1 = q.Y;
            float z1 = q.Z;
            int i = 0;
            for (; i < fractalParams.Iterations  && r < fractalParams.Bailout; i++)
            {
                q = TransformationCalculator.Transform(mat1, q);
                //Folding... These are some of the symmetry planes of the tetrahedron
                if (q.X + q.Y < 0) { x1 = -q.Y; q.Y = -q.X; q.X = x1; }
                if (q.X + q.Z < 0) { x1 = -q.Z; q.Z = -q.X; q.X = x1; }
                if (q.Y + q.Z < 0) { y1 = -q.Z; q.Z = -q.Y; q.Y = y1; }

                q = TransformationCalculator.Transform(mat2, q);

                //Stretche about the point [1,1,1]*(scale-1)/scale; The "(scale-1)/scale" is here in order to keep the size of the fractal constant wrt scale
                q.X = scale * q.X - center.X * (scale - 1);//equivalent to: x=scale*(x-cx); where cx=(scale-1)/scale;
                q.Y = scale * q.Y - center.Y * (scale - 1);
                q.Z = scale * q.Z - center.Z * (scale - 1);
                r = q.LengthSquared();
            }
            // return (float)((Math.Sqrt(r) - 2.0) * Math.Pow(scale, -i));//the estimated distance
            return q;
        }


        // https://www.fractalforums.com/sierpinski-gasket/kaleidoscopic-(escape-time-ifs)/?PHPSESSID=1daa670e46af931712e2c96a11f5002e
        public static float Knighty_no_bailout(Vector3 q, FractalParams fractalParams, Matrix4x4 mat1, Matrix4x4 mat2)
        {
            float scale = fractalParams.IfsScale;
            var center = fractalParams.IfsC;
            var r = q.LengthSquared();
            float x1 = q.X;
            float y1 = q.Y;
            float z1 = q.Z;
            int i = 0;
            for (; i < fractalParams.Iterations; i++)
            {
                q = TransformationCalculator.Transform(mat1, q);
                //Folding... These are some of the symmetry planes of the tetrahedron
                if (q.X + q.Y < 0) { x1 = -q.Y; q.Y = -q.X; q.X = x1; }
                if (q.X + q.Z < 0) { x1 = -q.Z; q.Z = -q.X; q.X = x1; }
                if (q.Y + q.Z < 0) { y1 = -q.Z; q.Z = -q.Y; q.Y = y1; }

                q = TransformationCalculator.Transform(mat2, q);
                //Stretche about the point [1,1,1]*(scale-1)/scale; The "(scale-1)/scale" is here in order to keep the size of the fractal constant wrt scale
                q.X = scale * q.X - center.X * (scale - 1);//equivalent to: x=scale*(x-cx); where cx=(scale-1)/scale;
                q.Y = scale * q.Y - center.Y * (scale - 1);
                q.Z = scale * q.Z - center.Z * (scale - 1);
                r = q.LengthSquared();
            }
            // return (float)((Math.Sqrt(r) - 2.0) * Math.Pow(scale, -i));//the estimated distance
            return (float)((Math.Sqrt(r) - 2.0) * Math.Pow(scale, -i));//the estimated distance
        }

        // https://www.fractalforums.com/sierpinski-gasket/kaleidoscopic-(escape-time-ifs)/?PHPSESSID=1daa670e46af931712e2c96a11f5002e
        public static Vector3 Knighty_no_bailout_vector(Vector3 q, FractalParams fractalParams, Matrix4x4 mat1, Matrix4x4 mat2)
        {
            float scale = fractalParams.IfsScale;
            var center = fractalParams.IfsC;
            var r = q.LengthSquared();
            float x1 = q.X;
            float y1 = q.Y;
            float z1 = q.Z;
            int i = 0;
            for (; i < fractalParams.Iterations; i++)
            {
                q = TransformationCalculator.Transform(mat1, q);
                //Folding... These are some of the symmetry planes of the tetrahedron
                if (q.X + q.Y < 0) { x1 = -q.Y; q.Y = -q.X; q.X = x1; }
                if (q.X + q.Z < 0) { x1 = -q.Z; q.Z = -q.X; q.X = x1; }
                if (q.Y + q.Z < 0) { y1 = -q.Z; q.Z = -q.Y; q.Y = y1; }

                q = TransformationCalculator.Transform(mat2, q);

                //Stretche about the point [1,1,1]*(scale-1)/scale; The "(scale-1)/scale" is here in order to keep the size of the fractal constant wrt scale
                q.X = scale * q.X - center.X * (scale - 1);//equivalent to: x=scale*(x-cx); where cx=(scale-1)/scale;
                q.Y = scale * q.Y - center.Y * (scale - 1);
                q.Z = scale * q.Z - center.Z * (scale - 1);
                r = q.LengthSquared();
            }
            // return (float)((Math.Sqrt(r) - 2.0) * Math.Pow(scale, -i));//the estimated distance
            return q;
        }

        public static float IntersectSierpinski(ref Vector3 startPt, Vector3 direction, FractalParams fractalParams, 
            Matrix4x4 mat1, Matrix4x4 mat2, SierpinskiDelegate sierpinskiDelegate)
        {
            float dist = 0;

            for (int i = 0; i < fractalParams.MaxRaySteps; i++)
            {
                dist = sierpinskiDelegate(startPt, fractalParams, mat1, mat2);
                dist /= fractalParams.StepDivisor;

                startPt += direction * dist;

                if (float.IsNaN(dist))
                    return 1;

                if (dist < fractalParams.MinRayDistance)
                    break;

                if (dist > fractalParams.MaxDistance)
                    break;
            }
            return dist;
        }

        public static Vector3 NormEstimateSierpinski(Vector3 p, FractalParams fractalParams, Matrix4x4 mat1, Matrix4x4 mat2, SierpinskiVectorDelegate sierpinskiVectorDelegate)
        {
            var distance = fractalParams.NormalDistance;
            Vector3 gx1 = new Vector3(p.X - distance, p.Y, p.Z);
            Vector3 gx2 = new Vector3(p.X + distance, p.Y, p.Z);
            Vector3 gy1 = new Vector3(p.X, p.Y - distance, p.Z);
            Vector3 gy2 = new Vector3(p.X, p.Y + distance, p.Z);
            Vector3 gz1 = new Vector3(p.X, p.Y, p.Z - distance);
            Vector3 gz2 = new Vector3(p.X, p.Y, p.Z + distance);

            for (int i = 0; i < fractalParams.Iterations; i++)
            {
                gx1 = sierpinskiVectorDelegate(gx1, fractalParams, mat1, mat2);
                gx2 = sierpinskiVectorDelegate(gx2, fractalParams, mat1, mat2);
                gy1 = sierpinskiVectorDelegate(gy1, fractalParams, mat1, mat2);
                gy2 = sierpinskiVectorDelegate(gy2, fractalParams, mat1, mat2);
                gz1 = sierpinskiVectorDelegate(gz1, fractalParams, mat1, mat2);
                gz2 = sierpinskiVectorDelegate(gz2, fractalParams, mat1, mat2);
            }

            var gradX = gx2.Length() - gx1.Length();
            var gradY = gy2.Length() - gy1.Length();
            var gradZ = gz2.Length() - gz1.Length();

            var v = new Vector3(gradX, gradY, gradZ);
            return Vector3.Normalize(v);
        }
    }
}
