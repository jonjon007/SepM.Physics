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
        public fp DepthSqrd;    // Length of B – A
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

        // TODO: Find a way to serialize this without depth limit issues!
        // private List<PhysTransform> m_children;
        public PhysTransform(){
            Position = fp3.zero;
            Scale = new fp3(1,1,1);
            Rotation = fpq.identity;
            m_parent = null;
            //m_children = new List<PhysTransform>();
        }
        public PhysTransform(fp3 p){
            Position = p;
            Scale = new fp3(1,1,1);
            Rotation = fpq.identity;
            m_parent = null;
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
    }

    [Serializable]
    public abstract class Collider {
        public Constants.coll_layers Layer;

        public abstract void Serialize(BinaryWriter bw);

        public abstract void Deserialize(BinaryReader br);

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
        //Center
            bw.Write(Center.x);
            bw.Write(Center.y);
            bw.Write(Center.z);
        //Radius
            bw.Write(Radius);
        }

        public override void Deserialize(BinaryReader br) {
        //Layer (write as an int)
            Layer = (Constants.coll_layers)br.ReadInt32();
        //Center
            Center.x = br.ReadDecimal();
            Center.y = br.ReadDecimal();
            Center.z = br.ReadDecimal();
            //Radius
            Radius = br.ReadDecimal();
        }

        public SphereCollider(fp r){
            Center = fp3.zero;
            Radius = r;
            Layer = Constants.coll_layers.normal;
        }

        public SphereCollider(fp r, Constants.coll_layers l){
            Center = fp3.zero;
            Radius = r;
            Layer = l;
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
            PhysTransform planeTransform){
            return algo.FindSphereAABBCollisionPoints(
                this, transform, box, planeTransform
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
        //Center
            bw.Write(Center.x);
            bw.Write(Center.y);
            bw.Write(Center.z);
        //Radius
            bw.Write(Radius);
        //Height
            bw.Write(Height);
        //Direction
            bw.Write(Direction.x);
            bw.Write(Direction.y);
            bw.Write(Direction.z);
        }

        public override void Deserialize(BinaryReader br) {
        //Layer (write as an int)
            Layer = (Constants.coll_layers)br.ReadInt32();
        //Center
            Center.x = br.ReadDecimal();
            Center.y = br.ReadDecimal();
            Center.z = br.ReadDecimal();
        //Radius
            Radius = br.ReadDecimal();
        //Height
            Height = br.ReadDecimal();
        //Direction
            Direction.x = br.ReadDecimal();
            Direction.y = br.ReadDecimal();
            Direction.z = br.ReadDecimal();
        }

        public CapsuleCollider(){
			Center = fp3.zero;
			Radius = 0.5m;
			Height = 2;
            Direction = new fp3(0,1,0);
            Layer = Constants.coll_layers.normal;
        }
    
        public CapsuleCollider(fp r, fp h){
            Center = fp3.zero;
            Radius = r;
            Height = h;
            Direction = new fp3(0,1,0);
            Layer = Constants.coll_layers.normal;
        }

        public CapsuleCollider(fp r, fp h, Constants.coll_layers l){
            Center = fp3.zero;
            Radius = r;
            Height = h;
            Direction = new fp3(0,1,0);
            Layer = l;
        }
        
        public CapsuleCollider(fp3 c, fp r, fp h){
            Center = c;
            Radius = r;
            Height = h;
            Direction = new fp3(0,1,0);
            Layer = Constants.coll_layers.normal;
        }

        public CapsuleCollider(fp3 c, fp r, fp h, Constants.coll_layers l){
            Center = c;
            Radius = r;
            Height = h;
            Direction = new fp3(0,1,0);
            Layer = l;
        }

        public CapsuleStats GetStats(PhysTransform transform){
            fp3 tip = transform.Position + Center + Direction *(Height/2m);
            fp3 bse = transform.Position + Center - Direction *(Height/2m);
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
            AABBoxCollider plane,
            PhysTransform planeTransform){
            // TODO: Make a aabbox version
            // return algo.FindCapsuleAABBCollisionPoints(
            //     this, transform, plane, planeTransform
            // );

            // TODO: Remove
            return new CollisionPoints();
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
        //MinValue
            bw.Write(MinValue.x);
            bw.Write(MinValue.y);
            bw.Write(MinValue.z);
        //MaxValue
            bw.Write(MaxValue.x);
            bw.Write(MaxValue.y);
            bw.Write(MaxValue.z);
        }

        public override void Deserialize(BinaryReader br) {
        //Layer (write as an int)
            Layer = (Constants.coll_layers)br.ReadInt32();
        //MinValue
            MinValue.x = br.ReadDecimal();
            MinValue.y = br.ReadDecimal();
            MinValue.z = br.ReadDecimal();
        //MaxValue
            MaxValue.x = br.ReadDecimal();
            MaxValue.y = br.ReadDecimal();
            MaxValue.z = br.ReadDecimal();
        }
        public AABBoxCollider(fp3 minVal, fp3 maxVal){
            MinValue = minVal;
            MaxValue = maxVal;
            Layer = Constants.coll_layers.normal;
        }

        public AABBoxCollider(fp3 minVal, fp3 maxVal, Constants.coll_layers l){
            MinValue = minVal;
            MaxValue = maxVal;
            Layer = l;
        }

        // TODO: Consider getting rid of the minval max val constructor?
        public AABBoxCollider(fp3 center, fp3 scale, bool isCenter){
            MinValue = center - scale/2;
            MaxValue = center + scale/2;
            Layer = Constants.coll_layers.normal;
        }

        public AABBoxCollider(fp3 center, fp3 scale, bool isCenter, Constants.coll_layers l){
            MinValue = center - scale/2;
            MaxValue = center + scale/2;
            Layer = l;
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
            PhysTransform sphereTransform)
        {
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
            // TODO: Make a aabbox version
            // return algo.FindCapsuleAABBCollisionPoints(
            //     this, transform, capsule, planeTransform
            // );

            // TODO: Remove
            return new CollisionPoints();
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
        //Normal
            bw.Write(Normal.x);
            bw.Write(Normal.y);
            bw.Write(Normal.z);
        //Distance
            bw.Write(Distance);
        }

        public override void Deserialize(BinaryReader br) {
        //Layer (write as an int)
            Layer = (Constants.coll_layers)br.ReadInt32();
        //Normal
            Normal.x = br.ReadDecimal();
            Normal.y = br.ReadDecimal();
            Normal.z = br.ReadDecimal();
        //Distance
            Distance = br.ReadDecimal();
        }

        public PlaneCollider(fp3 n, fp d){
            Normal = n;
            Distance = d;
            Layer = Constants.coll_layers.normal;
        }

        public PlaneCollider(fp3 n, fp d, Constants.coll_layers l){
            Normal = n;
            Distance = d;
            Layer = l;
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
        //PhysTransform
            Transform.Serialize(bw);
        //Velocity
            bw.Write(Velocity.x);
            bw.Write(Velocity.y);
            bw.Write(Velocity.z);
         //Gravity
            bw.Write(Gravity.x);
            bw.Write(Gravity.y);
            bw.Write(Gravity.z);
        //Force
            bw.Write(Force.x);
            bw.Write(Force.y);
            bw.Write(Force.z);
        //InverseMass
            bw.Write(InverseMass);
        //IsKinematic
            bw.Write(IsKinematic);
        //Restitution
            bw.Write(Restitution);
        //DynamicFriction
            bw.Write(DynamicFriction);
        //StaticFriction
            bw.Write(StaticFriction);
        // TODO: Figure out how to safely do this
        //Coll
            if(!(Coll is null))
                Coll.Serialize(bw);
        //IsDynamic
            bw.Write(IsDynamic);
        //IColl
            if (!(IColl is null))
                IColl.Serialize(bw);
        }

        public void Deserialize(BinaryReader br) {
        //PhysTransform
            Transform.Deserialize(br);
        //Velocity
            Velocity.x = br.ReadDecimal();
            Velocity.y = br.ReadDecimal();
            Velocity.z = br.ReadDecimal();
        //Gravity
            Gravity.x = br.ReadDecimal();
            Gravity.y = br.ReadDecimal();
            Gravity.z = br.ReadDecimal();
        //Force
            Force.x = br.ReadDecimal();
            Force.y = br.ReadDecimal();
            Force.z = br.ReadDecimal();
        //InverseMass
            InverseMass = br.ReadDecimal();
        //IsKinematic
            IsKinematic = br.ReadBoolean();
        //Restitution
            Restitution = br.ReadDecimal();
        //DynamicFriction
            DynamicFriction = br.ReadDecimal();
        //StaticFriction
            StaticFriction = br.ReadDecimal();
        //Coll
            if (!(Coll is null))
                Coll.Deserialize(br);
        //IsDynamic
            IsDynamic = br.ReadBoolean();
        //IColl
            if (!(IColl is null))
                IColl.Deserialize(br);
        }

        public PhysObject(){
            Transform = new PhysTransform();
            Velocity = new fp3();
            Force = new fp3();
            InverseMass = 1m/5m;
            Gravity = SepM.Physics.Constants.GRAVITY;
        }

        public PhysObject(int m){
            Transform = new PhysTransform();
            Velocity = new fp3();
            Force = new fp3();
            InverseMass = 1m/m;
            Gravity = SepM.Physics.Constants.GRAVITY;
        }
        public PhysObject(fp3 pos){
            PhysTransform newTransform = new PhysTransform();
            newTransform.Position = pos;
            
            Transform = newTransform;
            Velocity = new fp3();
            Force = new fp3();
            InverseMass = 1m/5m;
            Gravity = SepM.Physics.Constants.GRAVITY;
        }
        public PhysObject(PhysTransform t, fp3 v, fp3 f, fp m){
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