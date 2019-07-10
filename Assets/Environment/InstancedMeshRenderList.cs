using UnityEngine;
using UnityEngine.Rendering;
namespace RenderTools {
    
    public class InstancedMeshRenderList {
        const int maxRender = 1023;
        const int maxStack = 8;

        Mesh mesh;
        Material[] materials;
        Matrix4x4[][] instances;
        int count;
        bool receiveShadows, castShadows;
        public InstancedMeshRenderList(Mesh mesh, Material[] materials, bool receiveShadows, bool castShadows) {
            this.mesh = mesh;
            this.materials = materials;
            
            this.receiveShadows = receiveShadows;
            this.castShadows = castShadows;
            
            instances = new Matrix4x4[maxStack][];
            
            ResetList();
        }

        public void AddInstance (Matrix4x4 instance) {
            int stack = count/maxRender;
            if (stack < maxStack) {
                if (instances[stack] == null) instances[stack] = new Matrix4x4[maxRender];
                int stacksOffset = maxRender * stack;
                instances[stack][count - stacksOffset] = instance;
            }
            count++;
        }

        public void ResetList () {
            count = 0;
        }

        static void DrawMesh (
            Mesh mesh, Material[] materials, Matrix4x4[] instances, int count, 
            ShadowCastingMode shadowMode=ShadowCastingMode.On, bool receiveShadows=true,  MaterialPropertyBlock properties=null, int layer=0, Camera camera=null, LightProbeUsage lightProbes = LightProbeUsage.Off
        ) {
            for (int i = 0; i < materials.Length; i++) {
                Graphics.DrawMeshInstanced(mesh, i, materials[i], instances, count, properties, shadowMode, receiveShadows, layer, camera, lightProbes);
            }
        }

        public void Render () {
            ShadowCastingMode shadows = castShadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
            int c = count;
            int stack = 0;
            
            while (c > 0) {
                DrawMesh (mesh, materials, instances[stack], Mathf.Min(count, maxRender), shadows, receiveShadows);
                c -= maxRender;
                stack++;
                if (stack >= maxStack) {
                    break;
                }
            }
        }
    }
}