using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics.FixedPoint;
using UnityEngine;
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

    [Test]
    public void TestWorldSerializeDeleteObject(){
        int preGOCount = GameObject.FindObjectsOfType<GameObject>().Length;
        // Create one world
        PhysWorld world = CreateSampleWorld();
        // Store game object count
        int originalGOCount = GameObject.FindObjectsOfType<GameObject>().Length;
        // Write it with binary writer
        NativeArray<byte> seriWorld = ToBytes(world);
        // Add a gameobject to the world
        world.CreateSphereObject(
            new fp3(0,10,0), 2, true, true, Constants.GRAVITY
        );
        // Assert that GO count went up by 1
        Assert.AreEqual(originalGOCount+1, GameObject.FindObjectsOfType<GameObject>().Length);

        int finalGOCount = -1;
        // Read down the old one again
        try{
            // Read what was written into a new world and copy it
            FromBytes(seriWorld, world);
            finalGOCount = GameObject.FindObjectsOfType<GameObject>().Length;
        }
        finally{
            // Dispose of the NativeArray when we're done with it
            if(seriWorld.IsCreated)
                seriWorld.Dispose();
        }

        // Assert GO count is the originial count
        Assert.AreEqual(originalGOCount, finalGOCount);
    }
}