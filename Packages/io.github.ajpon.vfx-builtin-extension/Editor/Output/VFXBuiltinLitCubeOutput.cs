using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.VFX.Block;
using UnityEngine;
using UnityEngine.VFX;
using UnityEditor.VFX;

namespace UnityEditor.VFX.BuiltinExtension
{
    [VFXInfo]
    class VFXBuiltinLitCubeOutput : VFXAbstractParticleOutput
    {
        public override string name { get { return "Output Particle Lit Cube"; } }
        public override string codeGeneratorTemplate { get { return VFXBuiltinExtensionContext.BuiltinExtensionTemplate("VFXParticleLitCube"); } }
        public override VFXTaskType taskType { get { return VFXTaskType.ParticleHexahedronOutput; } }

        public override bool supportsUV { get { return true; } }
        public override bool implementsMotionVector { get { return true; } }

        public override CullMode defaultCullMode { get { return CullMode.Back; } }

        [VFXSetting(VFXSettingAttribute.VisibleFlags.InInspector), SerializeField, Tooltip("When enabled, particles will receive shadows.")]
        protected bool receiveShadows = true;
        public override bool hasShadowCasting { get { return (castShadows || receiveShadows); } }

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

            yield return slotExpressions.First(o => o.name == "mainTexture");
            // yield return slotExpressions.First(o => o.name == "smoothnesss");
            // yield return slotExpressions.First(o => o.name == "metallic");
        }

        protected override IEnumerable<VFXPropertyWithValue> inputProperties
        {
            get
            {
                var properties = base.inputProperties;
                properties = PropertiesFromType("LitInputProperties").Concat(properties);
                properties = PropertiesFromType("StandardProperties").Concat(properties);
                foreach (var input in properties)
                    yield return input;

                yield return new VFXPropertyWithValue(new VFXProperty(GetFlipbookType(), "mainTexture", new TooltipAttribute("Specifies the base color (RGB) and opacity (A) of the particle.")), (usesFlipbook ? null : VFXResources.defaultResources.particleTexture));
            }
        }

        protected override IEnumerable<string> filteredOutSettings
        {
            get
            {
                foreach (var setting in base.filteredOutSettings)
                    yield return setting;

                yield return "colorMapping";
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
    }
}
