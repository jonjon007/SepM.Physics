using NUnit.Framework;
using Unity.Mathematics.FixedPoint;
using SepM.Physics;

public class PhysObjTests
{
    [Test]
    public void OnCollisionSuccess(){
        // TODO: Write actually useful tests

        PhysObject o = new PhysObject();
        o.OnCollision(new PhysCollision());       
    }
}