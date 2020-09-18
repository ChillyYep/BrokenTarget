using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace chenyi
{
    public interface IDrawableGizmos
    {
        Mesh SphereMesh { get; }
        Transform transform { get; set; }
        void DrawMesh();
    }
    public class DetectionSphere : IConvertable, IDrawableGizmos
    {
        public Vector3 center { get; private set; }
        public float radius { get; set; }
        private Transform _transfrom;
        public Transform transform
        {
            get
            {
                return _transfrom;
            }
            set
            {
                _transfrom = value;
                center = _transfrom.position;
                World2Object = transform.worldToLocalMatrix;
                Object2World = transform.localToWorldMatrix;
            }
        }
        public Matrix4x4 World2Object { get; private set; }
        public Matrix4x4 Object2World { get; private set; }
        public SegmentVerticalType segmentVerticalType { get; set; }
        public SegmentCircleType segmentCircleType { get; set; }
        private Mesh sphereMesh;
        private GenDetectionObjectStrategy strategy;
        public Mesh SphereMesh
        {
            get
            {
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
        public void DrawMesh()
        {
            if (strategy == null)
            {
                strategy = new DefaultGenSphereStrategy(segmentVerticalType, segmentCircleType, radius);
            }
            sphereMesh = new Mesh();
            strategy.GenSphereData();
            strategy.GenMesh(ref sphereMesh);
        }
    }
}
