Shader "Custom/SceneTransition"
{
    Properties
    {
        _MainTex ("Pattern Texture (Kenney)", 2D) = "white" {}
        _Color ("Wipe Color", Color) = (0,0,0,1)
        _Cutoff ("Cutoff", Range(0,1)) = 0
        _SoftEdge ("Soft Edge", Range(0,0.1)) = 0.05
        _PatternScale ("Pattern Tile Scale", Float) = 10
        // Direction: (1,0)=left→right, (-1,1)=top-right→bottom-left, (0,-1)=top→bottom
        _WipeDir ("Wipe Direction (X,Y)", Vector) = (-1,1,0,0)
    }
    SubShader
    {
        Tags { "Queue"="Overlay" "RenderType"="Transparent" "IgnoreProjector"="True" }
        ZWrite Off
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _Color;
            float _Cutoff;
            float _SoftEdge;
            float _PatternScale;
            float4 _WipeDir;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // 1. Directional gradient across screen (0=start, 1=end of wipe)
                float2 dir = normalize(_WipeDir.xy);
                float gradient = dot(i.uv - 0.5, dir) * 0.5 + 0.5;

                // 2. Sample the tiling Kenney pattern
                float2 patternUV = i.uv * _PatternScale;
                float pattern = tex2D(_MainTex, patternUV).r;

                // 3. Combine: pattern adds local variation to the gradient edge
                float combined = gradient + (pattern - 0.5) * _SoftEdge * 10;

                // 4. Threshold against cutoff
                float alpha = smoothstep(_Cutoff - _SoftEdge, _Cutoff + _SoftEdge, combined);

                return fixed4(_Color.rgb, alpha);
            }
            ENDCG
        }
    }
}
