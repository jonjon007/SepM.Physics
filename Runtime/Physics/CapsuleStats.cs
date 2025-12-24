using System;
using Unity.Mathematics.FixedPoint;

namespace SepM.Physics
{
    [Serializable]
    public struct CapsuleStats{
        public fp3 a_Normal;
        public fp3 a_LineEndOffset;
        public fp3 A;
        public fp3 B;
    }
}