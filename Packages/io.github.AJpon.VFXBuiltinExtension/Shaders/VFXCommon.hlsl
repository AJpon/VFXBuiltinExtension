#include "AutoLight.cginc"

// Texture2D _CameraDepthTexture;

half VFXComputeForwardShadows(float2 lightmapUV, float3 worldPos, float4 screenPos){
    return UnityComputeForwardShadows(lightmapUV, worldPos, screenPos);
}

