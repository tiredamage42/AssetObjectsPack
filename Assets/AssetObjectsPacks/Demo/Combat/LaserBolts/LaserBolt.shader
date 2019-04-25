Shader "Hidden/LaserBolt"
{
    Properties {
        _TintColor("Tint Color", Color) = (0.5, 0.5, 0.5, 0.5)
        _AlphaSteepness("_AlphaSteepness", float) = 1
        _ColorSteepness("_ColorSteepness", float) = 1
    }
    CGINCLUDE
    #include "UnityCG.cginc"
    fixed4 _TintColor;
    fixed _AlphaSteepness, _ColorSteepness;
    struct appdata {
        float4 vertex : POSITION;
        float2 uv : TEXCOORD0;
    };
    struct v2f {
        float2 uv : TEXCOORD0;
        UNITY_FOG_COORDS(1)
        float4 vertex : SV_POSITION;
    };
    v2f vert (appdata v) {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.uv = v.uv;
        UNITY_TRANSFER_FOG(o,o.vertex);
        return o;
    }      
    fixed4 frag(v2f i) : SV_Target {
        fixed t = abs(0.5 - i.uv.y) / 0.5;
        fixed4 col = fixed4(lerp(fixed3(1.0,1.0,1.0), _TintColor.rgb, pow(t, _ColorSteepness)), 1.0 - pow(t, _AlphaSteepness));
        // apply fog
        UNITY_APPLY_FOG(i.fogCoord, col);
        return col;
    }
    ENDCG

    SubShader {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Lighting Off 
        Pass{
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_particles
            #pragma multi_compile_fog
            ENDCG
        }
    }
}
