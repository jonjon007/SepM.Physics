using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics.FixedPoint;
using SepM.Physics;
using SepM.Utils;

public class PhysWorld{
	private List<PhysObject> m_objects = new List<PhysObject>();
	private List<Solver> m_solvers = new List<Solver>();
 
    // By default, create Impulse and SmoothPosition solvers
    public PhysWorld(){
        AddSolver(new ImpulseSolver());
        AddSolver(new SmoothPositionSolver());
    }

    public Tuple<GameObject, PhysObject> CreateSphereObject(fp3 center, fp r, bool isDyn, bool isKin, fp3 g){
        // Set up the collider, the physics object, and the game object
        GameObject g_obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        // Destroy the default Unity collider
        GameObject.Destroy(g_obj.GetComponent<UnityEngine.SphereCollider>());
        SepM.Physics.SphereCollider coll = new SepM.Physics.SphereCollider(r);
        PhysObject p_obj = new PhysObject(center){
            IsDynamic = isDyn,
            IsKinematic = isKin,
            Gravity = g,    
            Coll = coll
        };

        // Update the GameObject
        float sphRadius = (float)coll.Radius*2;

        g_obj.transform.localPosition = center.toVector3();
        g_obj.transform.localScale = Vector3.one*sphRadius;

        Tuple<GameObject, PhysObject> result = new Tuple<GameObject, PhysObject>(
            g_obj, p_obj
        );

        // Add to list of the world's physics objects
        m_objects.Add(p_obj);
        
        return result;
    }

    public Tuple<GameObject, PhysObject> CreateCapsuleObject(fp3 center, fp r, fp h, bool isDyn, bool isKin, fp3 g){
        // Set up the collider, the physics object, and the game object
        GameObject g_obj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        // Destroy the default Unity collider
        GameObject.Destroy(g_obj.GetComponent<UnityEngine.CapsuleCollider>());
        SepM.Physics.CapsuleCollider coll = new SepM.Physics.CapsuleCollider(r, h);
        PhysObject p_obj = new PhysObject(center){
            IsDynamic = isDyn,
            IsKinematic = isKin,
            Gravity = g,
            Coll = coll
        };

        // Update the GameObject
        float capRadius = (float)coll.Radius*2;
        float capHeight = (float)coll.Height;

        g_obj.transform.localPosition = center.toVector3();
        g_obj.transform.localScale = new Vector3(capRadius, capHeight, capRadius);


        Tuple<GameObject, PhysObject> result = new Tuple<GameObject, PhysObject>(
            g_obj, p_obj
        );

        // Add to list of the world's physics objects
        m_objects.Add(p_obj);
        
        return result;
    }

    public Tuple<GameObject, PhysObject> CreateAABBoxObject(fp3 center, fp3 scale, bool isDyn, bool isKin, fp3 g){
        // Set up the collider, the physics object, and the game object
        GameObject g_obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        // Destroy the default Unity collider
        GameObject.Destroy(g_obj.GetComponent<UnityEngine.BoxCollider>());
        SepM.Physics.AABBoxCollider coll = new SepM.Physics.AABBoxCollider(center, scale, true);
        PhysObject p_obj = new PhysObject(center){
            IsDynamic = isDyn,
            IsKinematic = isKin,
            Gravity = g,
            Coll = coll
        };

        // Update the GameObject
        g_obj.transform.localPosition = center.toVector3();
        g_obj.transform.localScale = scale.toVector3();

        Tuple<GameObject, PhysObject> result = new Tuple<GameObject, PhysObject>(
            g_obj, p_obj
        );

        // Add to list of the world's physics objects
        m_objects.Add(p_obj);
        
        return result;
    }

    public void AddObject (PhysObject obj) {
        GameObject u_obj;
        if(obj.Coll is SepM.Physics.SphereCollider){
            u_obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            float sphRadius = (float)((SepM.Physics.SphereCollider)obj.Coll).Radius*2;
            u_obj.transform.localScale = Vector3.one*sphRadius;
        }
        else if(obj.Coll is SepM.Physics.CapsuleCollider){
            u_obj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            float capRadius = (float)((SepM.Physics.CapsuleCollider)obj.Coll).Radius*2;
            float capHeight = (float)((SepM.Physics.CapsuleCollider)obj.Coll).Height;
            u_obj.transform.localScale = new Vector3(capRadius, capHeight, capRadius);
        }
        else{
            u_obj = GameObject.CreatePrimitive(PrimitiveType.Plane);
            PlaneCollider c = (PlaneCollider)obj.Coll;
            Vector3 planeDir = c.Normal.toVector3();
            float scale = (float)c.Distance/10;
            u_obj.transform.Rotate(planeDir);
            u_obj.transform.localScale = Vector3.one*scale;
        }
        PhysObjController u_objCont = u_obj.AddComponent<PhysObjController>();
        u_objCont.setPhysObject(obj);
        
        m_objects.Add(obj);
    }
	public void RemoveObject(PhysObject obj) { /* TODO */ }
 
    public void AddSolver(Solver solver) { m_solvers.Add(solver); }
	public void RemoveSolver(Solver solver) { /* TODO */ }

    // TODO: Work with permissions
	public void Step(fp dt){
        ResolveCollisions(dt);

		foreach(PhysObject obj in m_objects) {
            fp mass = 1/obj.InverseMass;
            fp3 oldForce = obj.Force;
            fp3 oldVelocity = obj.Velocity;
            fp3 oldPosition = obj.Transform.Position;

            // Get combined forces
            fp3 newForce =  oldForce + mass * obj.Gravity; // apply a force
            fp3 newVelocity = oldVelocity + newForce / mass * dt;
            fp3 newPosition = oldPosition + newVelocity*dt;
			 
            obj.Velocity = (newVelocity);
            obj.Transform.Position = (newPosition);
			obj.Force = (fp3.zero); // reset net force at the end
		}
	}

    void ResolveCollisions(fp dt){
		List<PhysCollision> collisions = new List<PhysCollision>();
        // TODO: Work on that efficiency
		foreach (PhysObject a in m_objects) {
			foreach (PhysObject b in m_objects) {
				if (a == b) break;

                // Check if a collider is assigned
				if (a.Coll is null || b.Coll is null){
					continue;
				}
 
				CollisionPoints points = a.Coll.TestCollision(
					a.Transform,
					b.Coll,
					b.Transform);
 
				if (points.HasCollision) {
					collisions.Add(
                        new PhysCollision{
                            ObjA = a,
                            ObjB = b,
                            Points = points
                        }
                    );
				}
			}
		}
 
		foreach(Solver solver in m_solvers) {
			solver.Solve(collisions, dt);
		}
 	}
}
