using System;
using System.IO;
using SepM.Serialization;
using SepM.Utils;
using Unity.Mathematics.FixedPoint;

namespace SepM.Physics
{
    [Serializable]
    public struct CollisionPoints : Serial {
        public int Checksum => GetHashCode();
        public fp3 A; // Furthest point of A into B
        public fp3 B; // Furthest point of B into A
        public fp3 Normal; // B – A normalized
        public fp DepthSqrd; // Length of B – A
        public bool HasCollision;

        public static CollisionPoints noCollision = new CollisionPoints{
            A = fp3.zero,
            B = fp3.zero,
            Normal = fp3.zero,
            DepthSqrd = 0,
            HasCollision = false
        };

        public void Serialize(BinaryWriter bw)
        {
        //A
            bw.WriteFp(A.x);
            bw.WriteFp(A.y);
            bw.WriteFp(A.z);
        //B
            bw.WriteFp(B.x);
            bw.WriteFp(B.y);
            bw.WriteFp(B.z);
        //Normal
            bw.WriteFp(Normal.x);
            bw.WriteFp(Normal.y);
            bw.WriteFp(Normal.z);
        //DepthSqrd
            bw.WriteFp(DepthSqrd);
        //HasCollision
            bw.Write(HasCollision);
        }

        public Serial Deserialize<T>(BinaryReader br, T context)
        {
        //A
            A.x = br.ReadFp();
            A.y = br.ReadFp();
            A.z = br.ReadFp();
        //B
            B.x = br.ReadFp();
            B.y = br.ReadFp();
            B.z = br.ReadFp();
        //Normal
            Normal.x = br.ReadFp();
            Normal.y = br.ReadFp();
            Normal.z = br.ReadFp();
        //DepthSqrd
            DepthSqrd = br.ReadFp();
        //HasCollision
            HasCollision = br.ReadBoolean();

            return this;
        }

        public override int GetHashCode()
        {
            int hashCode = -1214587014;
            hashCode = hashCode * -1521134295 + A.GetHashCode();
            hashCode = hashCode * -1521134295 + B.GetHashCode();
            hashCode = hashCode * -1521134295 + Normal.GetHashCode();
            hashCode = hashCode * -1521134295 + DepthSqrd.GetHashCode();
            hashCode = hashCode * -1521134295 + HasCollision.GetHashCode();

            return hashCode;
        }
    };

}