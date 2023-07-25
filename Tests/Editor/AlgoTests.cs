using NUnit.Framework;
using Unity.Mathematics.FixedPoint;
using SepM.Math;
using SepM.Physics;
using SepM.Utils;

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
    public void SphereSphereCollision_ChildrenTouching(){
        SphereCollider a = new SphereCollider(1);
        SphereCollider b = new SphereCollider(2);
        PhysTransform parent_ta = new PhysTransform(new fp3(-10,0,0));
        PhysTransform parent_tb = new PhysTransform(new fp3(10,0,0));
        PhysTransform child_ta = new PhysTransform(new fp3(10,0,0));
        PhysTransform child_tb = new PhysTransform(new fp3(-10,0,0));
        child_ta.SetParent(parent_ta);
        child_tb.SetParent(parent_tb);
        bool expected = true;

        CollisionPoints cp = a.TestCollision(child_ta, b, child_tb);
        Assert.AreEqual(expected, cp.HasCollision);
    }

    [Test]
    public void SphereSphereCollision_ChildrenNotTouching(){
        SphereCollider a = new SphereCollider(1);
        SphereCollider b = new SphereCollider(2);
        PhysTransform parent_ta = new PhysTransform(new fp3(-10,0,0));
        PhysTransform parent_tb = new PhysTransform(new fp3(10,0,0));
        PhysTransform child_ta = new PhysTransform(new fp3(0,0,0));
        PhysTransform child_tb = new PhysTransform(new fp3(0,0,0));
        child_ta.SetParent(parent_ta);
        child_tb.SetParent(parent_tb);
        bool expected = false;

        CollisionPoints cp = a.TestCollision(child_ta, b, child_tb);
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
    public void SphereCapsuleCollision_ChildrenTouching(){
        SphereCollider a = new SphereCollider(1);
        CapsuleCollider b = new CapsuleCollider(fp3.zero, 1, 2);
        PhysTransform parent_ta = new PhysTransform(new fp3(-10,0,0));
        PhysTransform parent_tb = new PhysTransform(new fp3(10,0,0));
        PhysTransform child_ta = new PhysTransform(new fp3(10,0,0));
        PhysTransform child_tb = new PhysTransform(new fp3(-10,0,0));
        child_ta.SetParent(parent_ta);
        child_tb.SetParent(parent_tb);
        bool expected = true;

        CollisionPoints cp = a.TestCollision(child_ta, b, child_tb);
        Assert.AreEqual(expected, cp.HasCollision);
    }

    [Test]
    public void SphereCapsuleCollision_ChildrenNotTouching(){
        SphereCollider a = new SphereCollider(1);
        CapsuleCollider b = new CapsuleCollider(fp3.zero, 1, 2);
        PhysTransform parent_ta = new PhysTransform(new fp3(-10,0,0));
        PhysTransform parent_tb = new PhysTransform(new fp3(10,0,0));
        PhysTransform child_ta = new PhysTransform(new fp3(0,0,0));
        PhysTransform child_tb = new PhysTransform(new fp3(0,0,0));
        child_ta.SetParent(parent_ta);
        child_tb.SetParent(parent_tb);
        bool expected = false;

        CollisionPoints cp = a.TestCollision(child_ta, b, child_tb);
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
    public void SphereAABoxCollision_ChildrenTouching(){
        SphereCollider a = new SphereCollider(1);
        AABBoxCollider b = new AABBoxCollider(new fp3(-1,-1,-1), new fp3(1,1,1));
        PhysTransform parent_ta = new PhysTransform(new fp3(-10,0,0));
        PhysTransform parent_tb = new PhysTransform(new fp3(10,0,0));
        PhysTransform child_ta = new PhysTransform(new fp3(10,0,0));
        PhysTransform child_tb = new PhysTransform(new fp3(-10,0,0));
        child_ta.SetParent(parent_ta);
        child_tb.SetParent(parent_tb);
        bool expected = true;

        CollisionPoints cp = a.TestCollision(child_ta, b, child_tb);
        Assert.AreEqual(expected, cp.HasCollision);
    }

    [Test]
    public void SphereAABoxCollision_ChildrenNotTouching(){
        SphereCollider a = new SphereCollider(1);
        AABBoxCollider b = new AABBoxCollider(new fp3(-1,-1,-1), new fp3(1,1,1));
        PhysTransform parent_ta = new PhysTransform(new fp3(-10,0,0));
        PhysTransform parent_tb = new PhysTransform(new fp3(10,0,0));
        PhysTransform child_ta = new PhysTransform(new fp3(0,0,0));
        PhysTransform child_tb = new PhysTransform(new fp3(0,0,0));
        child_ta.SetParent(parent_ta);
        child_tb.SetParent(parent_tb);
        bool expected = false;

        CollisionPoints cp = a.TestCollision(child_ta, b, child_tb);
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
    public void CapsuleCapsuleCollision_ChildrenTouching(){
        CapsuleCollider a = new CapsuleCollider(fp3.zero, 2, 5);
        CapsuleCollider b = new CapsuleCollider(fp3.zero, 1, 2);
        PhysTransform parent_ta = new PhysTransform(new fp3(-10,0,0));
        PhysTransform parent_tb = new PhysTransform(new fp3(10,0,0));
        PhysTransform child_ta = new PhysTransform(new fp3(10,0,0));
        PhysTransform child_tb = new PhysTransform(new fp3(-10,0,0));
        child_ta.SetParent(parent_ta);
        child_tb.SetParent(parent_tb);
        bool expected = true;

        CollisionPoints cp = a.TestCollision(child_ta, b, child_tb);
        Assert.AreEqual(expected, cp.HasCollision);
    }

    [Test]
    public void CapsuleCapsuleCollision_ChildrenNotTouching(){
        CapsuleCollider a = new CapsuleCollider(fp3.zero, 2, 5);
        CapsuleCollider b = new CapsuleCollider(fp3.zero, 1, 2);
        PhysTransform parent_ta = new PhysTransform(new fp3(-10,0,0));
        PhysTransform parent_tb = new PhysTransform(new fp3(10,0,0));
        PhysTransform child_ta = new PhysTransform(new fp3(0,0,0));
        PhysTransform child_tb = new PhysTransform(new fp3(0,0,0));
        child_ta.SetParent(parent_ta);
        child_tb.SetParent(parent_tb);
        bool expected = false;

        CollisionPoints cp = a.TestCollision(child_ta, b, child_tb);
        Assert.AreEqual(expected, cp.HasCollision);
    }

    [Test]
    public void CapsuleAABoxCollision_SamePos(){
        CapsuleCollider a = new CapsuleCollider(new fp3(-1,-1,-1), 1, 1);
        AABBoxCollider b = new AABBoxCollider(new fp3(-1,-1,-1), new fp3(5,5,5), true);
        PhysTransform ta = new PhysTransform(new fp3(0,0,0));
        PhysTransform tb = new PhysTransform(new fp3(0,0,0));
        bool expected = true;

        CollisionPoints cp = a.TestCollision(ta, b, tb);
        Assert.AreEqual(expected, cp.HasCollision);
    }

    [Test]
    public void CapsuleAABoxCollision_Overlap(){
        CapsuleCollider a = new CapsuleCollider(new fp3(0), .5m, 2);
        AABBoxCollider b = new AABBoxCollider(new fp3(-2,0,-1), new fp3(2,.5m,1));
        PhysTransform ta = new PhysTransform(new fp3(0,0,0));
        PhysTransform tb = new PhysTransform(new fp3(0,0,0));
        bool expected = true;

        CollisionPoints cp = a.TestCollision(ta, b, tb);
        Assert.AreEqual(expected, cp.HasCollision);
    }

    [Test]
    public void CapsuleAABoxCollision_Edge(){
        CapsuleCollider a = new CapsuleCollider(new fp3(1.5m, 1, 1.5m), .5m, 2);
        AABBoxCollider b = new AABBoxCollider(new fp3(-1,-1,-1), new fp3(1,1,1));
        PhysTransform ta = new PhysTransform(new fp3(0,0,0));
        PhysTransform tb = new PhysTransform(new fp3(0,0,0));
        bool expected = true;

        CollisionPoints cp = a.TestCollision(ta, b, tb);
        Assert.AreEqual(expected, cp.HasCollision);
    }

    [Test]
    public void CapsuleAABoxCollision_NotTouching(){
        CapsuleCollider a = new CapsuleCollider(new fp3(0,2,0), .5m, 2);
        AABBoxCollider b = new AABBoxCollider(new fp3(-1,-1,-1), new fp3(1,-.5m,1));
        PhysTransform ta = new PhysTransform(new fp3(0,0,0));
        PhysTransform tb = new PhysTransform(new fp3(0,0,0));
        bool expected = false;

        CollisionPoints cp = a.TestCollision(ta, b, tb);
        Assert.AreEqual(expected, cp.HasCollision);
    }

    [Test]
    public void CapsuleAABoxCollision_ChildrenTouching(){
        CapsuleCollider a = new CapsuleCollider(new fp3(-1,-1,-1), 1, 1);
        AABBoxCollider b = new AABBoxCollider(new fp3(-1,-1,-1), new fp3(5,5,5), true);
        PhysTransform parent_ta = new PhysTransform(new fp3(-10,0,0));
        PhysTransform parent_tb = new PhysTransform(new fp3(10,0,0));
        PhysTransform child_ta = new PhysTransform(new fp3(10,0,0));
        PhysTransform child_tb = new PhysTransform(new fp3(-10,0,0));
        child_ta.SetParent(parent_ta);
        child_tb.SetParent(parent_tb);
        bool expected = true;

        CollisionPoints cp = a.TestCollision(child_ta, b, child_tb);
        Assert.AreEqual(expected, cp.HasCollision);
    }

    [Test]
    public void CapsuleAABoxCollision_ChildrenNotTouching(){
        CapsuleCollider a = new CapsuleCollider(new fp3(-1,-1,-1), 1, 1);
        AABBoxCollider b = new AABBoxCollider(new fp3(-1,-1,-1), new fp3(5,5,5), true);
        PhysTransform parent_ta = new PhysTransform(new fp3(-10,0,0));
        PhysTransform parent_tb = new PhysTransform(new fp3(10,0,0));
        PhysTransform child_ta = new PhysTransform(new fp3(0,0,0));
        PhysTransform child_tb = new PhysTransform(new fp3(0,0,0));
        child_ta.SetParent(parent_ta);
        child_tb.SetParent(parent_tb);
        bool expected = false;

        CollisionPoints cp = a.TestCollision(child_ta, b, child_tb);
        Assert.AreEqual(expected, cp.HasCollision);
    }

    [Test]
    public void AABBoxAABoxCollision_SamePos(){
        AABBoxCollider a = new AABBoxCollider(new fp3(-1,-1,-1), new fp3(1,1,1), true);
        AABBoxCollider b = new AABBoxCollider(new fp3(-1,-1,-1), new fp3(1,1,1), true);
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
        PhysTransform tb = new PhysTransform(new fp3(0,0,0));
        bool expected = true;

        CollisionPoints cp = a.TestCollision(ta, b, tb);
        Assert.AreEqual(expected, cp.HasCollision);
    }

    [Test]
    public void AABBoxAABoxCollision_NotTouching(){
        AABBoxCollider a = new AABBoxCollider(new fp3(10,-8.150096m,0), new fp3(1,1,1), true);
        AABBoxCollider b = new AABBoxCollider(new fp3(10,-10,0), new fp3(1,1,1), true);
        PhysTransform ta = new PhysTransform(new fp3(0,0,0));
        PhysTransform tb = new PhysTransform(new fp3(0,0,0));
        bool expected = false;

        CollisionPoints cp = a.TestCollision(ta, b, tb);
        Assert.AreEqual(expected, cp.HasCollision);
    }

    [Test]
    public void AABBoxAABoxCollision_ChildrenTouching(){
        AABBoxCollider a = new AABBoxCollider(new fp3(-1,-1,-1), new fp3(1,1,1), true);
        AABBoxCollider b = new AABBoxCollider(new fp3(-1,-1,-1), new fp3(1,1,1), true);
        PhysTransform parent_ta = new PhysTransform(new fp3(-10,0,0));
        PhysTransform parent_tb = new PhysTransform(new fp3(10,0,0));
        PhysTransform child_ta = new PhysTransform(new fp3(10,0,0));
        PhysTransform child_tb = new PhysTransform(new fp3(-10,0,0));
        child_ta.SetParent(parent_ta);
        child_tb.SetParent(parent_tb);
        bool expected = true;

        CollisionPoints cp = a.TestCollision(child_ta, b, child_tb);
        Assert.AreEqual(expected, cp.HasCollision);
    }

    [Test]
    public void AABBoxAABoxCollision_ChildrenNotTouching(){
        AABBoxCollider a = new AABBoxCollider(new fp3(-1,-1,-1), new fp3(1,1,1), true);
        AABBoxCollider b = new AABBoxCollider(new fp3(-1,-1,-1), new fp3(1,1,1), true);
        PhysTransform parent_ta = new PhysTransform(new fp3(-10,0,0));
        PhysTransform parent_tb = new PhysTransform(new fp3(10,0,0));
        PhysTransform child_ta = new PhysTransform(new fp3(0,0,0));
        PhysTransform child_tb = new PhysTransform(new fp3(0,0,0));
        child_ta.SetParent(parent_ta);
        child_tb.SetParent(parent_tb);
        bool expected = false;

        CollisionPoints cp = a.TestCollision(child_ta, b, child_tb);
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

    [Test]
    public void InverseTransformPoint(){
        UnityEngine.GameObject go = UnityEngine.GameObject.CreatePrimitive(UnityEngine.PrimitiveType.Cube);
        go.transform.position = new UnityEngine.Vector3(1,0,0);
        UnityEngine.Vector3 otherV3 = new UnityEngine.Vector3(0,1,0);
        UnityEngine.Vector3 tp = go.transform.InverseTransformPoint(new UnityEngine.Vector3(0,1,0));

        long expectedRawX;
        long expectedRawY;
        long expectedRawZ;
        fp3 actual;
        PhysObject po = new PhysObject(fp3.zero);
        fp3 otherPos;

        po.Transform.Position = new fp3(1,0,0);
        otherPos = new fp3(0,1,0);
        expectedRawX = -4294967296L; //-1
        expectedRawY = 4294967296L; //1
        expectedRawZ = 0; //0
        actual = po.Transform.inverseTransformPoint(otherPos);

        Assert.That(
            expectedRawX == actual.x.RawValue
            && expectedRawY == actual.y.RawValue
            && expectedRawZ == actual.z.RawValue
        );
    }

    [Test]
    public void InverseTransformPointRotated(){
        UnityEngine.GameObject go = UnityEngine.GameObject.CreatePrimitive(UnityEngine.PrimitiveType.Cube);
        go.transform.position = new UnityEngine.Vector3(1,0,0);
        go.transform.Rotate(0,90,0);
        UnityEngine.Vector3 otherV3 = new UnityEngine.Vector3(0,1,0);
        UnityEngine.Vector3 tp = go.transform.InverseTransformPoint(new UnityEngine.Vector3(0,1,0));

        long expectedRawX;
        long expectedRawY;
        long expectedRawZ;
        fp3 actual;
        PhysObject po = new PhysObject(fp3.zero);
        fp3 otherPos;

        po.Transform.Position = new fp3(1,0,0);
        po.Transform.Rotate(0,90,0);
        otherPos = new fp3(0,1,0);
        expectedRawX = -58L; //~0
        expectedRawY = 4294967296L; //~1
        expectedRawZ = -4294967292L; //~-1
        actual = po.Transform.inverseTransformPoint(otherPos);

        Assert.That(
            expectedRawX == actual.x.RawValue
            && expectedRawY == actual.y.RawValue
            && expectedRawZ == actual.z.RawValue
        );
    }
}
