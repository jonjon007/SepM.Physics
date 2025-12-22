using System;
using System.IO;
using UnityEngine;
using Unity.Mathematics.FixedPoint;
using Unity.Collections;
using SepM.Math;
using SepM.Serialization;
using SepM.Utils;
using Newtonsoft.Json;

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
    public struct PhysCollision : Serial{
        public uint ObjIdA;
        public uint ObjIdB;
        public CollisionPoints Points;

        public void Serialize(BinaryWriter bw)
        {
        //ObjIdA
            bw.Write(ObjIdA);
        //ObjIdB
            bw.Write(ObjIdB);
        //Points
            Points.Serialize(bw);
        }

        public Serial Deserialize<T>(BinaryReader br, T context)
        {
        //ObjIdA
            ObjIdA = br.ReadUInt32();
        //ObjIdB
            ObjIdB = br.ReadUInt32();
        //Points
            Points = (CollisionPoints)Points.Deserialize(br, context);

            return this;
        }

        /// <summary>
        /// Gets the deterministic checksum for this collision.
        /// Structs don't need caching as they're immutable value types.
        /// </summary>
        public int Checksum {
            get {
                using (var memoryStream = new System.IO.MemoryStream()) {
                    using (var writer = new System.IO.BinaryWriter(memoryStream, System.Text.Encoding.UTF8, leaveOpen: true)) {
                        Serialize(writer);
                        writer.Flush();
                    }
                    var bytes = new NativeArray<byte>(memoryStream.ToArray(), Allocator.Temp);
                    int checksum = Utilities.CalcFletcher32(bytes);
                    bytes.Dispose();
                    return checksum;
                }
            }
        }

        public override int GetHashCode()
        {
            int hashCode = -1214587014;
            hashCode = hashCode * -1521134295 + ObjIdA.GetHashCode();
            hashCode = hashCode * -1521134295 + ObjIdB.GetHashCode();
            hashCode = hashCode * -1521134295 + Points.GetHashCode();

            return hashCode;
        }
    }

    [Serializable]
    public struct CollisionPoints : Serial {
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

        public void Serialize(BinaryWriter bw)
        {
        //A
            bw.WriteFp(A.x);
            bw.WriteFp(A.y);
            bw.WriteFp(A.z);
        //B
            bw.WriteFp(B.x);
            bw.WriteFp(B.y);
            bw.WriteFp(B.z);
        //Normal
            bw.WriteFp(Normal.x);
            bw.WriteFp(Normal.y);
            bw.WriteFp(Normal.z);
        //DepthSqrd
            bw.WriteFp(DepthSqrd);
        //HasCollision
            bw.Write(HasCollision);
        }

        public Serial Deserialize<T>(BinaryReader br, T context)
        {
        //A
            A.x = br.ReadFp();
            A.y = br.ReadFp();
            A.z = br.ReadFp();
        //B
            B.x = br.ReadFp();
            B.y = br.ReadFp();
            B.z = br.ReadFp();
        //Normal
            Normal.x = br.ReadFp();
            Normal.y = br.ReadFp();
            Normal.z = br.ReadFp();
        //DepthSqrd
            DepthSqrd = br.ReadFp();
        //HasCollision
            HasCollision = br.ReadBoolean();

            return this;
        }

        /// <summary>
        /// Gets the deterministic checksum for this collision data.
        /// Structs don't need caching as they're immutable value types.
        /// </summary>
        public int Checksum {
            get {
                using (var memoryStream = new System.IO.MemoryStream()) {
                    using (var writer = new System.IO.BinaryWriter(memoryStream, System.Text.Encoding.UTF8, leaveOpen: true)) {
                        Serialize(writer);
                        writer.Flush();
                    }
                    var bytes = new NativeArray<byte>(memoryStream.ToArray(), Allocator.Temp);
                    int checksum = Utilities.CalcFletcher32(bytes);
                    bytes.Dispose();
                    return checksum;
                }
            }
        }

        public override int GetHashCode()
        {
            int hashCode = -1214587014;
            hashCode = hashCode * -1521134295 + A.GetHashCode();
            hashCode = hashCode * -1521134295 + B.GetHashCode();
            hashCode = hashCode * -1521134295 + Normal.GetHashCode();
            hashCode = hashCode * -1521134295 + DepthSqrd.GetHashCode();
            hashCode = hashCode * -1521134295 + HasCollision.GetHashCode();

            return hashCode;
        }
    };

    public interface ICollider : Serial {
        public void OnCollision<T>(PhysCollision c, T context);
    }

    [Serializable]
    public abstract class Collider : Serial {
        public Constants.coll_layers Layer;
        public Constants.coll_types Type = Constants.coll_types.none;

        public abstract void Serialize(BinaryWriter bw);

        public abstract Serial Deserialize<T>(BinaryReader br, T context);

        /// <summary>
        /// Gets the deterministic checksum for this collider.
        /// Implemented by concrete collider types.
        /// </summary>
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
        
        private int? _cachedChecksum;
        
        public override int Checksum {
            get {
                if (_cachedChecksum == null) {
                    using (var memoryStream = new System.IO.MemoryStream()) {
                        using (var writer = new System.IO.BinaryWriter(memoryStream, System.Text.Encoding.UTF8, leaveOpen: true)) {
                            Serialize(writer);
                            writer.Flush();
                        }
                        var bytes = new NativeArray<byte>(memoryStream.ToArray(), Allocator.Temp);
                        _cachedChecksum = Utilities.CalcFletcher32(bytes);
                        bytes.Dispose();
                    }
                }
                return _cachedChecksum.Value;
            }
        }

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
        
        private int? _cachedChecksum;
        
        public override int Checksum {
            get {
                if (_cachedChecksum == null) {
                    using (var memoryStream = new System.IO.MemoryStream()) {
                        using (var writer = new System.IO.BinaryWriter(memoryStream, System.Text.Encoding.UTF8, leaveOpen: true)) {
                            Serialize(writer);
                            writer.Flush();
                        }
                        var bytes = new NativeArray<byte>(memoryStream.ToArray(), Allocator.Temp);
                        _cachedChecksum = Utilities.CalcFletcher32(bytes);
                        bytes.Dispose();
                    }
                }
                return _cachedChecksum.Value;
            }
        }

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
        
        private int? _cachedChecksum;
        
        public override int Checksum {
            get {
                if (_cachedChecksum == null) {
                    using (var memoryStream = new System.IO.MemoryStream()) {
                        using (var writer = new System.IO.BinaryWriter(memoryStream, System.Text.Encoding.UTF8, leaveOpen: true)) {
                            Serialize(writer);
                            writer.Flush();
                        }
                        var bytes = new NativeArray<byte>(memoryStream.ToArray(), Allocator.Temp);
                        _cachedChecksum = Utilities.CalcFletcher32(bytes);
                        bytes.Dispose();
                    }
                }
                return _cachedChecksum.Value;
            }
        }

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
        
        private int? _cachedChecksum;
        
        public override int Checksum {
            get {
                if (_cachedChecksum == null) {
                    using (var memoryStream = new System.IO.MemoryStream()) {
                        using (var writer = new System.IO.BinaryWriter(memoryStream, System.Text.Encoding.UTF8, leaveOpen: true)) {
                            Serialize(writer);
                            writer.Flush();
                        }
                        var bytes = new NativeArray<byte>(memoryStream.ToArray(), Allocator.Temp);
                        _cachedChecksum = Utilities.CalcFletcher32(bytes);
                        bytes.Dispose();
                    }
                }
                return _cachedChecksum.Value;
            }
        }

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

            _cachedChecksum = null;
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
        
        private int? _cachedChecksum;
        
        /// <summary>
        /// Gets the deterministic checksum for this physics object.
        /// Uses Fletcher32 on serialized bytes with caching for performance.
        /// </summary>
        [JsonProperty]
        public int Checksum {
            get {
                if (_cachedChecksum == null) {
                    using (var memoryStream = new System.IO.MemoryStream()) {
                        using (var writer = new System.IO.BinaryWriter(memoryStream, System.Text.Encoding.UTF8, leaveOpen: true)) {
                            Serialize(writer);
                            writer.Flush();
                        }
                        var bytes = new NativeArray<byte>(memoryStream.ToArray(), Allocator.Temp);
                        _cachedChecksum = SepM.Utils.Utilities.CalcFletcher32(bytes);
                        bytes.Dispose();
                    }
                }
                return _cachedChecksum.Value;
            }
        }
        
        private void InvalidateChecksum() {
            _cachedChecksum = null;
        }

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
            InvalidateChecksum();
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
            InvalidateChecksum();
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
            InvalidateChecksum();
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