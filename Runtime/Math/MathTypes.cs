using UnityEngine;
using Unity.Mathematics.FixedPoint;
namespace SepM.Math{
    [System.Serializable]
    public struct fpq {
        public fp x;
        public fp y;
        public fp z;
        public fp w;
        public fpq(fp x, fp y, fp z, fp w){
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }
        public static fpq identity = new fpq(0,0,0,1);
        public static implicit operator Quaternion(fpq q)  { return new Quaternion((float)q.x, (float)q.y, (float)q.z, (float)q.w); }
        public static implicit operator fpq(Quaternion q)  { return new fpq((fp)q.x, (fp)q.y, (fp)q.z, (fp)q.w); }
        public override int GetHashCode() { return (int)fpmath.hash(new fp4(x,y,z,w)); }
        public override string ToString(){
            return string.Format("fpq({0}, {1}, {2}, {3})", w, x, y, z);
        }
    }
}