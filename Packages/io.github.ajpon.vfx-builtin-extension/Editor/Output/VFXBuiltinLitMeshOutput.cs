using System.Collections.Generic;
using System.Linq;
using UnityEditor.VFX.Block;
using UnityEngine;
using UnityEngine.VFX;

namespace UnityEditor.VFX.BuiltinExtension
{
    [VFXInfo]
    class VFXBuiltinLitMeshOutput : VFXShaderGraphParticleOutput, IVFXMultiMeshOutput
    {
        public override string name { get { return "Output Particle Lit Mesh"; } }
        public override string codeGeneratorTemplate { get { return VFXBuiltinExtensionContext.BuiltinExtensionTemplate("VFXParticleLitMeshes"); } }
        public override VFXTaskType taskType { get { return VFXTaskType.ParticleMeshOutput; } }
        public override bool supportsUV { get { return GetOrRefreshShaderGraphObject() == null; } }
        public override bool implementsMotionVector { get { return true; } }
        public override CullMode defaultCullMode { get { return CullMode.Back;  } }
        protected bool receiveShadows = true;
        public override bool hasShadowCasting { get { return (castShadows || receiveShadows); } }

        [VFXSetting(VFXSettingAttribute.VisibleFlags.InInspector), Range(1, 4), Tooltip("Specifies the number of different meshes (up to 4). Mesh per particle can be specified with the meshIndex attribute."), SerializeField]
        private uint MeshCount = 1;
        [VFXSetting(VFXSettingAttribute.VisibleFlags.InInspector), Tooltip("When enabled, screen space LOD is used to determine with meshIndex to use per particle."), SerializeField]
        private bool lod = false;
        public uint meshCount => HasStrips(true) ? 1 : MeshCount;

        public class LitInputProperties
        {
            [Range(0, 1), Tooltip("Controls the scale factor for the particle’s smoothness.")]
            public float smoothness = 0.5f;
        }

        public class StandardProperties
        {
            [Range(0, 1), Tooltip("Controls the scale factor for the particle’s metallicity.")]
            public float metallic = 0.0f;
        }
        public override VFXOutputUpdate.Features outputUpdateFeatures
        {
            get
            {
                VFXOutputUpdate.Features features = base.outputUpdateFeatures;
                if (!HasStrips(true)) // TODO make it compatible with strips
                {
                    if (MeshCount > 1)
                        features |= VFXOutputUpdate.Features.MultiMesh;
                    if (lod)
                        features |= VFXOutputUpdate.Features.LOD;
                    if (HasSorting() && VFXOutputUpdate.HasFeature(features, VFXOutputUpdate.Features.IndirectDraw))
                        features |= VFXOutputUpdate.Features.Sort;
                }
                return features;
            }
        }

        public override IEnumerable<VFXAttributeInfo> attributes
        {
            get
            {
                yield return new VFXAttributeInfo(VFXAttribute.Position, VFXAttributeMode.Read);
                yield return new VFXAttributeInfo(VFXAttribute.Color, VFXAttributeMode.Read);
                yield return new VFXAttributeInfo(VFXAttribute.Alpha, VFXAttributeMode.Read);
                yield return new VFXAttributeInfo(VFXAttribute.Alive, VFXAttributeMode.Read);
                yield return new VFXAttributeInfo(VFXAttribute.AxisX, VFXAttributeMode.Read);
                yield return new VFXAttributeInfo(VFXAttribute.AxisY, VFXAttributeMode.Read);
                yield return new VFXAttributeInfo(VFXAttribute.AxisZ, VFXAttributeMode.Read);
                yield return new VFXAttributeInfo(VFXAttribute.AngleX, VFXAttributeMode.Read);
                yield return new VFXAttributeInfo(VFXAttribute.AngleY, VFXAttributeMode.Read);
                yield return new VFXAttributeInfo(VFXAttribute.AngleZ, VFXAttributeMode.Read);
                yield return new VFXAttributeInfo(VFXAttribute.PivotX, VFXAttributeMode.Read);
                yield return new VFXAttributeInfo(VFXAttribute.PivotY, VFXAttributeMode.Read);
                yield return new VFXAttributeInfo(VFXAttribute.PivotZ, VFXAttributeMode.Read);

                yield return new VFXAttributeInfo(VFXAttribute.Size, VFXAttributeMode.Read);
                yield return new VFXAttributeInfo(VFXAttribute.ScaleX, VFXAttributeMode.Read);
                yield return new VFXAttributeInfo(VFXAttribute.ScaleY, VFXAttributeMode.Read);
                yield return new VFXAttributeInfo(VFXAttribute.ScaleZ, VFXAttributeMode.Read);

                if (usesFlipbook)
                    yield return new VFXAttributeInfo(VFXAttribute.TexIndex, VFXAttributeMode.Read);
            }
        }

        protected override IEnumerable<VFXNamedExpression> CollectGPUExpressions(IEnumerable<VFXNamedExpression> slotExpressions)
        {
            foreach (var exp in base.CollectGPUExpressions(slotExpressions))
                yield return exp;
            if (GetOrRefreshShaderGraphObject() == null)
                yield return slotExpressions.First(o => o.name == "mainTexture");
        }

        protected override IEnumerable<VFXPropertyWithValue> inputProperties
        {
            get
            {
                var properties = base.inputProperties;
                properties = PropertiesFromType("LitInputProperties").Concat(properties);
                properties = PropertiesFromType("StandardProperties").Concat(properties);
                foreach (var property in properties)
                    yield return property;

                foreach (var property in VFXMultiMeshHelper.GetInputProperties(MeshCount, outputUpdateFeatures))
                    yield return property;

                if (GetOrRefreshShaderGraphObject() == null)
                    foreach (var property in optionalInputProperties)
                        yield return property;
            }
        }

        protected override IEnumerable<string> filteredOutSettings
        {
            get
            {
                foreach (var s in base.filteredOutSettings)
                    yield return s;

                // TODO Add a experimental bool to setting attribute
                if (!VFXViewPreference.displayExperimentalOperator)
                {
                    yield return "MeshCount";
                    yield return "lod";
                }
            }
        }


        protected IEnumerable<VFXPropertyWithValue> optionalInputProperties
        {
            get
            {
                yield return new VFXPropertyWithValue(new VFXProperty(GetFlipbookType(), "mainTexture", new TooltipAttribute("Specifies the base color (RGB) and opacity (A) of the particle.")), (usesFlipbook ? null : VFXResources.defaultResources.particleTexture));
            }
        }

        /// <summary>
        /// Additional predefined macros
        /// </summary>
        public override IEnumerable<string> additionalDefines
        {
            get
            {
                var defines = base.additionalDefines;
                defines = defines.Concat(new string[] { "VFX_NEEDS_POSWS_INTERPOLATOR" });
                if (castShadows)
                    defines = defines.Concat(new string[] { "VFX_FEATURE_SHADOWCASTING_CAST" });
                if (receiveShadows)
                    defines = defines.Concat(new string[] { "VFX_FEATURE_SHADOWCASTING_RECEIVE" });
                return defines;
            }
        }

        public override VFXExpressionMapper GetExpressionMapper(VFXDeviceTarget target)
        {
            var mapper = base.GetExpressionMapper(target);

            switch (target)
            {
                case VFXDeviceTarget.CPU:
                {
                    foreach (var name in VFXMultiMeshHelper.GetCPUExpressionNames(MeshCount))
                        mapper.AddExpression(inputSlots.First(s => s.name == name).GetExpression(), name, -1);
                    break;
                }
                default:
                    break;
            }

            return mapper;
        }
    }
}