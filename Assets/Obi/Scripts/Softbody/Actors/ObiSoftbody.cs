using System;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace Obi
{
    [AddComponentMenu("Physics/Obi/Obi Softbody", 930)]
    public class ObiSoftbody : ObiActor, IShapeMatchingConstraintsUser
    {
        [SerializeField] protected ObiSoftbodyBlueprintBase m_SoftbodyBlueprint;

        [SerializeField] protected bool m_SelfCollisions = false;
        [SerializeField] protected float _plasticRecovery = 0;
        [SerializeField] [HideInInspector] private int centerBatch = -1;
        [SerializeField] [HideInInspector] private int centerShape = -1;
        //[SerializeField] protected float velocityMagnitude = 0;
        // shape matching constraints:
        [SerializeField] protected bool _shapeMatchingConstraintsEnabled = true;
        [SerializeField] [Range(0, 1)] protected float _deformationResistance = 1;
        [SerializeField] [Range(0, 1)] protected float _maxDeformation = 0;
        [SerializeField] protected float _plasticYield = 0;
        [SerializeField] protected float _plasticCreep = 0;
        private bool flag = false;
        /// <summary>  
        /// Whether this actor's shape matching constraints are enabled.
        /// </summary>
        public bool shapeMatchingConstraintsEnabled
        {
            get { return _shapeMatchingConstraintsEnabled; }
            set
            {
                if (value != _shapeMatchingConstraintsEnabled)
                {
                    _shapeMatchingConstraintsEnabled = value;
                    SetConstraintsDirty(Oni.ConstraintType.ShapeMatching);

                    if (!_shapeMatchingConstraintsEnabled)
                    {
                        Debug.Log("111111111");
                    }
                }
            }
        }

        private void Update()
        {
            if (!_shapeMatchingConstraintsEnabled)
            {
                // shapeMatchingConstraintsEnabled Ϊ false�����������ٶ�
                UpdateParticleVelocities();
                flag = true;
            }
            
        }
        Vector3 CalculateCenter()
        {
            Vector3 center = Vector3.zero;
            for (int i = 0; i < solver.positions.count; i++)
            {
                Vector4 currentPosition = solver.positions[i];
                center += new Vector3(currentPosition.x, currentPosition.y, currentPosition.z);
            }
            center /= solver.positions.count; // ��������λ�õľ�ֵ
            return center;
        }
        //private void UpdateParticleVelocities()
        //{
        //    float maxVelocityMultiplier = 4.0f; // ����ٶȳ���
        //    float minVelocityMultiplier = 0.8f; // ��С�ٶȳ���
        //    int numParticles = solver.positions.count;
        //    Vector3 attackPoint = new Vector3(solver.positions[0].x, solver.positions[0].y, solver.positions[0].z); // ���蹥����Ϊ��һ�����ӵ�λ��

        //    // �洢���������Ͷ�Ӧ�ľ���
        //    int[] particleIndices = new int[numParticles];
        //    float[] distances = new float[numParticles];

        //    // ����ÿ�����ӵ�������ľ��룬���洢
        //    for (int i = 0; i < numParticles; i++)
        //    {
        //        Vector4 currentPosition = solver.positions[i];
        //        distances[i] = Vector3.Distance(new Vector3(currentPosition.x, currentPosition.y, currentPosition.z), attackPoint);
        //        particleIndices[i] = i;
        //    }

        //    // ���ݾ������������������������
        //    Array.Sort(distances, particleIndices);

        //    // �����ٶȳ����ķ�Χ
        //    float multiplierRange = maxVelocityMultiplier - minVelocityMultiplier;

        //    // ���������ٶ�
        //    for (int i = 0; i < numParticles; i++)
        //    {

        //        // ����������������ȡԭʼ����
        //        int originalIndex = particleIndices[i];
        //        Vector4 currentPosition = solver.positions[originalIndex];
        //        Vector3 particlePosition = new Vector3(currentPosition.x, currentPosition.y, currentPosition.z);

        //        // �����ٶȳ������������ӵ�����λ�����Բ�ֵ
        //        float velocityMultiplier = Mathf.Lerp(minVelocityMultiplier, maxVelocityMultiplier, (float)i / (numParticles - 1));

        //        // �����ٶ�����
        //        Vector3 velocity = (particlePosition - CalculateCenter()).normalized * velocityMultiplier;
        //        velocity.x *= 0.5f;
        //        velocity.z *= 0.5f;
        //        // Ӧ���ٶ�
        //        solver.velocities[originalIndex] = new Vector4(velocity.x, velocity.y, velocity.z, 0);
        //    }

        //    // ��������...
        //    solver.gravity = new Vector3(0, -200f, 0); // ��������
        //    solver.gravitySpace = Space.World; // �������ÿռ�
        //    solver.frictionConstraintParameters.enabled = true; // Ħ����
        //}
        private void UpdateParticleVelocities()
        {
            int numParticles = solver.positions.count;
            if (!flag)
            {
                float maxVelocityMultiplier = 4.0f; // ����ٶȳ���
                float minVelocityMultiplier = 0.8f; // ��С�ٶȳ���
                Vector3 attackPoint = new Vector3(solver.positions[0].x, solver.positions[0].y, solver.positions[0].z); // ���蹥����Ϊ��һ�����ӵ�λ��

                // �洢���������Ͷ�Ӧ�ľ���
                int[] particleIndices = new int[numParticles];
                float[] distances = new float[numParticles];

                // ����ÿ�����ӵ�������ľ��룬���洢
                for (int i = 0; i < numParticles; i++)
                {
                    Vector4 currentPosition = solver.positions[i];
                    distances[i] = Vector3.Distance(new Vector3(currentPosition.x, currentPosition.y, currentPosition.z), attackPoint);
                    particleIndices[i] = i;
                }

                // ���ݾ������������������������
                Array.Sort(distances, particleIndices);

                // ����һ���������洢���º���ٶ�
                Vector4[] updatedVelocities = new Vector4[numParticles];

                // ���������ٶ�
                for (int i = 0; i < numParticles; i++)
                {
                    // ����������������ȡԭʼ����
                    int originalIndex = particleIndices[i];
                    Vector4 currentPosition = solver.positions[originalIndex];
                    Vector3 particlePosition = new Vector3(currentPosition.x, currentPosition.y, currentPosition.z);

                    // �����ٶȳ������������ӵ�����λ�����Բ�ֵ
                    float velocityMultiplier = Mathf.Lerp(minVelocityMultiplier, maxVelocityMultiplier, (float)i / (numParticles - 1));

                    // �����ٶ�����
                    Vector3 velocity = (particlePosition - CalculateCenter()).normalized * velocityMultiplier;


                    // Ӧ���ٶ�
                    updatedVelocities[originalIndex] = new Vector4(velocity.x, velocity.y, velocity.z, 0);
                }

                // ��ȡ�������ӵ� y ���ٶȲ�����
                float[] yVelocities = new float[numParticles];
                for (int i = 0; i < numParticles; i++)
                {
                    yVelocities[i] = updatedVelocities[i].y;
                }
                Array.Sort(yVelocities);

                // �ҵ�ǰ 30% y ���ٶ��������ӵ��ٶ���ֵ
                int thresholdIndex = (int)(numParticles * 0.7f);
                float velocityThreshold = yVelocities[thresholdIndex];

                // �� y ���ٶȴ�����ֵ�����ӵ� y ������С��ԭ���� 70%
                for (int i = 0; i < numParticles; i++)
                {
                    if (updatedVelocities[i].y > velocityThreshold)
                    {
                        updatedVelocities[i] = new Vector4(updatedVelocities[i].x, updatedVelocities[i].y * 0.6f, updatedVelocities[i].z, updatedVelocities[i].w);
                    }
                }

                // ���� solver ���ٶ�
                for (int i = 0; i < numParticles; i++)
                {
                    solver.velocities[i] = updatedVelocities[i];
                }

            }
            else
            {
                for (int i = 0; i < numParticles; i++)
                {

                    Vector4 velocity = solver.velocities[i];
                    if (velocity.x < 0.1) velocity.x = 0;
                    if (velocity.z < 0.1) velocity.z = 0;
                    velocity.x *= 0.4f;
                    velocity.z *= 0.4f;
                    Debug.Log("velocity.x:" + velocity.x);
                    Debug.Log("velocity.z:" + velocity.z);
                }

                
            }
           
        }

        /// <summary>  
        /// Deformation resistance for shape matching constraints.
        /// </summary>
        /// A value of 1 will make constraints to try and resist deformation as much as possible, given the current solver settings.
        /// Lower values will progressively make the softbody softer.
        public float deformationResistance
        {
            get { return _deformationResistance; }
            set
            {
                _deformationResistance = value; SetConstraintsDirty(Oni.ConstraintType.ShapeMatching);
            }
        }

        /// <summary>  
        /// Maximum amount of plastic deformation.
        /// </summary>
        /// This determines how much deformation can be permanently absorbed via plasticity by shape matching constraints.
        public float maxDeformation
        {
            get { return _maxDeformation; }
            set
            {
                _maxDeformation = value; SetConstraintsDirty(Oni.ConstraintType.ShapeMatching);
            }
        }

        /// <summary>  
        /// Threshold for plastic behavior. 
        /// </summary>
        /// Once softbody deformation goes above this value, a percentage of the deformation (determined by <see cref="plasticCreep"/>) will be permanently absorbed into the softbody's rest shape.
        public float plasticYield
        {
            get { return _plasticYield; }
            set
            {
                _plasticYield = value; SetConstraintsDirty(Oni.ConstraintType.ShapeMatching);
            }
        }

        /// <summary>  
        /// Percentage of deformation that gets absorbed into the rest shape, once deformation goes above the <see cref="plasticYield"/> threshold.
        /// </summary>
        public float plasticCreep
        {
            get { return _plasticCreep; }
            set
            {
                _plasticCreep = value; SetConstraintsDirty(Oni.ConstraintType.ShapeMatching);
            }
        }

        /// <summary>  
        /// Rate of recovery from plastic deformation.
        /// </summary>
        /// A value of 0 will make sure plastic deformation is permament, and the softbody never recovers from it. Any higher values will make the softbody return to
        /// its original shape gradually over time.
        public float plasticRecovery
        {
            get { return _plasticRecovery; }
            set
            {
                _plasticRecovery = value; SetConstraintsDirty(Oni.ConstraintType.ShapeMatching);
            }
        }

        public override ObiActorBlueprint sourceBlueprint
        {
            get { return m_SoftbodyBlueprint; }
        }

        public ObiSoftbodyBlueprintBase softbodyBlueprint
        {
            get { return m_SoftbodyBlueprint; }
            set
            {
                if (m_SoftbodyBlueprint != value)
                {
                    RemoveFromSolver();
                    ClearState();
                    m_SoftbodyBlueprint = value;
                    AddToSolver();
                }
            }
        }

        /// <summary>  
        /// Whether particles in this actor colide with particles using the same phase value.
        /// </summary>
        public bool selfCollisions
        {
            get { return m_SelfCollisions; }
            set { if (value != m_SelfCollisions) { m_SelfCollisions = value; SetSelfCollisions(selfCollisions); } }
        }

        /// <summary>
        /// If true, it means particles may not be completely spherical, but ellipsoidal.
        /// </summary>
        /// In the case of softbodies, this is true, as particles can be deformed to adapt to the body surface.
        public override bool usesAnisotropicParticles
        {
            get { return true; }
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            SetupRuntimeConstraints();
        }

        private void SetupRuntimeConstraints()
        {
            SetConstraintsDirty(Oni.ConstraintType.ShapeMatching);
            SetSelfCollisions(m_SelfCollisions);
            SetSimplicesDirty();
            UpdateCollisionMaterials();
        }

        public override void LoadBlueprint(ObiSolver solver)
        {
            base.LoadBlueprint(solver);
            RecalculateCenterShape();
            SetSelfCollisions(m_SelfCollisions);
        }

        public override void Teleport(Vector3 position, Quaternion rotation)
        {
            base.Teleport(position, rotation);

            if (!isLoaded)
                return;

            Matrix4x4 offset = solver.transform.worldToLocalMatrix *
                               Matrix4x4.TRS(position, Quaternion.identity, Vector3.one) *
                               Matrix4x4.TRS(Vector3.zero, rotation, Vector3.one) *
                               Matrix4x4.TRS(Vector3.zero, Quaternion.Inverse(transform.rotation), Vector3.one) *
                               Matrix4x4.TRS(-transform.position, Quaternion.identity, Vector3.one) *
                               solver.transform.localToWorldMatrix;

            Quaternion rotOffset = offset.rotation;

            var ac = GetConstraintsByType(Oni.ConstraintType.ShapeMatching) as ObiConstraints<ObiShapeMatchingConstraintsBatch>;
            var sc = solver.GetConstraintsByType(Oni.ConstraintType.ShapeMatching) as ObiConstraints<ObiShapeMatchingConstraintsBatch>;

            // rotate clusters accordingly:
            for (int i = 0; i < ac.GetBatchCount(); ++i)
            {
                int batchOffset = solverBatchOffsets[(int)Oni.ConstraintType.ShapeMatching][i];

                for (int j = 0; j < ac.batches[i].activeConstraintCount; ++j)
                {
                    sc.batches[j].orientations[batchOffset + i] = rotOffset * sc.batches[i].orientations[batchOffset + j];
                }
            }

        }

        public void RecalculateRestShapeMatching()
        {
            if (Application.isPlaying && isLoaded)
            {
                var sc = solver.GetConstraintsByType(Oni.ConstraintType.ShapeMatching) as ObiConstraints<ObiShapeMatchingConstraintsBatch>;

                foreach (var batch in sc.batches)
                    batch.RecalculateRestShapeMatching();
            }
        }

        private void RecalculateCenterShape()
        {

            centerShape = -1;
            centerBatch = -1;

            if (Application.isPlaying && isLoaded)
            {

                for (int i = 0; i < solverIndices.Length; ++i)
                {
                    if (solver.invMasses[solverIndices[i]] <= 0)
                        return;
                }

                var sc = m_SoftbodyBlueprint.GetConstraintsByType(Oni.ConstraintType.ShapeMatching) as ObiConstraints<ObiShapeMatchingConstraintsBatch>;

                // Get the particle whose center is closest to the actor's center (in blueprint space)
                float minDistance = float.MaxValue;
                for (int j = 0; j < sc.GetBatchCount(); ++j)
                {
                    var batch = sc.GetBatch(j) as ObiShapeMatchingConstraintsBatch;

                    for (int i = 0; i < batch.activeConstraintCount; ++i)
                    {
                        float dist = m_SoftbodyBlueprint.positions[batch.particleIndices[batch.firstIndex[i]]].sqrMagnitude;

                        if (dist < minDistance)
                        {
                            minDistance = dist;
                            centerShape = i;
                            centerBatch = j;
                        }
                    }
                }
            }
        }

        /// <summary>  
        /// Recalculates shape matching rest state.
        /// </summary>
        /// Recalculates the shape used as reference for transform position/orientation when there are no fixed particles, as well as the rest shape matching state.
        /// Should be called manually when changing the amount of fixed particles and/ or active particles.
        public override void UpdateParticleProperties()
        {
            RecalculateRestShapeMatching();
            RecalculateCenterShape();
        }

        public override void Interpolate()
        {
            var sc = solver.GetConstraintsByType(Oni.ConstraintType.ShapeMatching) as ObiConstraints<ObiShapeMatchingConstraintsBatch>;

            if (Application.isPlaying && isActiveAndEnabled && centerBatch > -1 && centerBatch < sc.batches.Count)
            {
                var batch = sc.batches[centerBatch] as ObiShapeMatchingConstraintsBatch;
                var offsets = solverBatchOffsets[(int)Oni.ConstraintType.ShapeMatching];

                if (centerShape > -1 && centerShape < batch.activeConstraintCount && centerBatch < offsets.Count)
                {
                    int offset = offsets[centerBatch] + centerShape;

                    transform.position = solver.transform.TransformPoint((Vector3)batch.coms[offset] - batch.orientations[offset] * batch.restComs[offset]);
                    transform.rotation = solver.transform.rotation * batch.orientations[offset];
                }
            }

            SetSelfCollisions(selfCollisions);

            base.Interpolate();
        }
        

        public override void SetRestPosDataToCS(ComputeBuffer buffer)
        {
            //set the data to the compute shader:
            buffer.SetData(solver.positions.AsNativeArray<Vector4>());
        }

        
        public void OnDrawGizmos()
        {
            var restPositions = solver.restPositions.AsNativeArray<Vector4>();
            var positions = solver.positions.AsNativeArray<Vector4>();
            for (int i = 0; i < positions.Length; ++i)
            {
                var pos = positions[i];
                var restPos = restPositions[i];
                Gizmos.color = Color.green;
                Gizmos.DrawLine(pos, restPos);
                Gizmos.DrawSphere(pos, 0.01f);
            }
        }

    }
    
    
}