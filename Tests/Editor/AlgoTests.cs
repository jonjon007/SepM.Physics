using NUnit.Framework;
using Unity.Mathematics.FixedPoint;
using SepM.Physics;

public class AlgoTests
{
    [Test]
    public void SphereSphereCollision_SamePos(){
        SphereCollider a = new SphereCollider(1);
        SphereCollider b = new SphereCollider(2);
        PhysTransform ta = new PhysTransform(new fp3(0,0,0));
        PhysTransform tb = new PhysTransform(new fp3(0,0,0));
        bool expected = true;

        CollisionPoints cp = a.TestCollision(ta, b, tb);
        Assert.AreEqual(expected, cp.HasCollision);
    }

    [Test]
    public void SphereSphereCollision_Overlap(){
        SphereCollider a = new SphereCollider(1);
        SphereCollider b = new SphereCollider(2);
        PhysTransform ta = new PhysTransform(new fp3(-1,1,0));
        PhysTransform tb = new PhysTransform(new fp3(1,0,0));
        bool expected = true;

        CollisionPoints cp = a.TestCollision(ta, b, tb);
        Assert.AreEqual(expected, cp.HasCollision);
    }

    [Test]
    public void SphereSphereCollision_Edge(){
        SphereCollider a = new SphereCollider(1);
        SphereCollider b = new SphereCollider(1);
        PhysTransform ta = new PhysTransform(new fp3(-1,0,0));
        PhysTransform tb = new PhysTransform(new fp3(1,0,0));
        bool expected = true;

        CollisionPoints cp = a.TestCollision(ta, b, tb);
        Assert.AreEqual(expected, cp.HasCollision);
    }

    [Test]
    public void SphereSphereCollision_NotTouching(){
        SphereCollider a = new SphereCollider(1);
        SphereCollider b = new SphereCollider(1);
        PhysTransform ta = new PhysTransform(new fp3(-1.1m,0,0));
        PhysTransform tb = new PhysTransform(new fp3(1,0,0));
        bool expected = false;

        CollisionPoints cp = a.TestCollision(ta, b, tb);
        Assert.AreEqual(expected, cp.HasCollision);
    }

    [Test]
    public void SpherePlaneCollision_SamePos(){
        SphereCollider a = new SphereCollider(1);
        PlaneCollider b = new PlaneCollider(new fp3(0,1,0), 2);
        PhysTransform ta = new PhysTransform(new fp3(0,0,0));
        PhysTransform tb = new PhysTransform(new fp3(0,0,0));
        bool expected = true;

        CollisionPoints cp = a.TestCollision(ta, b, tb);
        Assert.AreEqual(expected, cp.HasCollision);
    }

    [Test]
    public void SpherePlaneCollision_Overlap(){
        SphereCollider a = new SphereCollider(1);
        PlaneCollider b = new PlaneCollider(new fp3(0,1,0), 2);
        // Planes are infinite
        PhysTransform ta = new PhysTransform(new fp3(5,.5m,0));
        PhysTransform tb = new PhysTransform(new fp3(0,0,0));
        bool expected = true;

        CollisionPoints cp = a.TestCollision(ta, b, tb);
        Assert.AreEqual(expected, cp.HasCollision);
    }

    [Test]
    public void SpherePlaneCollision_Edge(){
        SphereCollider a = new SphereCollider(1);
        PlaneCollider b = new PlaneCollider(new fp3(1,0,0), 2);
        PhysTransform ta = new PhysTransform(new fp3(0,0,0));
        PhysTransform tb = new PhysTransform(new fp3(0,0,.5m));
        bool expected = true;

        CollisionPoints cp = a.TestCollision(ta, b, tb);
        Assert.AreEqual(expected, cp.HasCollision);
    }

    [Test]
    public void SpherePlaneCollision_NotTouching(){
        SphereCollider a = new SphereCollider(1);
        PlaneCollider b = new PlaneCollider(new fp3(1,0,0), 2);
        PhysTransform ta = new PhysTransform(new fp3(0,0,0));
        PhysTransform tb = new PhysTransform(new fp3(0,-1.1m,0));
        bool expected = true;

        CollisionPoints cp = a.TestCollision(ta, b, tb);
        Assert.AreEqual(expected, cp.HasCollision);
    }
    
    [Test]
    public void SphereCapsuleCollision_SamePos(){
        SphereCollider a = new SphereCollider(1);
        CapsuleCollider b = new CapsuleCollider(fp3.zero, 1, 2);
        PhysTransform ta = new PhysTransform(new fp3(0,0,0));
        PhysTransform tb = new PhysTransform(new fp3(0,0,0));
        bool expected = true;

        CollisionPoints cp = a.TestCollision(ta, b, tb);
        Assert.AreEqual(expected, cp.HasCollision);
    }

    [Test]
    public void SphereCapsuleCollision_Overlap(){
        SphereCollider a = new SphereCollider(1);
        CapsuleCollider b = new CapsuleCollider(fp3.zero, 1, 2);
        PhysTransform ta = new PhysTransform(new fp3(0,0,.5m));
        PhysTransform tb = new PhysTransform(new fp3(0,0,0));
        bool expected = true;

        CollisionPoints cp = a.TestCollision(ta, b, tb);
        Assert.AreEqual(expected, cp.HasCollision);
    }

    [Test]
    public void SphereCapsuleCollision_Edge(){
        SphereCollider a = new SphereCollider(1);
        CapsuleCollider b = new CapsuleCollider(fp3.zero, 1, 2);
        PhysTransform ta = new PhysTransform(new fp3(0,0,0));
        PhysTransform tb = new PhysTransform(new fp3(0,1.5m,0));
        bool expected = true;

        CollisionPoints cp = a.TestCollision(ta, b, tb);
        Assert.AreEqual(expected, cp.HasCollision);
    }

    [Test]
    public void SphereCapsuleCollision_NotTouching(){
        SphereCollider a = new SphereCollider(1);
        CapsuleCollider b = new CapsuleCollider(fp3.zero, 1, 2);
        PhysTransform ta = new PhysTransform(new fp3(0,0,0));
        PhysTransform tb = new PhysTransform(new fp3(0,3,0));
        bool expected = false;

        CollisionPoints cp = a.TestCollision(ta, b, tb);
        Assert.AreEqual(expected, cp.HasCollision);
    }

    [Test]
    public void SphereAABoxCollision_SamePos(){
        SphereCollider a = new SphereCollider(1);
        AABBoxCollider b = new AABBoxCollider(new fp3(-1,-1,-1), new fp3(1,1,1));
        PhysTransform ta = new PhysTransform(new fp3(0,0,0));
        PhysTransform tb = new PhysTransform(new fp3(0,0,0));
        bool expected = true;

        CollisionPoints cp = a.TestCollision(ta, b, tb);
        Assert.AreEqual(expected, cp.HasCollision);
    }

    [Test]
    public void SphereAABoxCollision_Overlap(){
        SphereCollider a = new SphereCollider(1);
        AABBoxCollider b = new AABBoxCollider(new fp3(-1,-.5m,-1), new fp3(1,1.5m,1));
        PhysTransform ta = new PhysTransform(new fp3(0,0,0));
        PhysTransform tb = new PhysTransform(new fp3(0,0,0));
        bool expected = true;

        CollisionPoints cp = a.TestCollision(ta, b, tb);
        Assert.AreEqual(expected, cp.HasCollision);
    }

    [Test]
    public void SphereAABoxCollision_Edge(){
        SphereCollider a = new SphereCollider(1);
        AABBoxCollider b = new AABBoxCollider(new fp3(-1,-1,-1), new fp3(1,1,1));
        PhysTransform ta = new PhysTransform(new fp3(0,0,0));
        PhysTransform tb = new PhysTransform(new fp3(0,2,0));
        bool expected = true;

        CollisionPoints cp = a.TestCollision(ta, b, tb);
        Assert.AreEqual(expected, cp.HasCollision);
    }

    [Test]
    public void SphereAABoxCollision_NotTouching(){
        SphereCollider a = new SphereCollider(1);
        AABBoxCollider b = new AABBoxCollider(new fp3(-1,-1,-1), new fp3(1,1,1));
        PhysTransform ta = new PhysTransform(new fp3(3,0,0));
        PhysTransform tb = new PhysTransform(new fp3(0,0,0));
        bool expected = false;

        CollisionPoints cp = a.TestCollision(ta, b, tb);
        Assert.AreEqual(expected, cp.HasCollision);
    }

    [Test]
    public void CapsuleCapsuleCollision_SamePos(){
        CapsuleCollider a = new CapsuleCollider(fp3.zero, 2, 5);
        CapsuleCollider b = new CapsuleCollider(fp3.zero, 1, 2);
        PhysTransform ta = new PhysTransform(new fp3(0,0,0));
        PhysTransform tb = new PhysTransform(new fp3(0,0,0));
        bool expected = true;

        CollisionPoints cp = a.TestCollision(ta, b, tb);
        Assert.AreEqual(expected, cp.HasCollision);
    }

    [Test]
    public void CapsuleCapsuleCollision_Overlap(){
        CapsuleCollider a = new CapsuleCollider(fp3.zero, 2, 5);
        CapsuleCollider b = new CapsuleCollider(fp3.zero, 1, 2);
        PhysTransform ta = new PhysTransform(new fp3(1,1.5m,0));
        PhysTransform tb = new PhysTransform(new fp3(0,0,0));
        bool expected = true;

        CollisionPoints cp = a.TestCollision(ta, b, tb);
        Assert.AreEqual(expected, cp.HasCollision);
    }

    [Test]
    public void CapsuleCapsuleCollision_Edge(){
        CapsuleCollider a = new CapsuleCollider(fp3.zero, 2, 6);
        CapsuleCollider b = new CapsuleCollider(fp3.zero, 1, 2);
        PhysTransform ta = new PhysTransform(new fp3(0,4,0));
        PhysTransform tb = new PhysTransform(new fp3(0,0,0));
        bool expected = true;

        CollisionPoints cp = a.TestCollision(ta, b, tb);
        Assert.AreEqual(expected, cp.HasCollision);
    }

    [Test]
    public void CapsuleCapsuleCollision_NotTouching(){
        CapsuleCollider a = new CapsuleCollider(fp3.zero, 2, 5);
        CapsuleCollider b = new CapsuleCollider(fp3.zero, 1, 2);
        PhysTransform ta = new PhysTransform(new fp3(0,4,0));
        PhysTransform tb = new PhysTransform(new fp3(0,0,0));
        bool expected = false;

        CollisionPoints cp = a.TestCollision(ta, b, tb);
        Assert.AreEqual(expected, cp.HasCollision);
    }

    [Test]
    public void AABBoxAABoxCollision_SamePos(){
        AABBoxCollider a = new AABBoxCollider(new fp3(-1,-1,-1), new fp3(1,1,1));
        AABBoxCollider b = new AABBoxCollider(new fp3(-1,-1,-1), new fp3(1,1,1));
        PhysTransform ta = new PhysTransform(new fp3(0,0,0));
        PhysTransform tb = new PhysTransform(new fp3(0,0,0));
        bool expected = true;

        CollisionPoints cp = a.TestCollision(ta, b, tb);
        Assert.AreEqual(expected, cp.HasCollision);
    }

    [Test]
    public void AABBoxAABoxCollision_Overlap(){
        AABBoxCollider a = new AABBoxCollider(new fp3(-1,-1,-1), new fp3(1,1,1));
        AABBoxCollider b = new AABBoxCollider(new fp3(-2,0,-1), new fp3(2,.5m,1));
        PhysTransform ta = new PhysTransform(new fp3(0,0,0));
        PhysTransform tb = new PhysTransform(new fp3(0,0,0));
        bool expected = true;

        CollisionPoints cp = a.TestCollision(ta, b, tb);
        Assert.AreEqual(expected, cp.HasCollision);
    }

    [Test]
    public void AABBoxAABoxCollision_Edge(){
        AABBoxCollider a = new AABBoxCollider(new fp3(1,1,1), new fp3(2,2,2));
        AABBoxCollider b = new AABBoxCollider(new fp3(-1,-1,-1), new fp3(1,1,1));
        PhysTransform ta = new PhysTransform(new fp3(0,0,0));
        PhysTransform tb = new PhysTransform(new fp3(0,2,0));
        bool expected = true;

        CollisionPoints cp = a.TestCollision(ta, b, tb);
        Assert.AreEqual(expected, cp.HasCollision);
    }

    [Test]
    public void AABBoxAABox_NotTouching(){
        AABBoxCollider a = new AABBoxCollider(new fp3(2,2,2), new fp3(3,3,3));
        AABBoxCollider b = new AABBoxCollider(new fp3(-1,-1,-1), new fp3(1,1,1));
        PhysTransform ta = new PhysTransform(new fp3(3,0,0));
        PhysTransform tb = new PhysTransform(new fp3(0,0,0));
        bool expected = false;

        CollisionPoints cp = a.TestCollision(ta, b, tb);
        Assert.AreEqual(expected, cp.HasCollision);
    }

    [Test]
    public void Raycast_AABB_Touching(){
        AABBoxCollider coll = new AABBoxCollider(new fp3(-1,-1,-1), new fp3(1,1,1));
        bool expected = true;

        CollisionPoints actual = algo.Raycast(coll, new fp3(0,2,0), new fp3(0,-1,0));
        Assert.AreEqual(expected, actual.HasCollision);
    }

    [Test]
    public void Raycast_AABB_Edge(){
        AABBoxCollider coll = new AABBoxCollider(new fp3(-1,-1,-1), new fp3(1,1,1));
        bool expected = true;

        CollisionPoints actual = algo.Raycast(coll, new fp3(1,1,0), new fp3(-1,0,0));
        Assert.AreEqual(expected, actual.HasCollision);
    }

    [Test]
    public void Raycast_AABB_Non_Unit_Vec(){
        AABBoxCollider coll = new AABBoxCollider(new fp3(-1,-1,-1), new fp3(1,1,1));
        bool expected = true;

        CollisionPoints actual = algo.Raycast(coll, new fp3(0,2,0), new fp3(1,-1,0));
        Assert.AreEqual(expected, actual.HasCollision);
    }

    [Test]
    public void Raycast_AABB_Not_Touching(){
        AABBoxCollider coll = new AABBoxCollider(new fp3(-1,-1,-1), new fp3(1,1,1));
        bool expected = false;

        CollisionPoints actual = algo.Raycast(coll, new fp3(2,2,0), new fp3(0,-1,0));
        Assert.AreEqual(expected, actual.HasCollision);
    }

    [Test]
    public void Raycast_AABB_Too_Short(){
        AABBoxCollider coll = new AABBoxCollider(new fp3(-1,-1,-1), new fp3(1,1,1));
        bool expected = false;

        CollisionPoints actual = algo.Raycast(coll, new fp3(0,2.5m,0), new fp3(0,-1,0));
        Assert.AreEqual(expected, actual.HasCollision);
    }

    [Test]
    public void Raycast_AABB_Phys_Obj_Non_Unit_Vec(){
        PhysObject p_obj = new PhysObject(new fp3(-1,0,0));
        AABBoxCollider coll = new AABBoxCollider(new fp3(0,-1,-1), new fp3(2,1,1));
        p_obj.Coll = coll;
        bool expected = true;

        CollisionPoints actual = algo.Raycast(p_obj, new fp3(0,2,0), new fp3(1,-1,0));
        Assert.AreEqual(expected, actual.HasCollision);
    }

    [Test]
    public void Raycast_No_Coll_Phys_Obj(){
        PhysObject p_obj = new PhysObject(new fp3(0,0,0));
        bool expected = false;

        CollisionPoints actual = algo.Raycast(p_obj, new fp3(0,2,0), new fp3(1,-1,0));
        Assert.AreEqual(expected, actual.HasCollision);
    }

    [Test]
    public void Raycast_AABB_Right_Layer(){
        AABBoxCollider coll = new AABBoxCollider(new fp3(-1,-1,-1), new fp3(1,1,1), Constants.coll_layers.ground);
        bool expected = true;

        CollisionPoints actual = algo.Raycast(coll, new fp3(0,2,0), new fp3(0,-1,0), Constants.layer_ground);
        Assert.AreEqual(expected, actual.HasCollision);
    }

    [Test]
    public void Raycast_AABB_Wrong_Layer(){
        AABBoxCollider coll = new AABBoxCollider(new fp3(-1,-1,-1), new fp3(1,1,1), Constants.coll_layers.ground);
        bool expected = false;

        CollisionPoints actual = algo.Raycast(coll, new fp3(0,2,0), new fp3(0,-1,0), Constants.layer_wall);
        Assert.AreEqual(expected, actual.HasCollision);
    }
}
