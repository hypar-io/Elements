using Elements.Validators;
using System;
using Newtonsoft.Json;

namespace Elements.Geometry
{
    /// <summary>
    /// A column-ordered 3x4 matrix.
    /// The first 3 columns represent the X, Y, and Z axes of the coordinate system.
    /// The fourth column represents the translation of the coordinate system.
    /// </summary>
    public partial class Matrix : IEquatable<Matrix>
    {
        /// <summary>The components of the matrix.</summary>
        [JsonProperty("Components", Required = Required.Always)]
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.MinLength(12)]
        [System.ComponentModel.DataAnnotations.MaxLength(12)]
        public double[] Components { get; set; } = new double[12];

        /// <summary>
        /// Construct a matrix.
        /// </summary>
        /// <param name="components">The components of the matrix.</param>
        [JsonConstructor]
        public Matrix(double[] @components)
        {
            if (!Validator.DisableValidationOnConstruction)
            {
                if (components.Length != 12)
                {
                    throw new ArgumentOutOfRangeException("The matrix could not be created. The component array must have 12 values.");
                }
            }

            this.Components = @components;
        }

        /// <summary>
        /// m11
        /// </summary>
        [JsonIgnore]
        public double m11
        {
            get { return this.Components[0]; }
            set { this.Components[0] = value; }
        }

        /// <summary>
        /// m21
        /// </summary>
        [JsonIgnore]
        public double m21
        {
            get { return this.Components[1]; }
            set { this.Components[1] = value; }
        }

        /// <summary>
        /// m31
        /// </summary>
        [JsonIgnore]
        public double m31
        {
            get { return this.Components[2]; }
            set { this.Components[2] = value; }
        }

        /// <summary>
        /// m12
        /// </summary>
        [JsonIgnore]
        public double m12
        {
            get { return this.Components[4]; }
            set { this.Components[4] = value; }
        }

        /// <summary>
        /// m22
        /// </summary>
        [JsonIgnore]
        public double m22
        {
            get { return this.Components[5]; }
            set { this.Components[5] = value; }
        }

        /// <summary>
        /// m32
        /// </summary>
        [JsonIgnore]
        public double m32
        {
            get { return this.Components[6]; }
            set { this.Components[6] = value; }
        }

        /// <summary>
        /// m13
        /// </summary>
        [JsonIgnore]
        public double m13
        {
            get { return this.Components[8]; }
            set { this.Components[8] = value; }
        }

        /// <summary>
        /// m23
        /// </summary>
        [JsonIgnore]
        public double m23
        {
            get { return this.Components[9]; }
            set { this.Components[9] = value; }
        }

        /// <summary>
        /// m33
        /// </summary>
        [JsonIgnore]
        public double m33
        {
            get { return this.Components[10]; }
            set { this.Components[10] = value; }
        }

        /// <summary>
        /// tx
        /// </summary>
        [JsonIgnore]
        public double tx
        {
            get { return this.Components[3]; }
            set { this.Components[3] = value; }
        }

        /// <summary>
        /// ty
        /// </summary>
        [JsonIgnore]
        public double ty
        {
            get { return this.Components[7]; }
            set { this.Components[7] = value; }
        }

        /// <summary>
        /// tz
        /// </summary>
        [JsonIgnore]
        public double tz
        {
            get { return this.Components[11]; }
            set { this.Components[11] = value; }
        }

        /// <summary>
        /// The X axis of the Matrix.
        /// </summary>
        [JsonIgnore]
        public Vector3 XAxis
        {
            get { return new Vector3(m11, m12, m13); }
        }

        /// <summary>
        /// The Y axis of the Matrix.
        /// </summary>
        [JsonIgnore]
        public Vector3 YAxis
        {
            get { return new Vector3(m21, m22, m23); }
        }

        /// <summary>
        /// The Z axis of the Matrix.
        /// </summary>
        [JsonIgnore]
        public Vector3 ZAxis
        {
            get { return new Vector3(m31, m32, m33); }
        }

        /// <summary>
        /// The translation component of the Matrix.
        /// </summary>
        [JsonIgnore]
        public Vector3 Translation
        {
            get { return new Vector3(tx, ty, tz); }
        }

        /// <summary>
        /// Construct a 4X3 matrix.
        /// </summary>
        public Matrix()
        {
            this.Components = new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            SetIdentity();
        }

        /// <summary>
        /// Construct a matrix from axes.
        /// </summary>
        /// <param name="xAxis">The X axis.</param>
        /// <param name="yAxis">The Y axis.</param>
        /// <param name="zAxis">The Z axis.</param>
        /// <param name="translation">The translation.</param>
        public Matrix(Vector3 xAxis, Vector3 yAxis, Vector3 zAxis, Vector3 translation)
        {
            this.Components = new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            m11 = xAxis.X; m12 = xAxis.Y; m13 = xAxis.Z;
            m21 = yAxis.X; m22 = yAxis.Y; m23 = yAxis.Z;
            m31 = zAxis.X; m32 = zAxis.Y; m33 = zAxis.Z;
            tx = translation.X; ty = translation.Y; tz = translation.Z;
        }

        /// <summary>
        /// Set the matrix to identity.
        /// </summary>
        public void SetIdentity()
        {
            m11 = 1.0; m12 = 0.0; m13 = 0.0;
            m21 = 0.0; m22 = 1.0; m23 = 0.0;
            m31 = 0.0; m32 = 0.0; m33 = 1.0;
            tx = 0.0; ty = 0.0; tz = 0.0;
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
            m11 = 1.0; m12 = 0.0; m13 = 0.0;
            m21 = 0.0; m22 = 1.0; m23 = 0.0;
            m31 = 0.0; m32 = 0.0; m33 = 1.0;
            tx = v.X; ty = v.Y; tz = v.Z;
        }

        /// <summary>
        /// Setup the matrix to rotate.
        /// </summary>
        /// <param name="axis">The axis of rotation. 1-x, 2-y, 3-z</param>
        /// <param name="theta">The angle of rotation in radians.</param>
        /// <exception>Thrown when the provided axis is not 1-3.</exception>
        public void SetupRotate(int axis, double theta)
        {
            double s = Math.Sin(theta);
            double c = Math.Cos(theta);
            switch (axis)
            {
                case 1:
                    m11 = 1.0; m12 = 0.0; m13 = 0.0;
                    m21 = 0.0; m22 = c; m23 = s;
                    m31 = 0.0; m32 = -s; m33 = c;
                    break;
                case 2:
                    m11 = c; m12 = 0.0; m13 = -s;
                    m21 = 0.0; m22 = 1.0; m23 = 0.0;
                    m31 = s; m32 = 0.0; m33 = c;
                    break;
                case 3:
                    m11 = c; m12 = s; m13 = 0.0;
                    m21 = -s; m22 = c; m23 = 0.0;
                    m31 = 0.0; m32 = 0.0; m33 = 1.0;
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
            if (Math.Abs(axis.Dot(axis)) - 1.0 > .01)
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
            m11 = s.X; m12 = 0.0; m13 = 0.0;
            m21 = 0.0; m22 = s.Y; m23 = 0.0;
            m31 = 0.0; m32 = 0.0; m33 = s.Z;

            tx = 0.0; ty = 0.0; tz = 0.0;
        }

        /// <summary>
        /// Setup the matrix to project.
        /// </summary>
        /// <param name="p">The plane on which to project.</param>
        /// <exception>Thrown when provided Plane's normal is not unit length.</exception>
        public void SetupProject(Plane p)
        {
            var n = p.Normal;
            if (Math.Abs(n.Dot(n) - 1.0) > 0.1)
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
        /// Setup the matrix to reflect about a plane with normal n.
        /// </summary>
        /// <param name="n">The normal of the reflection plane.</param>
        /// <exception>Thrown when provided Plane's normal is not unit length.</exception>
        public void SetupReflect(Vector3 n)
        {
            if (Math.Abs(n.Dot(n) - 1.0) > 0.1)
            {
                throw new Exception("The specified vector is not unit length.");
            }

            var ax = -2.0 * n.X;
            var ay = -2.0 * n.Y;
            var az = -2.0 * n.Z;

            m11 = 1.0 + ax * n.X;
            m22 = 1.0 + ay * n.Y;
            m32 = 1.0 + az * n.Z;

            m12 = m21 = ax * n.Y;
            m13 = m31 = ax * n.Z;
            m23 = m32 = ay * n.Z;

            tx = ty = tz = 0.0;
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
                p.X * m.m11 + p.Y * m.m21 + p.Z * m.m31 + m.tx,
                p.X * m.m12 + p.Y * m.m22 + p.Z * m.m32 + m.ty,
                p.X * m.m13 + p.Y * m.m23 + p.Z * m.m33 + m.tz
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

            r.m11 = a.m11 * b.m11 + a.m12 * b.m21 + a.m13 * b.m31;
            r.m12 = a.m11 * b.m12 + a.m12 * b.m22 + a.m13 * b.m32;
            r.m13 = a.m11 * b.m13 + a.m12 * b.m23 + a.m13 * b.m33;

            r.m21 = a.m21 * b.m11 + a.m22 * b.m21 + a.m23 * b.m31;
            r.m22 = a.m21 * b.m12 + a.m22 * b.m22 + a.m23 * b.m32;
            r.m23 = a.m21 * b.m13 + a.m22 * b.m23 + a.m23 * b.m33;

            r.m31 = a.m31 * b.m11 + a.m32 * b.m21 + a.m33 * b.m31;
            r.m32 = a.m31 * b.m12 + a.m32 * b.m22 + a.m33 * b.m32;
            r.m33 = a.m31 * b.m13 + a.m32 * b.m23 + a.m33 * b.m33;

            r.tx = a.tx * b.m11 + a.ty * b.m21 + a.tz * b.m31 + b.tx;
            r.ty = a.tx * b.m12 + a.ty * b.m22 + a.tz * b.m32 + b.ty;
            r.tz = a.tx * b.m13 + a.ty * b.m23 + a.tz * b.m33 + b.tz;

            return r;
        }

        /// <summary>
        /// Transpose the matrix.
        /// </summary>
        public Matrix Transpose()
        {
            var r = new Matrix();
            r.m11 = m11; r.m12 = m21; r.m13 = m31;
            r.m21 = m12; r.m22 = m22; r.m23 = m32;
            r.m31 = m13; r.m32 = m23; r.m33 = m33;
            r.tx = tx; r.ty = ty; r.tz = tz;
            return r;
        }

        /// <summary>
        /// Compute the determinant of the 3x3 portion of the matrix.
        /// </summary>
        public double Determinant()
        {
            return m11 * (m22 * m33 - m23 * m32)
                + m12 * (m23 * m31 - m21 * m33)
                + m13 * (m21 * m32 - m22 * m31);
        }

        /// <summary>
        /// Compute the inverse of the matrix.
        /// </summary>
        public Matrix Inverted()
        {
            var det = Determinant();
            if (Math.Abs(det) < 0.000001)
            {
                throw new Exception("The deterimant of the matrix must be greater than 0.000001.");
            }

            var oneOverDet = 1.0 / det;

            var m = new Matrix();
            m.m11 = (m22 * m33 - m23 * m32) * oneOverDet;
            m.m12 = (m13 * m32 - m12 * m33) * oneOverDet;
            m.m13 = (m12 * m23 - m13 * m22) * oneOverDet;

            m.m21 = (m23 * m31 - m21 * m33) * oneOverDet;
            m.m22 = (m11 * m33 - m13 * m31) * oneOverDet;
            m.m23 = (m13 * m21 - m11 * m23) * oneOverDet;

            m.m31 = (m21 * m32 - m22 * m31) * oneOverDet;
            m.m32 = (m12 * m31 - m11 * m32) * oneOverDet;
            m.m33 = (m11 * m22 - m12 * m21) * oneOverDet;

            m.tx = -(tx * m.m11 + ty * m.m21 + tz * m.m31);
            m.ty = -(tx * m.m12 + ty * m.m22 + tz * m.m32);
            m.tz = -(tx * m.m13 + ty * m.m23 + tz * m.m33);

            return m;
        }

        /// <summary>
        /// Compute the inverse of the matrix.
        /// </summary>
        [Obsolete("Use Matrix.Inverted() instead.")]
        public Matrix Inverse()
        {
            return Inverted();
        }

        /// <summary>
        /// Return the string representation of the matrix.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"X: {m11} {m12} {m13}\nY: {m21} {m22} {m23}\nZ: {m31} {m32} {m33}\nOrigin: {tx} {ty} {tz}";
        }

        /// <summary>
        /// Is this matrix equal to other?
        /// </summary>
        /// <param name="other">The transform to test.</param>
        /// <returns>True if the two transforms are equal, otherwise false.</returns>
        public bool Equals(Matrix other)
        {
            for (var i = 0; i < Components.Length; i++)
            {
                if (this.Components[i] != other.Components[i])
                {
                    return false;
                }
            }
            return true;
        }
    }
}
