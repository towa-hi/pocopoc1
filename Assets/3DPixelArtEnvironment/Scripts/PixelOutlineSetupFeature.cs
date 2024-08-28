using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// Credit https://www.cyanilux.com/tutorials/custom-renderer-features/#fullscreen
namespace Environment
{
    public class PixelOutlineSetupFeature : ScriptableRendererFeature // There is alot of useless stuff here currently but the overhead is minimal and its easy to expand from this template if needed
    {
        public class PixelOutlineSetupPass : ScriptableRenderPass
        {
            private ProfilingSampler _profilingSampler;

            public PixelOutlineSetupPass(string name)
            {
                _profilingSampler = new ProfilingSampler(name);
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                CommandBuffer cmd = CommandBufferPool.Get();
                // Set up profiling scope for Profiler & Frame Debugger
                using (new ProfilingScope(cmd, _profilingSampler))
                {
                    // Command buffer shouldn't contain anything, but apparently need to
                    // execute so DrawRenderers call is put under profiling scope title correctly
                    context.ExecuteCommandBuffer(cmd);
                    cmd.Clear();
                }
                // Execute Command Buffer one last time and release it
                // (otherwise we get weird recursive list in Frame Debugger)
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                CommandBufferPool.Release(cmd);
            }
        }

        // Exposed Settings

        [System.Serializable]
        public class Settings
        {
            public RenderPassEvent _event = RenderPassEvent.AfterRenderingOpaques;
        }

        public Settings settings = new Settings();
        private PixelOutlineSetupPass m_ScriptablePass;

        public override void Create()
        {
            m_ScriptablePass = new PixelOutlineSetupPass(name);
            m_ScriptablePass.renderPassEvent = settings._event;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            // Textures used to calculate outlines
            m_ScriptablePass.ConfigureInput(ScriptableRenderPassInput.Depth | ScriptableRenderPassInput.Normal | ScriptableRenderPassInput.Color);
            renderer.EnqueuePass(m_ScriptablePass);
        }
    }
}