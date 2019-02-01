using System;
using Newtonsoft.Json;

namespace Elements.Geometry
{   
    /// <summary>
    /// A column-ordered 4x3 matrix.
    /// </summary>
    public class Matrix
    {
        double m11 = 0.0;
        double m21 = 0.0;
        double m31 = 0.0;
        double m12 = 0.0;
        double m22 = 0.0;
        double m32 = 0.0;
        double m13 = 0.0;
        double m23 = 0.0;
        double m33 = 0.0;
        double tx = 0.0;
        double ty = 0.0;
        double tz = 0.0;

        /// <summary>
        /// The X axis of the Matrix.
        /// </summary>
        [JsonProperty("x_axis")]
        public Vector3 XAxis
        {
            get{return new Vector3(m11, m12, m13);}
        }

        /// <summary>
        /// The Y axis of the Matrix.
        /// </summary>
        [JsonProperty("y_axis")]
        public Vector3 YAxis
        {
            get{return new Vector3(m21, m22, m23);}
        }

        /// <summary>
        /// The Z axis of the Matrix.
        /// </summary>
        [JsonProperty("z_axis")]
        public Vector3 ZAxis
        {
            get{return new Vector3(m31, m32, m33);}
        }

        /// <summary>
        /// The translation component of the Matrix.
        /// </summary>
        [JsonProperty("translation")]
        public Vector3 Translation
        {
            get{return new Vector3(tx, ty, tz);}
        }
        
        /// <summary>
        /// Construct a 4X3 matrix.
        /// </summary>
        public Matrix()
        {
            SetIdentity();
        }

        /// <summary>
        /// Construct a matrix from axes.
        /// </summary>
        /// <param name="xAxis">The X axis.</param>
        /// <param name="yAxis">The Y axis.</param>
        /// <param name="zAxis">The Z axis.</param>
        /// <param name="translation">The translation.</param>
        [JsonConstructor]
        public Matrix(Vector3 xAxis, Vector3 yAxis, Vector3 zAxis, Vector3 translation)
        {
            m11 = xAxis.X;  m12 = xAxis.Y;  m13 = xAxis.Z;
            m21 = yAxis.X;  m22 = yAxis.Y;  m23 = yAxis.Z;
            m31 = zAxis.X;  m32 = zAxis.Y;  m33 = zAxis.Z;
            tx = translation.X;   ty = translation.Y;   tz = translation.Z;
        }

        /// <summary>
        /// Set the matrix to identity.
        /// </summary>
        public void SetIdentity()
        {
            m11 = 1.0;  m12 = 0.0;  m13 = 0.0;
            m21 = 0.0;  m22 = 1.0;  m23 = 0.0;
            m31 = 0.0;  m32 = 0.0;  m33 = 1.0;
            tx = 0.0;   ty = 0.0;   tz = 0.0;
        }

        /// <summary>
        /// Set the translation of the matrix to zero.
        /// </summary>
        public void ZeroTranslation()
        {
            tx = ty = tz = 0.0;
        }

        /// <summary>
        /// Set the translation of the matrix.
        /// </summary>
        /// <param name="v">The translation vector.</param>
        public void SetTranslation(Vector3 v)
        {
            tx = v.X;
            ty = v.Y;
            tz = v.Z;
        }

        /// <summary>
        /// Setup the matrix to translate.
        /// </summary>
        /// <param name="v">The translation.</param>
        public void SetupTranslation(Vector3 v)
        {
            m11 = 1.0;  m12 = 0.0;  m13 = 0.0;
            m21 = 0.0;  m22 = 1.0;  m23 = 0.0;
            m31 = 0.0;  m32 = 0.0;  m33 = 1.0;
            tx = v.X;   ty = v.Y;   tz = v.Z;
        }

        /// <summary>
        /// Setup the matrix to rotate.
        /// </summary>
        /// <param name="axis">The axis of rotation. 1-x, 2-y, 3-z</param>
        /// <param name="theta">The angle of rotation in radians.</param>
        /// <exception cref="System.ArgumentException">Thrown when the provided axis is not 1-3.</exception>
        public void SetupRotate(int axis, double theta)
        {
            double s = Math.Sin(theta);
            double c = Math.Cos(theta);
            switch(axis)
            {
                case 1:
                    m11 = 1.0;  m12 = 0.0; m13 = 0.0;
                    m21 = 0.0;  m22 = c; m23 = s;
                    m31 = 0.0;  m32 = -s; m33 = c;
                    break;
                case 2:
                    m11 = c;    m12 = 0.0; m13 = -s;
                    m21 = 0.0;  m22 = 1.0; m23 = 0.0;
                    m31 = s;    m32 = 0.0; m33 = c;
                    break;
                case 3:
                    m11 = c;    m12 = s;    m13 = 0.0;
                    m21 = -s;   m22 = c;    m23 = 0.0;
                    m31 = 0.0;  m32 = 0.0;  m33 = 1.0;
                    break;
                default:
                    throw new ArgumentException("You must specify and axis 1-3.");
            }

            tx = ty = tz = 0.0;
        }

        /// <summary>
        /// Setup the matrix to perform rotation.
        /// </summary>
        /// <param name="axis">The axis of rotation.</param>
        /// <param name="theta">The angle of rotation in radians.</param>
        public void SetupRotate(Vector3 axis, double theta)
        {
            if(Math.Abs(axis.Dot(axis)) - 1.0 > .01)
            {
                throw new Exception("The provided axis is not of unit length.");
            }

            double s = Math.Sin(theta);
            double c = Math.Cos(theta);

            var a = 1.0 - c;
            var ax = a * axis.X;
            var ay = a * axis.Y;
            var az = a * axis.Z;

            m11 = ax * axis.X + c;
            m12 = ax * axis.Y + axis.Z * s;
            m13 = ax * axis.Z - axis.Y * s;

            m21 = ay * axis.X - axis.Z * s;
            m22 = ay * axis.Y + c;
            m23 = ay * axis.Z + axis.X * s;

            m31 = az * axis.X + axis.Y * s;
            m32 = az * axis.Y - axis.X * s;
            m33 = az * axis.Z + c;

            tx = 0.0; ty = 0.0; tz = 0.0;
        }

        /// <summary>
        /// Setup the matrix to scale.
        /// </summary>
        /// <param name="s">The scale value.</param>
        public void SetupScale(Vector3 s)
        {
            m11 = s.X;  m12 = 0.0;  m13 = 0.0;
            m21 = 0.0;  m22 = s.Y;  m23 = 0.0;
            m31 = 0.0;  m32 = 0.0;  m33 = s.Z;

            tx = 0.0; ty = 0.0; tz = 0.0;
        }

        /// <summary>
        /// Setup the matrix to project.
        /// </summary>
        /// <param name="p">The plane on which to project.</param>
        /// <exception cref="System.Exception">Thrown when provided Plane's normal is not unit length.</exception>
        public void SetupProject(Plane p)
        {   
            var n = p.Normal;
            if(Math.Abs(n.Dot(n) - 1.0) > 0.1)
            {
                throw new Exception("The specified vector is not unit length.");
            }

            m11 = 1.0 - n.X * n.X;
            m22 = 1.0 - n.Y * n.Y;
            m33 = 1.0 - n.Z * n.Z;
            
            m12 = m21 = -n.X * n.Y;
            m13 = m31 = -n.X * n.Z;
            m23 = m32 = -n.Y * n.Z;
            
            tx = 0.0; ty = 0.0; tz = 0.0;
        }

        /// <summary>
        /// Transform the specified vector.
        /// </summary>
        /// <param name="p">The vector to transform.</param>
        /// <param name="m">The transformation matrix.</param>
        /// <returns></returns>
        public static Vector3 operator *(Vector3 p, Matrix m)
        {
            return new Vector3(
                p.X*m.m11 + p.Y*m.m21 + p.Z*m.m31 + m.tx,
                p.X*m.m12 + p.Y*m.m22 + p.Z*m.m32 + m.ty,
                p.X*m.m13 + p.Y*m.m23 + p.Z*m.m33 + m.tz
            );
        }

        /// <summary>
        /// Multiply two matrices.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Matrix operator *(Matrix a, Matrix b)
        {
            var r = new Matrix();

            r.m11 = a.m11*b.m11 + a.m12*b.m21 + a.m13*b.m31;
            r.m12 = a.m11*b.m12 + a.m12*b.m22 + a.m13*b.m32;
            r.m13 = a.m11*b.m13 + a.m12*b.m23 + a.m13*b.m33;

            r.m21 = a.m21*b.m11 + a.m22*b.m21 + a.m23*b.m31;
            r.m22 = a.m21*b.m12 + a.m22*b.m22 + a.m23*b.m32;
            r.m23 = a.m21*b.m13 + a.m22*b.m23 + a.m23*b.m33;

            r.m31 = a.m31*b.m11 + a.m32*b.m21 + a.m33*b.m31;
            r.m32 = a.m31*b.m12 + a.m32*b.m22 + a.m33*b.m32;
            r.m33 = a.m31*b.m13 + a.m32*b.m23 + a.m33*b.m33;

            r.tx = a.tx*b.m11 + a.ty*b.m21 + a.tz*b.m31 + b.tx;
            r.ty = a.tx*b.m12 + a.ty*b.m22 + a.tz*b.m32 + b.ty;
            r.tz = a.tx*b.m13 + a.ty*b.m23 + a.tz*b.m33 + b.tz;

            return r;
        }

        /// <summary>
        /// Transpose the matrix.
        /// </summary>
        public Matrix Transpose()
        {
            var r = new Matrix();
            r.m11 = m11;  r.m12 = m21;   r.m13 = m31;
            r.m21 = m12;  r.m22 = m22;   r.m23 = m32;
            r.m31 = m13;  r.m32 = m23;   r.m33 = m33;
            r.tx = tx;    r.ty = ty;    r.tz = tz;
            return r;
        }

        /// <summary>
        /// Return the string representation of the matrix.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"X: {m11} {m12} {m13}\nY: {m21} {m22} {m23}\nZ: {m31} {m32} {m33}\nOrigin: {tx} {ty} {tz}";
        }

    }
}