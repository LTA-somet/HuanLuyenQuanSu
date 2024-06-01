using System;
using UnityEngine;

namespace UnityEditor.CrossSection.URP.ShaderGUI
{
    internal class UnlitShader : BaseShaderGUI
    {
        MaterialProperty sectionColor = null;
        MaterialProperty inverse = null;
        MaterialProperty retractBackfaces = null;
        MaterialProperty stencilMask = null;
        MaterialProperty colorMask = null;

        // collect properties from the material properties
        public override void FindProperties(MaterialProperty[] properties)
        {
            base.FindProperties(properties);

            sectionColor = FindProperty("_SectionColor", properties);
            inverse = FindProperty("_inverse", properties, false);
            retractBackfaces = FindProperty("_retractBackfaces", properties, false);
            stencilMask = FindProperty("_stencilMask", properties, false);
            colorMask = FindProperty("_ColorMask", properties, false);
        }

        // material changed check
        public override void MaterialChanged(Material material)
        {
            if (material == null)
                throw new ArgumentNullException("material");

            SetMaterialKeywords(material);
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
            DrawTileOffset(materialEditor, baseMapProp);
            if (sectionColor != null)
                materialEditor.ColorProperty(sectionColor, "_SectionColor");
            if (inverse != null)
                materialEditor.ShaderProperty(inverse, "_inverse");
            if (retractBackfaces != null)
                materialEditor.ShaderProperty(retractBackfaces, "_retractBackfaces");
            if (stencilMask != null) materialEditor.RangeProperty(stencilMask, "_StencilMask");
            if (colorMask != null) materialEditor.ShaderProperty(colorMask, "_ColorMask");
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