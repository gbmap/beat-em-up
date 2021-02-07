using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Catacumba.LevelGen
{
    // 
    //  Organizes prop objects inside a cell based on their bounds
    //  and the provided cell's neighbors.
    //    
    public class PropPlacement
    {
        public class BoundsSize
        {
            public Bounds bounds;
            public EDirectionBitmask direction;
            public int axis;
            public int propsInArea;
        }

        public static List<BoundsSize> OrganizeProps(GameObject cellObject, 
                                                     EDirectionBitmask directions, 
                                                     GameObject[] props, 
                                                     float margin = 0.0175f,
                                                     float areaPercentage = 0.25f)
        {
            Bounds cellBounds = cellObject.GetComponentInChildren<MeshRenderer>().bounds;
            cellBounds.extents *= (1f - margin);
            List<BoundsSize> bounds = CreateBounds(directions, cellBounds, areaPercentage);

            int nProps = props.Length;
            for (int i = 0; i < nProps; i++)
            {
                GameObject obj = props[i];
                if (obj == null) continue;

                MeshRenderer renderer = obj.GetComponentInChildren<MeshRenderer>(); 
                if (!renderer)
                {
                    Debug.LogError($"No renderer in {obj.name}");
                    continue; 
                }

                Bounds objBounds = renderer.bounds;

                var availableBounds = bounds.Where(b => b.bounds.extents[OrthogonalAxis(b.axis)] >= objBounds.extents[b.axis]);
                BoundsSize[] boundsArray = availableBounds.ToArray();
                if (boundsArray.Length == 0)
                {
                    Debug.LogError("No available areas.");
                    return null;
                }

                BoundsSize bound = boundsArray[Random.Range(0, boundsArray.Length)];
                bound.propsInArea++;

                int axis = bound.axis == 2 ? 0 : 2;

                Vector3 position = GetPropPositionInBounds(objBounds, bound, axis);

                obj.transform.position = position;
                objBounds = renderer.bounds; 

                /*
                Vector3 delta = cellObject.transform.position - obj.transform.position;
                delta.y = 0f;
                */
                //obj.transform.rotation = Quaternion.Euler(delta);
                obj.transform.forward = -DirectionHelper.ToOffset3D(bound.direction);

                foreach (BoundsSize bsz in boundsArray)
                    SubtractBoundOnAxis(ref bsz.bounds, objBounds, bsz.axis == 2 ? 0 : 2);
            }

            return bounds;
        }

        private static List<BoundsSize> CreateBounds(EDirectionBitmask directions, Bounds cellBounds, float areaPercentage)
        {
            var bounds = new List<BoundsSize>();
            foreach (EDirectionBitmask direction in DirectionHelper.GetValues())
            {
                if (!DirectionHelper.IsSet(directions, direction))
                    continue;

                bounds.Add(new BoundsSize
                {
                    bounds      = GetDirectionBounds(direction, cellBounds, areaPercentage),
                    direction   = direction,
                    axis        = DirectionToAxis(direction),
                    propsInArea = 0
                });
            }
            return bounds;
        }

        private static Bounds GetDirectionBounds(EDirectionBitmask direction, Bounds cellBounds, float areaPercentage)
        {
            int     axisIndex = 0;
            Vector3 dir       = DirectionToVector(direction, out axisIndex);
            float   pp        = areaPercentage;
            Vector3 p1        = cellBounds.center;
            float   w1        = cellBounds.size[axisIndex];
            float   w2        = w1 * pp;
            float   x         = p1[axisIndex] + (w1/2 - w2/2) * Mathf.Sign(dir[axisIndex]);

            Vector3 center     = p1;
            center [axisIndex] = x;
            Vector3 size       = cellBounds.size;
            size   [axisIndex] = w2;

            return new Bounds(center, size);
        }

        private static int DirectionToAxis(EDirectionBitmask direction)
        {
            switch (direction)
            {
                case EDirectionBitmask.Down: return 2;
                case EDirectionBitmask.Up: return 2;
                case EDirectionBitmask.Left: return 0;
                case EDirectionBitmask.Right: return 0;
            }
            return 0;
        }

        private static Vector3 DirectionToVector(EDirectionBitmask direction, out int axisIndex)
        {
            switch (direction)
            {
                case EDirectionBitmask.Left:  axisIndex = 0; return Vector3.left;
                case EDirectionBitmask.Right: axisIndex = 0; return Vector3.right; 
                case EDirectionBitmask.Up:    axisIndex = 2; return Vector3.forward;
                case EDirectionBitmask.Down:  axisIndex = 2; return Vector3.back; 
                default: axisIndex = 0; return Vector3.zero;
            }
        }

        private static int OrthogonalAxis(int axis)
        {
            return axis == 2 ? 0 : 2;
        }

        private static Vector3 GetPropPositionInBounds(Bounds b1, BoundsSize b2, int axis)
        {
            Vector3 c1 = b2.bounds.center;
            Vector3 e1 = b1.extents;
            Vector3 c2 = c1; 
            Vector3 e2 = b2.bounds.extents;

            float sign = Mathf.Sign(Random.value -0.5f);

            c1[axis] = c2[axis] + e2[axis] * sign + e1[axis] * -sign;

            return c1;
        }

        private static void SubtractBoundOnAxis(ref Bounds b1, Bounds b2, int axis)
        {
            float c1 = b1.center[axis];
            float e1 = b1.extents[axis];
            float c2 = b2.center[axis];
            float e2 = b2.extents[axis];

            float bl1 = c1 - e1;
            float br1 = c1 + e1;
            float bl2 = c2 - e2;
            float br2 = c2 + e2;
            float dc = c1 - c2;
            float r = br1 - bl2;
            float l = bl1 - br2;

            float abs_dc = Mathf.Abs(dc);
            Vector3 dc3 = b1.center - b2.center;
            Vector3 e3 = b1.extents + b2.extents;
            if (dc3.sqrMagnitude >= e3.sqrMagnitude)
                return;

            float collisionSign = Mathf.Sign(dc);
            if (collisionSign < 0)
            {
                e1 -= r/2;
                c1 -= r/2;
            }
            else
            {
                e1 -= Mathf.Abs(l/2);
                c1 += Mathf.Abs(l/2);
            }

            Vector3 c = b1.center;
            Vector3 e = b1.extents;

            c[axis] = c1;
            e[axis] = e1;

            b1.center  = c;
            b1.extents = e;
        }
    }
}