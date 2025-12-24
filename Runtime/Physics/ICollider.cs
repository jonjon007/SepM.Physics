using System;
using System.IO;
using SepM.Math;
using SepM.Serialization;
using SepM.Utils;
using Unity.Collections;
using Unity.Mathematics.FixedPoint;

namespace SepM.Physics
{
    public interface ICollider : Serial {
        public void OnCollision<T>(PhysCollision c, T context);
    }

    [Serializable]
    public abstract class Collider : Serial {
        public Constants.coll_layers Layer;
        public Constants.coll_types Type = Constants.coll_types.none;

        public abstract void Serialize(BinaryWriter bw);

        public abstract Serial Deserialize<T>(BinaryReader br, T context);

        public abstract int Checksum { get; }

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
        
        public override int Checksum => GetHashCode();

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

        public override Serial Deserialize<T>(BinaryReader br, T context) {
        //Layer (write as an int)
            Layer = (Constants.coll_layers)br.ReadInt32();
        //No need to read type; redundant
        //Center
            Center.x = br.ReadFp();
            Center.y = br.ReadFp();
            Center.z = br.ReadFp();
            //Radius
            Radius = br.ReadFp();

            return this;
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
        
        public override int Checksum => GetHashCode();

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

        public override Serial Deserialize<T>(BinaryReader br, T context) {
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

            return this;
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

        public CapsuleCollider(fp3 c, fp r, fp h, fp3 d)
        {
            Center = c;
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
            fp3 worldPosition = transform == null ? fp3.zero : transform.WorldPosition();
            fpq worldRotation = transform == null ? fpq.identity : transform.WorldRotation();

            fp3 netDirection = Direction.multiply(worldRotation);
            fp3 tip = worldPosition + Center + netDirection *(Height/2m);
            fp3 bse = worldPosition + Center - netDirection * (Height/2m);
            fp3 a_Normal = netDirection.normalized();
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
        
        public override int Checksum => GetHashCode();

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

        public override Serial Deserialize<T>(BinaryReader br, T context) {
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

            return this;
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
    public class PlaneCollider : Collider{
        public fp3 Normal;
        public fp Distance;
        
        public override int Checksum => GetHashCode();

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

        public override Serial Deserialize<T>(BinaryReader br, T context) {
        //Layer (write as an int)
            Layer = (Constants.coll_layers)br.ReadInt32();
        //No need to read type; redundant
        //Normal
            Normal.x = br.ReadFp();
            Normal.y = br.ReadFp();
            Normal.z = br.ReadFp();
        //Distance
            Distance = br.ReadFp();
            return this;
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

}