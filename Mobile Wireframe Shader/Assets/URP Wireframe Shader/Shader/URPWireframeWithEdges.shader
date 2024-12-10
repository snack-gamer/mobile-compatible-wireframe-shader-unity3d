Shader "Custom/URPWireframeWithEdges_Mobile_V2"
{
    Properties
    {
        [MainTexture] _MainTex ("Main Texture", 2D) = "white" {}
        [MainColor] _MainColor ("Main Color", Color) = (1,1,1,0.5)
        _EdgeColor ("Wireframe Edge Color", Color) = (0,0,0,1)
        _EdgeWidth ("Wireframe Edge Width", Range(0, 2)) = 1
        _EdgeThreshold ("Wireframe Edge Threshold", Range(0, 1)) = 0.1
        
        // Texture edge detection properties
        _TextureEdgeThreshold ("Texture Edge Threshold", Range(0.0, 1.0)) = 0.2
        _TextureEdgeSharpness ("Texture Edge Sharpness", Range(1.0, 64.0)) = 32.0
        _TextureEdgeColor ("Texture Edge Color", Color) = (0,0,0,1)
        
        // Color correction properties
        _Brightness ("Brightness", Range(0, 2)) = 1
        _Contrast ("Contrast", Range(0, 2)) = 1
        _Saturation ("Saturation", Range(0, 2)) = 1
        _Hue ("Hue Shift", Range(0, 1)) = 0
        
        // Simple lighting
        _AmbientStrength ("Ambient Light Strength", Range(0, 1)) = 0.1
        
        // Depth testing control
        [Toggle] _ZWrite ("ZWrite", Float) = 1
        _ZTest ("ZTest", Float) = 4
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
        }

        Pass
        {
            Name "EdgeDetectionPass"
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite [_ZWrite]
            ZTest [_ZTest]
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 barycentricCoords : TEXCOORD0;
                float2 uv : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _MainColor;
                float4 _EdgeColor;
                float _EdgeWidth;
                float _EdgeThreshold;
                float _Brightness;
                float _Contrast;
                float _Saturation;
                float _Hue;
                float _TextureEdgeThreshold;
                float _TextureEdgeSharpness;
                float4 _TextureEdgeColor;
                float _AmbientStrength;
            CBUFFER_END

            float3 RGBToHSV(float3 rgb)
            {
                float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
                float4 p = lerp(float4(rgb.bg, K.wz), float4(rgb.gb, K.xy), step(rgb.b, rgb.g));
                float4 q = lerp(float4(p.xyw, rgb.r), float4(rgb.r, p.yzx), step(p.x, rgb.r));
                float d = q.x - min(q.w, q.y);
                float e = 1e-10;
                return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
            }

            float3 HSVToRGB(float3 hsv)
            {
                float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
                float3 p = abs(frac(hsv.xxx + K.xyz) * 6.0 - K.www);
                return hsv.z * lerp(K.xxx, saturate(p - K.xxx), hsv.y);
            }

            float3 ApplyColorCorrection(float3 color)
            {
                float3 hsv = RGBToHSV(color);
                hsv.x = frac(hsv.x + _Hue);
                hsv.y *= _Saturation;
                float3 rgb = HSVToRGB(hsv);
                rgb = (rgb - 0.5) * _Contrast + 0.5;
                rgb *= _Brightness;
                return saturate(rgb);
            }

            float sigmoid(float a, float f)
            {
                return 1.0 / (1.0 + exp(-f * a));
            }

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normal);
                output.barycentricCoords = input.color.rgb;
                
                // Properly transform UVs using the _MainTex_ST property
                output.uv = input.texcoord * _MainTex_ST.xy + _MainTex_ST.zw;
                
                return output;
            }

            float calculateWireframe(float3 barycentric, float width)
            {
                float3 derivative = fwidth(barycentric);
                float3 smoothed = smoothstep(float3(0,0,0), derivative * width, barycentric);
                return 1.0 - min(min(smoothed.x, smoothed.y), smoothed.z);
            }

            float4 frag(Varyings input) : SV_Target
            {
                // Sample main texture
                float4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                
                // Simple lighting
                Light mainLight = GetMainLight();
                float NdotL = saturate(dot(normalize(input.normalWS), mainLight.direction));
                float3 lighting = lerp(_AmbientStrength, 1.0, NdotL) * mainLight.color;
                
                // Wireframe edge detection
                float wireframeEdge = calculateWireframe(input.barycentricCoords, _EdgeWidth);
                wireframeEdge = wireframeEdge > (1.0 - _EdgeThreshold) ? 1.0 : 0.0;
                
                // Texture edge detection
                float3 ddxCol = ddx_fine(texColor.rgb);
                float3 ddyCol = ddy_fine(texColor.rgb);
                float textureDelta = length(ddxCol) + length(ddyCol);
                float textureEdge = sigmoid(textureDelta - _TextureEdgeThreshold, _TextureEdgeSharpness);
                
                // Apply color correction to texture color
                float3 correctedTexColor = ApplyColorCorrection(texColor.rgb);
                
                // Blend texture with main color for faces
                float4 faceColor = float4(correctedTexColor * _MainColor.rgb, texColor.a * _MainColor.a);
                
                // Apply lighting to face color
                faceColor.rgb *= lighting;
                
                // Combine wireframe and texture edges
                float combinedEdge = max(wireframeEdge, textureEdge);
                float4 edgeColor = lerp(_EdgeColor, _TextureEdgeColor, textureEdge > wireframeEdge ? 1 : 0);
                
                // Final color blend
                float4 finalColor = lerp(faceColor, edgeColor, combinedEdge);
                
                // Keep edges fully opaque while faces use blended alpha
                finalColor.a = combinedEdge > 0 ? edgeColor.a : faceColor.a;
                
                return finalColor;
            }
            ENDHLSL
        }
    }
}