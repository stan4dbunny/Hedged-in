using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VolumetricRenderPassFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        //future settings
        public Material material;
        public Texture3D CloudShapeTexture;
        public Texture3D CloudDetailTexture;
        public Texture2D BlueNoise;
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        //public Vector3 BoundsMin = new Vector3(0, 1000, 0);
        //public Vector3 BoundsMax = new Vector3(1000, 2000, 1000);
        public Vector3 CloudOffset = new Vector3(0, 0, 0);
        public Vector4 ShapeNoiseWeights = new Vector4(0.5f, 0.5f, 0.5f, 0.5f);
        public Vector4 DetailNoiseWeights = new Vector4(0.5f, 0.5f, 0.5f, 0.5f);
        public Vector2 PhaseParams = new Vector2(0.5f ,0.5f);
        [Range(0,1)] public float PhaseWeight = 0.5f;
        [Range(0,50)] public float CloudsScale = 1;
        [Range(0,5)] public float DensityMultiplier = 0.4f;
        [Range(0,1)] public float LightAbsorptionTowardSun = 0.25f;
        [Range(0,1)] public float LightAbsorptionThroughClouds = 0.15f;
        //[Range(0,1)] public float DarknessThreshold = 0.1f;
        [Range(0,1)] public float TransmittanceCutoff = 0.01f;
        [Range(0,100)] public int NumSteps = 15;
        [Range(0,100)] public int NumStepsLight = 10;
    }

    public Settings settings = new Settings();

    class Pass : ScriptableRenderPass
    {
        public Settings settings;
        private RenderTargetIdentifier source;
        RenderTargetHandle tempTexture;

        private string profilerTag;

        public void Setup(RenderTargetIdentifier source)
        {
            this.source = source;
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
            CommandBuffer cmd = CommandBufferPool.Get(profilerTag);
            cmd.Clear();

            //it is very important that if something fails our code still calls 
            //CommandBufferPool.Release(cmd) or we will have a HUGE memory leak
            try
            {
                settings.material.SetTexture("ShapeNoise", settings.CloudShapeTexture);
                settings.material.SetTexture("DetailNoise", settings.CloudDetailTexture);
                settings.material.SetTexture("BlueNouse", settings.BlueNoise);
                //settings.material.SetVector("BoundsMin", settings.BoundsMin);
                //settings.material.SetVector("BoundsMax", settings.BoundsMax);
                settings.material.SetVector("CloudOffset", settings.CloudOffset);
                settings.material.SetVector("PhaseParams", settings.PhaseParams);
                settings.material.SetVector("ShapeNoiseWeights", settings.ShapeNoiseWeights);
                settings.material.SetVector("DetailNoiseWeights", settings.DetailNoiseWeights);
                settings.material.SetFloat("PhaseWeight", settings.PhaseWeight);
                settings.material.SetFloat("CloudScale", settings.CloudsScale);
                settings.material.SetFloat("DensityMultiplier", settings.DensityMultiplier);
                settings.material.SetFloat("LightAbsorptionTowardSun", settings.LightAbsorptionTowardSun);
                settings.material.SetFloat("LightAbsorptionThroughClouds", settings.LightAbsorptionThroughClouds);
                //settings.material.SetFloat("DarknessThreshold", settings.DarknessThreshold);
                settings.material.SetFloat("TransmittanceCutoff", settings.TransmittanceCutoff);
                settings.material.SetInt("NumSteps", settings.NumSteps);
                settings.material.SetInt("NumStepsLight", settings.NumStepsLight);


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
    RenderTargetHandle renderTextureHandle;
    public override void Create()
    {
        pass = new Pass("Volumetric Cloud");
        name = "Volumetric Cloud";
        pass.settings = settings;
        pass.renderPassEvent = settings.renderPassEvent;
    }

    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        var cameraColorTargetIdent = renderer.cameraColorTarget;
        pass.Setup(cameraColorTargetIdent);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(pass);
    }
}


