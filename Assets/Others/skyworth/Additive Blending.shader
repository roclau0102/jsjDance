// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Blending-Additive" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
	}
	SubShader {
		LOD 200
		
        Pass
        {
		    Tags { "RenderType"="Transparent" "Queue"="Transparent" }
            ZWrite Off
            Cull Off
            Blend SrcAlpha One
            
		    CGPROGRAM
		    #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

		    sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;

            struct a2v
            {
                float4 vertex:POSITION;
                float4 color:COLOR;
                float4 texcoord:TEXCOORD0;
            };

            struct v2f
            {
                float4 pos:SV_POSITION;
                float4 color:COLOR;
                float2 uv:TEXCOORD0;
            };

            v2f vert(a2v i)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(i.vertex);
				o.color = i.color;
                o.uv = TRANSFORM_TEX(i.texcoord,_MainTex);
                return o;
            }

            float4 frag(v2f i):COLOR
            {
                float4 texColor;
                texColor = tex2D(_MainTex, i.uv);
                float4 fragColor;
                fragColor = texColor * _Color * i.color*2;
                return fragColor;
            }
		    
		    ENDCG
        }
	} 
	FallBack "Diffuse"
}
