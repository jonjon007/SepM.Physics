using System.Collections.Generic;
using Unity.Mathematics.FixedPoint;
using SepM.Utils;

namespace SepM.Physics{
    public static class algo{
        public static CollisionPoints FindSphereSphereCollisionPoints(
            SphereCollider a, PhysTransform ta,
            SphereCollider b, PhysTransform tb)
        {
            fp3 A = a.Center + ta.WorldPosition();
            fp3 B = b.Center + tb.WorldPosition();

            fp Ar = a.Radius * ta.WorldScale().major();
            fp Br = b.Radius * tb.WorldScale().major();

            fp3 AtoB = B - A;
            fp3 BtoA = A - B;

            if (AtoB.lengthSqrd() > (Ar + Br).sqrd()) {
                return CollisionPoints.noCollision;
            }

            A += AtoB.normalized() * Ar;
            B += BtoA.normalized() * Br;

            AtoB = B - A;

            return new CollisionPoints{
                A = A,
                B = B,
                Normal = AtoB.normalized(),
                DepthSqrd = AtoB.lengthSqrd(),
                HasCollision = true
            };
        }

        // Transforms dont work for plane
        // TODO: Assumes plane is infinite; add bounds calculations
        public static CollisionPoints FindSpherePlaneCollisionPoints(
            SphereCollider a, PhysTransform ta,
            PlaneCollider  b, PhysTransform tb)
        {
            fp3 A  = a.Center + ta.WorldPosition();
            fp Ar = a.Radius * ta.WorldScale().major();

            fp3 N = b.Normal.multiply(tb.WorldRotation());
            N.normalize();
            
            fp3 P = N * b.Distance + tb.WorldPosition();

            fp d = (A - P).dot(N); // distance from center of sphere to plane surface

            if (d > Ar) {
                return CollisionPoints.noCollision;
            }
            
            fp3 B = A - N * d;
                    A = A - N * Ar;

            fp3 AtoB = B - A;

            return new CollisionPoints{
                A = A,
                B = B, 
                Normal = AtoB.normalized(), 
                DepthSqrd = AtoB.lengthSqrd(),
                HasCollision = true
            };
        }

        public static CollisionPoints FindSphereCapsuleCollisionPoints(
            SphereCollider  a, PhysTransform ta,
            CapsuleCollider b, PhysTransform tb)
        {
            fp Bhs = 1.0m;
            fp Brs = 1.0m;

            fp3 s = tb.WorldScale();
            // TODO: Will need to verify this condition
            // Right
            if (b.Direction.Equals(new fp3(1,0,0))) {
                Bhs = s.x;
                Brs = new fp2(s.y, s.z).major();
            }
            // Up
            else if (b.Direction.Equals(new fp3(0,1,0))) {
                Bhs = s.y;
                Brs = new fp2(s.x, s.z).major();
            }
            // Forward
            else if (b.Direction.Equals(new fp3(0,0,1))) {
                Bhs = s.z;
                Brs = new fp2(s.x, s.y).major();
            }

            fp3 offset = b.Direction.multiply(tb.WorldRotation()) * (b.Height * Bhs / 2 - b.Radius * Brs);

            fp3 A = a.Center          + ta.WorldPosition();
            fp3 B = b.Center - offset + tb.WorldPosition();
            fp3 C = b.Center + offset + tb.WorldPosition(); // might not be correct
            
            fp Ar = a.Radius * ta.WorldScale().major();
            fp Br = b.Radius * Brs;

            fp3 BtoA = A - B;
            fp3 BtoC = C - B;

            fp d = BtoC.normalized().dot(BtoA).clamp(0, BtoC.lengthSqrd().sqrt());
            fp3 D = B + BtoC.normalized() * d;

            fp3 AtoD = D - A;
            fp3 DtoA = A - D;

            if (AtoD.lengthSqrd() > (Ar + Br).sqrd()) {
                return CollisionPoints.noCollision;
            }

            A += AtoD.normalized() * Ar;
            D += DtoA.normalized() * Br;

            AtoD = D - A;

            return new CollisionPoints(){
                A = A,
                B = D,
                Normal = AtoD.normalized(),
                DepthSqrd = AtoD.lengthSqrd(),
                HasCollision = true
            };
        }

        public static CollisionPoints FindCapsuleCapsuleCollisionPoints(
            CapsuleCollider a, PhysTransform ta,
            CapsuleCollider b, PhysTransform tb)
        {
            CapsuleStats aStats = a.GetStats(ta);
            CapsuleStats bStats = b.GetStats(tb);

            // vectors between line endpoints:
            fp3 v0 = bStats.A - aStats.A; 
            fp3 v1 = bStats.B - aStats.A; 
            fp3 v2 = bStats.A - aStats.B; 
            fp3 v3 = bStats.B - aStats.B;

            // squared distances:
            fp d0 = v0.dot(v0); 
            fp d1 = v1.dot(v1); 
            fp d2 = v2.dot(v2); 
            fp d3 = v3.dot(v3);

            // select best potential endpoint on capsule A:
            fp3 bestA;
            if (d2 < d0 || d2 < d1 || d3 < d0 || d3 < d1){
                bestA = aStats.B;
            }
            else{
                bestA = aStats.A;
            }
            
            // select point on capsule B line segment nearest to best potential endpoint on A capsule:
            fp3 bestB = Utilities.closestPointOnLineSegment(bStats.A, bStats.B, bestA);
            
            // now do the same for capsule A segment:
            bestA = Utilities.closestPointOnLineSegment(aStats.A, aStats.B, bestB);

            bool intersects = false;
            fp3 penetration_normal = bestA - bestB;
            fp len = 0;

            if(!penetration_normal.Equals(fp3.zero)){
                len = penetration_normal.lengthSqrd().sqrt();
                if(len == 0)
                    penetration_normal = fp3.zero;
                else
                    penetration_normal /= len;  // normalize
            }
            fp penetration_depth = a.Radius + b.Radius - len;
            intersects = penetration_depth >= 0;

            if(intersects){
                return new CollisionPoints(){
                    A = bestA,
                    B = bestB,
                    Normal = penetration_normal,
                    DepthSqrd = penetration_depth.sqrd(),
                    HasCollision = true
                };
            }

            return CollisionPoints.noCollision;
        }

        public static CollisionPoints FindCapsuleAABBCollisionPoints(
            CapsuleCollider a, PhysTransform ta,
            AABBoxCollider b, PhysTransform tb)
        {
            AABBoxCollider tempBox = new AABBoxCollider(
                a.Center, new fp3(a.Radius*2, a.Height*2, a.Radius*2), true);
            return FindAABBoxAABBoxCollisionPoints(
                tempBox,
                ta,
                b,
                tb
            );

            // TODO: Make an accurate calculation instead of subbing for another box
            /*
            CapsuleStats aStats = a.GetStats(ta);

            // Find point (p) on AABB closest to Sphere center
            fp3 pA = b.closestPointAABB(tb, aStats.A);
            fp3 pB = b.closestPointAABB(tb, aStats.B);

            fp dA = (pA-aStats.A).dot(pA-aStats.A);
            fp dB = (pB-aStats.B).dot(pB-aStats.B);

            // If the two clamped points are equal, test capsule-point intersection to that point
            // Sphere w/ radius 0
            if(pA.Equals(pB)){
                SphereCollider tempPoint = new SphereCollider(0);
                // TODO: Should I copy the PhysTransform and alter or just create a new one?
                PhysTransform tempTransform = new PhysTransform(dA < dB ? pA : pB);
                return FindSphereCapsuleCollisionPoints(
                    tempPoint,
                    ta,
                    a,
                    tempTransform
                );
            }
            // If the two clamped poitns are different, test capsule-line sesgment intersection
            // Capsule w/ radius 0
            else{
                fp3 lineNormal = Raycast(
                    b,
                    dA < dB ? aStats.A : aStats.B,
                    dA < dB ? (pA - aStats.A).normalized() : (pB - aStats.B).normalized()
                    ).Normal;
                fp height;
                if(lineNormal.x > 0){
                    height = b.Size().x*2;
                }
                else if(lineNormal.y > 0){
                    height = b.Size().y*2;
                }
                else{
                    height = b.Size().z*2;
                }
                CapsuleCollider tempLine = new CapsuleCollider(0, height, lineNormal);
                // TODO: Should I copy the PhysTransform and alter or just create a new one?
                PhysTransform tempTransform = new PhysTransform(dA < dB ? pA : pB);
                return FindCapsuleCapsuleCollisionPoints(
                    tempLine,
                    ta,
                    a,
                    tempTransform
                );
            }
            */
        }

        public static CollisionPoints FindCapsuleAABBsCollisionPoints(
            CapsuleCollider a, PhysTransform ta,
            AABBoxCollider b, PhysTransform tb)
        {
            // TODO: Decide is we really need to do a capsule or can just compare AABBs
            AABBoxCollider tempBox = new AABBoxCollider(a.Center, new fp3(a.Radius*2, a.Height, a.Radius*2), true);
            return FindAABBoxAABBoxCollisionPoints(tempBox, ta, b, tb);
        }

        public static CollisionPoints FindSphereAABBCollisionPoints(
            SphereCollider a, PhysTransform ta,
            AABBoxCollider b, PhysTransform tb)
        {
            fp dist;

            fp3 A = a.Center + ta.WorldPosition();

            // Find point (p) on AABB closest to Sphere center
            fp3 p = b.closestPointAABB(tb, A);

            // Sphere and AABB intersect if the (squared) distance from sphere center to point (p)
            // is less than the (squared) sphere radius
            fp3 v = p - A;

            if (v.dot(v) <= a.Radius * a.Radius)
            {
                dist = b.SqDistPointAABB(tb, A);

                // Calculate normal using sphere center a closest point on AABB
                fp3 normal = (p - A).normalized();

                fp3 B = p - normal*a.Radius;
                fp3 AtoB = B - A;

                if (AtoB.Equals(fp3.zero)){
                    // Sphere is inside AABB
                    AtoB = new fp3(0,1,0);
                }

                fp dsqrd = AtoB.lengthSqrd();

                return new CollisionPoints(){
                    A = a.Center + ta.WorldPosition(),
                    B = B,
                    Normal = AtoB.normalized(),
                    DepthSqrd = AtoB.lengthSqrd(),
                    HasCollision = true
                };
            }

            // No intersection
            return CollisionPoints.noCollision;
        }

        public static CollisionPoints FindAABBoxAABBoxCollisionPoints(
            AABBoxCollider a, PhysTransform ta,
            AABBoxCollider b, PhysTransform tb)
        {
            fp3 boxAPos = ta.WorldPosition() + a.Center();
            fp3 boxBPos = tb.WorldPosition() + b.Center();

            fp3 boxASize = a.Size();
            fp3 boxBSize = b.Size();

            bool overlap = AABBTest (boxAPos, boxBPos, boxASize, boxBSize);
            if (overlap) {
                fp3[] faces = {
                    new fp3 (-1, 0, 0), new fp3 (1, 0, 0),
                    new fp3 (0, -1, 0), new fp3 (0, 1, 0),
                    new fp3 (0, 0, -1), new fp3 (0, 0, 1),
                };

                fp3 maxA = boxAPos + boxASize;
                fp3 minA = boxAPos - boxASize;

                fp3 maxB = boxBPos + boxBSize;
                fp3 minB = boxBPos - boxBSize;

                fp[] distances = {
                    (maxB . x - minA . x ), // distance of box ’b ’ to ’ left ’ of ’a ’.
                    (maxA . x - minB . x ), // distance of box ’b ’ to ’ right ’ of ’a ’.
                    (maxB . y - minA . y ), // distance of box ’b ’ to ’ bottom ’ of ’a ’.
                    (maxA . y - minB . y ), // distance of box ’b ’ to ’ top ’ of ’a ’.
                    (maxB . z - minA . z ), // distance of box ’b ’ to ’ far ’ of ’a ’.
                    (maxA . z - minB . z ) // distance of box ’b ’ to ’ near ’ of ’a ’.
                };
                fp penetration = fp.max_value;
                fp3 bestAxis = fp3.zero;

                for (int i = 0; i < 6; i ++){
                    if (distances [ i ] < penetration ) {
                        penetration = distances [ i ];
                        bestAxis = faces [ i ];
                    }
                }
                return new CollisionPoints{
                    A = boxAPos,
                    B = boxBPos,
                    Normal = -bestAxis,
                    DepthSqrd = penetration.sqrd(),
                    HasCollision = true
                };
            }
            return CollisionPoints.noCollision;
        }

        //tell us whether the objects are colliding
        static bool AABBTest (fp3 posA, fp3 posB, fp3 halfSizeA, fp3 halfSizeB) {
            fp3 delta = posB - posA;
            fp3 totalSize = halfSizeA + halfSizeB;

            if (System.Math.Abs(delta.x) <= totalSize.x
                && System.Math.Abs(delta.y) <= totalSize.y
                && System.Math.Abs(delta.z) <= totalSize.z){
                    return true ;
            }

            return false;
        }

        public static fp3 closestPointAABB(this AABBoxCollider box, PhysTransform boxTransform, fp3 point) // P131
        {
            // For each coordinate axis, if the point coordinate value is outside box,
            // clamp it to the box, else keep it as is
            fp3 min = box.MinValue + boxTransform.WorldPosition();
            fp3 max = box.MaxValue + boxTransform.WorldPosition();
            fp3 q = fp3.zero;
            fp v = 0;
            v = point.x;
            v = System.Math.Max(v, min.x);
            v = System.Math.Min(v, max.x);
            q.x = v;
            v = point.y;
            v = System.Math.Max(v, min.y);
            v = System.Math.Min(v, max.y);
            q.y = v;
            v = point.z;
            v = System.Math.Max(v, min.z);
            v = System.Math.Min(v, max.z);
            q.z = v;
            
            return q;
        }

        public static fp SqDistPointAABB(this AABBoxCollider box, PhysTransform boxTransform, fp3 point){
            fp result = (box.closestPointAABB(boxTransform, point) - point).lengthSqrd();
            return result;
        }

        public static CollisionPoints Raycast(List<PhysObject> p_objs, fp3 origin, fp3 dir, long layers){
            List<PhysObject> filetered_colls = p_objs.FindAll(p => !(p.Coll is null) && p.Coll.InLayers(layers));
            foreach(PhysObject p_obj in filetered_colls){
                CollisionPoints raycastResult = Raycast(p_obj, origin, dir, layers);
                if(raycastResult.HasCollision)
                    return raycastResult;
            }
            return CollisionPoints.noCollision;
        }

        public static CollisionPoints Raycast(PhysObject p_obj, fp3 origin, fp3 dir){
            return Raycast(p_obj, origin, dir, Constants.layer_all);
        }

        public static CollisionPoints Raycast(PhysObject p_obj, fp3 origin, fp3 dir, long layers){
            if(p_obj.Coll is null)
                return CollisionPoints.noCollision;
            else
                return Raycast(p_obj.Coll, origin - p_obj.Transform.WorldPosition(), dir, layers);
        }

        public static CollisionPoints Raycast(List<Collider> collList, fp3 origin, fp3 dir, long layers){
            List<Collider> filetered_colls = collList.FindAll(c => c.InLayers(layers));
            foreach (Collider coll in filetered_colls){
                CollisionPoints raycastResult = Raycast(coll, origin, dir, layers);
                if(raycastResult.HasCollision)
                    return raycastResult;
            }
            return CollisionPoints.noCollision;
        }

        public static List<CollisionPoints> RaycastAll(List<Collider> collList, fp3 origin, fp3 dir, long layers){
            List<CollisionPoints> resultList = new List<CollisionPoints>();

            List<Collider> filetered_colls = collList.FindAll(c => c.InLayers(layers));
            foreach (Collider coll in filetered_colls){
                CollisionPoints raycastResult = Raycast(coll, origin, dir, layers);
                if(raycastResult.HasCollision)
                    resultList.Add(raycastResult);
            }
            return resultList;
        }

        public static CollisionPoints Raycast(Collider coll, fp3 origin, fp3 dir){
            return Raycast(coll, origin, dir, Constants.layer_all);
        }

        public static CollisionPoints Raycast(Collider coll, fp3 origin, fp3 dir, long layers){
            if(coll is AABBoxCollider)
                return Raycast((AABBoxCollider) coll, origin, dir, layers);
            // TODO: Write for other collider types
            else
                return CollisionPoints.noCollision;
        }

        public static CollisionPoints Raycast(AABBoxCollider coll, fp3 origin, fp3 dir, long layers){
            if(!coll.InLayers(layers)){
                return CollisionPoints.noCollision;
            }

            // We'll return this huge number if no intersection
            fp NoIntersection = fp.max_value;
            fp3 collNormal = fp3.zero;

            // Check for point inside box, trivial reject, and determine parametric distance to each front face
            bool inside = true;

            fp xt, xn = 0;
            if(origin.x < coll.MinValue.x){
                xt = coll.MinValue.x - origin.x;
                if(xt > dir.x) return CollisionPoints.noCollision;
                xt /= dir.x;
                inside = false;
                xn = -1;
            }
            else if(origin.x > coll.MaxValue.x){
                xt = coll.MaxValue.x - origin.x;
                if(xt < dir.x) return CollisionPoints.noCollision;
                xt /= dir.x;
                inside = false;
                xn = 1;
            }
            else{
                xt = -1;
            }

            fp yt, yn = 0;
            if(origin.y < coll.MinValue.y){
                yt = coll.MinValue.y - origin.y;
                if(yt > dir.y) return CollisionPoints.noCollision;
                yt /= dir.y;
                inside = false;
                yn = -1;
            }
            else if(origin.y > coll.MaxValue.y){
                yt = coll.MaxValue.y - origin.y;
                if(yt < dir.y) return CollisionPoints.noCollision;
                yt /= dir.y;
                inside = false;
                yn = 1;
            }
            else{
                yt = -1;
            }

            fp zt, zn = 0;
            if(origin.z < coll.MinValue.z){
                zt = coll.MinValue.z - origin.z;
                if(zt > dir.z) return CollisionPoints.noCollision;
                zt /= dir.z;
                inside = false;
                zn = -1;
            }
            else if(origin.z > coll.MaxValue.z){
                zt = coll.MaxValue.z - origin.z;
                if(zt < dir.z) return CollisionPoints.noCollision;
                zt /= dir.z;
                inside = false;
                zn = 1;
            }
            else{
                zt = -1;
            }

            // inside box?
            if(inside){
                collNormal = -dir.normalized();
                fp3 B = coll.Center();
                fp3 AtoB = B - origin;
                return new CollisionPoints{
                    A = origin,
                    B = coll.Center(),
                    Normal = collNormal,
                    DepthSqrd = 0,
                    HasCollision = true
                };
            }

            // select farthest plan - this is the plane of intersection
            int which = 0;
            fp t = xt;
            if (yt > t){
                which = 1;
                t= yt;
            }
            if (zt > t){
                which = 2;
                t= zt;
            }

            switch(which){
                case 0: // intersect with yz plane
                {
                    fp y = origin.y + dir.y*t;
                    if(y < coll.MinValue.y || y > coll.MaxValue.y) return CollisionPoints.noCollision;
                    fp z = origin.z + dir.z*t;
                    if(z < coll.MinValue.z || z > coll.MaxValue.z) return CollisionPoints.noCollision;

                    collNormal = new fp3(xn, 0, 0);
                }   break;

                case 1: // intersect with xz plane
                {
                    fp x = origin.x + dir.x*t;
                    if(x < coll.MinValue.x || x > coll.MaxValue.x) return CollisionPoints.noCollision;
                    fp z = origin.z + dir.z*t;
                    if(z < coll.MinValue.z || z > coll.MaxValue.z) return CollisionPoints.noCollision;

                    collNormal = new fp3(0, yn, 0);
                    break;
                }

                case 2: // intersect with xy plane
                {
                    fp x = origin.x + dir.x*t;
                    if(x < coll.MinValue.x || x > coll.MaxValue.x) return CollisionPoints.noCollision;
                    fp y = origin.y + dir.y*t;
                    if(y < coll.MinValue.y || y > coll.MaxValue.y) return CollisionPoints.noCollision;

                    collNormal = new fp3(0, 0, zn);
                    break;
                }
            }

            return new CollisionPoints{
                A = origin,
                B = coll.Center(),
                Normal = collNormal,
                DepthSqrd = 0,
                HasCollision = true
            };
        }
    }
}
/*
        // Swaps

        void SwapPoints(
            ManifoldPoints& points)
        {
            iw::fp3 T = points.A;
            points.A = points.B;
            points.B = T;

            points.Normal = -points.Normal;
        }

        ManifoldPoints FindPlaneSphereMaifoldPoints(
            PlaneCollider*  a, PhysTransform* ta, 
            SphereCollider* b, PhysTransform* tb)
        {
            ManifoldPoints points = FindSpherePlaneMaifoldPoints(b, tb, a, ta);
            SwapPoints(points);

            return points;
        }

        ManifoldPoints FindCapsuleSphereMaifoldPoints(
            CapsuleCollider* a, PhysTransform* ta,
            SphereCollider*  b, PhysTransform* tb)
        {
            ManifoldPoints points = FindSphereCapsuleMaifoldPoints(b, tb, a, ta);
            SwapPoints(points);

            return points;
        }
    }
}
*/