using UnityEngine;
using Unity.Mathematics.FixedPoint;
using SepM.Utils;
using SepM.Math;
using System;
using System.IO;

namespace SepM.Physics{
    public static class Constants{
        public enum coll_layers {
            normal = 0,
            ground = 1,
            wall = 2,
            player = 3,
            noPlayer = 4, // Collides with ground and such, but no players,
            danger = 5,
            win = 6,
            hitbox = 7,
        };
        public enum coll_types {
            none = 0,
            sphere = 1,
            capsule = 2,
            aabb = 3
        };
        // Bitwise for collision layers
        public static long layer_none = 0;
        public static long layer_all = ~0;
        public static long layer_normal = 1 << ((int)coll_layers.normal);
        public static long layer_ground = 1 << ((int)coll_layers.ground);
        public static long layer_wall = 1 << ((int)coll_layers.wall);
        public static long layer_player = 1 << ((int)coll_layers.player);
        public static long layer_noPlayer = 1 << ((int)coll_layers.noPlayer);
        public static fp3 GRAVITY = new fp3(0,-9.81m, 0);
    }

    [Serializable]
    public struct PhysCollision{
        public PhysObject ObjA;
        public PhysObject ObjB;
        public CollisionPoints Points;
    }

    [Serializable]
    public struct CollisionPoints {
        public fp3 A; // Furthest point of A into B
        public fp3 B; // Furthest point of B into A
        public fp3 Normal; // B – A normalized
        public fp DepthSqrd; // Length of B – A
        public bool HasCollision;
        public static CollisionPoints noCollision = new CollisionPoints{ 
            A = fp3.zero,
            B = fp3.zero, 
            Normal = fp3.zero, 
            DepthSqrd = 0,
            HasCollision = false
        };
    };

    // TODO: Come up with a better name
    public interface ICollider {
        // TODO: May be a good idea to return an int or something. May not be necessary
        public void OnCollision(PhysCollision c);
        public void Serialize(BinaryWriter bw);
        public void Deserialize(BinaryReader br);
    }

    [Serializable]
    public class PhysTransform { // Describes an objects location
        public fp3 Position;
        public fp3 Scale;
        public fpq Rotation;
        private PhysTransform m_parent;

        public void Serialize(BinaryWriter bw) {
        //Position
            bw.Write(Position.x);
            bw.Write(Position.y);
            bw.Write(Position.z);
        //Scale
            bw.Write(Scale.x);
            bw.Write(Scale.y);
            bw.Write(Scale.z);
        //Rotation
            bw.Write(Rotation.x);
            bw.Write(Rotation.y);
            bw.Write(Rotation.z);
        //m_parent
            //TODO: Parent reference?
        }

        public void Deserialize(BinaryReader br) {
        //Position
            Position.x = br.ReadDecimal();
            Position.y = br.ReadDecimal();
            Position.z = br.ReadDecimal();
        //Scale
            Scale.x = br.ReadDecimal();
            Scale.y = br.ReadDecimal();
            Scale.z = br.ReadDecimal();
        //Rotation
            Rotation.x = br.ReadDecimal();
            Rotation.y = br.ReadDecimal();
            Rotation.z = br.ReadDecimal();
        //m_parent
            //TODO: Parent reference?
        }

        public override int GetHashCode() {
            int hashCode = 1858537542;
            hashCode = hashCode * -1521134295 + Position.GetHashCode();
            hashCode = hashCode * -1521134295 + Scale.GetHashCode();
            hashCode = hashCode * -1521134295 + Rotation.GetHashCode();
            return hashCode;
        }

        // TODO: Find a way to serialize this without depth limit issues!
        // private List<PhysTransform> m_children;
        public PhysTransform(PhysTransform parent = null){
            Position = fp3.zero;
            Scale = new fp3(1,1,1);
            Rotation = fpq.identity;
            m_parent = parent;
            //m_children = new List<PhysTransform>();
        }
        public PhysTransform(fp3 p, PhysTransform parent = null){
            Position = p;
            Scale = new fp3(1,1,1);
            Rotation = fpq.identity;
            m_parent = parent;
            //m_children = new List<PhysTransform>();
        }

        public fp3 Right(){
            return new fp3(1,0,0).multiply(Rotation);
        }

        public fp3 Forward(){
            return new fp3(0,0,1).multiply(Rotation);
        }

        public fp3 Up(){
            return new fp3(0,1,0).multiply(Rotation);
        }

        /* TODO: Comment */
        public fp3 WorldPosition(){
            fp3 parentPos = fp3.zero;

            if (!(m_parent is null)) {
                parentPos = m_parent.WorldPosition();
            }

            return Position + parentPos;
        }
        public fpq WorldRotation(){            
            fpq parentRot = fpq.identity;

            if (!(m_parent is null)) {
                parentRot = m_parent.WorldRotation();
            }

            return Rotation.multiply(parentRot);
        }
        /* TODO: Comment */
        public fp3 WorldScale(){
            fp3 parentScale = new fp3(1,1,1);

            if (!(m_parent is null)) {
                parentScale = m_parent.WorldScale();
            }

            return Scale * parentScale;
        }

        public void Rotate(fp3 eulers){
            fpq eulerRot = eulers.toQuaternionFromDegrees();
            Rotation = Rotation.multiply(eulerRot);
        }

        public void Rotate(fp x, fp y, fp z){
            Rotate(new fp3(x,y,z));
        }

        public void SetParent(PhysTransform t){
            m_parent = t;
        }
    }

    [Serializable]
    public abstract class Collider {
        public Constants.coll_layers Layer;
        public Constants.coll_types Type = Constants.coll_types.none;

        public abstract void Serialize(BinaryWriter bw);

        public abstract void Deserialize(BinaryReader br);

        public abstract override int GetHashCode();

        public abstract CollisionPoints TestCollision(
            PhysTransform transform,
            Collider collider,
            PhysTransform colliderTransform
        );
        public abstract CollisionPoints TestCollision(
            PhysTransform transform,
            SphereCollider sphere,
            PhysTransform sphereTransform
        ); 
        public abstract CollisionPoints TestCollision(
            PhysTransform transform,
            PlaneCollider plane,
            PhysTransform planeTransform
        );

        public abstract CollisionPoints TestCollision(
            PhysTransform transform,
            CapsuleCollider capsule,
            PhysTransform capsuleTransform
        );

        public abstract CollisionPoints TestCollision(
            PhysTransform transform,
            AABBoxCollider capsule,
            PhysTransform capsuleTransform
        );

        public virtual bool InLayers(long layers){
            return (
                (1 << ((int)Layer)) & layers
            ) != 0;
        }
    };

    [Serializable]
    public class SphereCollider : Collider{
        public fp3 Center;
        public fp Radius;

        public override void Serialize(BinaryWriter bw) {
        //Layer (read as an int)
            bw.Write((int)Layer);
        //No need to write type; redundant
        //Center
            bw.WriteFp(Center.x);
            bw.WriteFp(Center.y);
            bw.WriteFp(Center.z);
        //Radius
            bw.WriteFp(Radius);
        }

        public override void Deserialize(BinaryReader br) {
        //Layer (write as an int)
            Layer = (Constants.coll_layers)br.ReadInt32();
        //No need to read type; redundant
        //Center
            Center.x = br.ReadFp();
            Center.y = br.ReadFp();
            Center.z = br.ReadFp();
            //Radius
            Radius = br.ReadFp();
        }

        public override int GetHashCode() {
            int hashCode = 1858597544;
            hashCode = hashCode * -1521134295 + Layer.GetHashCode();
            hashCode = hashCode * -1521134295 + Type.GetHashCode();
            hashCode = hashCode * -1521134295 + Center.GetHashCode();
            hashCode = hashCode * -1521134295 + Radius.GetHashCode();
            return hashCode;
        }

        public SphereCollider(){
            Center = fp3.zero;
            Radius = 1;
            Layer = Constants.coll_layers.normal;
            Type = Constants.coll_types.sphere;
        }

        public SphereCollider(fp r){
            Center = fp3.zero;
            Radius = r;
            Layer = Constants.coll_layers.normal;
            Type = Constants.coll_types.sphere;
        }

        public SphereCollider(fp r, Constants.coll_layers l){
            Center = fp3.zero;
            Radius = r;
            Layer = l;
            Type = Constants.coll_types.sphere;
        }

        public override CollisionPoints TestCollision(
            PhysTransform transform,
            Collider collider,
            PhysTransform colliderTransform)
        {
            return collider.TestCollision(colliderTransform, this, transform);
        }
    
        public override CollisionPoints TestCollision(
            PhysTransform transform,
            SphereCollider sphere,
            PhysTransform sphereTransform)
        {
            return algo.FindSphereSphereCollisionPoints(
                this, transform, sphere, sphereTransform);
        }
    
        public override CollisionPoints TestCollision(
            PhysTransform transform,
            PlaneCollider plane,
            PhysTransform planeTransform){
            return algo.FindSpherePlaneCollisionPoints(
                this, transform, plane, planeTransform
            );
        }

        public override CollisionPoints TestCollision(
            PhysTransform transform,
            CapsuleCollider capsule,
            PhysTransform capsuleTransform){
            return algo.FindSphereCapsuleCollisionPoints(
                this, transform, capsule, capsuleTransform
            );
        }

        public override CollisionPoints TestCollision(
            PhysTransform transform,
            AABBoxCollider box,
            PhysTransform boxTransform){
            return algo.FindSphereAABBCollisionPoints(
                this, transform, box, boxTransform
            );
        }
    }
    [Serializable]
    public class CapsuleCollider : Collider{
        public fp3 Center;
        public fp Radius;
        public fp Height;
        public fp3 Direction;

        public override void Serialize(BinaryWriter bw) {
        //Layer (read as an int)
            bw.Write((int)Layer);
        //No need to write type; redundant
        //Center
            bw.WriteFp(Center.x);
            bw.WriteFp(Center.y);
            bw.WriteFp(Center.z);
        //Radius
            bw.WriteFp(Radius);
        //Height
            bw.WriteFp(Height);
        //Direction
            bw.WriteFp(Direction.x);
            bw.WriteFp(Direction.y);
            bw.WriteFp(Direction.z);
        }

        public override void Deserialize(BinaryReader br) {
        //Layer (write as an int)
            Layer = (Constants.coll_layers)br.ReadInt32();
        //No need to read type; redundant
        //Center
            Center.x = br.ReadFp();
            Center.y = br.ReadFp();
            Center.z = br.ReadFp();
        //Radius
            Radius = br.ReadFp();
        //Height
            Height = br.ReadFp();
        //Direction
            Direction.x = br.ReadFp();
            Direction.y = br.ReadFp();
            Direction.z = br.ReadFp();
        }

        public override int GetHashCode() {
            int hashCode = 1858597544;
            hashCode = hashCode * -1521134295 + Layer.GetHashCode();
            hashCode = hashCode * -1521134295 + Type.GetHashCode();
            hashCode = hashCode * -1521134295 + Center.GetHashCode();
            hashCode = hashCode * -1521134295 + Radius.GetHashCode();
            hashCode = hashCode * -1521134295 + Height.GetHashCode();
            hashCode = hashCode * -1521134295 + Direction.GetHashCode();
            return hashCode;
        }

        public CapsuleCollider(){
			Center = fp3.zero;
			Radius = 0.5m;
			Height = 2;
            Direction = new fp3(0,1,0);
            Layer = Constants.coll_layers.normal;
            Type = Constants.coll_types.capsule;
        }
    
        public CapsuleCollider(fp r, fp h){
            Center = fp3.zero;
            Radius = r;
            Height = h;
            Direction = new fp3(0,1,0);
            Layer = Constants.coll_layers.normal;
            Type = Constants.coll_types.capsule;
        }

        public CapsuleCollider(fp r, fp h, fp3 d){
            Center = fp3.zero;
            Radius = r;
            Height = h;
            Direction = d;
            Layer = Constants.coll_layers.normal;
            Type = Constants.coll_types.capsule;
        }

        public CapsuleCollider(fp r, fp h, Constants.coll_layers l){
            Center = fp3.zero;
            Radius = r;
            Height = h;
            Direction = new fp3(0,1,0);
            Layer = l;
            Type = Constants.coll_types.capsule;
        }
        
        public CapsuleCollider(fp3 c, fp r, fp h){
            Center = c;
            Radius = r;
            Height = h;
            Direction = new fp3(0,1,0);
            Layer = Constants.coll_layers.normal;
            Type = Constants.coll_types.capsule;
        }

        public CapsuleCollider(fp3 c, fp r, fp h, Constants.coll_layers l){
            Center = c;
            Radius = r;
            Height = h;
            Direction = new fp3(0,1,0);
            Layer = l;
            Type = Constants.coll_types.capsule;
        }

        public CapsuleStats GetStats(PhysTransform transform){
            fp3 tip = transform.WorldPosition() + Center + Direction *(Height/2m);
            fp3 bse = transform.WorldPosition() + Center - Direction *(Height/2m);
            fp3 a_Normal = Direction.normalized(); 
            fp3 a_LineEndOffset = a_Normal * Radius; 
            fp3 A = bse + a_LineEndOffset; 
            fp3 B = tip - a_LineEndOffset;

            CapsuleStats result = new CapsuleStats{
                a_Normal = a_Normal,
                a_LineEndOffset = a_LineEndOffset,
                A = A,
                B = B
            };

            return result;
        }

        public override CollisionPoints TestCollision(
            PhysTransform transform,
            Collider collider,
            PhysTransform colliderTransform)
        {
            return collider.TestCollision(colliderTransform, this, transform);
        }
    
        public override CollisionPoints TestCollision(
            PhysTransform transform,
            SphereCollider sphere,
            PhysTransform sphereTransform)
        {
            return algo.FindSphereCapsuleCollisionPoints(
                sphere, transform, this, sphereTransform);
        }
    
        public override CollisionPoints TestCollision(
            PhysTransform transform,
            PlaneCollider plane,
            PhysTransform planeTransform){
            // TODO: Make a capsule version
            // return algo.FindCapsulePlaneCollisionPoints(
            //     this, transform, plane, planeTransform
            // );

            // TODO
            return new CollisionPoints();
        }

        public override CollisionPoints TestCollision(
            PhysTransform transform,
            CapsuleCollider capsule,
            PhysTransform capsuleTransform){
            return algo.FindCapsuleCapsuleCollisionPoints(
                this, transform, capsule, capsuleTransform
            );
        }

        public override CollisionPoints TestCollision(
            PhysTransform transform,
            AABBoxCollider box,
            PhysTransform boxTransform){
            return algo.FindCapsuleAABBCollisionPoints(
                this, transform, box, boxTransform
            );
        }
    };

    // Axis-Aligned Bounding Box
    [Serializable]
    public class AABBoxCollider : Collider{
        public fp3 MinValue;
        public fp3 MaxValue;

        public override void Serialize(BinaryWriter bw) {
        //Layer (read as an int)
            bw.Write((int)Layer);
        ///No need to write type; redundant
        //MinValue
            bw.WriteFp(MinValue.x);
            bw.WriteFp(MinValue.y);
            bw.WriteFp(MinValue.z);
        //MaxValue
            bw.WriteFp(MaxValue.x);
            bw.WriteFp(MaxValue.y);
            bw.WriteFp(MaxValue.z);
        }

        public override void Deserialize(BinaryReader br) {
        //Layer (write as an int)
            Layer = (Constants.coll_layers)br.ReadInt32();
        //No need to read type; redundant
        //MinValue
            MinValue.x = br.ReadFp();
            MinValue.y = br.ReadFp();
            MinValue.z = br.ReadFp();
        //MaxValue
            MaxValue.x = br.ReadFp();
            MaxValue.y = br.ReadFp();
            MaxValue.z = br.ReadFp();
        }

        public override int GetHashCode() {
            int hashCode = 1858597544;
            hashCode = hashCode * -1521134295 + Layer.GetHashCode();
            hashCode = hashCode * -1521134295 + Type.GetHashCode();
            hashCode = hashCode * -1521134295 + MinValue.GetHashCode();
            hashCode = hashCode * -1521134295 + MaxValue.GetHashCode();
            return hashCode;
        }

        public AABBoxCollider(){
            MinValue = fp3.zero;
            MaxValue = fp3.zero;
            Layer = Constants.coll_layers.normal;
            Type = Constants.coll_types.aabb;
        }

        public AABBoxCollider(fp3 minVal, fp3 maxVal){
            MinValue = minVal;
            MaxValue = maxVal;
            Layer = Constants.coll_layers.normal;
            Type = Constants.coll_types.aabb;
        }

        public AABBoxCollider(fp3 minVal, fp3 maxVal, Constants.coll_layers l){
            MinValue = minVal;
            MaxValue = maxVal;
            Layer = l;
            Type = Constants.coll_types.aabb;
        }

        // TODO: Come up with nicer-looking overload?
        public AABBoxCollider(fp3 center, fp3 scale, bool isCenter){
            MinValue = center - scale/2;
            MaxValue = center + scale/2;
            Layer = Constants.coll_layers.normal;
            Type = Constants.coll_types.aabb;
        }

        public AABBoxCollider(fp3 center, fp3 scale, bool isCenter, Constants.coll_layers l){
            MinValue = center - scale/2;
            MaxValue = center + scale/2;
            Layer = l;
            Type = Constants.coll_types.aabb;
        }

        public fp3 Center(){
            return MinValue + (MaxValue - MinValue)/2;
        }

        // TODO: Write tests (0 size, negatives, max > min, etc.)
        public fp3 Size(){
            return (MaxValue - MinValue)/2;
        }

        public override CollisionPoints TestCollision(
            PhysTransform transform,
            Collider collider,
            PhysTransform colliderTransform)
        {
            return collider.TestCollision(colliderTransform, this, transform);
        }
    
        public override CollisionPoints TestCollision(
            PhysTransform transform,
            SphereCollider sphere,
            PhysTransform sphereTransform){
            return algo.FindSphereAABBCollisionPoints(
                sphere, transform, this, sphereTransform
            );
        }
    
        public override CollisionPoints TestCollision(
            PhysTransform transform,
            PlaneCollider plane,
            PhysTransform planeTransform){
            // TODO: Make a aabbox version
            // return algo.FindPlaneAABBCollisionPoints(
            //     this, transform, plane, planeTransform
            // );

            // TODO: Remove
            return new CollisionPoints();
        }

        public override CollisionPoints TestCollision(
            PhysTransform transform,
            CapsuleCollider capsule,
            PhysTransform capsuleTransform){
            return algo.FindCapsuleAABBCollisionPoints(
                capsule, transform, this, capsuleTransform
            );
        }

        public override CollisionPoints TestCollision(
            PhysTransform transform,
            AABBoxCollider box,
            PhysTransform boxTransform){
            return algo.FindAABBoxAABBoxCollisionPoints(
                this, transform, box, boxTransform
            );
        }
    };

    [Serializable]
    public struct CapsuleStats{
        public fp3 a_Normal;
        public fp3 a_LineEndOffset;
        public fp3 A;
        public fp3 B;
    }

    [Serializable]
    public class PlaneCollider : Collider{
        public fp3 Normal;
        public fp Distance;

        public override void Serialize(BinaryWriter bw) {
        //Layer (read as an int)
            bw.Write((int)Layer);
        //No need to write type; redundant
        //Normal
            bw.WriteFp(Normal.x);
            bw.WriteFp(Normal.y);
            bw.WriteFp(Normal.z);
        //Distance
            bw.WriteFp(Distance);
        }

        public override void Deserialize(BinaryReader br) {
        //Layer (write as an int)
            Layer = (Constants.coll_layers)br.ReadInt32();
        //No need to read type; redundant
        //Normal
            Normal.x = br.ReadFp();
            Normal.y = br.ReadFp();
            Normal.z = br.ReadFp();
        //Distance
            Distance = br.ReadFp();
        }

        public override int GetHashCode() {
            int hashCode = 1858597544;
            hashCode = hashCode * -1521134295 + Layer.GetHashCode();
            hashCode = hashCode * -1521134295 + Type.GetHashCode();
            hashCode = hashCode * -1521134295 + Normal.GetHashCode();
            hashCode = hashCode * -1521134295 + Distance.GetHashCode();
            return hashCode;
        }

        public PlaneCollider(fp3 n, fp d){
            Normal = n;
            Distance = d;
            Layer = Constants.coll_layers.normal;
            // TODO: Add plane type?
        }

        public PlaneCollider(fp3 n, fp d, Constants.coll_layers l){
            Normal = n;
            Distance = d;
            Layer = l;
            // TODO: Add plane type?
        }

        public override CollisionPoints TestCollision(
            PhysTransform transform,
            Collider collider,
            PhysTransform colliderTransform)
        {
            return collider.TestCollision(colliderTransform, this, transform);
        }
    
        public override CollisionPoints TestCollision(
            PhysTransform transform,
            SphereCollider sphere,
            PhysTransform sphereTransform)
        {
            return algo.FindSpherePlaneCollisionPoints(
                sphere, transform, this, sphereTransform);
        }
    
        public override CollisionPoints TestCollision(
            PhysTransform transform,
            PlaneCollider plane,
            PhysTransform planeTransform){
            // return algo.FindPlanePlaneCollisionPoints(
            // 	this, transform, plane, planeTransform
            // );

            // TODO
            return new CollisionPoints();
        }

        public override CollisionPoints TestCollision(
            PhysTransform transform,
            CapsuleCollider capsule,
            PhysTransform capsuleTransform){
            // return algo.FindCapsulePlaneCollisionPoints(
            // 	this, transform, capsule, capsuleTransform
            // );

            // TODO
            return new CollisionPoints();
        }

        public override CollisionPoints TestCollision(
            PhysTransform transform,
            AABBoxCollider plane,
            PhysTransform planeTransform){
            // TODO: Make a aabbox version
            // return algo.FindPlaneAABBCollisionPoints(
            //     this, transform, plane, planeTransform
            // );

            // TODO: Remove
            return new CollisionPoints();
        }
    };

    [Serializable]
    /* Using as a class (instead of a struct) since it represents a combination of values and will be mutated often. */
    public class PhysObject {
        public static uint CurrentInstanceId = 0; //Incrementing static PhysObject counter
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
        public ICollider IColl = null; // Attached script with OnCollision callbacks

        public void Serialize(BinaryWriter bw) {
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
            if(Coll is null){
                //Collider Type (read as an int)
                bw.Write((int)Constants.coll_types.none);
            }
            else{
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

        public void Deserialize(BinaryReader br) {
        //InstanceId
            InstanceId = br.ReadUInt32();
        //Transform
            Transform.Deserialize(br);
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
            if(collType != Constants.coll_types.none){
                // Create a new collider
                if(collType == Constants.coll_types.sphere) Coll = new SphereCollider();
                else if(collType == Constants.coll_types.capsule) Coll = new CapsuleCollider();
                else if(collType == Constants.coll_types.aabb) Coll = new AABBoxCollider();
                else{
                    Debug.LogWarning("No valid collider type found! Defaulting to SphereCollider!");
                     Coll = new SphereCollider();
                }
                // Then deserialize it
                Coll.Deserialize(br);
            }
        //IsDynamic
            IsDynamic = br.ReadBoolean();
        //IColl
            // if (!(IColl is null))
            //     IColl.Deserialize(br);
        }

        public override int GetHashCode() {
            int hashCode = 1858597544;
            // Don't account for InstanceId, as this can change
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

        public PhysObject(){
            InstanceId = PhysObject.CurrentInstanceId++;
            Transform = new PhysTransform();
            Velocity = new fp3();
            Force = new fp3();
            InverseMass = 1m/5m;
            Gravity = SepM.Physics.Constants.GRAVITY;
        }

        public PhysObject(int m){
            InstanceId = PhysObject.CurrentInstanceId++;
            Transform = new PhysTransform();
            Velocity = new fp3();
            Force = new fp3();
            InverseMass = 1m/m;
            Gravity = SepM.Physics.Constants.GRAVITY;
        }
        public PhysObject(fp3 pos){
            InstanceId = PhysObject.CurrentInstanceId++;
            PhysTransform newTransform = new PhysTransform();
            newTransform.Position = pos;
            
            Transform = newTransform;
            Velocity = new fp3();
            Force = new fp3();
            InverseMass = 1m/5m;
            Gravity = SepM.Physics.Constants.GRAVITY;
        }
        public PhysObject(PhysTransform t, fp3 v, fp3 f, fp m){
            InstanceId = PhysObject.CurrentInstanceId++;
            Transform = t;
            Velocity = v;
            Force = f;
            InverseMass = 1m/m;
            Gravity = SepM.Physics.Constants.GRAVITY;
        }

        public void AddForce(fp3 f){
            Force += f;
        }

        public fp GetMass(){
            return 1m/InverseMass;
        }

        /* If it's assigned, calls the ICollider's OnCollision method. */
        public void OnCollision(PhysCollision c){
            if(!(IColl is null)){
                IColl.OnCollision(c);
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
    };
}