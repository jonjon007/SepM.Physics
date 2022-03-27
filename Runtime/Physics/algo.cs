using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics.FixedPoint;
using SepM.Utils;
using SepM.Physics;

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

        public static CollisionPoints FindSphereAABBCollisionPoints(
            SphereCollider a, PhysTransform ta,
            AABBoxCollider b, PhysTransform tb)
        {
            fp dist;

            fp3 A = a.Center + ta.WorldPosition();

            // Find point (p) on AABB closest to Sphere center
            fp3 p = b.closestPointAABB(A);

            // Sphere and AABB intersect if the (squared) distance from sphere center to point (p)
            // is less than the (squared) sphere radius
            fp3 v = p - A;

            if (v.dot(v) <= a.Radius * a.Radius)
            {
                dist = b.SqDistPointAABB(A);

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

        public static fp3 closestPointAABB(this AABBoxCollider box, fp3 point) // P131
        {
            // For each coordinate axis, if the point coordinate value is outside box,
            // clamp it to the box, else keep it as is
            fp3 min = box.MinValue;
            fp3 max = box.MaxValue;
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

        public static fp SqDistPointAABB(this AABBoxCollider box, fp3 point){
            fp result = (box.closestPointAABB(point) - point).lengthSqrd();
            return result;
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