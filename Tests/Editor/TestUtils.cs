using Unity.Collections;
using Unity.Mathematics.FixedPoint;
using UnityEngine;
using System.IO;
using SepM.Serialization;

public class TestUtils
{
    fp fixedStep = 1m/60m;

    public bool AreByteArraysEqual(NativeArray<byte> a, NativeArray<byte> b){
        if(a.Length != b.Length)
            return false;
        for(int i = 0; i < a.Length; i++){
            if(a[i] != b[i]) return false;
        }

        return true;
    }

    public static NativeArray<byte> ToBytes(Serial s) {
        using (var memoryStream = new MemoryStream()) {
            using (var writer = new BinaryWriter(memoryStream)) {
                s.Serialize(writer);
            }
            return new NativeArray<byte>(memoryStream.ToArray(), Allocator.Persistent);
        }
    }

    public static Serial FromBytes(NativeArray<byte> bytes, Serial s) {
        using (var memoryStream = new MemoryStream(bytes.ToArray())) {
            using (var reader = new BinaryReader(memoryStream)) {
                Debug.Log($"Reading state size of {(float)(bytes.Length) / 1000000} MBs");
                s.Deserialize(reader, 0);
            }
        }
        return s;
    }
}
