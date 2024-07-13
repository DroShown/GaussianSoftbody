// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Profiling;
using Unity.Profiling.LowLevel;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using Obi;


namespace GaussianSplatting.Runtime
{


    [ExecuteInEditMode]
    public class GaussianSplatRendererDynamic : GaussianSplatRenderer
    {
        
        public ObiSoftbody m_ObiSoftbody;
        ComputeBuffer m_GpuParticlePosData;
        ComputeBuffer m_GpuParticleRestPosData;
        ComputeBuffer m_GpuParticleRotData;
        ComputeBuffer m_GpuParticleRestRotData;
        ComputeBuffer m_GpuBoneIndices;
        ComputeBuffer m_GpuBoneWeights;
        
        public float4[] boneWeights;
        public int4[] boneIndices;
        public float3[] splatPos;
        public float3[] splatRestPos;
        public float4[] particlePos;
        
        public ComputeShader m_ComputeShader => m_CSSplatUtilities;
        public int m_ParticleCount;
        
        public new void OnEnable()
        {
            
            base.OnEnable();

            m_ParticleCount = m_ObiSoftbody.blueprint.particleCount;
            Debug.Log("particle count:"+m_ObiSoftbody.solverIndices.Length);

            m_GpuParticlePosData = new ComputeBuffer(m_ParticleCount, sizeof(float) * 4);
            m_GpuParticleRestPosData = new ComputeBuffer(m_ParticleCount, sizeof(float) * 4);
            m_GpuParticleRotData = new ComputeBuffer(m_ParticleCount, sizeof(float) * 4);
            m_GpuParticleRestRotData = new ComputeBuffer(m_ParticleCount, sizeof(float) * 4);
            m_GpuBoneIndices = new ComputeBuffer(m_SplatCount, sizeof(int) * 4);
            m_GpuBoneWeights = new ComputeBuffer(m_SplatCount, sizeof(float) * 4);
            
            
            
            if(m_GpuParticlePosData == null)
                Debug.Log("m_GPUpos is null");
            m_ObiSoftbody.SetRotDataToCS(m_GpuParticleRotData);
            m_ObiSoftbody.SetRestRotDataToCS(m_GpuParticleRestRotData);
            m_ObiSoftbody.SetPosDataToCS(m_GpuParticlePosData);
            m_ObiSoftbody.SetRestPosDataToCS(m_GpuParticleRestPosData);

            BindBuffersToCS((int)KernelIndices.CalcIndicesAndWeight);
            
            m_ComputeShader.Dispatch((int)KernelIndices.CalcIndicesAndWeight,m_SplatCount,1,1);

            //create a temp array to store the data
            boneWeights = new float4[m_SplatCount];
            m_GpuBoneWeights.GetData(boneWeights);
            boneIndices = new int4[m_SplatCount];
            m_GpuBoneIndices.GetData(boneIndices);
            
            // splatPos = new float3[m_SplatCount];
            // splatRestPos = new float3[m_SplatCount];
            // m_GpuPosData.GetData(splatPos);
            // m_GpuRestPosData.GetData(splatRestPos);
            particlePos = new float4[m_ParticleCount];

        }
        public new void Update()
        {
            //调用基类的Update方法
            base.Update();
            m_ObiSoftbody.SetRotDataToCS(m_GpuParticleRotData);
            m_ObiSoftbody.SetPosDataToCS(m_GpuParticlePosData);
            //TestTranslatePos();
            BindBuffersToCS((int)KernelIndices.UpdatePos);
            m_ComputeShader.Dispatch((int)KernelIndices.UpdatePos,(m_SplatCount+1023)/1024,1,1);
            //m_GpuPosData.GetData(splatPos);
            //m_GpuParticlePosData.GetData(particlePos);
            
            
        }

        private void TestTranslatePos()
        {
            // float time = Time.time;
            // using var cmb = new CommandBuffer { name = "SplatTranslateAll" };
            // SetAssetDataOnCS(cmb, KernelIndices.TranslateAll);
            //
            // Vector3 posDelta = new Vector3(0,(float)Math.Sin(5.0f * time) * Time.deltaTime,0);
            // //Debug.Log("posDelta:"+posDelta.y);
            // cmb.SetComputeVectorParam(m_CSSplatUtilities, Props.PosDelta, posDelta);
            //
            // DispatchUtilsAndExecute(cmb, KernelIndices.TranslateAll, m_SplatCount);
            //
            // //create a temp array to store the data
            // // float[] data = new float[m_GpuPosData.count];
            // // //get position data from GPU to see if it has been modified
            // // m_GpuPosData.GetData(data);
            // // Debug.Log(data[1]);
        }
        
        private void BindBuffersToCS(int kernelIndex)
        {
            m_ComputeShader.SetInt("_BoneCount",m_ParticleCount);
            m_ComputeShader.SetBuffer(kernelIndex,"_SplatPos",m_GpuPosData);
            m_ComputeShader.SetBuffer(kernelIndex,"_SplatRestPos",m_GpuRestPosData);
            m_ComputeShader.SetBuffer(kernelIndex,"_BoneRestPosBuffer", m_GpuParticleRestPosData);
            m_ComputeShader.SetBuffer(kernelIndex, "_BonePosBuffer", m_GpuParticlePosData);
            m_ComputeShader.SetBuffer(kernelIndex, "_BoneRestRotBuffer", m_GpuParticleRestRotData);
            m_ComputeShader.SetBuffer(kernelIndex, "_BoneRotBuffer", m_GpuParticleRotData);
            m_ComputeShader.SetBuffer(kernelIndex, "_BoneIndicesBuffer", m_GpuBoneIndices);
            m_ComputeShader.SetBuffer(kernelIndex, "_BoneWeightBuffer", m_GpuBoneWeights);
        }
        
        

        // public void OnDrawGizmos()
        // {
        //     for(int i = 0; i < m_ParticleCount; i++)
        //     {
        //         Gizmos.color = Color.red;
        //         Gizmos.DrawSphere(particlePos[i].xyz,0.03f);
        //     }
        //     
        // }
    }
}