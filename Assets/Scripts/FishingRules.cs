using System;
using System.Collections.Generic;
namespace Balik {
    [Serializable] public class CatchDefinition {
        public string id, name, sheet; public float x,y,w,h,width,depthMin,depthMax,weight,difficulty; public int value;
        public bool IsCreature => sheet=="fish"||sheet=="boss"||sheet=="giants";
        public float Seconds => value<=0?0:Math.Max(1f, value / 45f);
        public bool Heavy => difficulty >= 1;
        public CatchDefinition(string id,string name,string sheet,float x,float y,float w,float h,float width,int value,float difficulty,float min,float max,float weight) {
            this.id=id;this.name=name;this.sheet=sheet;this.x=x;this.y=y;this.w=w;this.h=h;this.width=width;this.value=value;this.difficulty=difficulty;depthMin=min;depthMax=max;this.weight=weight;
        }
    }
    public static class FishingRules {
        public const float StartSeconds=120, MaxSeconds=180;
        public static float RewardTime(float time, float reward)=>Math.Min(MaxSeconds,time+reward);
        public static float GripAfter(float grip,float seconds,float difficulty)=>Math.Max(0,grip-seconds*(.19f+difficulty*.035f));
        public static readonly CatchDefinition[] Catalog={
            new CatchDefinition("gold","Altın balık","fish",285,88,280,195,78,60,0,310,450,15),
            new CatchDefinition("clown","Palyaço balığı","fish",945,65,335,225,90,90,0,330,510,14),
            new CatchDefinition("stripe","Çizgili mercan","fish",220,330,420,260,105,125,0,400,600,12),
            new CatchDefinition("purple","Mor mercan","fish",850,310,510,280,125,160,.4f,450,730,10),
            new CatchDefinition("tuna","Orkinos","fish",20,640,760,310,165,240,.7f,570,850,8),
            new CatchDefinition("snapper","Kırmızı mercan","fish",820,595,680,375,145,210,.5f,540,790,9),
            new CatchDefinition("shark","Köpek balığı","boss",15,20,1170,525,250,450,1.5f,740,1010,4),
            new CatchDefinition("octopus","Ahtapot","boss",160,665,885,640,185,360,1.25f,820,1050,4),
            new CatchDefinition("sword","Kılıç balığı","giants",95,10,1060,360,230,400,1.35f,640,920,3),
            new CatchDefinition("whale","Mavi balina","giants",0,440,1198,370,295,600,2f,880,1060,1.3f),
            new CatchDefinition("orca","Orka","giants",55,825,1020,480,240,500,1.7f,790,1030,2),
            new CatchDefinition("can","Konserve kutusu","trash",80,5,460,585,52,0,0,410,780,4),
            new CatchDefinition("sock","Eski çorap","trash",800,0,400,605,42,0,0,350,800,3),
            new CatchDefinition("shoe","Eski ayakkabı","trash",10,700,655,430,83,0,.25f,640,990,3),
            new CatchDefinition("bag","Çöp poşeti","trash",735,610,545,580,74,0,.6f,620,1020,3),
            new CatchDefinition("chest","Hazine sandığı","chest",65,30,1270,1010,140,650,1.8f,910,1050,2),
        };
        public static CatchDefinition Pick(Random random) {
            double total=0;foreach(var d in Catalog)total+=d.weight;
            double roll=random.NextDouble()*total;
            foreach(var d in Catalog){roll-=d.weight;if(roll<=0)return d;}return Catalog[0];
        }
    }
    public sealed class MapSchedule {
        public int Attempts {get;private set;}
        public bool Collected {get;private set;}
        public bool Poll(float elapsed) { if(Collected||Attempts>=2)return false; if(elapsed<(Attempts==0?18:48))return false;Attempts++;return true; }
        public void Collect(){Collected=true;}
        public void Reset(){Attempts=0;Collected=false;}
    }
}
