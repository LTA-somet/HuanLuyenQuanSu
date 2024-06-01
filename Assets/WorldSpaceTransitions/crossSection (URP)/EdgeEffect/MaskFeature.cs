using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace WorldSpaceTransitions
{
    public class MaskFeature : ScriptableRendererFeature
    {
        class MaskPass : ScriptableRenderPass
        {
            private RenderTargetHandle destination { get; set; }
            public LayerMask layerMask;

            private Material maskMaterial = null;
            private FilteringSettings m_FilteringSettings;
            ShaderTagId m_ShaderTagId = new ShaderTagId("DepthOnly");

            public MaskPass(RenderQueueRange renderQueueRange, LayerMask layerMask, Material material)
            {
                m_FilteringSettings = new FilteringSettings(renderQueueRange, layerMask);
                this.maskMaterial = material;
                this.layerMask = layerMask;
            }

            public void Setup(RenderTargetHandle destination)
            {
                this.destination = destination;
            }

            // This method is called before executing the render pass.
            // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
            // When empty this render pass will render to the active camera render target.
            // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
            // The render pipeline will ensure target setup and clearing happens in an performance manner.
            public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
            {
                RenderTextureDescriptor descriptor = cameraTextureDescriptor;
                descriptor.depthBufferBits = 32;
                descriptor.colorFormat = RenderTextureFormat.ARGB32;

                cmd.GetTemporaryRT(destination.id, descriptor, FilterMode.Point);
                ConfigureTarget(destination.Identifier());
                ConfigureClear(ClearFlag.All, Color.black);
            }

            // Here you can implement the rendering logic.
            // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
            // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
            // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                CommandBuffer cmd = CommandBufferPool.Get("Mask Prepass");

                using (new ProfilingScope(cmd, new ProfilingSampler("Mask Prepass")))
                {
                    context.ExecuteCommandBuffer(cmd);
                    cmd.Clear();

                    var sortFlags = renderingData.cameraData.defaultOpaqueSortFlags;
                    var drawSettings = CreateDrawingSettings(m_ShaderTagId, ref renderingData, sortFlags);
                    drawSettings.perObjectData = PerObjectData.None;

                    ref CameraData cameraData = ref renderingData.cameraData;
                    Camera camera = cameraData.camera;
                    if (cameraData.isStereoEnabled)
                        context.StartMultiEye(camera);


                    drawSettings.overrideMaterial = maskMaterial;


                    context.DrawRenderers(renderingData.cullResults, ref drawSettings,
                        ref m_FilteringSettings);

                    cmd.SetGlobalTexture("_EdgeMap", destination.id);
                }

                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }

            /// Cleanup any allocated resources that were created during the execution of this render pass.
            public override void FrameCleanup(CommandBuffer cmd)
            {
                if (destination != RenderTargetHandle.CameraTarget)
                {
                    cmd.ReleaseTemporaryRT(destination.id);
                    destination = RenderTargetHandle.CameraTarget;
                }
            }
        }
        [System.Serializable]
        public class MaskPassSettings
        {
            public LayerMask layerMask = -1;
            public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPrePasses;
            public Shader maskShader;// this is a reference to ensure to get the shader into the build
        }

        public MaskPassSettings settings = new MaskPassSettings();
        MaskPass maskPass;
        RenderTargetHandle maskTexture;
        Material maskMaterial;

        public override void Create()
        {
            if (settings.maskShader != null) // to explain: why always null ?
            {
                maskMaterial = CoreUtils.CreateEngineMaterial(settings.maskShader);
            }
            else
            {
                //Debug.LogWarningFormat("Missing mask Shader");
                maskMaterial = CoreUtils.CreateEngineMaterial("Hidden/CrossSectionURP/FaceSideMask");
            }
            //maskMaterial = CoreUtils.CreateEngineMaterial("Hidden/CrossSectionURP/FaceSideMask");
            maskPass = new MaskPass(RenderQueueRange.opaque, settings.layerMask, maskMaterial);
            maskPass.renderPassEvent = settings.renderPassEvent;
            maskTexture.Init("_EdgeMap");
        }

        // Here you can inject one or multiple render passes in the renderer.
        // This method is called when setting up the renderer once per-camera.
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            maskPass.Setup(maskTexture);
            renderer.EnqueuePass(maskPass);
        }
    }
}


