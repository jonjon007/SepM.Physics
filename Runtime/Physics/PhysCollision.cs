using System;
using System.IO;
using SepM.Serialization;

namespace SepM.Physics
{
    [Serializable]
    public struct PhysCollision : Serial{
        public int Checksum => GetHashCode();
        public uint ObjIdA;
        public uint ObjIdB;
        public CollisionPoints Points;

        public void Serialize(BinaryWriter bw)
        {
        //ObjIdA
            bw.Write(ObjIdA);
        //ObjIdB
            bw.Write(ObjIdB);
        //Points
            Points.Serialize(bw);
        }

        public Serial Deserialize<T>(BinaryReader br, T context)
        {
        //ObjIdA
            ObjIdA = br.ReadUInt32();
        //ObjIdB
            ObjIdB = br.ReadUInt32();
        //Points
            Points = (CollisionPoints)Points.Deserialize(br, context);

            return this;
        }
        public override int GetHashCode()
        {
            int hashCode = -1214587014;
            hashCode = hashCode * -1521134295 + ObjIdA.GetHashCode();
            hashCode = hashCode * -1521134295 + ObjIdB.GetHashCode();
            hashCode = hashCode * -1521134295 + Points.GetHashCode();

            return hashCode;
        }
    }

}