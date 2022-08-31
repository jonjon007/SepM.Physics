using System;
using NUnit.Framework;
using Unity.Collections;
using SepM.Physics;

partial class PhysObjTests
{
    [Test]
    public void TestWorldSerialize(){
        bool sameHash = false;

        PhysWorld wStart = CreateSampleWorld();
        NativeArray<byte> seriWorld = ToBytes(wStart);
        try{
            // Read what was written into a new world and copy it
            PhysWorld wFinish = new PhysWorld();
            FromBytes(seriWorld, wFinish);
            sameHash = wStart.GetHashCode() == wFinish.GetHashCode();
        }
        finally{
            // Dispose of the NativeArray when we're done with it
            if(seriWorld.IsCreated)
                seriWorld.Dispose();
        }

        Assert.IsTrue(sameHash);
    }
}