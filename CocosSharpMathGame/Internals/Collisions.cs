using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    internal abstract class CollisionType
    { }

    internal class CollisionTypePosition : CollisionType
    { }

    internal class CollisionTypeBoundingBox : CollisionType
    { }

    internal class CollisionTypeCircle : CollisionType
    {
        internal float radius;
        internal CollisionTypeCircle(float radius)
        {
            this.radius = radius;
        }
    }

    internal class CollisionTypePolygon : CollisionType
    {
        /// <summary>
        /// The untransformed (unmoved, unrotated, unscaled) bounding polygon
        /// </summary>
        internal Polygon collisionPolygon;
        internal CollisionTypePolygon(Polygon collisionPolygon)
        {
            this.collisionPolygon = collisionPolygon;
        }
    }

    internal static class Collisions
    {
        internal static bool Collide(ICollidible collidible1, ICollidible collidible2)
        {
            // first get the collision data from the two collidables
            CollisionType cType1 = collidible1.CollisionType;
            CollisionType cType2 = collidible2.CollisionType;
            // then call the appropriate funtion to compute the collision
            if (cType1 is CollisionTypePosition)
            {
                // check cType2
                if (cType2 is CollisionTypePosition)
                    return CollidePositionPosition(collidible1, collidible2);
                else if (cType2 is CollisionTypePolygon)
                    return CollidePositionPolygon(collidible1, collidible2, cType2 as CollisionTypePolygon);
                else if (cType2 is CollisionTypeBoundingBox)
                    return CollidePositionBoundingBox(collidible1, collidible2);
                else if (cType2 is CollisionTypeCircle)
                    return CollidePositionCircle(collidible1, collidible2, cType2 as CollisionTypeCircle);
            }
            else if (cType1 is CollisionTypeBoundingBox)
            {
                // check cType2
                if (cType2 is CollisionTypePosition)
                    return CollidePositionBoundingBox(collidible2, collidible1);
                else if (cType2 is CollisionTypePolygon)
                    return CollideBoundingBoxPolygon(collidible1, collidible2, cType2 as CollisionTypePolygon);
                else if (cType2 is CollisionTypeBoundingBox)
                    return CollideBoundingBoxBoundingBox(collidible1, collidible2);
                else if (cType2 is CollisionTypeCircle)
                    return CollideBoundingBoxCircle(collidible1, collidible2, cType2 as CollisionTypeCircle);
            }
            else if (cType1 is CollisionTypeCircle)
            {
                // check cType2
                if (cType2 is CollisionTypePosition)
                    return CollidePositionCircle(collidible2, collidible1, cType1 as CollisionTypeCircle);
                else if (cType2 is CollisionTypePolygon)
                    return CollideCirclePolygon(collidible1, cType1 as CollisionTypeCircle, collidible2, cType2 as CollisionTypePolygon);
                else if (cType2 is CollisionTypeBoundingBox)
                    return CollideBoundingBoxCircle(collidible2, collidible1, cType1 as CollisionTypeCircle);
                else if (cType2 is CollisionTypeCircle)
                    return CollideCircleCircle(collidible1, cType1 as CollisionTypeCircle, collidible2, cType2 as CollisionTypeCircle);
            }
            else if (cType1 is CollisionTypePolygon)
            {
                // check cType2
                if (cType2 is CollisionTypePosition)
                    return CollidePositionPolygon(collidible2, collidible1, cType1 as CollisionTypePolygon);
                else if (cType2 is CollisionTypePolygon)
                    return CollidePolygonPolygon(collidible1, cType1 as CollisionTypePolygon, collidible2, cType2 as CollisionTypePolygon);
                else if (cType2 is CollisionTypeBoundingBox)
                    return CollideBoundingBoxPolygon(collidible2, collidible1, cType1 as CollisionTypePolygon);
                else if (cType2 is CollisionTypeCircle)
                    return CollideCirclePolygon(collidible2, cType2 as CollisionTypeCircle, collidible1, cType1 as CollisionTypePolygon);
            }
            return false;
        }

        internal static bool CollidePositionPosition(ICollidible collidible1, ICollidible collidible2)
        {
            return ((CCNode)collidible1).PositionWorldspace.Equals(((CCNode)collidible2).PositionWorldspace);
        }

        internal static bool CollidePositionBoundingBox(ICollidible posCollidible, ICollidible boxCollidible)
        {
            return ((CCNode)boxCollidible).BoundingBoxTransformedToWorld.ContainsPoint(((CCNode)posCollidible).PositionWorldspace);
        }

        internal static bool CollidePositionCircle(ICollidible posCollidible, ICollidible circleCollidible, CollisionTypeCircle cTypeCircle)
        {
            return ((CCNode)circleCollidible).PositionWorldspace.IsNear(((CCNode)posCollidible).PositionWorldspace, cTypeCircle.radius);
        }

        internal static bool CollidePositionPolygon(ICollidible posCollidible, ICollidible polyCollidible, CollisionTypePolygon cTypePoly)
        {
            // first check for bounding box collision (for efficience)
            if (CollidePositionBoundingBox(posCollidible,polyCollidible))
            {
                // transform the polygon to match the positioning, rotation and scale of the node
                Polygon transformedPolygon = ((Polygon)cTypePoly.collisionPolygon.Clone());
                transformedPolygon.TransformAccordingToGameObject(polyCollidible);
                return transformedPolygon.ContainsPoint(((CCNode)posCollidible).PositionWorldspace);
            }
            return false;
        }

        internal static bool CollideBoundingBoxBoundingBox(ICollidible boxCollidible1, ICollidible boxCollidible2)
        {
            return ((CCNode)boxCollidible1).BoundingBoxTransformedToWorld.IntersectsRect(((CCNode)boxCollidible2).BoundingBoxTransformedToWorld);
        }

        internal static bool CollideBoundingBoxCircle(ICollidible boxCollidible, ICollidible circleCollidible, CollisionTypeCircle cTypeCircle)
        {
            CCRect box = ((CCNode)boxCollidible).BoundingBoxTransformedToWorld;
            float radius = cTypeCircle.radius;
            CCPoint pos = ((CCNode)circleCollidible).PositionWorldspace;
            CCPoint[] boxPoints = Constants.CCRectPoints(box);
            foreach (var point in boxPoints)
                if (point.IsNear(pos, radius))
                    return true;
            return false;
        }

        internal static bool CollideBoundingBoxPolygon(ICollidible boxCollidible, ICollidible polyCollidible, CollisionTypePolygon cTypePoly)
        {
            // first check the bounding box of the polygon (for performance)
            if (((CCNode)boxCollidible).BoundingBoxTransformedToWorld.IntersectsRect(((CCNode)polyCollidible).BoundingBoxTransformedToWorld))
            {
                CCPoint[] boxPoints = Constants.CCRectPoints(((CCNode)boxCollidible).BoundingBoxTransformedToWorld);
                // transform the polygon to match the positioning, rotation and scale of the node
                Polygon transformedPolygon = ((Polygon)cTypePoly.collisionPolygon.Clone());
                transformedPolygon.TransformAccordingToGameObject(polyCollidible);
                foreach (var point in boxPoints)
                    if (transformedPolygon.ContainsPoint(point))
                        return true;
            }
            return false;
        }

        internal static bool CollideCirclePolygon(ICollidible circleCollidible, CollisionTypeCircle cTypeCircle, ICollidible polyCollidible, CollisionTypePolygon cTypePoly)
        {
            float radius = cTypeCircle.radius;
            CCPoint pos = ((CCNode)circleCollidible).PositionWorldspace;
            // transform the polygon to match the positioning, rotation and scale of the node
            Polygon transformedPolygon = ((Polygon)cTypePoly.collisionPolygon.Clone());
            transformedPolygon.TransformAccordingToGameObject(polyCollidible);
            // for each point of the polygon check whether its contained in the circle
            foreach (var point in transformedPolygon.Points)
                if (point.IsNear(pos, radius))
                    return true;
            return false;
        }

        internal static bool CollideCircleCircle(ICollidible circleCollidible1, CollisionTypeCircle cTypeCircle1, ICollidible circleCollidible2, CollisionTypeCircle cTypeCircle2)
        {
            CCPoint pos1 = ((CCNode)circleCollidible1).PositionWorldspace;
            CCPoint pos2 = ((CCNode)circleCollidible2).PositionWorldspace;
            float radius1 = cTypeCircle1.radius;
            float radius2 = cTypeCircle2.radius;
            return pos1.IsNear(pos2, radius1 + radius2);
        }

        internal static bool CollidePolygonPolygon(ICollidible polyCollidible1, CollisionTypePolygon cTypePoly1, ICollidible polyCollidible2, CollisionTypePolygon cTypePoly2)
        {
            // first check the bounding boxes of the polygons (for performance)
            if (((CCNode)polyCollidible1).BoundingBoxTransformedToWorld.IntersectsRect(((CCNode)polyCollidible2).BoundingBoxTransformedToWorld))
            {
                // transform the polygon to match the positioning, rotation and scale of the node
                Polygon transformedPolygon1 = ((Polygon)cTypePoly1.collisionPolygon.Clone());
                transformedPolygon1.TransformAccordingToGameObject(polyCollidible1);
                Polygon transformedPolygon2 = ((Polygon)cTypePoly2.collisionPolygon.Clone());
                transformedPolygon2.TransformAccordingToGameObject(polyCollidible2);
                foreach (var point in transformedPolygon1.Points)
                    if (transformedPolygon2.ContainsPoint(point))
                        return true;
            }
            return false;
        }
    }
}
