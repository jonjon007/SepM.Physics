using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Unity.Mathematics.FixedPoint;
using SepM.Serialization;
using SepM.Utils;
using Newtonsoft.Json;

namespace SepM.Physics {
    [Serializable]
    public class PhysWorld : Serial {
        [SerializeField]
        private uint currentPhysObjId = 0; //Incrementing PhysObject counter
        [SerializeField]
        private List<PhysObject> m_objects = new List<PhysObject>();
        private List<Solver> m_solvers = new List<Solver>();
        public List<PhysCollision> collisions = new List<PhysCollision>();
        [JsonIgnore]
        public Dictionary<uint, GameObject> objectsMap = new Dictionary<uint, GameObject>();
        public CollisionMatrix collisionMatrix = new CollisionMatrix();

        public PhysObject GetPhysObjectById(uint instanceId){
            foreach(PhysObject p in m_objects)
                if(p.InstanceId == instanceId){
                    return p;
                }

            Debug.LogWarning($"Could not find PhysObject with instanceId: {instanceId}");
            return null;
        }

        public PhysObject GetPhysObjectByIndex(int i){
            if (i == -1)
                return null;
            if(m_objects.Count <= i)
                Debug.LogWarning($"Could not find PhysObject at index: {i}");

            return m_objects[i];
        }

        public int GetPhysObjectIndexById(uint instanceId){
            for(int i = 0; i < m_objects.Count; i++){
                PhysObject p = m_objects[i];
                if(p.InstanceId == instanceId){
                    return i;
                }
            }

            Debug.LogWarning($"Could not find PhysObject with instanceId: {instanceId}");
            return -1;
        }

        public PhysObject[] GetPhysObjectsOfLayers(long layers)
        {
            return m_objects.FindAll(o => o.Coll == null ? false : o.Coll.InLayers(layers)).ToArray();
        }

        private GameObject FindGameObjectById(int instanceId){
            UnityEngine.GameObject[] all = GameObject.FindObjectsOfType<GameObject>();
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i].GetInstanceID() == instanceId)
                {
                    return all[i];
                }
            }
            Debug.LogWarning($"Could not find an object or component in the scene with the ID: {instanceId}.");
            return null;
        }

        // By default, create Impulse and SmoothPosition solvers
        public PhysWorld() {
            AddSolver(new ImpulseSolver());
            AddSolver(new SmoothPositionSolver());
        }

        public void CleanUp(PhysObject p)
        {
            CleanUp(new PhysObject[] {p});
        }

        // TODO: Write test
        public void CleanUp(PhysObject[] p = null) {
            if(p == null){
                // Clear everything
                if (Application.isEditor) objectsMap.Values.ToList().ForEach(go => GameObject.DestroyImmediate(go));
                else objectsMap.Values.ToList().ForEach(go => GameObject.Destroy(go));

                objectsMap.Clear();
                m_objects.Clear();
            }
            else if(p.Length > 0){
                // Clear the given physObjects
                var physObjectIds = p.Select(p => p.InstanceId);
                var kvpsToClean = objectsMap.Where(kvp => physObjectIds.Contains(kvp.Key)).ToList();

                if (Application.isEditor) kvpsToClean.ForEach(kvp => GameObject.DestroyImmediate(kvp.Value));
                else kvpsToClean.ForEach(kvp => GameObject.Destroy(kvp.Value));

                kvpsToClean.ForEach(kvp => {
                    objectsMap.Remove(kvp.Key);
                    var physObjToRemove = m_objects.First(k => k.InstanceId == kvp.Key);
                    m_objects.Remove(physObjToRemove);
                    });
            }

            // No need to clear solvers
            // m_solvers.Clear();
        }

        public void AssignGameObject(GameObject gameObj, PhysObject physObj) {
            objectsMap[physObj.InstanceId] = gameObj;

            // If you're seeing this, find a cleaner way to create your objects.
            if (!m_objects.Contains(physObj)) {
                Debug.LogWarning($"m_objects missing PhysObject from objectsMap with ID of {physObj.InstanceId}!\nCreating a new one.");
                m_objects.Add(physObj);
            }
        }

        public Tuple<GameObject, PhysObject> CreateSphereObject(fp3 center, fp r, bool isDyn, bool isKin, fp3 g, PhysTransform parent = null) {
            return CreateSphereObject(center, r, isDyn, isKin, g, Constants.coll_layers.normal, parent);
        }

        public Tuple<GameObject, PhysObject> CreateSphereObject(fp3 center, fp r, bool isDyn, bool isKin, fp3 g, Constants.coll_layers l, PhysTransform parent = null) {
            // Set up the collider, the physics object, and the game object
            GameObject gameObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            // Destroy the default Unity collider
            if (Application.isEditor)
                GameObject.DestroyImmediate(gameObj.GetComponent<UnityEngine.SphereCollider>());
            else
                GameObject.Destroy(gameObj.GetComponent<UnityEngine.SphereCollider>());
            SepM.Physics.SphereCollider coll = new SepM.Physics.SphereCollider(r, l);
            PhysObject physObj = new PhysObject(++currentPhysObjId, center) {
                IsDynamic = isDyn,
                IsKinematic = isKin,
                Gravity = g,
                Coll = coll
            };
            physObj.Transform.SetParent(parent);

            // Update the GameObject
            float sphRadius = (float)coll.Radius * 2;

            gameObj.transform.position = physObj.Transform.WorldPosition().toVector3();
            gameObj.transform.rotation = physObj.Transform.WorldRotation();
            gameObj.transform.localScale = Vector3.one * sphRadius;

            Tuple<GameObject, PhysObject> result = new Tuple<GameObject, PhysObject>(
                gameObj, physObj
            );

            // Add to list of the world's physics objects
            m_objects.Add(physObj);

            // Add to the map
            AssignGameObject(gameObj, physObj);

            return result;
        }

        public Tuple<GameObject, PhysObject> CreateCapsuleObject(fp3 center, fp r, fp h, bool isDyn, bool isKin, fp3 g, PhysTransform parent = null){
            return CreateCapsuleObject(center, r, h, isDyn, isKin, g, Constants.coll_layers.normal, parent);
        }

        public Tuple<GameObject, PhysObject> CreateCapsuleObject(fp3 center, fp r, fp h, bool isDyn, bool isKin, fp3 g, Constants.coll_layers l, PhysTransform parent = null, PhysObject physObj = null) {
            // Set up the collider, the physics object, and the game object
            GameObject gameObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            // Destroy the default Unity collider
            if (Application.isEditor)
                GameObject.DestroyImmediate(gameObj.GetComponent<UnityEngine.CapsuleCollider>());
            else
                GameObject.Destroy(gameObj.GetComponent<UnityEngine.CapsuleCollider>());
            SepM.Physics.CapsuleCollider coll = new SepM.Physics.CapsuleCollider(r, h, l);
            if (physObj == null)
            {
                physObj = new PhysObject(++currentPhysObjId, center)
                {
                    IsDynamic = isDyn,
                    IsKinematic = isKin,
                    Gravity = g,
                    Coll = coll
                };

                // Add to list of the world's physics objects
                m_objects.Add(physObj);
            }
            physObj.Transform.SetParent(parent);

            // Update the GameObject
            float capRadius = (float)coll.Radius * 2;
            float capHeight = (float)coll.Height + (float)coll.Radius/2;

            gameObj.transform.position = physObj.Transform.WorldPosition().toVector3();
            gameObj.transform.rotation = physObj.Transform.WorldRotation();
            gameObj.transform.localScale = new Vector3(capRadius, capHeight, capRadius);


            Tuple<GameObject, PhysObject> result = new Tuple<GameObject, PhysObject>(
                gameObj, physObj
            );

            // Add to the map
            AssignGameObject(gameObj, physObj);

            return result;
        }

        public Tuple<GameObject, PhysObject> CreateAABBoxObject(fp3 center, fp3 scale, bool isDyn, bool isKin, fp3 g, PhysTransform parent = null) {
            return CreateAABBoxObject(center, scale, isDyn, isKin, g, Constants.coll_layers.normal, parent);
        }

        public Tuple<GameObject, PhysObject> CreateAABBoxObject(fp3 center, fp3 scale, bool isDyn, bool isKin, fp3 g, Constants.coll_layers l, PhysTransform parent = null, PhysObject physObj = null) {
            // Set up the collider, the physics object, and the game object
            GameObject gameObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            // Destroy the default Unity collider
            if (Application.isEditor)
                GameObject.DestroyImmediate(gameObj.GetComponent<UnityEngine.BoxCollider>());
            else
                GameObject.Destroy(gameObj.GetComponent<UnityEngine.BoxCollider>());
            SepM.Physics.AABBoxCollider coll = new SepM.Physics.AABBoxCollider(fp3.zero, scale, true, l);
            if (physObj == null)
            {
                physObj = new PhysObject(++currentPhysObjId, center)
                {
                    IsDynamic = isDyn,
                    IsKinematic = isKin,
                    Gravity = g,
                    Coll = coll
                };

                // Add to list of the world's physics objects
                m_objects.Add(physObj);
            }
            physObj.Transform.SetParent(parent);

            // Update the GameObject
            gameObj.transform.position = physObj.Transform.WorldPosition().toVector3();
            gameObj.transform.rotation = physObj.Transform.WorldRotation();
            gameObj.transform.localScale = scale.toVector3();

            Tuple<GameObject, PhysObject> result = new Tuple<GameObject, PhysObject>(
                gameObj, physObj
            );

            // Add to the map
            AssignGameObject(gameObj, physObj);

            return result;
        }

        public List<PhysObject> GetAllPhysObjectsInLayer(long layers)
        {
            return m_objects.FindAll(o => o.Coll != null && o.Coll.InLayers(layers));
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

        public void AddSolver(Solver solver) { m_solvers.Add(solver); }
        public uint IncrementIDCounter()
        {
            return ++currentPhysObjId;
        }
        public void RemoveSolver(Solver solver) { /* TODO */ }

        // Call in fixed timestep
        public void Step<T>(fp dt, T context) {
            ResolveCollisions(dt, context);

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

        // Call with regular Unity Update()
        public void UpdateGameObjects() {
            // Update GameObjects
            foreach (var k in objectsMap.Keys) {
                PhysObject physObject = GetPhysObjectById(k);
                GameObject gameObject = objectsMap[k];
                gameObject.transform.position = physObject.Transform.WorldPosition().toVector3();
                gameObject.transform.rotation = physObject.Transform.WorldRotation();
            }
        }

        public void ResetIDCounter()
        {
            currentPhysObjId = 0;
        }

        void ResolveCollisions<T>(fp dt, T context) {
            // Reset collisions list
            collisions = new List<PhysCollision>();
            // TODO: Work on that efficiency
            foreach (PhysObject a in m_objects) {
                foreach (PhysObject b in m_objects) {
                    if (a == b) continue;
                    // Check if a collider is assigned
                    if (a.Coll is null || b.Coll is null) continue;
                    // Check if the layers register collisions with each other
                    if (!collisionMatrix.CanLayersCollide(a.Coll.Layer, b.Coll.Layer)) continue;

                    CollisionPoints points = a.Coll.TestCollision(
                        a.Transform,
                        b.Coll,
                        b.Transform);

                    if (points.HasCollision) {
                        collisions.Add(
                            new PhysCollision {
                                ObjIdA = a.InstanceId,
                                ObjIdB = b.InstanceId,
                                Points = points
                            }
                        );
                    }
                }
            }

            foreach (Solver solver in m_solvers) {
                solver.Solve(collisions, dt, this);
            }

            // Since each pair will be coming twice in opposite order, just run the first OnCollision
            foreach (PhysCollision cp in collisions) {
                PhysObject po = this.GetPhysObjectById(cp.ObjIdA);
                po.OnCollision(cp, context);
            }
        }

        public void Serialize(BinaryWriter bw)
        {
        //physObject and physTransform IDs
            bw.Write(currentPhysObjId);
        //m_objects
            bw.Write(m_objects.Count);
            for (int i = 0; i < m_objects.Count; i++)
                m_objects[i].Serialize(bw);
        //m_solvers
            // No need to serialize
        //collisions
            bw.Write(collisions.Count);
            for (int i = 0; i < collisions.Count; i++)
                collisions[i].Serialize(bw);
        //objectsMap
            bw.Write(objectsMap.Count);
            // Write each tuple's PhysObject id and GameObject id
            List<uint> mapKeys = objectsMap.Keys.ToList();
            // Sort for deterministic ordering
            mapKeys.Sort();
            foreach (uint id in mapKeys)
            {
                // Write PhysObject ID
                bw.Write(id);
                // Write GameObject ID
                bw.Write(objectsMap[id].GetInstanceID());
            }
        //collisionMatrix
            collisionMatrix.Serialize(bw);
        }

        public Serial Deserialize<T>(BinaryReader br, T context)
        {
        //physObject and physTransform IDs
            currentPhysObjId = br.ReadUInt32();
        //m_objects
            int m_objects_length = br.ReadInt32();
            if (m_objects_length != m_objects.Count)
            {
                // Create a new list if the counts aren't the same
                m_objects = new List<PhysObject>(new PhysObject[m_objects_length]);
                for (int i = 0; i < m_objects_length; i++)
                    m_objects[i] = new PhysObject(id: 0);
            }
            // Read down the data for each object
            for (int i = 0; i < m_objects_length; i++)
            {
                m_objects[i].Deserialize(br, context);
            }
            // Assign each object's Transform's parents; may be a bit slow
            {
                PhysTransform[] transforms = m_objects.Select(o => o.Transform).ToArray();
                foreach (PhysTransform t in transforms)
                {
                    if (t.m_parent_id != 0)
                    {
                        PhysTransform parentFound = transforms.FirstOrDefault(other => other.InstanceId == t.m_parent_id);
                        if (parentFound != null)
                            t.SetParent(parentFound);
                        else
                            Debug.LogError($"Can't find PhysTransform parent with ID: {t.InstanceId}!");
                    }
                }
            }
        //m_solvers
            // Don't serialize
        //collisions
            int collisions_count = br.ReadInt32();
            // Create a new list if the counts aren't the same
            if (collisions_count != collisions.Count)
            {
                collisions = new List<PhysCollision>(new PhysCollision[collisions_count]);
            }
            // Read down the data for each object
            for (int i = 0; i < collisions_count; i++)
            {
                collisions[i] = (PhysCollision)collisions[i].Deserialize(br, context);
            }
        //objectsMap
            int objectsMapLength = br.ReadInt32();
            // Keep track of current GameObjects
            var oldGameObjects = objectsMap.Values;
            // Create a new map to populate
            objectsMap = new Dictionary<uint, GameObject>();
            // Read down the data for each object
            for (int i = 0; i < objectsMapLength; i++)
            {
                uint poId = br.ReadUInt32();
                int goId = br.ReadInt32();
                GameObject go = FindGameObjectById(goId);
                if (go is null)
                {
                    // TODO: Create the right kind of object
                    go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                }
                objectsMap.Add(poId, go);
            }

            var orphanedGameObjects = oldGameObjects.Except(objectsMap.Values);

            // Destroy any old game objects
            foreach (GameObject go in orphanedGameObjects)
            {
                Debug.LogWarning($"Found orphaned GameObject with ID {go.GetInstanceID()}");
                if (Application.isEditor) GameObject.DestroyImmediate(go);
                else GameObject.Destroy(go);
            }
        //collisionMatrix
            collisionMatrix.Deserialize(br, context);

            return this;
        }

        public override int GetHashCode() {
            int hashCode = -1214587014;
        //physObject and physTransform IDs
            hashCode = hashCode * -1521134295 + currentPhysObjId.GetHashCode();
        //m_objects
            foreach (var m_obj in m_objects) {
                hashCode = hashCode * -1521134295 + m_obj.GetHashCode();
            }
        //collisions
            foreach (var c in collisions)
            {
                hashCode = hashCode * -1521134295 + c.GetHashCode();
            }
        //objectsMap
            foreach (var k in objectsMap.Keys)
            {
                hashCode = hashCode * -1521134295 + k.GetHashCode();
            }
        //collisionMatrix
            hashCode = hashCode * -1521134295 + collisionMatrix.GetHashCode();
            return hashCode;
        }
    }
}