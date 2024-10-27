using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VREffectRenderPassFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        //future settings
        public Material material;
        
    }

    public Settings settings = new Settings();
    private bool isActive = false;

    public void SetActive(bool active)
    {
        Debug.Log("set active" + active);
        isActive = active;

    }

    class Pass : ScriptableRenderPass
    {
        public Settings settings;
        private RenderTargetIdentifier source;
        RenderTargetHandle tempTexture;
        private string profilerTag;
        private bool isActive;

        public void Setup(RenderTargetIdentifier source, bool isActive)
        {
            this.source = source;
            this.isActive = isActive;
        }

        public Pass(string profilerTag)
        {
            this.profilerTag = profilerTag;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            //R8 has noticeable banding
            cameraTextureDescriptor.colorFormat = RenderTextureFormat.Default;
            //we dont need to resolve AA in every single Blit
            cameraTextureDescriptor.msaaSamples = 1;

            cmd.GetTemporaryRT(tempTexture.id, cameraTextureDescriptor);
            ConfigureTarget(tempTexture.Identifier());
            ConfigureClear(ClearFlag.All, Color.black);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (!isActive ||!renderingData.cameraData.camera.CompareTag("MainCamera"))
            return;
            
            Debug.Log("execute");
            CommandBuffer cmd = CommandBufferPool.Get(profilerTag);
            cmd.Clear();


            //it is very important that if something fails our code still calls 
            //CommandBufferPool.Release(cmd) or we will have a HUGE memory leak
            try
            {
                //never use a Blit from source to source, as it only works with MSAA
                // enabled and the scene view doesnt have MSAA,
                // so the scene view will be pure black
                cmd.Blit(source, tempTexture.Identifier());
                cmd.Blit(tempTexture.Identifier(), source, settings.material, 0);

                context.ExecuteCommandBuffer(cmd);
            }
            catch
            {
                Debug.LogError("Error");
            }
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }
    }

    Pass pass;
    
    public override void Create()
    {
        pass = new Pass("VR Effects");
        name = "VR Effects";
        pass.settings = settings;
        pass.renderPassEvent = settings.renderPassEvent;
    }

    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        var cameraColorTargetIdent = renderer.cameraColorTarget;
        pass.Setup(cameraColorTargetIdent, isActive);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        
        renderer.EnqueuePass(pass);
         
    }
    
}
