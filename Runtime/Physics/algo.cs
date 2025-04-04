using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics.FixedPoint;
using SepM.Utils;

namespace SepM.Physics{
    public static class algo {
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

            return new CollisionPoints {
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
            PlaneCollider b, PhysTransform tb)
        {
            fp3 A = a.Center + ta.WorldPosition();
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

            return new CollisionPoints {
                A = A,
                B = B,
                Normal = AtoB.normalized(),
                DepthSqrd = AtoB.lengthSqrd(),
                HasCollision = true
            };
        }

        public static CollisionPoints FindSphereCapsuleCollisionPoints(
            SphereCollider a, PhysTransform ta,
            CapsuleCollider b, PhysTransform tb)
        {
            fp Bhs = 1.0m;
            fp Brs = 1.0m;

            fp3 s = tb.WorldScale();
            // TODO: Will need to verify this condition
            // Right
            if (b.Direction.Equals(new fp3(1, 0, 0))) {
                Bhs = s.x;
                Brs = new fp2(s.y, s.z).major();
            }
            // Up
            else if (b.Direction.Equals(new fp3(0, 1, 0))) {
                Bhs = s.y;
                Brs = new fp2(s.x, s.z).major();
            }
            // Forward
            else if (b.Direction.Equals(new fp3(0, 0, 1))) {
                Bhs = s.z;
                Brs = new fp2(s.x, s.y).major();
            }

            fp3 offset = b.Direction.multiply(tb.WorldRotation()) * (b.Height * Bhs / 2 - b.Radius * Brs);

            fp3 A = a.Center + ta.WorldPosition();
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

            return new CollisionPoints() {
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
            if (d2 < d0 || d2 < d1 || d3 < d0 || d3 < d1) {
                bestA = aStats.B;
            }
            else {
                bestA = aStats.A;
            }

            // select point on capsule B line segment nearest to best potential endpoint on A capsule:
            fp3 bestB = Utilities.closestPointOnLineSegment(bStats.A, bStats.B, bestA);

            // now do the same for capsule A segment:
            bestA = Utilities.closestPointOnLineSegment(aStats.A, aStats.B, bestB);

            bool intersects = false;
            fp3 penetration_normal = bestA - bestB;
            fp len = 0;

            if (!penetration_normal.Equals(fp3.zero)) {
                len = penetration_normal.lengthSqrd().sqrt();
                if (len == 0)
                    penetration_normal = fp3.zero;
                else
                    penetration_normal /= len;  // normalize
            }
            fp penetration_depth = a.Radius + b.Radius - len;
            intersects = penetration_depth >= 0;

            if (intersects) {
                return new CollisionPoints() {
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
                a.Center, new fp3(a.Radius * 2, a.Height * 2, a.Radius * 2), true);
            return FindAABBoxAABBoxCollisionPoints(
                tempBox,
                ta,
                b,
                tb
            );
        }

        public static CollisionPoints FindCapsuleAABBsCollisionPoints(
            CapsuleCollider a, PhysTransform ta,
            AABBoxCollider b, PhysTransform tb)
        {
            // TODO: Decide is we really need to do a capsule or can just compare AABBs
            AABBoxCollider tempBox = new AABBoxCollider(a.Center, new fp3(a.Radius * 2, a.Height, a.Radius * 2), true);
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

                fp3 B = p - normal * a.Radius;
                fp3 AtoB = B - A;

                if (AtoB.Equals(fp3.zero)) {
                    // Sphere is inside AABB
                    AtoB = new fp3(0, 1, 0);
                }

                fp dsqrd = AtoB.lengthSqrd();

                return new CollisionPoints() {
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

            bool overlap = AABBTest(boxAPos, boxBPos, boxASize, boxBSize);
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

                for (int i = 0; i < 6; i++) {
                    if (distances[i] < penetration) {
                        penetration = distances[i];
                        bestAxis = faces[i];
                    }
                }
                return new CollisionPoints {
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
        static bool AABBTest(fp3 posA, fp3 posB, fp3 halfSizeA, fp3 halfSizeB) {
            fp3 delta = posB - posA;
            fp3 totalSize = halfSizeA + halfSizeB;

            if (System.Math.Abs(delta.x) <= totalSize.x
                && System.Math.Abs(delta.y) <= totalSize.y
                && System.Math.Abs(delta.z) <= totalSize.z) {
                return true;
            }

            return false;
        }

        public static CollisionPoints Capsulecast(Collider coll, fp3 center, fp radius,  fp height, fp3 dir, long layers, PhysTransform physTransform = null) {
            if (!coll.InLayers(layers))
            {
                return CollisionPoints.noCollision;
            }

            CapsuleCollider capsule = new CapsuleCollider(center, radius, height, dir);
            return coll.TestCollision(physTransform, capsule, null);
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

        public static fp3 inverseTransformPoint(this PhysTransform ta, fp3 point) {
            fp3 result = Utilities.scale(
                new fp3(1 / ta.Scale.x, 1 / ta.Scale.y, 1 / ta.Scale.z),
                (point - ta.Position).multiply(ta.Rotation.conjugate())
                );
            return result;
        }

        public static fp SqDistPointAABB(this AABBoxCollider box, PhysTransform boxTransform, fp3 point) {
            fp result = (box.closestPointAABB(boxTransform, point) - point).lengthSqrd();
            return result;
        }

        public static CollisionPoints Raycast(List<PhysObject> p_objs, fp3 origin, fp3 dir, long layers, PhysTransform physTransform = null) {
            foreach (PhysObject p_obj in p_objs) {
                CollisionPoints raycastResult = Raycast(p_obj, origin, dir, layers);
                if (raycastResult.HasCollision)
                    return raycastResult;
            }
            return CollisionPoints.noCollision;
        }

        public static CollisionPoints Raycast(PhysObject p_obj, fp3 origin, fp3 dir) {
            return Raycast(p_obj, origin, dir, Constants.layer_all);
        }

        public static CollisionPoints Raycast(PhysObject p_obj, fp3 origin, fp3 dir, long layers) {
            if (p_obj.Coll is null)
                return CollisionPoints.noCollision;
            else
                return Raycast(p_obj.Coll, origin, dir, layers, p_obj.Transform);
        }

        public static List<Tuple<PhysObject, CollisionPoints>> RaycastAll(PhysObject physObj, fp3 origin, fp3 dir, long layers)
        {
            return RaycastAll(new List<PhysObject>{ physObj }, origin, dir, layers);
        }

        public static List<Tuple<PhysObject, CollisionPoints>> RaycastAll(List<PhysObject> physObjects, fp3 origin, fp3 dir, long layers)
        {
            List<Tuple<PhysObject, CollisionPoints>> result = new List<Tuple<PhysObject, CollisionPoints>>();
            foreach (PhysObject physObject in physObjects)
            {
                var a = Raycast(physObject, origin, dir, layers);
                if (a.HasCollision)
                    result.Add(new Tuple<PhysObject, CollisionPoints>(physObject, a));
            }
            return result;
        }

        public static CollisionPoints Raycast(Collider coll, fp3 origin, fp3 dir, PhysTransform physTransform = null) {
            return Raycast(coll, origin, dir, Constants.layer_all, physTransform);
        }

        public static CollisionPoints Raycast(Collider coll, fp3 origin, fp3 dir, long layers, PhysTransform physTransform = null) {
            if (coll is AABBoxCollider)
                return RaycastAABBox((AABBoxCollider)coll, physTransform, origin, dir, layers);
            else if (coll is CapsuleCollider)
                return RaycastCapsule((CapsuleCollider)coll, physTransform, origin, dir, layers);
            // TODO: Write for other collider types
            else
            {
                UnityEngine.Debug.LogWarning($"{coll.GetType().FullName} not impemented for Raycast.");
                return CollisionPoints.noCollision;
            }
        }

        private static CollisionPoints RaycastAABBox(AABBoxCollider coll, PhysTransform physTransform, fp3 origin, fp3 dir, long layers) {
            if (!coll.InLayers(layers)) {
                return CollisionPoints.noCollision;
            }

            // We'll return this huge number if no intersection
            fp NoIntersection = fp.max_value;
            fp3 collNormal = fp3.zero;

            // Check for point inside box, trivial reject, and determine parametric distance to each front face
            bool inside = true;

            fp3 minValue = coll.MinValue + (physTransform == null ? fp3.zero : physTransform.WorldPosition());
            fp3 maxValue = coll.MaxValue + (physTransform == null ? fp3.zero : physTransform.WorldPosition());

            fp xt, xn = 0;
            if (origin.x < minValue.x) {
                xt = minValue.x - origin.x;
                if (xt > dir.x) return CollisionPoints.noCollision;
                xt /= dir.x;
                inside = false;
                xn = -1;
            }
            else if (origin.x > maxValue.x) {
                xt = maxValue.x - origin.x;
                if (xt < dir.x) return CollisionPoints.noCollision;
                xt /= dir.x;
                inside = false;
                xn = 1;
            }
            else {
                xt = -1;
            }

            fp yt, yn = 0;
            if (origin.y < minValue.y) {
                yt = minValue.y - origin.y;
                if (yt > dir.y) return CollisionPoints.noCollision;
                yt /= dir.y;
                inside = false;
                yn = -1;
            }
            else if (origin.y > maxValue.y) {
                yt = maxValue.y - origin.y;
                if (yt < dir.y) return CollisionPoints.noCollision;
                yt /= dir.y;
                inside = false;
                yn = 1;
            }
            else {
                yt = -1;
            }

            fp zt, zn = 0;
            if (origin.z < minValue.z) {
                zt = minValue.z - origin.z;
                if (zt > dir.z) return CollisionPoints.noCollision;
                zt /= dir.z;
                inside = false;
                zn = -1;
            }
            else if (origin.z > maxValue.z) {
                zt = maxValue.z - origin.z;
                if (zt < dir.z) return CollisionPoints.noCollision;
                zt /= dir.z;
                inside = false;
                zn = 1;
            }
            else {
                zt = -1;
            }

            // inside box?
            if (inside) {
                collNormal = -dir.normalized();
                fp3 B = coll.Center();
                fp3 AtoB = B - origin;
                return new CollisionPoints {
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
            if (yt > t) {
                which = 1;
                t = yt;
            }
            if (zt > t) {
                which = 2;
                t = zt;
            }

            switch (which) {
                case 0: // intersect with yz plane
                    {
                        fp y = origin.y + dir.y * t;
                        if (y < minValue.y || y > maxValue.y) return CollisionPoints.noCollision;
                        fp z = origin.z + dir.z * t;
                        if (z < minValue.z || z > maxValue.z) return CollisionPoints.noCollision;

                        collNormal = new fp3(xn, 0, 0);
                    } break;

                case 1: // intersect with xz plane
                    {
                        fp x = origin.x + dir.x * t;
                        if (x < minValue.x || x > maxValue.x) return CollisionPoints.noCollision;
                        fp z = origin.z + dir.z * t;
                        if (z < minValue.z || z > maxValue.z) return CollisionPoints.noCollision;

                        collNormal = new fp3(0, yn, 0);
                        break;
                    }

                case 2: // intersect with xy plane
                    {
                        fp x = origin.x + dir.x * t;
                        if (x < minValue.x || x > maxValue.x) return CollisionPoints.noCollision;
                        fp y = origin.y + dir.y * t;
                        if (y < minValue.y || y > maxValue.y) return CollisionPoints.noCollision;

                        collNormal = new fp3(0, 0, zn);
                        break;
                    }
            }

            return new CollisionPoints {
                A = origin,
                B = coll.Center(),
                Normal = collNormal,
                DepthSqrd = 0,
                HasCollision = true
            };
        }

        private static CollisionPoints RaycastCapsule(CapsuleCollider coll, PhysTransform physTransform, fp3 origin, fp3 dir, long layers)
        {
            if (!coll.InLayers(layers))
            {
                return CollisionPoints.noCollision;
            }

            CapsuleCollider ray = new CapsuleCollider(origin + (dir/2), 0, dir.lengthSqrd().sqrt(), dir.normalized());
            return FindCapsuleCapsuleCollisionPoints(coll, physTransform, ray, null);
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