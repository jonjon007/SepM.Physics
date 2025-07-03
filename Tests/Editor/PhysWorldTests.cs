using System;
using NUnit.Framework;
using SepM.Physics;
using Unity.Collections;
using Unity.Mathematics.FixedPoint;
using UnityEngine;

public class PhysWorldTests : MonoBehaviour
{
    [Test]
    public void TestPhysWorldSerialize()
    {
        bool sameHash = false;

        PhysWorld start = new PhysWorld();

        // Make it so that players can't collide with noPlayer layer
        start.collisionMatrix.SetLayerCollisions(Constants.coll_layers.player, Constants.coll_layers.noPlayer, false);
        // Make it so that hitboxes can't collide with anything layer
        start.collisionMatrix.SetLayerCollisions(Constants.coll_layers.hitbox, Constants.coll_layers.player, false);
        start.collisionMatrix.SetLayerCollisions(Constants.coll_layers.hitbox, Constants.coll_layers.noPlayer, false);
        start.collisionMatrix.SetLayerCollisions(Constants.coll_layers.hitbox, Constants.coll_layers.ground, false);

        Tuple<GameObject, PhysObject> objTupleChildPersist = start.CreateAABBoxObject(
            fp3.zero, new fp3(1, 1, 1), true, true, Constants.GRAVITY * 2, Constants.coll_layers.player
        );

        Tuple<GameObject, PhysObject> objTupleChildDisappear = start.CreateAABBoxObject(
            new fp3(3, 1, 3), new fp3(3, 1, 3), true, true, Constants.GRAVITY * 2, Constants.coll_layers.player
        );

        Tuple<GameObject, PhysObject> objTupleParent = start.CreateCapsuleObject(
            fp3.zero + 1, 1, 2, true, true, Constants.GRAVITY * 10, Constants.coll_layers.player
        );

        // Make the boxes children of the capsule
        objTupleChildPersist.Item2.Transform.SetParent(objTupleParent.Item2.Transform);
        objTupleChildPersist.Item1.transform.SetParent(objTupleParent.Item1.transform);

        objTupleChildDisappear.Item2.Transform.SetParent(objTupleParent.Item2.Transform);
        objTupleChildDisappear.Item1.transform.SetParent(objTupleParent.Item1.transform);

        CollisionPoints points = new CollisionPoints()
        {
            A = new fp3(1, 2, 3),
            B = new fp3(3, 1, 2),
            Normal = new fp3(2, 1, 3),
            DepthSqrd = 30,
            HasCollision = true
        };

        PhysCollision collision = new PhysCollision()
        {
            ObjIdA = objTupleChildPersist.Item2.InstanceId,
            ObjIdB = objTupleParent.Item2.InstanceId,
            Points = points,
        };

        start.collisions.Add(collision);

        NativeArray<byte> serialized = TestUtils.ToBytes(start);

        // Then delete the disappear's gameobject
        GameObject.DestroyImmediate(objTupleChildDisappear.Item1);

        try
        {
            // Read what was written into new character and copy it
            PhysWorld finish = new PhysWorld();
            TestUtils.FromBytes(serialized, finish);
            sameHash = start.GetHashCode() == finish.GetHashCode();
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
    public void TestGetAllPhysObjectsInLayer()
    {
        PhysWorld world = new PhysWorld();

        Tuple<GameObject, PhysObject> objTup1 = world.CreateAABBoxObject(
            fp3.zero, new fp3(1, 1, 1), true, true, Constants.GRAVITY * 2, Constants.coll_layers.player
        );
        Tuple<GameObject, PhysObject> objTup2 = world.CreateAABBoxObject(
            fp3.zero, new fp3(1, 1, 1), true, true, Constants.GRAVITY * 2, Constants.coll_layers.wall
        );
        world.AddObject(new PhysObject(id: 0));

        var actual = world.GetAllPhysObjectsInLayer(Constants.layer_wall);

        Assert.That(actual.Count == 1);
        Assert.AreEqual(actual[0], objTup2.Item2);

    }

    [Test]
    public void TestGetPhysObjectByIndex()
    {
        PhysWorld world = new PhysWorld();

        Tuple<GameObject, PhysObject> objTupleChildPersist = world.CreateAABBoxObject(
            fp3.zero, new fp3(1, 1, 1), true, true, Constants.GRAVITY * 2, Constants.coll_layers.player
        );

        PhysObject expected = objTupleChildPersist.Item2;
        PhysObject actual = world.GetPhysObjectByIndex(0);

        // Check hash
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void TestGetPhysObjectByIndexMissing()
    {
        PhysWorld world = new PhysWorld();

        Tuple<GameObject, PhysObject> objTupleChildPersist = world.CreateAABBoxObject(
            fp3.zero, new fp3(1, 1, 1), true, true, Constants.GRAVITY * 2, Constants.coll_layers.player
        );

        PhysObject expected = null;
        PhysObject actual = world.GetPhysObjectByIndex(-1);

        // Check hash
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void TestGetPhysObjectIndexById()
    {
        PhysWorld world = new PhysWorld();

        Tuple<GameObject, PhysObject> objTupleChildPersist = world.CreateAABBoxObject(
            fp3.zero, new fp3(1, 1, 1), true, true, Constants.GRAVITY * 2, Constants.coll_layers.player
        );

        int expected = 0;
        int actual = world.GetPhysObjectIndexById(objTupleChildPersist.Item2.InstanceId);

        // Check hash
        Assert.AreEqual(expected, actual);
    }
}
