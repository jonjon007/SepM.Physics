using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics.FixedPoint;
using SepM.Physics;

public class PhysObjController : MonoBehaviour
{
    PhysObject physObject;
    // Start is called before the first frame update
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
        // TODO: Turn into a Utilities method
        Vector3 newPos = new Vector3(
            (float)this.physObject.Transform.Position.x,
            (float)this.physObject.Transform.Position.y,
            (float)this.physObject.Transform.Position.z
        );
        this.gameObject.transform.position = newPos;
    }

    public void setPhysObject(PhysObject po){
        this.physObject = po;
    }
}
