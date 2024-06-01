﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace WorldSpaceTransitions.URP
{
    //[ExecuteInEditMode]
    public class OutlinesToggle : MonoBehaviour
    {
        public Material edgeMaterial;
        public RenderPipelineAsset plainPipelineAsset;
        private Color c = Color.white;
        private bool kwdOn = true;
        private RenderPipelineAsset renderPipelineAsset;


        void Start()
        {
            kwdOn = edgeMaterial.IsKeywordEnabled("ALL_EDGES");
            renderPipelineAsset = GraphicsSettings.renderPipelineAsset;
        }
        void OnEnable()
        {
            kwdOn = edgeMaterial.IsKeywordEnabled("ALL_EDGES");
            renderPipelineAsset = GraphicsSettings.renderPipelineAsset;
        }

        void OnDisable()
        {
            if (kwdOn) edgeMaterial.EnableKeyword("ALL_EDGES");
            else edgeMaterial.DisableKeyword("ALL_EDGES");
            edgeMaterial.SetFloat("_all_edges", kwdOn ? 1 : 0);
        }

        public void ShowEdges(bool val)
        {
            GraphicsSettings.renderPipelineAsset = val? renderPipelineAsset: plainPipelineAsset;
            QualitySettings.renderPipeline = val ? renderPipelineAsset : plainPipelineAsset;
        }

        public void BackfaceEdgesOnly(bool val)
        {
            edgeMaterial.SetFloat("_all_edges", val? 0:1);
            if (val)
            {
                edgeMaterial.DisableKeyword("ALL_EDGES");
            }
            else
            {
                edgeMaterial.EnableKeyword("ALL_EDGES");
            }

        }
    }
}