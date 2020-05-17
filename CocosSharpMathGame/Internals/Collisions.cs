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

    /// <summary>
    /// WARNING: Collisions with polygons are not solved exactly. A exact solution would make it necessary to calculate m*n line intersections.
    /// Instead we calculate for m points whether the point is inside the other polygon.
    /// Still, polygon collisions are very costly.
    /// </summary>
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

    internal class CollisionTypeLine : CollisionType
    {
        internal CCPoint StartPoint, EndPoint;
        internal CollisionTypeLine(CCPoint startPoint, CCPoint endPoint)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
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
                else if (cType2 is CollisionTypeLine)
                    return CollidePositionLine(collidible1, cType2 as CollisionTypeLine);
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
                else if (cType2 is CollisionTypeLine)
                    return CollideBoundingBoxLine(collidible1, cType2 as CollisionTypeLine);
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
                else if (cType2 is CollisionTypeLine)
                    return CollideCircleLine(collidible1, (CollisionTypeCircle)cType1, (CollisionTypeLine)cType2);
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
                else if (cType2 is CollisionTypeLine)
                    return CollidePolygonLine(collidible1, (CollisionTypePolygon)cType1, (CollisionTypeLine)cType2);
                else if (cType2 is CollisionTypePolygon)
                    return CollidePolygonPolygon(collidible1, cType1 as CollisionTypePolygon, collidible2, cType2 as CollisionTypePolygon);
                else if (cType2 is CollisionTypeBoundingBox)
                    return CollideBoundingBoxPolygon(collidible2, collidible1, cType1 as CollisionTypePolygon);
                else if (cType2 is CollisionTypeCircle)
                    return CollideCirclePolygon(collidible2, cType2 as CollisionTypeCircle, collidible1, cType1 as CollisionTypePolygon);
            }
            else if (cType1 is CollisionTypeLine)
            {
                // check cType2
                if (cType2 is CollisionTypePosition)
                    return CollidePositionLine(collidible2, (CollisionTypeLine)cType1);
                else if (cType2 is CollisionTypeLine)
                    return CollideLineLine((CollisionTypeLine)cType1, (CollisionTypeLine)cType2);
                else if (cType2 is CollisionTypePolygon)
                    return CollidePolygonLine(collidible2, (CollisionTypePolygon)cType2, (CollisionTypeLine)cType1);
                else if (cType2 is CollisionTypeBoundingBox)
                    return CollideBoundingBoxLine(collidible2, (CollisionTypeLine)cType1);
                else if (cType2 is CollisionTypeCircle)
                    return CollideCircleLine(collidible2, (CollisionTypeCircle)cType2, (CollisionTypeLine)cType1);
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

        internal static bool CollidePositionLine(ICollidible posCollidible, CollisionTypeLine cTypeLine)
        {
            CCPoint pos = ((CCNode)posCollidible).PositionWorldspace;
            return pos.Equals(cTypeLine.StartPoint) || pos.Equals(cTypeLine.EndPoint);
        }

        internal static bool CollideBoundingBoxBoundingBox(ICollidible boxCollidible1, ICollidible boxCollidible2)
        {
            return ((CCNode)boxCollidible1).BoundingBoxTransformedToWorld.IntersectsRect(((CCNode)boxCollidible2).BoundingBoxTransformedToWorld);
        }

        internal static bool CollideBoundingBoxCircle(ICollidible boxCollidible, ICollidible circleCollidible, CollisionTypeCircle cTypeCircle)
        {
            return CollideBoundingBoxCircle(((CCNode)boxCollidible).BoundingBoxTransformedToWorld, ((CCNode)circleCollidible).PositionWorldspace, cTypeCircle.radius);
        }

        internal static bool CollideBoundingBoxCircle(CCRect box, CCPoint circlePos, float radius)
        {
            // for peformance first approximate the box with a circle and check whether these two collide
            // if they don't then the circle can't collide with the box either
            float boxRadius = box.Size.Width > box.Size.Height ? box.Size.Width/2 : box.Size.Height/2;
            if (!CollideCircleCircle(circlePos, radius, box.Center, boxRadius))
                return false;
            // check whether the circle center is inside the bounding box
            if (box.ContainsPoint(circlePos)) return true;
            // check if the circle collides with the lines of the box
            var boxPoints = Constants.CCRectPoints(box);
            int i, j;
            for (i = 0, j = -1; i < 3; j = i++)
                if (CollideCircleLine(circlePos, radius, boxPoints[i], boxPoints[j]))
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

        internal static bool CollideBoundingBoxLine(ICollidible boxCollidible, CollisionTypeLine cTypeLine)
        {
            CCRect box = ((CCNode)boxCollidible).BoundingBoxTransformedToWorld;
            // check whether the start or end point is contained in the box
            if (box.ContainsPoint(cTypeLine.StartPoint) || box.ContainsPoint(cTypeLine.EndPoint))
                return true;
            // check for intersections of the line and the box boundaries
            CCPoint[] boxPoints = Constants.CCRectPoints(box);
            for (int i = 0; i < 3; i++)
                if (CCPoint.SegmentIntersect(cTypeLine.StartPoint, cTypeLine.EndPoint, boxPoints[i], boxPoints[i + 1]))
                    return true;
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

        internal static bool CollideCircleCircle(CCPoint circlePos1, float radius1, CCPoint circlePos2, float radius2)
        {
            return circlePos1.IsNear(circlePos2, radius1 + radius2);
        }

        internal static bool CollideCircleLine(ICollidible circleCollidible1, CollisionTypeCircle cTypeCircle1, CollisionTypeLine cTypeLine)
        {
            // calculate the length of the perpendicular line from the line to the center of the circle
            CCPoint vectorPerpToLine = CCPoint.PerpendicularCCW((cTypeLine.EndPoint - cTypeLine.StartPoint));
            CCPoint vectorLineStartToCircle = ((CCNode)circleCollidible1).PositionWorldspace - cTypeLine.StartPoint;
            float perpLength = (float)Math.Abs( CCPoint.Dot(vectorPerpToLine, vectorLineStartToCircle) / vectorPerpToLine.Length );
            return perpLength <= cTypeCircle1.radius;
        }

        internal static bool CollideCircleLine(CCPoint circlePos, float radius, CCPoint LineStart, CCPoint LineEnd)
        {
            // calculate the length of the perpendicular line from the line to the center of the circle
            CCPoint vectorPerpToLine = CCPoint.PerpendicularCCW((LineEnd - LineStart));
            CCPoint vectorLineStartToCircle = circlePos - LineStart;
            float perpLength = (float)Math.Abs(CCPoint.Dot(vectorPerpToLine, vectorLineStartToCircle) / vectorPerpToLine.Length);
            return perpLength <= radius;
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

        internal static bool CollidePolygonLine(ICollidible polyCollidible, CollisionTypePolygon cTypePoly, CollisionTypeLine cTypeLine)
        {
            // for performance reasons first check the bounding box
            if (CollideBoundingBoxLine(polyCollidible, cTypeLine))
            {
                // transform the polygon to match the positioning, rotation and scale of the node
                Polygon transformedPolygon = ((Polygon)cTypePoly.collisionPolygon.Clone());
                transformedPolygon.TransformAccordingToGameObject(polyCollidible);
                // first check if the polygon contains some of the two line points
                if (transformedPolygon.ContainsPoint(cTypeLine.StartPoint) || transformedPolygon.ContainsPoint(cTypeLine.EndPoint))
                    return true;
                // solve exactly: check for line intersections
                var polyPoints = transformedPolygon.Points;
                int i, j;
                for (i = 0, j = polyPoints.Length - 1; i < polyPoints.Length; j = i++)
                    if (CCPoint.SegmentIntersect(cTypeLine.StartPoint, cTypeLine.EndPoint, polyPoints[i], polyPoints[j]))
                        return true;
            }
            return false;
        }

        internal static bool CollideLineLine(CollisionTypeLine cTypeLine1, CollisionTypeLine cTypeLine2)
        {
            return CCPoint.SegmentIntersect(cTypeLine1.StartPoint, cTypeLine1.EndPoint, cTypeLine2.StartPoint, cTypeLine2.EndPoint);
        }
    }
}
