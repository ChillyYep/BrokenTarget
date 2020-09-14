using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace chenyi2
{
    public class DetectionSphere : IVistor
    {
        public Vector3 center { get; set; }
        public float radius { get; set; }
        public SegmentVerticalType segmentVerticalType { get; set; }
        public SegmentCircleType segmentCircleType { get; set; }
        private Mesh sphereMesh;
        private GenDetectionObjectStrategy strategy;
        public Mesh SphereMesh
        {
            get
            {
                if (sphereMesh == null)
                {
                    DrawSphere();
                }
                return sphereMesh;
            }
        }

        public DetectionSphere(Vector3 center, float radius = 1f, SegmentVerticalType segmentVerticalType = SegmentVerticalType.Ten,
            SegmentCircleType segmentCircleType = SegmentCircleType.Eighteen)
        {
            this.segmentVerticalType = segmentVerticalType;
            this.segmentCircleType = segmentCircleType;
            this.center = center;
            this.radius = radius;
        }
        public bool IsInSphere(Vector3 targetPoint)
        {
            return Vector3.Distance(targetPoint, center) <= radius;
        }
        public void DrawSphere()
        {
            if (strategy == null)
            {
                strategy = new DefaultGenSphereStrategy(segmentVerticalType, segmentCircleType, radius);
            }
            sphereMesh = new Mesh();
            strategy.GenSphereData();
            strategy.GenMesh(ref sphereMesh);
        }
        public void Traversal(IBreakable breakable)
        {
            foreach (var item in breakable.ModelInfos)
            {
                if (IsInSphere(item.bound.center))
                {
                    breakable.AddExplodeQuery(item.gameObject, center - item.bound.center);
                }

            }
        }
    }
}
