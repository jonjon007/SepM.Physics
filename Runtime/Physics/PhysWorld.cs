using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Unity.Mathematics.FixedPoint;
using SepM.Utils;

namespace SepM.Physics {
    [Serializable]
    public class PhysWorld {
        // TODO: Serialize
        private List<PhysObject> m_objects = new List<PhysObject>();
        // TODO: Be wary of finding solvers Create new ones on reload?
        private List<Solver> m_solvers = new List<Solver>();
        // TODO: Serialize
        public List<PhysCollision> collisions = new List<PhysCollision>();

        public void Serialize(BinaryWriter bw) {
            //m_objects
            bw.Write(m_objects.Count);
            for(int i = 0; i < m_objects.Count; i++)
                m_objects[i].Serialize(bw);
        //m_objects
            // TODO: Serialize?
        //m_objects
            // TODO: Serialize?
        }

        public void Deserialize(BinaryReader br) {
            //m_objects
            int m_objects_length = br.ReadInt32();
            // Create a new list if the counts aren't the same
            if (m_objects_length != m_objects.Count) {
                m_objects = new List<PhysObject>(new PhysObject[m_objects_length]);
            }
            // Read down the data for each object
            for(int i = 0; i < m_objects_length; i++){
                if(m_objects[i] is null) m_objects[i] = new PhysObject();
                m_objects[i].Deserialize(br);
            }
        //m_objects
            // TODO: Serialize?
        //m_objects
            // TODO: Serialize?
        }

        // By default, create Impulse and SmoothPosition solvers
        public PhysWorld() {
            AddSolver(new ImpulseSolver());
            AddSolver(new SmoothPositionSolver());
        }

        public Tuple<GameObject, PhysObject> CreateSphereObject(fp3 center, fp r, bool isDyn, bool isKin, fp3 g) {
            return CreateSphereObject(center, r, isDyn, isKin, g, Constants.coll_layers.normal);
        }

        public Tuple<GameObject, PhysObject> CreateSphereObject(fp3 center, fp r, bool isDyn, bool isKin, fp3 g, Constants.coll_layers l) {
            // Set up the collider, the physics object, and the game object
            GameObject g_obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            // Destroy the default Unity collider
            if (Application.isEditor)
                GameObject.DestroyImmediate(g_obj.GetComponent<UnityEngine.SphereCollider>());
            else
                GameObject.Destroy(g_obj.GetComponent<UnityEngine.SphereCollider>());
            SepM.Physics.SphereCollider coll = new SepM.Physics.SphereCollider(r, l);
            PhysObject p_obj = new PhysObject(center) {
                IsDynamic = isDyn,
                IsKinematic = isKin,
                Gravity = g,
                Coll = coll
            };

            // Update the GameObject
            float sphRadius = (float)coll.Radius * 2;

            g_obj.transform.localPosition = center.toVector3();
            g_obj.transform.localScale = Vector3.one * sphRadius;

            Tuple<GameObject, PhysObject> result = new Tuple<GameObject, PhysObject>(
                g_obj, p_obj
            );

            // Add to list of the world's physics objects
            m_objects.Add(p_obj);

            return result;
        }

        public Tuple<GameObject, PhysObject> CreateCapsuleObject(fp3 center, fp r, fp h, bool isDyn, bool isKin, fp3 g) {
            return CreateCapsuleObject(center, r, h, isDyn, isKin, g, Constants.coll_layers.normal);
        }

        public Tuple<GameObject, PhysObject> CreateCapsuleObject(fp3 center, fp r, fp h, bool isDyn, bool isKin, fp3 g, Constants.coll_layers l) {
            // Set up the collider, the physics object, and the game object
            GameObject g_obj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            // Destroy the default Unity collider
            if (Application.isEditor)
                GameObject.DestroyImmediate(g_obj.GetComponent<UnityEngine.CapsuleCollider>());
            else
                GameObject.Destroy(g_obj.GetComponent<UnityEngine.CapsuleCollider>());
            SepM.Physics.CapsuleCollider coll = new SepM.Physics.CapsuleCollider(r, h, l);
            PhysObject p_obj = new PhysObject(center) {
                IsDynamic = isDyn,
                IsKinematic = isKin,
                Gravity = g,
                Coll = coll
            };

            // Update the GameObject
            float capRadius = (float)coll.Radius * 2;
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

        public Tuple<GameObject, PhysObject> CreateAABBoxObject(fp3 center, fp3 scale, bool isDyn, bool isKin, fp3 g) {
            return CreateAABBoxObject(center, scale, isDyn, isKin, g, Constants.coll_layers.normal);
        }

        public Tuple<GameObject, PhysObject> CreateAABBoxObject(fp3 center, fp3 scale, bool isDyn, bool isKin, fp3 g, Constants.coll_layers l) {
            // Set up the collider, the physics object, and the game object
            GameObject g_obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            // Destroy the default Unity collider
            if (Application.isEditor)
                GameObject.DestroyImmediate(g_obj.GetComponent<UnityEngine.BoxCollider>());
            else
                GameObject.Destroy(g_obj.GetComponent<UnityEngine.BoxCollider>());
            SepM.Physics.AABBoxCollider coll = new SepM.Physics.AABBoxCollider(fp3.zero, scale, true, l);
            PhysObject p_obj = new PhysObject(center) {
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

        /// <summary>
        /// Adds the given PhysObject to the world
        /// </summary>
        public void AddObject(PhysObject obj) {
            // GameObject u_obj;
            // if(obj.Coll is SepM.Physics.SphereCollider){
            //     u_obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //     float sphRadius = (float)((SepM.Physics.SphereCollider)obj.Coll).Radius*2;
            //     u_obj.transform.localScale = Vector3.one*sphRadius;
            // }
            // else if(obj.Coll is SepM.Physics.CapsuleCollider){
            //     u_obj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            //     float capRadius = (float)((SepM.Physics.CapsuleCollider)obj.Coll).Radius*2;
            //     float capHeight = (float)((SepM.Physics.CapsuleCollider)obj.Coll).Height;
            //     u_obj.transform.localScale = new Vector3(capRadius, capHeight, capRadius);
            // }
            // else{
            //     u_obj = GameObject.CreatePrimitive(PrimitiveType.Plane);
            //     PlaneCollider c = (PlaneCollider)obj.Coll;
            //     Vector3 planeDir = c.Normal.toVector3();
            //     float scale = (float)c.Distance/10;
            //     u_obj.transform.Rotate(planeDir);
            //     u_obj.transform.localScale = Vector3.one*scale;
            // }
            // PhysObjController u_objCont = u_obj.AddComponent<PhysObjController>();
            // u_objCont.setPhysObject(obj);

            m_objects.Add(obj);
        }
        public void RemoveObject(PhysObject obj) { /* TODO */ }

        public void AddSolver(Solver solver) { m_solvers.Add(solver); }
        public void RemoveSolver(Solver solver) { /* TODO */ }

        // TODO: Work with permissions
        public void Step(fp dt) {
            ResolveCollisions(dt);

            foreach (PhysObject obj in m_objects) {
                fp mass = 1 / obj.InverseMass;
                fp3 oldForce = obj.Force;
                fp3 oldVelocity = obj.Velocity;
                fp3 oldPosition = obj.Transform.Position;

                // Get combined forces
                fp3 newForce = oldForce + mass * obj.Gravity; // apply a force
                fp3 newVelocity = oldVelocity + newForce / mass * dt;
                fp3 newPosition = oldPosition + newVelocity * dt;

                obj.Velocity = (newVelocity);
                obj.Transform.Position = (newPosition);
                obj.Force = (fp3.zero); // reset net force at the end
            }
        }

        void ResolveCollisions(fp dt) {
            // Reset collisions list
            collisions = new List<PhysCollision>();
            // TODO: Work on that efficiency
            foreach (PhysObject a in m_objects) {
                foreach (PhysObject b in m_objects) {
                    if (a == b) break;

                    // Check if a collider is assigned
                    if (a.Coll is null || b.Coll is null) {
                        continue;
                    }

                    CollisionPoints points = a.Coll.TestCollision(
                        a.Transform,
                        b.Coll,
                        b.Transform);

                    if (points.HasCollision) {
                        collisions.Add(
                            new PhysCollision {
                                ObjA = a,
                                ObjB = b,
                                Points = points
                            }
                        );
                    }
                }
            }

            foreach (Solver solver in m_solvers) {
                solver.Solve(collisions, dt);
            }

            //Run on collision for each pair
            foreach (PhysCollision cp in collisions) {
                cp.ObjA.OnCollision(cp);
                cp.ObjB.OnCollision(cp);
            }
        }

        public override int GetHashCode() {
            int hashCode = -1214587014;
            foreach (var m_obj in m_objects) {
                hashCode = hashCode * -1521134295 + m_obj.GetHashCode();
            }
            return hashCode;
        }
    }
}