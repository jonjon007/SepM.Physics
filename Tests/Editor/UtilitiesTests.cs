using System;
using System.IO;
using NUnit.Framework;
using Unity.Collections;
using UnityEngine;
using SepM.Utils;
using Unity.Mathematics.FixedPoint;
using SepM.Math;

public class UtilitiesTests
{
    [Test]
    public void TestClampMin(){
        fp min = -1;
        fp max = 1;
        fp val = -2;
        fp expected = -1;
        
        fp actual = val.clamp(min, max);
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void TestClampMax(){
        fp min = -1;
        fp max = 1;
        fp val = 2;
        fp expected = 1;
        
        fp actual = val.clamp(min, max);
        Assert.AreEqual(expected, actual);
    }

    //TODO: Use rawvalue for all tests

    [Test]
    public void TestClampInBetween(){
        fp min = -1;
        fp max = 1;
        fp val = .5m;
        fp expected = .5m;
        
        fp actual = val.clamp(min, max);
        Assert.AreEqual(expected, actual);
    }
    
    [Test]
    public void TestCross(){
        fp3 v1 = new fp3(1,0,0);
        fp3 v2 = new fp3(0,1,0);
        fp3 expected = new fp3(0,0,1);
        
        fp3 actual = v1.cross(v2);
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void TestDot(){
        fp3 v1 = new fp3(1,0,0);
        fp3 v2 = new fp3(-1,0,0);
        fp expected = -1;
        
        fp actual = v1.dot(v2);
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void Test2DLengthSqrd(){
        fp2 vec1 = new fp2(1,-2);
        fp expected = 5;

        fp actual = vec1.lengthSqrd();
        Assert.AreEqual(expected, actual);
    }
    
    [Test]
    public void Test3DLengthSqrd(){
        fp3 vec1 = new fp3(1,-2,3);
        fp expected = 14;

        fp actual = vec1.lengthSqrd();
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void TestMajor2Normal(){
        // Normal condition
        fp2 yMajor = new fp2(3,11);
        fp expected = 11;

        fp actual = yMajor.major();
        Assert.That(expected == actual, "Incorrect major calculation");
    }

    [Test]
    public void TestMajor2Tie(){
        // Tie
        fp2 tieMajor = new fp2(5,5);
        fp expected = 5;

        fp actual = tieMajor.major();
        Assert.AreEqual(expected, actual, "Incorrect major calculation");
    }

    [Test]
    public void TestMajor2Zero(){
        // Tie
        fp2 zeroMajor = new fp2(0,0);
        fp expected = 0;

        fp actual = zeroMajor.major();
        Assert.AreEqual(expected, actual, "Incorrect major calculation");
    }
    
    [Test]
    public void TestMajor3Normal(){
        // Normal condition
        fp3 yMajor = new fp3(3,11,.5m);
        fp expected = 11;

        fp actual = yMajor.major();
        Assert.That(expected == actual, "Incorrect major calculation");
    }

    [Test]
    public void TestMajor3Tie(){
        // Tie
        fp3 tieMajor = new fp3(0,5,5);
        fp expected = 5;

        fp actual = tieMajor.major();
        Assert.AreEqual(expected, actual, "Incorrect major calculation");
    }

    [Test]
    public void TestMajor3Zero(){
        // Tie
        fp3 zeroMajor = new fp3(0,0,0);
        fp expected = 0;

        fp actual = zeroMajor.major();
        Assert.AreEqual(expected, actual, "Incorrect major calculation");
    }

    [Test]
    public void TestMax(){
        fp small = -1;
        fp large = 1.1m;
        fp expected = 1.1m;

        fp actual = Utilities.max(small, large);
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void TestMin(){
        fp small = -1;
        fp large = 1.1m;
        fp expected = -1;

        fp actual = Utilities.min(small, large);
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void TestMultiplyQQ(){
        fpq q1 = new fpq(1,2,3,1);
        fpq q2 = new fpq(3,2,1,1);
        fpq expected = new fpq(0,12,0,-9);

        fpq actual = q1.multiply(q2);
        Assert.AreEqual(expected, actual);
    }
    [Test]
    public void TestMultiplyVQ(){
        fpq q = new fpq(0.382683m,0,0,0.92388m);
        fp3 v = new fp3(0,0,1);
        long expectedRawX = 0L; // 0
        long expectedRawY = -3036998604L; // -0.7071063397
        long expectedRawZ = 3037003342L; // 0.7071074429

        fp3 actual = v.multiply(q);
        Assert.That(
            actual.x.RawValue == expectedRawX
            && actual.y.RawValue == expectedRawY
            && actual.z.RawValue == expectedRawZ
        );
    }
    
    [Test]
    public void TestNormalizedOneDir(){
        fp3 vec1 = new fp3(123,0,0);
        fp3 expected = new fp3(1, 0, 0);

        fp3 actual = vec1.normalized();
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void TestNormalizedMultiDir(){
        fp3 vec1 = new fp3(1,-2,3);
        long expectedRawX = 1147878294L; // 0.2672612418
        long expectedRawY = -2295756587L; // -0.5345224838
        long expectedRawZ = 3443634881L; // 3443634880

        fp3 actual = vec1.normalized();
        Assert.That(
            actual.x.RawValue == expectedRawX
            && actual.y.RawValue == expectedRawY
            && actual.z.RawValue == expectedRawZ
        );
    }

    [Test]
    public void TestNormalizedZero(){
        fp3 vec1 = new fp3(0,0,0);
        fp3 expected = new fp3(0, 0, 0);

        fp3 actual = vec1.normalized();
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void TestNormalize(){
        fp3 vec = new fp3(1,-2,3);
        long expectedRawX = 1147878294L; // 0.267261242
        long expectedRawY = -2295756587L; // -0.5345224838
        long expectedRawZ = 3443634881L; //0.8017837259

        vec.normalize();
        Assert.That(
            vec.x.RawValue == expectedRawX
            && vec.y.RawValue == expectedRawY
            && vec.z.RawValue == expectedRawZ
        );
    }

    [Test]
    public void TestRoundToNearestQuarterOnQuater(){
        float f = 0.75f;
        fp expected = 0.75m;

        fp actual = f.roundToNearestQuarter();
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void TestRoundToNearestQuarterByQuater(){
        float f = 0.45f;
        fp expected = 0.5m;

        fp actual = f.roundToNearestQuarter();
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void TestSqrd(){
        fp num = 4;
        fp expected = 16;

        fp actual = num.sqrd();
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void TestSqrdNegative(){
        fp num = -10;
        fp expected = 100;

        fp actual = num.sqrd();
        Assert.AreEqual(expected, actual);
    }
    
    [Test]
    public void TestSqrt(){
        fp num = 16;
        fp expected = 4;

        fp actual = num.sqrt();
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void TestSqrtZero(){
        fp num = -1;
        Assert.Throws<ArgumentOutOfRangeException>(() => num.sqrt());
    }

    [Test]
    public void TestToVector3(){
        fp3 f3 = new fp3(1,2,3);
        Vector3 expected = new Vector3(1,2,3);

        Vector3 actual = f3.toVector3();
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void TestSerializeFp(){
        fp f1 = .2m;
        fp f2;
        NativeArray<byte> bytes;

        // Write value to memory stream
        using (var memoryStream = new MemoryStream()) {
            using (var writer = new BinaryWriter(memoryStream)) {
                writer.WriteFp(f1);
            }
            bytes = new NativeArray<byte>(memoryStream.ToArray(), Allocator.Persistent);
        }

        // Read value from memory stream
        using (var memoryStream = new MemoryStream(bytes.ToArray())) {
            using (var reader = new BinaryReader(memoryStream)) {
                f2 = reader.ReadFp();
            }
        }

        Assert.AreEqual(f1, f2);
    }
}
