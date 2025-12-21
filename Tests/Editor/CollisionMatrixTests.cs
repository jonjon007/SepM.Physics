using NUnit.Framework;
using SepM.Physics;
using Unity.Collections;
using UnityEngine;

public class CollisionMatrixTests : MonoBehaviour
{
    [Test]
    public void TestCollisionPointsSerialize()
    {
        bool sameHash = false;

        CollisionMatrix start = new CollisionMatrix();

        // Make it so that players can't collide with noPlayer layer
        start.SetLayerCollisions(Constants.coll_layers.player, Constants.coll_layers.noPlayer, false);
        // Make it so that hitboxes can't collide with anything layer
        start.SetLayerCollisions(Constants.coll_layers.hitbox, Constants.coll_layers.player, false);
        start.SetLayerCollisions(Constants.coll_layers.hitbox, Constants.coll_layers.noPlayer, false);
        start.SetLayerCollisions(Constants.coll_layers.hitbox, Constants.coll_layers.ground, false);

        NativeArray<byte> serialized = TestUtils.ToBytes(start);

        try
        {
            CollisionMatrix finish = new CollisionMatrix();
            TestUtils.FromBytes(serialized, finish);
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
}
