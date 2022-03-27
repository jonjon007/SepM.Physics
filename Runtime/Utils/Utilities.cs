using UnityEngine;
using Unity.Mathematics.FixedPoint;
using System;
using SepM.Math;

namespace SepM.Utils{
    public static class Utilities
    {
        // TODO: Move to math library and make a general static function (not extension)
        public static fp clamp(this fp x, fp min, fp max){
            if (min > x) return min;
            if (max < x) return max;

            return x;
        }

        public static fp3 closestPointOnLineSegment(fp3 A, fp3 B, fp3 Point){
            fp3 AB = B - A;

            // TODO: Write test for this case
            // If point A is the same as point B, just return A
            if(AB.Equals(fp3.zero))
                return A;

            fp t = (Point - A).dot(AB) / AB.dot(AB);
            return A + clamp(t, 0, 1) * AB;
        }

        public static fp3 cross(this fp3 va, fp3 vb){
            return new fp3(
                va.y * vb.z - va.z * vb.y,
                va.z * vb.x - va.x * vb.z,
                va.x * vb.y - va.y * vb.x);
        }

        public static fp dot(this fp3 va, fp3 vb){
            return va.x * vb.x + va.y * vb.y + va.z * vb.z;
        }

        public static bool equalsWError(this fp x, fp other, fp errorPercent){
            int sign = x < 0 ? -1 : 1;
            fp maxVal = x*(1+errorPercent*sign);
            fp minVal = x*(1-errorPercent*sign);
            return other >= minVal && other <= maxVal;
        }
        
        /* Returns the 2D vector length squared, avoiding the slow operation */
        public static fp lengthSqrd(this fp2 vec){
            return vec.x * vec.x + vec.y * vec.y;
        }
        
        /* Returns the 3D vector length squared, avoiding the slow operation */
        public static fp lengthSqrd(this fp3 vec){
            return vec.x * vec.x + vec.y * vec.y + vec.z * vec.z;
        }
        
        public static fp major(this fp2 vec){
            fp major = vec.x;
            if (System.Math.Abs(vec.y) > System.Math.Abs(major)) major = vec.y;

            return major;
        }
        
        public static fp major(this fp3 vec){
            fp major = vec.x;
            if (System.Math.Abs(vec.y) > System.Math.Abs(major)) major = vec.y;
            if (System.Math.Abs(vec.z) > System.Math.Abs(major)) major = vec.z;

            return major;
        }
        
        public static fp max(fp a, fp b){
            return a > b ? a : b;
        }
        
        /* Returns the vector length squared, avoiding the slow operation */
        public static void normalize(ref this fp3 vec){
            fp3 normalized = vec.normalized();
            vec = normalized;
        }

        /* Returns the vector length squared, avoiding the slow operation */
        public static fp3 normalized(this fp3 vec){
            fp lengthSqrd = vec.lengthSqrd();

            if(lengthSqrd == 0)
                return fp3.zero;

            fp3 result = vec/lengthSqrd.sqrt();

            return result;
        }

        public static fp3 multiply(this fp3 v, fpq q){
            fp3 u = new fp3((fp)q.x, (fp)q.y, (fp)q.z);
            fp s = (fp)q.w;

            return v + ((u.cross(v) * s) + u.cross(u.cross(v))) * 2.0m;
        }

        public static fpq multiply(this fpq qa, fpq qb){
            fpq result = new fpq();
            result.w = qa.w * qb.w - qa.x * qb.x - qa.y * qb.y - qa.z * qb.z;
            result.x = qa.w * qb.x + qa.x * qb.w + qa.y * qb.z - qa.z * qb.y;
            result.y = qa.w * qb.y + qa.y * qb.w + qa.z * qb.x - qa.x * qb.z;
            result.z = qa.w * qb.z + qa.z * qb.w + qa.x * qb.y - qa.y * qb.x;
            return result;
        }

        /* Squares the passed FixedPoint number */
        public static fp sqrd(this fp x){
            return x*x;
        }

        public static Vector3 toVector3(this fp3 v){
            Vector3 result = new Vector3(
                (float)v.x,
                (float)v.y,
                (float)v.z
            );
            return result;
        }

        // Referenced from: https://github.com/asik/FixedMath.Net/blob/b2adac7713eda01fdd31578dd5a1d15f8f7ba067/src/Fix64.cs#L575-L645
        public static fp sqrt(this fp x)
        {
            int NUM_BITS = 64;
            var xl = x.RawValue;
            if (xl < 0)
            {
                // We cannot represent infinities like Single and Double, and Sqrt is
                // mathematically undefined for x < 0. So we just throw an exception.
                throw new ArgumentOutOfRangeException("Negative value passed to Sqrt", "x");
            }

            var num = (ulong)xl;
            var result = 0UL;

            // second-to-top bit
            var bit = 1UL << (NUM_BITS - 2);

            while (bit > num)
            {
                bit >>= 2;
            }

            // The main part is executed twice, in order to avoid
            // using 128 bit values in computations.
            for (var i = 0; i < 2; ++i)
            {
                // First we get the top 48 bits of the answer.
                while (bit != 0)
                {
                    if (num >= result + bit)
                    {
                        num -= result + bit;
                        result = (result >> 1) + bit;
                    }
                    else
                    {
                        result = result >> 1;
                    }
                    bit >>= 2;
                }

                if (i == 0)
                {
                    // Then process it again to get the lowest 16 bits.
                    if (num > (1UL << (NUM_BITS / 2)) - 1)
                    {
                        // The remainder 'num' is too large to be shifted left
                        // by 32, so we have to add 1 to result manually and
                        // adjust 'num' accordingly.
                        // num = a - (result + 0.5)^2
                        //       = num + result^2 - (result + 0.5)^2
                        //       = num - result - 0.5
                        num -= result;
                        num = (num << (NUM_BITS / 2)) - 0x80000000UL;
                        result = (result << (NUM_BITS / 2)) + 0x80000000UL;
                    }
                    else
                    {
                        num <<= (NUM_BITS / 2);
                        result <<= (NUM_BITS / 2);
                    }

                    bit = 1UL << (NUM_BITS / 2 - 2);
                }
            }
            // Finally, if next bit would have been 1, round the result upwards.
            if (num > result)
            {
                ++result;
            }
            return fp.FromRaw((long)result);
        }
    }
}