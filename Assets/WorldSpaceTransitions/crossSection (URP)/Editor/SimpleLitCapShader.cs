using System;
using UnityEngine;


namespace UnityEditor.CrossSection.URP.ShaderGUI
{
    internal class SimpleLitCapShader : BaseShaderGUI
    {
        // Properties
        private UnityEditor.Rendering.Universal.ShaderGUI.SimpleLitGUI.SimpleLitProperties shadingModelProperties;

        MaterialProperty stencilMask = null;
        MaterialProperty stencilComp = null;
        //_StencilComp

        // collect properties from the material properties
        public override void FindProperties(MaterialProperty[] properties)
        {
            base.FindProperties(properties);
            shadingModelProperties = new UnityEditor.Rendering.Universal.ShaderGUI.SimpleLitGUI.SimpleLitProperties(properties);

            stencilMask = FindProperty("_StencilMask", properties, false);
            stencilComp = FindProperty("_StencilComp", properties, false);
        }

        // material changed check
        public override void MaterialChanged(Material material)
        {
            if (material == null)
                throw new ArgumentNullException("material");

            SetMaterialKeywords(material, UnityEditor.Rendering.Universal.ShaderGUI.SimpleLitGUI.SetMaterialKeywords);
        }
        
        // material main surface options
        public override void DrawSurfaceOptions(Material material)
        {
            if (material == null)
                throw new ArgumentNullException("material");

            // Use default labelWidth
            EditorGUIUtility.labelWidth = 0f;

            // Detect any changes to the material
            EditorGUI.BeginChangeCheck();
            {
                base.DrawSurfaceOptions(material);
            }
            if (EditorGUI.EndChangeCheck())
            {
                foreach (var obj in blendModeProp.targets)
                    MaterialChanged((Material)obj);
            }
        }

        // material main surface inputs
        public override void DrawSurfaceInputs(Material material)
        {
            base.DrawSurfaceInputs(material);
            UnityEditor.Rendering.Universal.ShaderGUI.SimpleLitGUI.Inputs(shadingModelProperties, materialEditor, material);
            DrawEmissionProperties(material, true);
            DrawTileOffset(materialEditor, baseMapProp);
            //CrossSection
            if (stencilMask != null) materialEditor.RangeProperty(stencilMask, "_StencilMask");
            if (stencilComp != null) materialEditor.RangeProperty(stencilComp, "_StencilComp");
        }

        public override void DrawAdvancedOptions(Material material)
        {
            UnityEditor.Rendering.Universal.ShaderGUI.SimpleLitGUI.Advanced(shadingModelProperties);
            base.DrawAdvancedOptions(material);
        }

        public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader)
        {
            if (material == null)
                throw new ArgumentNullException("material");

            // _Emission property is lost after assigning Standard shader to the material
            // thus transfer it before assigning the new shader
            if (material.HasProperty("_Emission"))
            {
                material.SetColor("_EmissionColor", material.GetColor("_Emission"));
            }

            base.AssignNewShaderToMaterial(material, oldShader, newShader);

            if (oldShader == null || !oldShader.name.Contains("Legacy Shaders/"))
            {
                SetupMaterialBlendMode(material);
                return;
            }

            SurfaceType surfaceType = SurfaceType.Opaque;
            BlendMode blendMode = BlendMode.Alpha;
            if (oldShader.name.Contains("/Transparent/Cutout/"))
            {
                surfaceType = SurfaceType.Opaque;
                material.SetFloat("_AlphaClip", 1);
            }
            else if (oldShader.name.Contains("/Transparent/"))
            {
                // NOTE: legacy shaders did not provide physically based transparency
                // therefore Fade mode
                surfaceType = SurfaceType.Transparent;
                blendMode = BlendMode.Alpha;
            }
            material.SetFloat("_Surface", (float)surfaceType);
            material.SetFloat("_Blend", (float)blendMode);

            MaterialChanged(material);
        }
    }
}
