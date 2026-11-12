using UnityEngine;
using UnityEngine.UI;
namespace Balik {
// Vertex-only metallic lighting: Turkish lettering remains live, crisp UI text.
public sealed class StudioGoldText : BaseMeshEffect {
    public float sweep=-1;
    public override void ModifyMesh(VertexHelper mesh){
        if(!IsActive())return;
        var bounds=((RectTransform)transform).rect;
        var vertex=new UIVertex();
        for(int i=0;i<mesh.currentVertCount;i++){
            mesh.PopulateUIVertex(ref vertex,i);
            float y=Mathf.InverseLerp(bounds.yMin,bounds.yMax,vertex.position.y);
            float x=Mathf.InverseLerp(bounds.xMin,bounds.xMax,vertex.position.x);
            Color shade=Color.Lerp(new Color(.60f,.34f,.10f),new Color(1,.91f,.65f),y);
            float glint=Mathf.Clamp01(1-Mathf.Abs(x-sweep)/.15f);
            shade=Color.Lerp(shade,new Color(1,.99f,.90f),glint*.9f);
            shade.a=vertex.color.a/255f;vertex.color=shade;mesh.SetUIVertex(vertex,i);
        }
    }
    public void SetSweep(float value){sweep=value;graphic.SetVerticesDirty();}
}
}
