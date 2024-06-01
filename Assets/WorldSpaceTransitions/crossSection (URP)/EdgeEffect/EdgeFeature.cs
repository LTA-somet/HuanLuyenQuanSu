using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace WorldSpaceTransitions
{
    public class EdgeFeature : ScriptableRendererFeature
    {
        class EdgePass : ScriptableRenderPass
        {
            private RenderTargetIdentifier source { get; set; }
            private RenderTargetHandle destination { get; set; }
            public Material edgeMaterial = null;
            RenderTargetHandle temporaryColorTexture;

            public void Setup(RenderTargetIdentifier source, RenderTargetHandle destination)
            {
                this.source = source;
                this.destination = destination;
            }

            public EdgePass(Material edgeMaterial)
            {
                this.edgeMaterial = edgeMaterial;
            }



            // This method is called before executing the render pass.
            // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
            // When empty this render pass will render to the active camera render target.
            // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
            // The render pipeline will ensure target setup and clearing happens in an performance manner.
            public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
            {

            }

            // Here you can implement the rendering logic.
            // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
            // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
            // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                CommandBuffer cmd = CommandBufferPool.Get("Edge Pass");

                RenderTextureDescriptor opaqueDescriptor = renderingData.cameraData.cameraTargetDescriptor;
                opaqueDescriptor.depthBufferBits = 0;

                if (destination == RenderTargetHandle.CameraTarget)
                {
                    cmd.GetTemporaryRT(temporaryColorTexture.id, opaqueDescriptor, FilterMode.Point);
                    Blit(cmd, source, temporaryColorTexture.Identifier(), edgeMaterial, 0);
                    Blit(cmd, temporaryColorTexture.Identifier(), source);

                }
                else Blit(cmd, source, destination.Identifier(), edgeMaterial, 0);

                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }

            /// Cleanup any allocated resources that were created during the execution of this render pass.
            public override void FrameCleanup(CommandBuffer cmd)
            {

                if (destination == RenderTargetHandle.CameraTarget)
                    cmd.ReleaseTemporaryRT(temporaryColorTexture.id);
            }
        }

        [System.Serializable]
        public class EdgeSettings
        {
            public Material edgeMaterial = null;
        }

        public EdgeSettings settings = new EdgeSettings();
        EdgePass edgePass;
        RenderTargetHandle edgeTexture;

        public override void Create()
        {
            edgePass = new EdgePass(settings.edgeMaterial);
            edgePass.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
            edgeTexture.Init("_EdgeTexture");
        }

        // Here you can inject one or multiple render passes in the renderer.
        // This method is called when setting up the renderer once per-camera.
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (settings.edgeMaterial == null)
            {
                Debug.LogWarningFormat("Missing Edge Material");
                return;
            }
            edgePass.Setup(renderer.cameraColorTarget, RenderTargetHandle.CameraTarget);
            renderer.EnqueuePass(edgePass);
        }
    }
}


