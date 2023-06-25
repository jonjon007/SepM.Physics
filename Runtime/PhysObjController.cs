using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics.FixedPoint;
using SepM.Physics;
using SepM.Utils;

public class PhysObjController : MonoBehaviour
{
    PhysObject physObject;
    // Start is called before the first frame update
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
        Vector3 newPos = this.physObject.Transform.Position.toVector3();
        this.gameObject.transform.position = newPos;
    }

    public void setPhysObject(PhysObject po){
        this.physObject = po;
    }
}
