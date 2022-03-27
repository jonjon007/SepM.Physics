using System;
using UnityEngine;
using Unity.Mathematics.FixedPoint;
using SepM.Physics;

public class PersonController : MonoBehaviour
{
    [Header("Set in Inspector")]
    Rigidbody rb;
    public int jumpPower = 400;
    public int moveSpeed = 500;
    public PhysWorld physWorld;
    // Start is called before the first frame update
    void Awake(){
        rb = GetComponent<Rigidbody>();

        physWorld = new PhysWorld();

        
        
        // PhysObject newObj = new PhysObject(new fp3(0,0,14));
        // newObj.IsDynamic = true;
        // newObj.Gravity = fp3.zero;
        // newObj.IsKinematic = false;
        // // newObj.coll = new SepM.Physics.SphereCollider(5);
        // newObj.Coll = new SepM.Physics.CapsuleCollider(fp3.zero, 5, 6);
        // physWorld.AddObject(newObj);

        // Create stationary capsule
        Tuple<GameObject, PhysObject> capsuleTuple = physWorld.CreateCapsuleObject(
            fp3.zero, 5, 6, true, false, fp3.zero
        );
        PhysObjController capsuleObjCont = capsuleTuple.Item1.AddComponent<PhysObjController>();
        capsuleObjCont.setPhysObject(capsuleTuple.Item2);

        // Create falling sphere
        Tuple<GameObject, PhysObject> sphereTuple = physWorld.CreateSphereObject(
            new fp3(5,10,0), 2, true, true, Constants.GRAVITY);
        PhysObjController sphereObjCont = sphereTuple.Item1.AddComponent<PhysObjController>();
        sphereObjCont.setPhysObject(sphereTuple.Item2);

        // Create stationary box
        Tuple<GameObject, PhysObject> boxTuple = physWorld.CreateAABBoxObject(
            new fp3(10,-10,0), new fp3(5,2,3), true, false, fp3.zero);
        PhysObjController boxObjCont = boxTuple.Item1.AddComponent<PhysObjController>();
        boxObjCont.setPhysObject(boxTuple.Item2);



        // CollisionPoints collData = newObj2.Coll.TestCollision(newObj2.Transform, newObj.Coll, newObj.Transform);
        // bool isTouching = collData.HasCollision;
        // Debug.Log(isTouching);
    }

    // void Awake(){
    //     rb = GetComponent<Rigidbody>();

    //     physWorld = new PhysWorld();

    //     physWorld.AddSolver(new ImpulseSolver());
    //     physWorld.AddSolver(new SmoothPositionSolver());

    //     PhysObject newObj = new PhysObject(new fp3(0,0,14));
    //     newObj.IsDynamic = true;
    //     newObj.Gravity = fp3.zero;
    //     newObj.IsKinematic = false;
    //     // newObj.coll = new SepM.Physics.SphereCollider(5);
    //     newObj.Coll = new SepM.Physics.CapsuleCollider(fp3.zero, 5, 6);
    //     physWorld.AddObject(newObj);

    //     // PhysObject newObj2 = new PhysObject(new fp3(0,10,10));
    //     // newObj2.IsDynamic = true;
    //     // newObj2.IsKinematic = true;
    //     // newObj2.coll = new SepM.Physics.SphereCollider(2);
    //     // physWorld.AddObject(newObj2);

    //     PhysObject newObj2 = new PhysObject(new fp3(0,30,10));
    //     newObj2.IsDynamic = true;
    //     newObj2.IsKinematic = true;
    //     newObj2.Coll = new SepM.Physics.CapsuleCollider(fp3.zero, 2, 3);
    //     physWorld.AddObject(newObj2);

    //     PhysObject newObj3 = new PhysObject(new fp3(0,-3,0));
    //     newObj3.IsDynamic = true;
    //     newObj3.IsKinematic = false;
    //     newObj3.Gravity = fp3.zero;
    //     newObj3.Coll = new PlaneCollider(new fp3(1,1,0), 2);
    //     physWorld.AddObject(newObj3);

    //     CollisionPoints collData = newObj2.Coll.TestCollision(newObj2.Transform, newObj.Coll, newObj.Transform);
    //     bool isTouching = collData.HasCollision;
    //     Debug.Log(isTouching);
    // }


    // Update is called once per frame
    void Update()
    {
        //Move
        rb.AddForce(transform.forward*Input.GetAxis("Horizontal")*moveSpeed*Time.deltaTime);
        //Jump
        if(Input.GetKeyDown("space"))
            Jump();
        
        physWorld.Step((fp)Time.deltaTime);
    }

    void Jump(){
        rb.AddForce(Vector3.up*jumpPower);
    }
}
