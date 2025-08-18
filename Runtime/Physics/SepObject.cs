/* Attach this MonoBehaviour to an existing GameObject to convert it into SepM a PhysObject. Assign properties in Inspector or on creation.
 * After values are set, run Initialize() to attach the object to the given PhysWorld. */

using System;
using UnityEngine;
using Unity.Mathematics.FixedPoint;
using SepM.Utils;

namespace SepM.Physics {
    public class SepObject : MonoBehaviour {
        public enum eCollType { sphere = 0, aabb = 1, cylinder = 2, plane = 3 };
        public eCollType colliderType;
        public Constants.coll_layers colliderLayer;
        public PhysObject physObj;


        // Update is called once per frame
        public void Initialize(PhysWorld world) {
            physObj.InstanceId = world.IncrementIDCounter();
            physObj.Transform.InstanceId = physObj.InstanceId;

            // Set the PhysObject's position
            physObj.Transform.Position = new fp3(
                transform.position.x.roundToNearestQuarter(),
                transform.position.y.roundToNearestQuarter(),
                transform.position.z.roundToNearestQuarter()
            );

            // Based on type, create the appropriate collider
            if (colliderType == eCollType.aabb) {
                AABBoxCollider coll = new AABBoxCollider(
                    fp3.zero,
                    new fp3(
                        transform.localScale.x.roundToNearestQuarter(),
                        transform.localScale.y.roundToNearestQuarter(),
                        transform.localScale.z.roundToNearestQuarter()
                    ),
                    true,
                    Constants.coll_layers.ground
                );
                physObj.Coll = coll;
            }
            else {
                Debug.LogWarningFormat("No SepObject implementation for type '{0}'!", colliderType.ToString());
            }
            world.AddObject(physObj);

            // Get the result
            Tuple<GameObject, PhysObject> result = new Tuple<GameObject, PhysObject>(
                this.gameObject, physObj
            );

            // Add to list of the world's physics objects
            world.AssignGameObject(result.Item1, result.Item2);

            // Set the collision layer
            physObj.Coll.Layer = colliderLayer;
        }
    }
}