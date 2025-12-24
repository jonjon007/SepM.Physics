using SepM.Math;
using SepM.Serialization;
using SepM.Utils;
using System;
using System.IO;
using Unity.Mathematics.FixedPoint;
using Newtonsoft.Json;

namespace SepM.Physics
{
    [Serializable]
    public class PhysTransform : Serial
    { // Describes an objects location
        public uint InstanceId; // UUID for object
        public fp3 Position;
        public fp3 Scale;
        public fpq Rotation;
        private PhysTransform m_parent;
        public uint m_parent_id = 0;
        
        [JsonProperty]
        public int Checksum => GetHashCode();

        // TODO: Find a way to serialize this without depth limit issues!
        // private List<PhysTransform> m_children;
        public PhysTransform(uint id, PhysTransform parent = null)
        {
            InstanceId = id;
            Position = fp3.zero;
            Scale = new fp3(1, 1, 1);
            Rotation = fpq.identity;
            SetParent(parent);
            //m_children = new List<PhysTransform>();
        }
        public PhysTransform(uint id, fp3 p, PhysTransform parent = null)
        {
            InstanceId = id;
            Position = p;
            Scale = new fp3(1, 1, 1);
            Rotation = fpq.identity;
            SetParent(parent);
            //m_children = new List<PhysTransform>();
        }

        public fp3 Right()
        {
            return new fp3(1, 0, 0).multiply(Rotation);
        }

        public fp3 Forward()
        {
            return new fp3(0, 0, 1).multiply(Rotation);
        }

        public fp3 Up()
        {
            return new fp3(0, 1, 0).multiply(Rotation);
        }

        /* TODO: Comment */
        public fp3 WorldPosition()
        {
            if (!(m_parent is null))
            {
                return m_parent.WorldPosition() + Position.multiply(m_parent.Rotation);
            }

            return Position;


        }
        public fpq WorldRotation()
        {
            fpq parentRot = fpq.identity;

            if (!(m_parent is null))
            {
                parentRot = m_parent.WorldRotation();
            }

            return Rotation.multiply(parentRot);
        }
        /* TODO: Comment */
        public fp3 WorldScale()
        {
            fp3 parentScale = new fp3(1, 1, 1);

            if (!(m_parent is null))
            {
                parentScale = m_parent.WorldScale();
            }

            return Scale * parentScale;
        }

        public void Rotate(fp3 eulers)
        {
            fpq eulerRot = eulers.toQuaternionFromDegrees();
            Rotation = Rotation.multiply(eulerRot);
        }

        public void Rotate(fp x, fp y, fp z)
        {
            Rotate(new fp3(x, y, z));
        }

        public void SetParent(PhysTransform t)
        {
            m_parent = t;
            if (t != null)
                m_parent_id = t.InstanceId;
        }

        public void Serialize(BinaryWriter bw)
        {
        //InstanceId
            bw.Write(InstanceId);
        //Position
            bw.WriteFp(Position.x);
            bw.WriteFp(Position.y);
            bw.WriteFp(Position.z);
        //Scale
            bw.WriteFp(Scale.x);
            bw.WriteFp(Scale.y);
            bw.WriteFp(Scale.z);
        //Rotation
            bw.WriteFp(Rotation.x);
            bw.WriteFp(Rotation.y);
            bw.WriteFp(Rotation.z);
            bw.WriteFp(Rotation.w);
        //m_parent
            bw.Write(m_parent_id);
        }

        public Serial Deserialize<T>(BinaryReader br, T context)
        {
        //InstanceId
            InstanceId = br.ReadUInt32();
        //Position
            Position.x = br.ReadFp();
            Position.y = br.ReadFp();
            Position.z = br.ReadFp();
        //Scale
            Scale.x = br.ReadFp();
            Scale.y = br.ReadFp();
            Scale.z = br.ReadFp();
        //Rotation
            Rotation.x = br.ReadFp();
            Rotation.y = br.ReadFp();
            Rotation.z = br.ReadFp();
            Rotation.w = br.ReadFp();
        //m_parent
            m_parent_id = br.ReadUInt32(); // Used in PhysWorld deserialization to tie to parent

            return this;
        }
        public override int GetHashCode()
        {
            int hashCode = 1858537542;
            hashCode = hashCode * -1521134295 + InstanceId.GetHashCode();
            hashCode = hashCode * -1521134295 + Position.GetHashCode();
            hashCode = hashCode * -1521134295 + Scale.GetHashCode();
            hashCode = hashCode * -1521134295 + Rotation.GetHashCode();
            hashCode = hashCode * -1521134295 + m_parent_id.GetHashCode();
            return hashCode;
        }

    }
}
