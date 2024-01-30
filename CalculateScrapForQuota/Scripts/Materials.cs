using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;
using P = CalculateScrapForQuota.Plugin;

namespace CalculateScrapForQuota.Scripts;

public static class Materials
{
    public static Material HighlightMaterial
    {
        get
        {
            if (HDRP_LIT_SHADER)
                return HighlightMaterialHDRP;
            if (SRP_SHADER)
                return HighlightMaterialSRP;
            else
                throw new();
        }
    }

    private static Shader HDRP_LIT_SHADER = Shader.Find("HDRP/Lit");
    private static Material _highlightMaterialHDRP;
    private static Material HighlightMaterialHDRP
    {
        get
        {
            if (_highlightMaterialHDRP != null) return _highlightMaterialHDRP;
            _highlightMaterialHDRP = new(HDRP_LIT_SHADER);
            _highlightMaterialHDRP.shader = HDRP_LIT_SHADER;
            // Set material to transparent
            _highlightMaterialHDRP.SetFloat("_SurfaceType", 1); // 0 is Opaque, 1 is Transparent
            // Blend Mode for HDRP - Alpha blending
            // Note: HDRP uses different numeric values. You may need to adjust these based on your specific needs.
            _highlightMaterialHDRP.SetFloat("_BlendMode", 0); // 0 is Alpha, 1 is Additive, etc.
            // Additional transparency settings
            _highlightMaterialHDRP.SetFloat("_AlphaCutoffEnable", 0); // Disable alpha cutoff
            _highlightMaterialHDRP.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            _highlightMaterialHDRP.SetFloat("_ZWrite", 0); // Disable ZWrite for transparency
            _highlightMaterialHDRP.SetFloat("_TransparentZWrite", 0);
            _highlightMaterialHDRP.SetFloat("_TransparentCullMode", (float)CullMode.Off);
            _highlightMaterialHDRP.SetFloat("_TransparentSortPriority", 0); // Adjust if needed
            _highlightMaterialHDRP.SetFloat("_CullModeForward", (float)CullMode.Off);
            return _highlightMaterialHDRP;
        }
    }
    private static Shader SRP_SHADER = Shader.Find("Standard");
    private static Material _highlightMaterialSRP;
    private static Material HighlightMaterialSRP
    {
        get
        {
            if (_highlightMaterialSRP != null) return _highlightMaterialSRP;
            _highlightMaterialSRP = new(SRP_SHADER);
            _highlightMaterialSRP.shader = SRP_SHADER;
            _highlightMaterialSRP.SetFloat("_Mode", 3);
            _highlightMaterialSRP.SetInt("_SrcBlend", (int)BlendMode.One);
            _highlightMaterialSRP.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            _highlightMaterialSRP.SetInt("_ZWrite", 0);
            _highlightMaterialSRP.DisableKeyword("_ALPHATEST_ON");
            _highlightMaterialSRP.EnableKeyword("_ALPHABLEND_ON");
            _highlightMaterialSRP.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            _highlightMaterialSRP.renderQueue = 3000;
            return _highlightMaterialSRP;
        }
    }
}