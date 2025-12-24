using System;
using System.IO;
using UnityEngine;
using Unity.Mathematics.FixedPoint;
using SepM.Serialization;
using SepM.Utils;
using Newtonsoft.Json;

namespace SepM.Physics{
    [Serializable]
    /* Using as a class (instead of a struct) since it represents a combination of values and will be mutated often. */
    public class PhysObject : Serial {
        public class IColliderConverter : JsonConverter<ICollider>
        {
            public override void WriteJson(JsonWriter writer, ICollider value, JsonSerializer serializer)
            {
                writer.WriteValue(value == null ? string.Empty : value.ToString());
            }

            public override ICollider ReadJson(JsonReader reader, Type objectType, ICollider existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                return existingValue;
            }
        }

        public uint InstanceId; // UUID for object
        public PhysTransform Transform; // struct with 3 floats for x, y, z or i + j + k
        public fp3 Velocity;
        public fp3 Gravity; // Gravitational acceleration
        public fp3 Force; // Net force
        // Why use inverse mass? http://obi.virtualmethodstudio.com/forum/thread-2130.html
        public fp InverseMass;
        public bool IsKinematic;
        public fp Restitution = 0.5m; // Elasticity of collisions (bounciness)
        public fp DynamicFriction = 0.5m; // Dynamic friction coefficient
        public fp StaticFriction = 0.5m; // Static friction coefficient
        public Collider Coll = null; // Collider attached to PhysObject
        public bool IsDynamic = false;
        [JsonProperty(ReferenceLoopHandling = ReferenceLoopHandling.Ignore)]
        [JsonConverter(typeof(IColliderConverter))]
        public ICollider IColl = null; // Attached script with OnCollision callbacks
        
        /// <summary>
        /// Gets the deterministic checksum for this physics object.
        /// Uses Fletcher32 on serialized bytes with caching for performance.
        /// </summary>
        [JsonProperty]
        public int Checksum => GetHashCode();

        public PhysObject(uint id){
            InstanceId = id;
            Transform = new PhysTransform(id: id);
            Velocity = new fp3();
            Force = new fp3();
            InverseMass = 1m/5m;
            Gravity = SepM.Physics.Constants.GRAVITY;
        }

        public PhysObject(uint id, int m){
            InstanceId = id;
            Transform = new PhysTransform(id: id);
            Velocity = new fp3();
            Force = new fp3();
            InverseMass = 1m/m;
            Gravity = SepM.Physics.Constants.GRAVITY;
        }
        public PhysObject(uint id, fp3 pos){
            InstanceId = id;
            PhysTransform newTransform = new PhysTransform(id: id);
            newTransform.Position = pos;
            Transform = newTransform;
            Velocity = new fp3();
            Force = new fp3();
            InverseMass = 1m/5m;
            Gravity = SepM.Physics.Constants.GRAVITY;
        }

        public void AddForce(fp3 f){
            Force += f;
        }

        public fp GetMass(){
            return 1m/InverseMass;
        }

        /* If it's assigned, calls the ICollider's OnCollision method. */
        public void OnCollision<T>(PhysCollision c, T context){
            if(!(IColl is null)){
                IColl.OnCollision(c, context);
            }
        }

        public void SetVelocity(fp3 v){
            Velocity = v;
        }

        public override string ToString(){
            string collType = "";
            if(!(Coll is null)){
                if(Coll is SphereCollider)
                    collType = string.Format("Sphere({0})", ((SphereCollider)Coll).Radius);
                else if(Coll is PlaneCollider)
                    collType = "Plane";
                else if(Coll is CapsuleCollider)
                    collType = "Capsule";
                else if(Coll is AABBoxCollider)
                    collType = "AABBox";
            }
            string result = collType + "PhysObject";

            return result;
        }

        public void Serialize(BinaryWriter bw)
        {
        //InstanceId
            bw.Write(InstanceId);
        //PhysTransform
            Transform.Serialize(bw);
        //Velocity
            bw.WriteFp(Velocity.x);
            bw.WriteFp(Velocity.y);
            bw.WriteFp(Velocity.z);
        //Gravity
            bw.WriteFp(Gravity.x);
            bw.WriteFp(Gravity.y);
            bw.WriteFp(Gravity.z);
        //Force
            bw.WriteFp(Force.x);
            bw.WriteFp(Force.y);
            bw.WriteFp(Force.z);
        //InverseMass
            bw.WriteFp(InverseMass);
        //IsKinematic
            bw.Write(IsKinematic);
        //Restitution
            bw.WriteFp(Restitution);
        //DynamicFriction
            bw.WriteFp(DynamicFriction);
        //StaticFriction
            bw.WriteFp(StaticFriction);
        //Coll
            // Write the kind of collider, or none if one doesn't exist
            if (Coll is null)
            {
                //Collider Type (read as an int)
                bw.Write((int)Constants.coll_types.none);
            }
            else
            {
                //Collider Type (read as an int)
                bw.Write((int)Coll.Type);
                // Then serialize the collider
                Coll.Serialize(bw);
            }
        //IsDynamic
            bw.Write(IsDynamic);
        //IColl
            // TODO: Figure out references
            // if (!(IColl is null))
            //     IColl.Serialize(bw);
        }

        public Serial Deserialize<T>(BinaryReader br, T context)
        {
        //InstanceId
            InstanceId = br.ReadUInt32();
        //Transform
            Transform.Deserialize(br, context);
        //Velocity
            Velocity.x = br.ReadFp();
            Velocity.y = br.ReadFp();
            Velocity.z = br.ReadFp();
        //Gravity
            Gravity.x = br.ReadFp();
            Gravity.y = br.ReadFp();
            Gravity.z = br.ReadFp();
        //Force
            Force.x = br.ReadFp();
            Force.y = br.ReadFp();
            Force.z = br.ReadFp();
        //InverseMass
            InverseMass = br.ReadFp();
        //IsKinematic
            IsKinematic = br.ReadBoolean();
        //Restitution
            Restitution = br.ReadFp();
        //DynamicFriction
            DynamicFriction = br.ReadFp();
        //StaticFriction
            StaticFriction = br.ReadFp();
        //Coll
            // Get the kind of collider, or none if one didn't exist
            Constants.coll_types collType = (Constants.coll_types)br.ReadInt32();
            if (collType != Constants.coll_types.none)
            {
                // Create a new collider
                if (collType == Constants.coll_types.sphere) Coll = new SphereCollider();
                else if (collType == Constants.coll_types.capsule) Coll = new CapsuleCollider();
                else if (collType == Constants.coll_types.aabb) Coll = new AABBoxCollider();
                else
                {
                    Debug.LogWarning("No valid collider type found! Defaulting to SphereCollider!");
                    Coll = new SphereCollider();
                }
                // Then deserialize it
                Coll.Deserialize(br, context);
            }
        //IsDynamic
            IsDynamic = br.ReadBoolean();
        //IColl
            // if (!(IColl is null))
            //     IColl.Deserialize(br);

            return this;
        }

        public override int GetHashCode()
        {
            int hashCode = 1858597544;
            hashCode = hashCode * -1521134295 + InstanceId.GetHashCode();
            hashCode = hashCode * -1521134295 + Transform.GetHashCode();
            hashCode = hashCode * -1521134295 + Velocity.GetHashCode();
            hashCode = hashCode * -1521134295 + Gravity.GetHashCode();
            hashCode = hashCode * -1521134295 + Force.GetHashCode();
            hashCode = hashCode * -1521134295 + InverseMass.GetHashCode();
            hashCode = hashCode * -1521134295 + IsKinematic.GetHashCode();
            hashCode = hashCode * -1521134295 + Restitution.GetHashCode();
            hashCode = hashCode * -1521134295 + DynamicFriction.GetHashCode();
            hashCode = hashCode * -1521134295 + StaticFriction.GetHashCode();
            if (!(Coll is null)) hashCode = hashCode * -1521134295 + Coll.GetHashCode();
            hashCode = hashCode * -1521134295 + IsDynamic.GetHashCode();
            return hashCode;
        }
    };
}