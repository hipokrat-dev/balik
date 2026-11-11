using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace Balik {
public static class SeaArt {
    static readonly Dictionary<string,Sprite> cache=new Dictionary<string,Sprite>();
    public static Sprite Crop(string sheet,float x,float y,float w,float h) {
        string key=$"{sheet}:{x}:{y}:{w}:{h}";
        if(cache.TryGetValue(key,out var s))return s;
        var t=Resources.Load<Texture2D>("Art/"+sheet);
        s=Sprite.Create(t,new Rect(x,t.height-y-h,w,h),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);cache[key]=s;return s;
    }
    public static Sprite Whole(string name) {var t=Resources.Load<Texture2D>("Art/"+name);return Crop(name,0,0,t.width,t.height);}
    public static Sprite Panel(bool round=false,bool teal=false) {
        string key="panel"+round+teal;if(cache.TryGetValue(key,out var result))return result;
        int w=round?256:256,h=round?256:96;var t=new Texture2D(w,h,TextureFormat.RGBA32,false);
        for(int y=0;y<h;y++)for(int x=0;x<w;x++) {
            float r=round?125:28;
            float qx=Mathf.Abs(x-w*.5f)-(w*.5f-r-2),qy=Mathf.Abs(y-h*.5f)-(h*.5f-r-2);
            float dist=Mathf.Sqrt(Mathf.Pow(Mathf.Max(qx,0),2)+Mathf.Pow(Mathf.Max(qy,0),2))+Mathf.Min(Mathf.Max(qx,qy),0)-r;
            Color c=Color.clear;
            if(dist<0){float v=(float)y/h;
                if(dist> -3)c=new Color(.30f,.15f,.035f);
                else if(dist> -8)c=Color.Lerp(new Color(.64f,.32f,.06f),new Color(1,.87f,.39f),v);
                else if(dist> -12)c=new Color(1,.76f,.27f);
                else if(dist> -16)c=new Color(.25f,.12f,.025f);
                else if(teal)c=Color.Lerp(new Color(.005f,.18f,.22f),new Color(.09f,.82f,.75f),v);
                else c=Color.Lerp(new Color(.10f,.055f,.025f),new Color(.31f,.19f,.085f),v);
                if(dist< -17){float grain=Mathf.Sin(y*.85f+x*.015f)*.018f;c+=new Color(grain,grain*.65f,grain*.2f,0);}
                c.a=Mathf.Clamp01(-dist);
            }t.SetPixel(x,y,c);
        }t.Apply();t.wrapMode=TextureWrapMode.Clamp;
        result=Sprite.Create(t,new Rect(0,0,w,h),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect,new Vector4(32,32,32,32));cache[key]=result;return result;
    }
    public static Sprite Disc() {
        if(cache.TryGetValue("disc",out var s))return s;var t=new Texture2D(64,64,TextureFormat.RGBA32,false);
        for(int y=0;y<64;y++)for(int x=0;x<64;x++){float d=Vector2.Distance(new Vector2(x,y),new Vector2(31.5f,31.5f));t.SetPixel(x,y,new Color(1,1,1,Mathf.Clamp01(31-d)));}t.Apply();
        return cache["disc"]=Sprite.Create(t,new Rect(0,0,64,64),new Vector2(.5f,.5f));
    }
}
}
