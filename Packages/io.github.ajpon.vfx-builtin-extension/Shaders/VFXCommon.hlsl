#include "UnityCG.cginc"
#include "AutoLight.cginc"

// Texture2D _CameraDepthTexture;
Texture2D _ShadowMapTexture;

// half VFXComputeForwardShadows(float2 lightmapUV, float3 worldPos, float4 screenPos){
//     return UnityComputeForwardShadows(lightmapUV, worldPos, screenPos);
// }


float VFXComputeForwardShadows(float4 posSS){
    float2 shadow = _ShadowMapTexture.Load(int3(posSS.xy, 0)).r;
    return shadow;
}

// float4 VFXComputeScreenPos(float4 vertex)
// {
//     return ComputeScreenPos(vertex);
// }

float VFXSampleDepthTest(float4 posSS)
{
    // return SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, int4(posSS.xy, 0, 0)).r;
    return _CameraDepthTexture.Load(int3(posSS.xy, 0)).r;
}

float invLerp(float from, float to, float value){
  return (value - from) / (to - from);
}

float remap(float origFrom, float origTo, float targetFrom, float targetTo, float value){
  float rel = invLerp(origFrom, origTo, value);
  return lerp(targetFrom, targetTo, rel);
}