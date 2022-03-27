using System;
using System.Collections.Generic;
using Unity.Mathematics.FixedPoint;
using SepM.Utils;

namespace SepM.Physics{
    public interface Solver{
        public abstract void Solve(List<PhysCollision> collisions, fp deltaTime);
    }

    public class SmoothPositionSolver : Solver{
        public void Solve(List<PhysCollision> collisions, fp deltaTime){
            List<Tuple<fp3, fp3>> deltas = new List<Tuple<fp3, fp3>>();

            foreach (PhysCollision collision in collisions) {
                PhysObject aBody = collision.ObjA.IsDynamic ? collision.ObjA : null;
                PhysObject bBody = collision.ObjB.IsDynamic ? collision.ObjB : null;

                // TODO: Handle when they're both 0
                fp aInvMass = !(aBody is null) ? aBody.InverseMass : 0;
                fp bInvMass = !(bBody is null) ? bBody.InverseMass : 0;

                fp percent = 0.8m;
                fp slop = 0.01m;

                fp3 correction = collision.Points.Normal * percent
                    * Utilities.max(collision.Points.DepthSqrd - slop, 0)
                    / (aInvMass + bInvMass);
            
                fp3 deltaA = fp3.zero;
                fp3 deltaB = fp3.zero;

                if (!(aBody is null) ? aBody.IsKinematic : false) {
                    deltaA = aInvMass * correction;
                }

                if (!(bBody is null) ? bBody.IsKinematic : false) {
                    deltaB = bInvMass * correction;
                }

                deltas.Add(new Tuple<fp3, fp3>(deltaA, deltaB));
            }

            for (int i = 0; i < collisions.Count; i++) {
                PhysObject aBody = collisions[i].ObjA.IsDynamic ? collisions[i].ObjA : null;
                PhysObject bBody = collisions[i].ObjB.IsDynamic ? collisions[i].ObjB : null;

                if (!(aBody is null) ? aBody.IsKinematic : false) {
                    aBody.Transform.Position -= deltas[i].Item1;
                }

                if (!(bBody is null) ? bBody.IsKinematic : false) {
                    bBody.Transform.Position += deltas[i].Item2;
                }
            }
        }
    }

    public class ImpulseSolver : Solver{
        public void Solve(List<PhysCollision> collisions, fp deltaTime){
            foreach (PhysCollision collision in collisions) {
                // Replaces non dynamic objects with default values.

                PhysObject aBody = collision.ObjA.IsDynamic ? collision.ObjA : null;
                PhysObject bBody = collision.ObjB.IsDynamic ? collision.ObjB : null;

                fp3 aVel = !(aBody is null) ? aBody.Velocity : fp3.zero;
                fp3 bVel = !(bBody is null) ? bBody.Velocity : fp3.zero;
                fp3 rVel = bVel - aVel;
                
                fp nSpd = rVel.dot(collision.Points.Normal);

                fp aInvMass = !(aBody is null) ? aBody.InverseMass : 1;
                fp bInvMass = !(bBody is null) ? bBody.InverseMass : 1;

                // Impluse

                // This is important for convergence
                // a negitive impulse would drive the objects closer together
                if (nSpd >= 0)
                    continue;

                fp e = (!(aBody is null) ? aBody.Restitution : 1)
                        * (!(bBody is null) ? bBody.Restitution : 1);

                fp j = -(1 + e) * nSpd / (aInvMass + bInvMass);

                fp3 impluse = j * collision.Points.Normal;

                if (!(aBody is null) ? aBody.IsKinematic : false) {
                    aVel -= impluse * aInvMass;
                }

                if (!(bBody is null) ? bBody.IsKinematic : false) {
                    bVel += impluse * bInvMass;
                }

                // Friction

                rVel = bVel - aVel;
                nSpd = rVel.dot(collision.Points.Normal);

                fp3 tangent = rVel - nSpd * collision.Points.Normal;

                if (tangent.lengthSqrd() > 0.0001m) { // safe normalize
                    tangent = tangent.normalized();
                }

                fp fVel = rVel.dot(tangent);

                fp aSF = !(aBody is null) ? aBody.StaticFriction  : 0;
                fp bSF = !(bBody is null) ? bBody.StaticFriction  : 0;
                fp aDF = !(aBody is null) ? aBody.DynamicFriction : 0;
                fp bDF = !(bBody is null) ? bBody.DynamicFriction : 0;
                fp mu  = new fp2(aSF, bSF).lengthSqrd().sqrt();

                fp f  = -fVel / (aInvMass + bInvMass);

                fp3 friction;
                if (System.Math.Abs(f) < j * mu) {
                    friction = f * tangent;
                }

                else {
                    mu = new fp2(aDF, bDF).lengthSqrd().sqrt();
                    friction = -j * tangent * mu;
                }

                if (!(aBody is null) ? aBody.IsKinematic : false) {
                    aBody.Velocity = aVel - friction * aInvMass;
                }

                if (!(bBody is null) ? bBody.IsKinematic : false) {
                    bBody.Velocity = bVel + friction * bInvMass;
                }
            }
        }
    }
}