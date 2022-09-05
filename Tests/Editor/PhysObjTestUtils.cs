using Unity.Collections;
using Unity.Mathematics.FixedPoint;
using UnityEngine;
using System.IO;
using SepM.Physics;

partial class PhysObjTests
{
    private PhysWorld CreateSampleWorld(){
        PhysWorld w = new PhysWorld();

        // Sphere
        w.CreateSphereObject(
            new fp3(5,10,0), 2, true, true, Constants.GRAVITY
        );
        // Capsule
        w.CreateCapsuleObject(
            fp3.zero, 5, 6, true, false, fp3.zero
        );
        // AABB
        w.CreateAABBoxObject(
            new fp3(0, 10, 0), new fp3(1, 1, 1), true, true, Constants.GRAVITY * 2, Constants.coll_layers.player
        );

        return w;

        // TODO: Create world.Destroy
    }

    public NativeArray<byte> ToBytes(PhysWorld w) {
        using (var memoryStream = new MemoryStream()) {
            using (var writer = new BinaryWriter(memoryStream)) {
                w.Serialize(writer);
            }
            return new NativeArray<byte>(memoryStream.ToArray(), Allocator.Persistent);
        }
    }
    
    public void FromBytes(NativeArray<byte> bytes, PhysWorld w) {
        using (var memoryStream = new MemoryStream(bytes.ToArray())) {
            using (var reader = new BinaryReader(memoryStream)) {
                Debug.Log($"Reading state size of {(float)(bytes.Length)/1000000} MBs");
                w.Deserialize(reader);
            }
        }
    }

    public bool AreByteArraysEqual(NativeArray<byte> a, NativeArray<byte> b){
        if(a.Length != b.Length)
            return false;
        for(int i = 0; i < a.Length; i++){
            if(a[i] != b[i]) return false;
        }

        return true;
    }
}
