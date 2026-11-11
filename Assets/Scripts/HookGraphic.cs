using UnityEngine;
using UnityEngine.UI;
namespace Balik {
public class HookGraphic : MaskableGraphic {
    protected override void OnPopulateMesh(VertexHelper vh){vh.Clear();
        void Segment(Vector2 a,Vector2 b,float thickness){Vector2 n=new Vector2(-(b-a).y,(b-a).x).normalized*thickness/2;int start=vh.currentVertCount;vh.AddVert(a-n,color,Vector2.zero);vh.AddVert(a+n,color,Vector2.zero);vh.AddVert(b+n,color,Vector2.zero);vh.AddVert(b-n,color,Vector2.zero);vh.AddTriangle(start,start+1,start+2);vh.AddTriangle(start,start+2,start+3);}
        Segment(new Vector2(8,16),new Vector2(8,-7),3.5f);
        for(int i=0;i<16;i++){float a=Mathf.PI*i/16,b=Mathf.PI*(i+1)/16;Segment(new Vector2(Mathf.Cos(a)*9-1,-7-Mathf.Sin(a)*9),new Vector2(Mathf.Cos(b)*9-1,-7-Mathf.Sin(b)*9),3.5f);}
        Segment(new Vector2(-10,-7),new Vector2(-9,1),3);Segment(new Vector2(-9,1),new Vector2(-5,-3),2);
        for(int i=0;i<16;i++){float a=Mathf.PI*2*i/16,b=Mathf.PI*2*(i+1)/16;Segment(new Vector2(8+Mathf.Cos(a)*3,18+Mathf.Sin(a)*3),new Vector2(8+Mathf.Cos(b)*3,18+Mathf.Sin(b)*3),2);}
        raycastTarget=false;
    }
}
}
