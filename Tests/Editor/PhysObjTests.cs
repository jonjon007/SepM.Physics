using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.Collections;
using SepM.Physics;

partial class PhysObjTests
{
    [Test]
    public void TestCollisionMatrixDefault(){
        CollisionMatrix matrix = new CollisionMatrix();
        bool result = matrix.CanLayersCollide(Constants.coll_layers.player, Constants.coll_layers.player);
        Assert.IsTrue(result);
    }

    [Test]
    public void TestCollisionMatrixAvoidOtherLayer(){
        CollisionMatrix matrix = new CollisionMatrix();
        matrix.SetLayerCollisions(Constants.coll_layers.player, Constants.coll_layers.noPlayer, false);
        bool result1 = matrix.CanLayersCollide(Constants.coll_layers.player, Constants.coll_layers.noPlayer);
        bool result2 = matrix.CanLayersCollide(Constants.coll_layers.player, Constants.coll_layers.player);
        Assert.IsFalse(result1);
        Assert.IsTrue(result2);
    }

    [Test]
    public void TestCollisionMatrixAvoidSameLayer(){
        CollisionMatrix matrix = new CollisionMatrix();
        matrix.SetLayerCollisions(Constants.coll_layers.player, Constants.coll_layers.player, false);
        bool result1 = matrix.CanLayersCollide(Constants.coll_layers.player, Constants.coll_layers.player);
        bool result2 = matrix.CanLayersCollide(Constants.coll_layers.player, Constants.coll_layers.ground);
        Assert.IsFalse(result1);
        Assert.IsTrue(result2);
    }

    [Test]
    public void TestWorldSerialize(){
        bool sameHash = false;
        List<Tuple<int, uint>> objsMapIds = new List<Tuple<int, uint>>();

        PhysObject.CurrentInstanceId = 0; // Reset instance id sequence
        PhysWorld wStart = CreateSampleWorld();
        NativeArray<byte> seriWorld = ToBytes(wStart);
        try{
            // Read what was written into a new world and copy it
            PhysWorld wFinish = new PhysWorld();
            FromBytes(seriWorld, wFinish);
            sameHash = wStart.GetHashCode() == wFinish.GetHashCode();
            objsMapIds = wFinish.objectsMap.Select(t =>
                new Tuple<int, uint>(t.Item1.GetInstanceID(), t.Item2.InstanceId)
            ).ToList();
        }
        finally{
            // Dispose of the NativeArray when we're done with it
            if(seriWorld.IsCreated)
                seriWorld.Dispose();
        }

        // Check hash
        Assert.IsTrue(sameHash);
        // Check instance ids
        Assert.AreEqual(0, objsMapIds[0].Item2);
        Assert.AreEqual(1, objsMapIds[1].Item2);
        Assert.AreEqual(2, objsMapIds[2].Item2);
    }
}