using System.Collections.Generic;
using Unity.Mathematics.FixedPoint;
using SepM.Utils;
using SepM.Math;
using System;

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
    }

    [Serializable]
    public class PhysTransform { // Describes an objects location
        public fp3 Position;
        public fp3 Scale;
        public fpq Rotation;
        private PhysTransform m_parent;
        private List<PhysTransform> m_children;
        // private PhysTransform t_parent;
        public PhysTransform(){
            Position = fp3.zero;
            Scale = new fp3(1,1,1);
            Rotation = fpq.identity;
            m_parent = null;
            m_children = new List<PhysTransform>();
        }
        public PhysTransform(fp3 p){
            Position = p;
            Scale = new fp3(1,1,1);
            Rotation = fpq.identity;
            m_parent = null;
            m_children = new List<PhysTransform>();
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
    };

    [Serializable]
    public abstract class Collider {
        public Constants.coll_layers Layer;
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
    };

    [Serializable]
    public class CapsuleCollider : Collider{
        public fp3 Center;
        public fp Radius;
        public fp Height;
        public fp3 Direction;

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
        public Collider Coll = null;
        public bool IsDynamic = false;
        public ICollider IColl = null;
        
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