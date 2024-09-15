using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Twenty.Effects
{
    public class RubberEffect : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MeshRenderer _renderer;
        [SerializeField] private MeshFilter _meshFilter;
        [Header("Presets")]
        public RubberType _currentPreset;
        
        [SerializeField] private float _effectIntensity = 1;
        [SerializeField] private float _damping = 0.7f;
        [SerializeField] private float _mass = 1;
        [SerializeField] private float _stiffness = 0.2f;

        private Mesh _workingMesh;
        private Mesh _originalMesh;

        private NativeArray<RubberData> _rubberDataArray;
        
        public bool _sleeping = true;

        private Vector3 _lastWorldPosition;
        private Quaternion _lastWorldRotation;
        
        private void OnValidate()
        {
            CheckPreset();
        }

        private void Start()
        {
            CheckPreset();
            
            _originalMesh = _meshFilter.sharedMesh;

            _workingMesh = Instantiate(_meshFilter.sharedMesh);
            _meshFilter.sharedMesh = _workingMesh;
            
            Initialize();

            WakeUp();
        }

        private void Initialize()
        {
            int count = _workingMesh.vertexCount;
            
            _rubberDataArray = new NativeArray<RubberData>(count, Allocator.Persistent);
            
            for (int i = 0; i < count; i++)
            {
                Vector3 vTarget = transform.TransformPoint(_workingMesh.vertices[i]);
                _rubberDataArray[i] = new RubberData(_mass, _stiffness, _damping, vTarget);
            }
        }

        private void WakeUp()
        {
            for (int i = 0; i < _rubberDataArray.Length; i++)
            {
                var rubber = _rubberDataArray[i];
                rubber.Sleeping = false;
                rubber.Vel = Vector3.zero; // Reset velocity when waking up
                _rubberDataArray[i] = rubber;
            }
            
            _sleeping = false;
        }

        private void FixedUpdate()
        {
            if ((transform.position != _lastWorldPosition 
                 || transform.rotation != _lastWorldRotation))
            {
                WakeUp();
                _lastWorldPosition = transform.position;
                _lastWorldRotation = transform.rotation;
            }

            if (_sleeping) return;
            
            VertexRubberJob rubberJob = new VertexRubberJob(
                new NativeArray<Vector3>(_originalMesh.vertices, Allocator.TempJob),
                _rubberDataArray,
                _effectIntensity,
                transform.localToWorldMatrix,
                transform.worldToLocalMatrix,
                new Vector2(_renderer.bounds.max.y, _renderer.bounds.size.y));

            JobHandle handle = rubberJob.Schedule(_rubberDataArray.Length, 64);
            handle.Complete();

            _workingMesh.vertices = rubberJob.Vertices.ToArray();
            _workingMesh.RecalculateNormals();

            rubberJob.Vertices.Dispose();

            if (_rubberDataArray.All(r => r.Sleeping))
                _sleeping = true;
        }
        
        private void CheckPreset()
        {
            switch (_currentPreset)
            {
                case RubberType.HardRubber:
                    _mass = 8f;
                    _stiffness = 0.5f;
                    _damping = 0.9f;
                    _effectIntensity = 0.5f;
                    break;
                case RubberType.Jelly:
                    _mass = 1f;
                    _stiffness = 0.95f;
                    _damping = 0.95f;
                    _effectIntensity = 1f;
                    break;
                case RubberType.RubberDuck:
                    _mass = 2f;
                    _stiffness = 0.5f;
                    _damping = 0.85f;
                    _effectIntensity = 1f;
                    break;
                case RubberType.SoftLatex:
                    _mass = 0.9f;
                    _stiffness = 0.3f;
                    _damping = 0.25f;
                    _effectIntensity = 1f;
                    break;
            }

            _mass = Mathf.Max(_mass, 0);
            _stiffness = Mathf.Max(_stiffness, 0);
            _effectIntensity = Mathf.Clamp(_effectIntensity, 0, 1);
        }
        
        [BurstCompile]
        public struct RubberData
        {
            public float Mass;
            public float Stiffness;
            public float Damping;
            public float Intensity;
            
            public Vector3 Pos;
            
            public Vector3 Vel;  // Store velocity
            public bool Sleeping;

            public RubberData(float mass, float stiffness, float damping, Vector3 target) : this()
            {
                Mass = mass;
                Stiffness = stiffness;
                Damping = damping;
                Pos = target;
                Intensity = 1;
                Vel = Vector3.zero;  // Initialize velocity
                Sleeping = false;
            }
        }
        
        [BurstCompile]
        public struct VertexRubberJob : IJobParallelFor
        {
            public NativeArray<Vector3> Vertices;
            private NativeArray<RubberData> _rubbers;
            
            private float _intensity;
            private Matrix4x4 _localToWorld;
            private Matrix4x4 _worldToLocal;
            private Vector2 _rendererData;
            
            private const float _STOP_LIMIT = 0.0001f;
            
            public VertexRubberJob(NativeArray<Vector3> vertices, 
                NativeArray<RubberData> rubbers, float intensity, 
                Matrix4x4 localToWorld, Matrix4x4 worldToLocal, Vector2 rendererData) : this()
            {
                Vertices = vertices;
                _rubbers = rubbers;
                _intensity = intensity;
                _localToWorld = localToWorld;
                _worldToLocal = worldToLocal;
                _rendererData = rendererData;
            }

            public void Execute(int index)
            {
                var rubber = _rubbers[index];
                var vertex = Vertices[index];

                Vector3 v3Target = _localToWorld.MultiplyPoint3x4(vertex);
                rubber.Intensity = (1 - (_rendererData.x - v3Target.y) / _rendererData.y) * _intensity;
                
                Vector3 force = (v3Target - rubber.Pos) * rubber.Stiffness;
                Vector3 acc = force / rubber.Mass;
                rubber.Vel = (rubber.Vel + acc) * rubber.Damping;
                rubber.Pos += rubber.Vel;

                // Check if the vertex should sleep
                if (rubber.Vel.magnitude < _STOP_LIMIT)
                {
                    rubber.Pos = v3Target;
                    rubber.Sleeping = true;
                }

                v3Target = _worldToLocal.MultiplyPoint3x4(rubber.Pos);

                vertex = Vector3.Lerp(vertex, v3Target, rubber.Intensity);

                _rubbers[index] = rubber;
                Vertices[index] = vertex;
            }
        }

        private void OnDestroy()
        {
            _rubberDataArray.Dispose();
        }

        public enum RubberType
        {
            Custom,
            RubberDuck,
            HardRubber,
            Jelly,
            SoftLatex
        }
    }
}
