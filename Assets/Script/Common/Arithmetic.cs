using System;
using UnityEngine;
 
namespace OccupancyFieldStudy.Utility
{

    public static class VectorExtensions
    {
        public static Vector3 Mul(this Vector3 vec, Vector3 vec2)
        {
            return new Vector3(vec.x * vec2.x, vec.y * vec2.y, vec.z*vec2.z);
        }
        public static Vector3 Dev(this Vector3 vec, Vector3 vec2)
        {
            return new Vector3(vec.x / vec2.x, vec.y / vec2.y, vec.z / vec2.z);
        }

        public static Vector3 Recip(this Vector3 vec)
        {
            return new Vector3(1/vec.x,1/vec.y,1/vec.z);
        }
        public static Vector3 Abs(this Vector3 vec)
        {
            return new Vector3(Mathf.Abs( vec.x), Mathf.Abs(vec.y), Mathf.Abs(vec.z));
        }
        public static Vector3 Round(this Vector3 vec,int num)
        {
            return new Vector3(
                (float)Math.Round(vec.x, num),
                (float)Math.Round(vec.y, num),
                (float)Math.Round(vec.z, num)
                );
        }


        public static float[] ToArray(this Vector3 vec)
        {
            return new float[3]{ vec.x,vec.y,vec.z };
        }
    }
    public static class MatrixExtensions
    {
        public static Quaternion ExtractRotation(this Matrix4x4 matrix)
        {
            Vector3 forward;
            forward.x = matrix.m02;
            forward.y = matrix.m12;
            forward.z = matrix.m22;

            Vector3 upwards;
            upwards.x = matrix.m01;
            upwards.y = matrix.m11;
            upwards.z = matrix.m21;

            return Quaternion.LookRotation(forward, upwards);
        }

        public static Vector3 ExtractPosition(this Matrix4x4 matrix)
        {
            Vector3 position;
            position.x = matrix.m03;
            position.y = matrix.m13;
            position.z = matrix.m23;
            return position;
        }

        public static Vector3 ExtractScale(this Matrix4x4 matrix)
        {
            Vector3 scale;
            scale.x = new Vector4(matrix.m00, matrix.m10, matrix.m20, matrix.m30).magnitude;
            scale.y = new Vector4(matrix.m01, matrix.m11, matrix.m21, matrix.m31).magnitude;
            scale.z = new Vector4(matrix.m02, matrix.m12, matrix.m22, matrix.m32).magnitude;
            return scale;
        }

        // 生成Scale矩阵
        public static Matrix4x4 MakeScale(Vector3 factor)
        {
            Matrix4x4 scaleMatrix = Matrix4x4.identity;
            scaleMatrix.m00 *= factor.x;
            scaleMatrix.m11 *= factor.y;
            scaleMatrix.m22 *= factor.z;
            return scaleMatrix;
        }

        // 生成Translate矩阵
        public static Matrix4x4 MakeTranslate(Vector3 translate)
        {
            Matrix4x4 translateMatrix = Matrix4x4.identity;
            translateMatrix.m03 = translate.x;
            translateMatrix.m13 = translate.y;
            translateMatrix.m23 = translate.z;
            return translateMatrix;
        }


        // 计算矩阵加法
        public static Matrix4x4 Add(this Matrix4x4 lhs, Matrix4x4 rhs)
        {
            Matrix4x4 retMatrix;
            retMatrix.m00 = lhs.m00 + rhs.m00;
            retMatrix.m01 = lhs.m01 + rhs.m01;
            retMatrix.m02 = lhs.m02 + rhs.m02;
            retMatrix.m03 = lhs.m03 + rhs.m03;
            retMatrix.m10 = lhs.m10 + rhs.m10;
            retMatrix.m11 = lhs.m11 + rhs.m11;
            retMatrix.m12 = lhs.m12 + rhs.m12;
            retMatrix.m13 = lhs.m13 + rhs.m13;
            retMatrix.m20 = lhs.m20 + rhs.m20;
            retMatrix.m21 = lhs.m21 + rhs.m21;
            retMatrix.m22 = lhs.m22 + rhs.m22;
            retMatrix.m23 = lhs.m23 + rhs.m23;
            retMatrix.m30 = lhs.m30 + rhs.m30;
            retMatrix.m31 = lhs.m31 + rhs.m31;
            retMatrix.m32 = lhs.m32 + rhs.m32;
            retMatrix.m33 = lhs.m33 + rhs.m33;
            return retMatrix;
        }

        // 计算矩阵减法
        public static Matrix4x4 Subtract(this Matrix4x4 lhs, Matrix4x4 rhs)
        {
            Matrix4x4 retMatrix;
            retMatrix.m00 = lhs.m00 - rhs.m00;
            retMatrix.m01 = lhs.m01 - rhs.m01;
            retMatrix.m02 = lhs.m02 - rhs.m02;
            retMatrix.m03 = lhs.m03 - rhs.m03;
            retMatrix.m10 = lhs.m10 - rhs.m10;
            retMatrix.m11 = lhs.m11 - rhs.m11;
            retMatrix.m12 = lhs.m12 - rhs.m12;
            retMatrix.m13 = lhs.m13 - rhs.m13;
            retMatrix.m20 = lhs.m20 - rhs.m20;
            retMatrix.m21 = lhs.m21 - rhs.m21;
            retMatrix.m22 = lhs.m22 - rhs.m22;
            retMatrix.m23 = lhs.m23 - rhs.m23;
            retMatrix.m30 = lhs.m30 - rhs.m30;
            retMatrix.m31 = lhs.m31 - rhs.m31;
            retMatrix.m32 = lhs.m32 - rhs.m32;
            retMatrix.m33 = lhs.m33 - rhs.m33;
            return retMatrix;
        }

        // 计算矩阵与标量乘法
        public static Matrix4x4 Scale(this Matrix4x4 lhs, float s)
        {
            Matrix4x4 retMatrix;
            retMatrix.m00 = lhs.m00 * s;
            retMatrix.m01 = lhs.m01 * s;
            retMatrix.m02 = lhs.m02 * s;
            retMatrix.m03 = lhs.m03 * s;
            retMatrix.m10 = lhs.m10 * s;
            retMatrix.m11 = lhs.m11 * s;
            retMatrix.m12 = lhs.m12 * s;
            retMatrix.m13 = lhs.m13 * s;
            retMatrix.m20 = lhs.m20 * s;
            retMatrix.m21 = lhs.m21 * s;
            retMatrix.m22 = lhs.m22 * s;
            retMatrix.m23 = lhs.m23 * s;
            retMatrix.m30 = lhs.m30 * s;
            retMatrix.m31 = lhs.m31 * s;
            retMatrix.m32 = lhs.m32 * s;
            retMatrix.m33 = lhs.m33 * s;
            return retMatrix;
        }

        // 列向量乘以行向量
        public static Matrix4x4 ColMultiply(this Vector3 a, Vector3 b)
        {
            Matrix4x4 retMatrix = Matrix4x4.zero;
            retMatrix.m00 = a.x * b.x;
            retMatrix.m01 = a.x * b.y;
            retMatrix.m02 = a.x * b.z;
            retMatrix.m10 = a.y * b.x;
            retMatrix.m11 = a.y * b.y;
            retMatrix.m12 = a.y * b.z;
            retMatrix.m20 = a.z * b.x;
            retMatrix.m21 = a.z * b.y;
            retMatrix.m22 = a.z * b.z;
            retMatrix.m33 = 1;
            return retMatrix;
        }

        // 将 Vector 转换为 Cross Product 矩阵
        public static Matrix4x4 ToCrossMatrix(this Vector3 a)
        {
            //Get the cross product matrix of vector a
            Matrix4x4 A = Matrix4x4.zero;
            A.m00 = 0;
            A.m01 = -a[2];
            A.m02 = a[1];
            A.m10 = a[2];
            A.m11 = 0;
            A.m12 = -a[0];
            A.m20 = -a[1];
            A.m21 = a[0];
            A.m22 = 0;
            A.m33 = 1;
            return A;
        }

    }

}
