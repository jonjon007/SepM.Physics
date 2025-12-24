using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics.FixedPoint;
using UnityEngine;
using SepM.Math;
using SepM.Physics;

partial class PhysObjTests
{
    PhysWorld world;

    [SetUp]
    public void Init()
    {
        world = new PhysWorld();
    }

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
    public void TestCollisionPointsSerialize()
    {
        bool sameHash = false;

        CollisionPoints start = new CollisionPoints()
        {
            A = new fp3(1,2,3),
            B = new fp3(3,1,2),
            Normal = new fp3(2,1,3),
            DepthSqrd = 30,
            HasCollision = true
        };
        NativeArray<byte> serialized = TestUtils.ToBytes(start);

        try
        {
            CollisionPoints finish = new CollisionPoints();
            finish = (CollisionPoints)TestUtils.FromBytes(serialized, finish);
            sameHash = start.Checksum == finish.Checksum;
        }
        finally
        {
            // Dispose of the NativeArray when we're done with it
            if (serialized.IsCreated)
                serialized.Dispose();
        }

        // Check hash
        Assert.IsTrue(sameHash);
    }

    [Test]
    public void TestPhysTransformSerialize()
    {
        bool sameHash = false;

        PhysTransform parent = new PhysTransform(id: 1);

        PhysTransform start = new PhysTransform(id: 1, new fp3(1,2,3), parent);
        start.Scale = new fp3(6, 5, 4);
        start.Rotation = new fpq(
            fp.FromRaw(2765320905L),
            fp.FromRaw(48809278L),
            fp.FromRaw(-41079300L),
            fp.FromRaw(3285677180L));

        NativeArray<byte> serialized = TestUtils.ToBytes(start);

        try
        {
            PhysTransform finish = new PhysTransform(id: 1);
            finish = (PhysTransform)TestUtils.FromBytes(serialized, finish);
            sameHash = start.Checksum == finish.Checksum;
        }
        finally
        {
            // Dispose of the NativeArray when we're done with it
            if (serialized.IsCreated)
                serialized.Dispose();
        }

        // Check hash
        Assert.IsTrue(sameHash);
    }

    [Test]
    public void TestPhysCollisionSerialize()
    {
        bool sameHash = false;

        var tuple1 = world.CreateAABBoxObject(
            new fp3(0, 10, 0), new fp3(1, 1, 1), true, true, Constants.GRAVITY * 2, Constants.coll_layers.player
        );
        var tuple2 = world.CreateAABBoxObject(
            new fp3(10, 0, 10), new fp3(1, 2, 1), true, true, Constants.GRAVITY * 2, Constants.coll_layers.player
        );
        CollisionPoints points = new CollisionPoints()
        {
            A = new fp3(1, 2, 3),
            B = new fp3(3, 1, 2),
            Normal = new fp3(2, 1, 3),
            DepthSqrd = 30,
            HasCollision = true
        };

        PhysCollision start = new PhysCollision()
        {
            ObjIdA = tuple1.Item2.InstanceId,
            ObjIdB = tuple2.Item2.InstanceId,
            Points = points,
        };
        NativeArray<byte> serialized = TestUtils.ToBytes(start);

        try
        {
            PhysCollision finish = new PhysCollision();
            finish = (PhysCollision)TestUtils.FromBytes(serialized, finish);
            sameHash = start.Checksum == finish.Checksum;
        }
        finally
        {
            // Dispose of the NativeArray when we're done with it
            if (serialized.IsCreated)
                serialized.Dispose();
        }

        // Check hash
        Assert.IsTrue(sameHash);
    }

    [Test]
    public void TestWorldPosition()
    {
        fp3 expected;
        fp3 actual;

        PhysTransform parent_t = new PhysTransform(id: 1, fp3.zero);
        PhysTransform child_t = new PhysTransform(id: 1, new fp3(0,0,10));
        child_t.SetParent(parent_t);
        expected = new fp3(0, 0, fp.FromRaw(42949672960L)); // 0,0,10
        actual = child_t.WorldPosition();
        Assert.AreEqual(expected, actual);

        parent_t.Rotate(new fp3(0, 90, 0));
        expected = new fp3(fp.FromRaw(42949672918L), 0, fp.FromRaw(566L)); // ~10,0,0
        actual = child_t.WorldPosition();
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void TestWorldRollbackAfterNewObject()
    {
        bool sameHash = false;

        world.ResetIDCounter();
        PhysWorld wStart = CreateSampleWorld();
        NativeArray<byte> seriWorld = ToBytes(wStart);
        try
        {
            // Read what was written into a new world and copy it
            PhysWorld wFinish = new PhysWorld();
            FromBytes(seriWorld, wFinish);

            // Add a new object
            wFinish.AddObject(new PhysObject(id: 0));

            // Then roll back
            FromBytes(seriWorld, wFinish);

            sameHash = wStart.Checksum == wFinish.Checksum;
        }
        finally
        {
            // Dispose of the NativeArray when we're done with it
            if (seriWorld.IsCreated)
                seriWorld.Dispose();
        }

        // Check hash
        Assert.IsTrue(sameHash);
    }


    [Test]
    public void TestWorldSerialize(){
        bool sameHash = false;

        world.ResetIDCounter();
        PhysWorld wStart = CreateSampleWorld();
        NativeArray<byte> seriWorld = ToBytes(wStart);
        try{
            // Read what was written into a new world and copy it
            PhysWorld wFinish = new PhysWorld();
            FromBytes(seriWorld, wFinish);
            sameHash = wStart.Checksum == wFinish.Checksum;
        }
        finally{
            // Dispose of the NativeArray when we're done with it
            if(seriWorld.IsCreated)
                seriWorld.Dispose();
        }

        // Check hash
        Assert.IsTrue(sameHash);
    }

    [Test]
    public void TestWorldSerializeCreateObjects(){
        int preGOCount = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None).Length;
        // Create one world
        PhysWorld world = CreateSampleWorld();
        // Store game object count
        int originalGOCount = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None).Length;
        // Write it with binary writer
        NativeArray<byte> seriWorld = ToBytes(world);

        //Clear the world
        world.CleanUp();
        // Assert that GO count is the same as it originally was
        Assert.AreEqual(preGOCount, GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None).Length);

        int finalGOCount = -1;
        // Read down the old one again
        try{
            // Read what was written into a new world and copy it
            FromBytes(seriWorld, world);
            finalGOCount = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None).Length;
        }
        finally{
            // Dispose of the NativeArray when we're done with it
            if(seriWorld.IsCreated)
                seriWorld.Dispose();
        }

        // Assert GO count is the originial count
        Assert.AreEqual(originalGOCount, finalGOCount);

        // Run a step without error to make sure the state is stable
        world.Step(fixedStep, 0);
        world.UpdateGameObjects();
    }

    [Test]
    public void TestWorldSerializeDeleteObject(){
        int preGOCount = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None).Length;
        // Create one world
        PhysWorld world = CreateSampleWorld();
        // Store game object count
        int originalGOCount = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None).Length;
        // Write it with binary writer
        NativeArray<byte> seriWorld = ToBytes(world);
        // Add a gameobject to the world
        world.CreateSphereObject(
            new fp3(0,10,0), 2, true, true, Constants.GRAVITY
        );
        // Assert that GO count went up by 1
        Assert.AreEqual(originalGOCount+1, GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None).Length);

        int finalGOCount = -1;
        // Read down the old one again
        try{
            // Read what was written into a new world and copy it
            FromBytes(seriWorld, world);
            finalGOCount = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None).Length;
        }
        finally{
            // Dispose of the NativeArray when we're done with it
            if(seriWorld.IsCreated)
                seriWorld.Dispose();
        }

        // Assert GO count is the originial count
        Assert.AreEqual(originalGOCount, finalGOCount);

        // Run a step without error to make sure the state is stable
        world.Step(fixedStep, 0);
        world.UpdateGameObjects();
    }
}