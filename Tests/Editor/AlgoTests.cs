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
}
