using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;
using MathNet.Symbolics;

namespace CocosSharpMathGame
{
    internal abstract class CollisionType
    { }

    internal class CollisionTypePosition : CollisionType
    { }

    internal class CollisionTypeBoundingBox : CollisionType
    { }

    internal class CollisionTypeDiamond : CollisionType
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
            switch (cType1)
            {
                case CollisionTypePosition ctp1:
                    switch (cType2)
                    {
                        case CollisionTypePosition ctp2:
                            return CollidePositionPosition(collidible1, collidible2);
                        case CollisionTypeLine ctl2:
                                return CollidePositionLine(collidible1, ctl2);
                        case CollisionTypePolygon ctpoly2:
                                return CollidePositionPolygon(collidible1, collidible2, ctpoly2);
                        case CollisionTypeBoundingBox ctb2:
                                return CollidePositionBoundingBox(collidible1, collidible2);
                        case CollisionTypeCircle ctc2:
                                return CollidePositionCircle(collidible1, collidible2, ctc2);
                    }
                    break;
                case CollisionTypeBoundingBox ctb1:
                    switch (cType2)
                    {
                        case CollisionTypePosition ctp2:
                            return CollidePositionBoundingBox(collidible2, collidible1);
                        case CollisionTypeLine ctl2:
                            return CollideBoundingBoxLine(collidible1, ctl2);
                        case CollisionTypePolygon ctpoly2:
                            return CollideBoundingBoxPolygon(collidible1, collidible2, ctpoly2);
                        case CollisionTypeBoundingBox ctb2:
                            return CollideBoundingBoxBoundingBox(collidible1, collidible2);
                        case CollisionTypeCircle ctc2:
                            return CollideBoundingBoxCircle(collidible1, collidible2, ctc2);
                    }
                    break;
                case CollisionTypeCircle ctc1:
                    switch (cType2)
                    {
                        case CollisionTypePosition ctp2:
                            return CollidePositionCircle(collidible2, collidible1, ctc1);
                        case CollisionTypeLine ctl2:
                            return CollideCircleLine(collidible1, ctc1, ctl2);
                        case CollisionTypePolygon ctpoly2:
                            return CollideCirclePolygon(collidible1, ctc1, collidible2, ctpoly2);
                        case CollisionTypeBoundingBox ctb2:
                            return CollideBoundingBoxCircle(collidible2, collidible1, ctc1);
                        case CollisionTypeCircle ctc2:
                            return CollideCircleCircle(collidible1, ctc1, collidible2, ctc2);
                    }
                    break;
                case CollisionTypePolygon ctpoly1:
                    switch (cType2)
                    {
                        case CollisionTypePosition ctp2:
                            return CollidePositionPolygon(collidible2, collidible1, ctpoly1);
                        case CollisionTypeLine ctl2:
                            return CollidePolygonLine(collidible1, ctpoly1, ctl2);
                        case CollisionTypePolygon ctpoly2:
                            return CollidePolygonPolygon(collidible1, ctpoly1, collidible2, ctpoly2);
                        case CollisionTypeBoundingBox ctb2:
                            return CollideBoundingBoxPolygon(collidible2, collidible1, ctpoly1);
                        case CollisionTypeCircle ctc2:
                            return CollideCirclePolygon(collidible2, ctc2, collidible1, ctpoly1);
                    }
                    break;
                case CollisionTypeLine ctl1:
                    switch (cType2)
                    {
                        case CollisionTypePosition ctp2:
                            return CollidePositionLine(collidible2, ctl1);
                        case CollisionTypeLine ctl2:
                            return CollideLineLine(ctl1, ctl2);
                        case CollisionTypePolygon ctpoly2:
                            return CollidePolygonLine(collidible2, ctpoly2, ctl1);
                        case CollisionTypeBoundingBox ctb2:
                            return CollideBoundingBoxLine(collidible2, ctl1);
                        case CollisionTypeCircle ctc2:
                            return CollideCircleLine(collidible2, ctc2, ctl1);
                    }
                    break;
            }
            return false;
        }

        internal static bool CollidePositionPolygon(CCPoint position, ICollidible polyCollidible)
        {
            // transform the polygon to match the positioning, rotation and scale of the node
            Polygon transformedPolygon = ((Polygon)((CollisionTypePolygon)polyCollidible.CollisionType).collisionPolygon.Clone());
            transformedPolygon.TransformAccordingToGameObject(polyCollidible);
            return transformedPolygon.ContainsPoint(position);
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
            for (i = 0, j = 3; i < 4; j = i++)
                if (CollideCircleLine(circlePos, radius, boxPoints[i], boxPoints[j]))
                    return true;
            return false;
        }

        // dirty as it only checks whether one contains a point of the other
        internal static bool CollideBoundingBoxPolygon(ICollidible boxCollidible, ICollidible polyCollidible, CollisionTypePolygon cTypePoly)
        {
            // first check the bounding box of the polygon (for performance)
            CCRect box = ((CCNode)boxCollidible).BoundingBoxTransformedToWorld;
            if (box.IntersectsRect(((CCNode)polyCollidible).BoundingBoxTransformedToWorld))
            {
                CCPoint[] boxPoints = Constants.CCRectPoints(box);
                // transform the polygon to match the positioning, rotation and scale of the node
                Polygon transformedPolygon = ((Polygon)cTypePoly.collisionPolygon.Clone());
                transformedPolygon.TransformAccordingToGameObject(polyCollidible);
                foreach (var point in boxPoints)
                    if (transformedPolygon.ContainsPoint(point))
                        return true;
                foreach (var point in transformedPolygon.Points)
                    if (box.ContainsPoint(point))
                        return true;
            }
            return false;
        }

        internal static bool CollideBoundingBoxLine(ICollidible boxCollidible, CollisionTypeLine cTypeLine)
        {
            CCRect box = ((CCNode)boxCollidible).BoundingBoxTransformedToWorld;
            float sx = cTypeLine.StartPoint.X;
            float sy = cTypeLine.StartPoint.Y;
            float ex = cTypeLine.EndPoint.X;
            float ey = cTypeLine.EndPoint.Y;
            // for performance first check whether both points of the line are outside and on one side of the box
            if (   (sx < box.MinX && ex < box.MinX)
                || (sx > box.MaxX && ex > box.MaxX)
                || (sy < box.MinY && ey < box.MinY)
                || (sy > box.MaxY && ey > box.MaxY))
                return false;
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

        // is dirty, as it only checks whether polygon points are contained in the circle, not whether polygon lines are crossed by it
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

        // dirty
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

        // dirty, because only the center of the bounding box + all points of the box are used to check the angle;
        // this function is using CCDegrees
        internal static bool CollideArcBoundingBox(CCPoint posCircle, float radius, float angleArcCenter, float angleHalfArcWidth, CCRect box)
        {
            if (CollideBoundingBoxCircle(box, posCircle,radius))
            {
                // check whether the center of the box is inside the arc
                CCPoint vectorCirclePoint = box.Center - posCircle;
                float anglePoint      = Constants.DxDyToCCDegrees(vectorCirclePoint.X, vectorCirclePoint.Y);
                float angleDifference = Constants.AbsAngleDifferenceDeg(angleArcCenter, anglePoint);
                if (angleDifference <= angleHalfArcWidth)
                    return true;
                // check whether a point of the box is inside the arc
                foreach (CCPoint point in Constants.CCRectPoints(box))
                {
                    vectorCirclePoint = point - posCircle;
                    anglePoint      = Constants.DxDyToCCDegrees(vectorCirclePoint.X, vectorCirclePoint.Y);
                    angleDifference = Constants.AbsAngleDifferenceDeg(angleArcCenter, anglePoint);
                    if (angleDifference <= angleHalfArcWidth)
                        return true;
                }
            }
            return false;
        }

        internal static bool CollideDiamondPosition(ICollidible collidibleDiamond, ICollidible collidiblePos)
        {
            CCRect diamondBox = ((CCNode)collidibleDiamond).BoundingBoxTransformedToWorld;
            CCPoint center = diamondBox.Center;
            CCPoint pos = ((CCNode)collidiblePos).PositionWorldspace;
            return CollideDiamondPosition(diamondBox, center, pos);
        }

        internal static bool CollideDiamondPosition(CCRect diamondBox, CCPoint center, CCPoint pos)
        {
            float dx = Math.Abs(pos.X - center.X);
            float dy = Math.Abs(pos.Y - center.Y);
            float width = (diamondBox.Size.Width / 2);
            float height = (diamondBox.Size.Height / 2);
            if (dx > width || dy > height) return false;
            float ratioWidth = dx / width;
            float ratioHeight = dy / height;
            return dx <= width - ratioHeight * width && dy <= height - ratioWidth * height;
        }

        // dirty as it only checks whether one contains a point of the other (analog to polygon case)
        internal static bool CollideDiamondBoundingBox(ICollidible collidibleDiamond, ICollidible collidibleBox)
        {
            if (CollideBoundingBoxBoundingBox(collidibleDiamond, collidibleBox))
            {
                var diamondBox = ((CCNode)collidibleDiamond).BoundingBoxTransformedToWorld;
                var diamond = BoxToDiamond(diamondBox);
                var box = ((CCNode)collidibleBox).BoundingBoxTransformedToWorld;
                var boxPoints = Constants.CCRectPoints(box);
                foreach (var point in diamond)
                    if (box.ContainsPoint(point))
                        return true;
                foreach (var point in boxPoints)
                    if (CollideDiamondPosition(diamondBox, diamondBox.Center, point))
                        return true;
            }
            return false;
        }

        // exact
        internal static bool CollideDiamondCircle(ICollidible collidibleDiamond, ICollidible collidibleCircle, CollisionTypeCircle cTypeCircle)
        {
            var circlePos = ((CCNode)collidibleCircle).PositionWorldspace;
            var radius = cTypeCircle.radius;
            // for performance first check whether they are too far to collide anyway
            var diamondBox = ((CCNode)collidibleDiamond).BoundingBoxTransformedToWorld;
            float maxSizeBox = diamondBox.Size.Width > diamondBox.Size.Height ? diamondBox.Size.Width : diamondBox.Size.Height;
            if (   Math.Abs(circlePos.X - diamondBox.Center.X) > radius + diamondBox.Size.Width  / 2
                || Math.Abs(circlePos.Y - diamondBox.Center.Y) > radius + diamondBox.Size.Height / 2)
                return false;
            var diamond = BoxToDiamond(diamondBox);
            int i, j;
            for (i = 0, j = diamond.Length - 1; i < diamond.Length; j = i++)
                if (CollideCircleLine(circlePos, radius, diamond[i], diamond[j]))
                    return true;
            return false;
        }

        /// <summary>
        /// NOT YET IMPLEMENTED, AS I REALISED THAT I PROBABLY WONT USE DIAMONDS ANYWAY...
        /// </summary>
        /// <param name="collidible1"></param>
        /// <param name="collidible2"></param>
        /// <returns></returns>
        internal static bool CollideDiamondDiamond(ICollidible collidible1, ICollidible collidible2)
        {
            throw new NotImplementedException();
        }

        internal static CCPoint[] BoxToDiamond(CCRect box)
        {
            return new CCPoint[] { new CCPoint(box.MinX, box.MinY + (box.Size.Width / 2)), new CCPoint(box.MinX + (box.Size.Width / 2), box.MaxY), new CCPoint(box.MaxX, box.MinY + (box.Size.Height / 2)), new CCPoint(box.MinX + (box.Size.Width / 2), box.MinY) };
        }

        internal static CollisionTypePolygon CreateDiamondCollisionPolygon(IGameObject gameObject)
        {
            var collisionPoly = new Polygon(gameObject.DiamondCollisionPoints());
            collisionPoly.PivotPoint = ((CCNode)gameObject).AnchorPointInPoints;// new CCPoint(ContentSize.Width / 2, ContentSize.Height / 2);
            return new CollisionTypePolygon(collisionPoly);
        }

        /// <summary>
        /// Return a position that is both on the line and in the polygon.
        /// </summary>
        /// <param name="testProjectile"></param>
        /// <param name="part"></param>
        /// <returns></returns>
        internal static CCPoint CollisionPosLinePoly(CollisionTypeLine cTypeLine, ICollidible polyCollidible)
        {
            // for performance reasons first check the bounding box
            if (CollideBoundingBoxLine(polyCollidible, cTypeLine))
            {
                // transform the polygon to match the positioning, rotation and scale of the node
                Polygon transformedPolygon = ((Polygon)((CollisionTypePolygon)polyCollidible.CollisionType).collisionPolygon.Clone());
                transformedPolygon.TransformAccordingToGameObject(polyCollidible);
                // first check if the polygon contains some of the two line points
                if (transformedPolygon.ContainsPoint(cTypeLine.StartPoint))
                    return cTypeLine.StartPoint;
                else if (transformedPolygon.ContainsPoint(cTypeLine.EndPoint))
                    return cTypeLine.EndPoint;
                // solve exactly: check for line intersections
                var polyPoints = transformedPolygon.Points;
                int i, j;
                for (i = 0, j = polyPoints.Length - 1; i < polyPoints.Length; j = i++)
                    if (CCPoint.SegmentIntersect(cTypeLine.StartPoint, cTypeLine.EndPoint, polyPoints[i], polyPoints[j]))
                        return CCPoint.IntersectPoint(cTypeLine.StartPoint, cTypeLine.EndPoint, polyPoints[i], polyPoints[j]);
            }
            return CCPoint.Zero;
        }
    }
}
