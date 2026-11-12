using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Random=UnityEngine.Random;
namespace Balik {
public sealed class FishingGame : MonoBehaviour {
    enum HookState { Ready, Sinking, Reeling, Fighting }
    class Target {public CatchDefinition def;public RectTransform rect;public float x,y,speed,phase,scale,ttl;public bool map,escaped;}
    class Bubble {public RectTransform rect;public float x,y,speed;}
    readonly List<Target> targets=new List<Target>(); readonly List<Bubble> bubbles=new List<Bubble>();
    readonly System.Random rng=new System.Random(); readonly MapSchedule schedule=new MapSchedule();
    RectTransform root,world,hud,overlay,boat,rod,hook,line,action,gripBar,gripFill,toastRoot;
    Text timerText,coinText,scoreText,actionText,hintText,toastText,gripText,recordText;
    Image buttonImage;
    Font font; AudioSource effects,voice,ambience,music; Dictionary<string,AudioClip> clips=new Dictionary<string,AudioClip>();
    HookState hookState; Target caught;
    float trafficRemaining; bool crowded;
    float width=720,height=1280,seconds=120,runElapsed,nextSpawn,hookX,hookY,grip,castWind,lastTap=-10,toastUntil,toastStart,buttonPulse,windClock,voiceCooldown;
    int coins,score,best,wind,toastPriority,runTarget; bool playing,paused,newRecord,low30,low10,warning,muted,testMode;
    const float DockY=245, FloorY=1080;
    Color cream=new Color(1,.95f,.76f),gold=new Color(1,.78f,.26f);
    public bool IsPlaying=>playing; public int Score=>score;
    void Awake(){testMode=Array.Exists(Environment.GetCommandLineArgs(),a=>a.StartsWith("--balik-"));if(testMode)Application.runInBackground=true;Application.targetFrameRate=60;Screen.orientation=ScreenOrientation.Portrait; best=PlayerPrefs.GetInt("balik.best",0);muted=PlayerPrefs.GetInt("balik.muted",0)==1;Build();ShowHome();}
    RectTransform Node(string name,Transform parent,float x,float y,float w,float h){var g=new GameObject(name,typeof(RectTransform));g.transform.SetParent(parent,false);var r=g.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=new Vector2(0,1);r.pivot=new Vector2(.5f,.5f);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);return r;}
    Image Pic(string name,Transform parent,Sprite s,float x,float y,float w,float h,Color? color=null){var r=Node(name,parent,x,y,w,h);var i=r.gameObject.AddComponent<Image>();i.sprite=s;i.color=color??Color.white;i.raycastTarget=false;return i;}
    Text Label(string name,Transform parent,string value,float x,float y,float w,float h,int size,Color? color=null){var r=Node(name,parent,x,y,w,h);var t=r.gameObject.AddComponent<Text>();t.font=font;t.text=value;t.fontSize=size;t.fontStyle=FontStyle.Bold;t.alignment=TextAnchor.MiddleCenter;t.color=color??cream;t.raycastTarget=false;t.horizontalOverflow=HorizontalWrapMode.Wrap;t.verticalOverflow=VerticalWrapMode.Truncate;var sh=r.gameObject.AddComponent<Shadow>();sh.effectColor=new Color(.02f,.08f,.10f,.85f);sh.effectDistance=new Vector2(1,-3);return t;}
    Image Panel(string name,Transform parent,float x,float y,float w,float h,bool teal=false,bool round=false){var i=Pic(name,parent,SeaArt.Panel(round,teal),x,y,w,h);i.type=round?Image.Type.Simple:Image.Type.Sliced;return i;}
    Button Button(string title,Transform parent,float x,float y,float w,float h,Action onClick,bool round=false){var p=Panel(title,parent,x,y,w,h,true,round);p.raycastTarget=true;var b=p.gameObject.AddComponent<Button>();b.targetGraphic=p;var c=b.colors;c.pressedColor=new Color(.65f,.8f,.8f);c.highlightedColor=Color.white;b.colors=c;b.onClick.AddListener(()=>onClick());Label(title+" label",p.transform,title,w*.5f,h*.5f,w-22,h-12,round?37:29);return b;}
    void Build(){font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var camera=new GameObject("Camera").AddComponent<Camera>();camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.01f,.08f,.15f);camera.gameObject.AddComponent<AudioListener>();
        var canvas=new GameObject("Fishing Canvas",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));canvas.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
        var scaler=canvas.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(720,1280);scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;scaler.matchWidthOrHeight=1;
        root=canvas.GetComponent<RectTransform>();width=1280f*Screen.width/Screen.height;height=1280;
        Pic("Ocean background",root,SeaArt.Whole("ocean"),width/2,height/2,width,height);
        world=Node("Ocean actors",root,0,0,width,height);world.pivot=new Vector2(0,1);
        for(int i=0;i<24;i++){var b=new Bubble{x=Random.Range(0,width),y=Random.Range(290,1250),speed=Random.Range(10,30)};b.rect=Pic("Bubble",world,SeaArt.Disc(),b.x,b.y,Random.Range(2f,7f),Random.Range(2f,7f),new Color(.6f,1,1,.25f)).rectTransform;bubbles.Add(b);}
        boat=Node("Boat assembly",world,width*.31f,190,330,150);
        Pic("Fisherman",boat,SeaArt.Crop("boat",190,510,400,510),220,47,105,135);
        rod=Pic("Rod",boat,SeaArt.Crop("boat",750,525,680,485),290,20,170,121).rectTransform;
        Pic("Hull",boat,SeaArt.Crop("boat",210,0,1100,505),165,82,330,151);
        line=Pic("Fishing line",world,null,(width*.31f+200),200,2,80,new Color(.83f,1,1,.8f)).rectTransform;
        hook=Node("Hook",world,(width*.31f+200),DockY,30,42);
        hook.gameObject.AddComponent<HookGraphic>().color=gold;
        hud=Node("HUD",root,0,0,width,height);hud.pivot=new Vector2(0,1);
        float safeTop=(Screen.height-Screen.safeArea.yMax)*1280f/Screen.height;
        float top=47+safeTop;
        var time=Panel("Clock panel",hud,width*.14f,top,width*.25f,76);timerText=Label("Time",time.transform,"02:00",width*.125f,39,width*.23f,64,34);
        var money=Panel("Money panel",hud,width*.465f,top,width*.36f,76);coinText=Label("Run score",money.transform,"SKOR 0",width*.18f,40,width*.33f,60,27,gold);
        Button("Ⅱ",hud,width-47,top,68,76,TogglePause);
        Button("×",hud,width*.76f,top,68,76,ConfirmExit);
        recordText=Label("Record",hud,"REKOR "+best,width-133,top+60,245,36,18);
        Label("Depth shallow",world,"10 m",width-34,380,65,26,16,new Color(.7f,1,1,.48f));
        Label("Depth mid",world,"30 m",width-34,660,65,26,16,new Color(.7f,1,1,.40f));
        Label("Depth deep",world,"60 m",width-34,950,65,26,16,new Color(.7f,1,1,.35f));
        float bottomInset=Screen.safeArea.y*1280f/Screen.height;
        var recordPanel=Panel("Record challenge",hud,158,1190-bottomInset,284,104);
        Label("Challenge title",recordPanel.transform,"REKOR AVI",142,24,250,28,19,gold);
        scoreText=Label("Challenge progress",recordPanel.transform,"REKORU GEÇ!",142,69,250,45,23);
        var act=Button("SAL",hud,width-109,1169-bottomInset,182,182,Press,true);action=act.GetComponent<RectTransform>();actionText=act.GetComponentInChildren<Text>();buttonImage=act.GetComponent<Image>();
        hintText=Label("Action hint",hud,"DOKUN • OLTAYI SAL",width-130,1058-bottomInset,245,42,18);
        gripBar=Node("Catch effort",hud,width/2,650,230,72);
        Panel("Grip frame",gripBar,115,46,230,28);
        gripFill=Pic("Grip fill",gripBar,SeaArt.Disc(),115,46,205,12,new Color(.2f,1,.7f)).rectTransform;
        gripText=Label("Catch prompt",gripBar,"ÇEK! ÇEK! ÇEK!",115,12,300,42,25);gripBar.gameObject.SetActive(false);
        toastRoot=Node("Animated message",hud,width/2,320,width-36,100);toastText=Label("Message",toastRoot,"",(width-36)/2,50,width-36,100,37,gold);
        new GameObject("EventSystem",typeof(EventSystem),typeof(StandaloneInputModule));
        effects=gameObject.AddComponent<AudioSource>();voice=gameObject.AddComponent<AudioSource>();effects.volume=.35f;voice.volume=.85f;
        ambience=gameObject.AddComponent<AudioSource>();ambience.clip=Resources.Load<AudioClip>("Audio/waves");ambience.loop=true;ambience.volume=.55f;ambience.mute=muted;ambience.Play();
        music=gameObject.AddComponent<AudioSource>();music.clip=Resources.Load<AudioClip>("Audio/background-music");music.loop=true;music.volume=.3f;music.mute=muted;music.Play();
        overlay=Node("Screens",root,0,0,width,height);overlay.pivot=new Vector2(0,1);
        for(int i=0;i<3;i++)Spawn(true);
        hookX=(width*.31f+200);hookY=DockY;UpdateHook();
    }
    IEnumerator Start(){
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        var args=Environment.GetCommandLineArgs();
        if(Array.IndexOf(args,"--balik-intro-preview")>=0){yield return StudioIntro(true);Application.Quit(0);yield break;}
        if(Array.IndexOf(args,"--balik-smoke")>=0){yield return null;yield return SmokeTest();yield return new WaitForEndOfFrame();Application.Quit(0);}
        if(Array.IndexOf(args,"--balik-mouth-preview")>=0){
            StartRun();var d=FishingRules.Catalog[6];
            var fish=new Target{def=d,x=width/2,y=600,scale=1,ttl=100,rect=Pic("Preview shark",world,SeaArt.Crop(d.sheet,d.x,d.y,d.w,d.h),width/2,600,250,250*d.h/d.w).rectTransform};
            targets.Add(fish);hookX=width/2;hookY=570;Catch(fish);actionText.text="ÇEK";paused=true;UpdateHook();
            yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot("/tmp/balik-mouth.png");yield return new WaitForSeconds(.5f);Application.Quit(0);
        }
        if(Array.IndexOf(args,"--balik-preview")>=0){StartRun();yield return new WaitForSeconds(1);paused=true;ScreenCapture.CaptureScreenshot("/tmp/balik-phone.png");yield return new WaitForSeconds(.5f);Application.Quit(0);}
#endif
        if(!testMode)yield return StudioIntro();
    }
    IEnumerator StudioIntro(bool preview=false){
        playing=false;hud.gameObject.SetActive(false);ClearOverlay();
        var sea=Pic("Cinematic sea",overlay,SeaArt.Whole("ocean"),width/2,height/2,width,height).rectTransform;
        var backdrop=Pic("Midnight veil",overlay,null,width/2,height/2,width,height,new Color(.003f,.014f,.035f,.91f));
        backdrop.raycastTarget=true;bool skip=false;
        backdrop.gameObject.AddComponent<Button>().onClick.AddListener(()=>skip=true);
        var content=Node("Cinematic titles",overlay,0,0,width,height);content.pivot=new Vector2(0,1);
        var all=content.gameObject.AddComponent<CanvasGroup>();all.blocksRaycasts=false;
        // Soft light behind the lettering, built from nested translucent discs.
        for(int i=18;i>0;i--)Pic("Underwater bloom",content,SeaArt.Disc(),width/2,560,150+i*26,150+i*26,new Color(.03f,.35f,.38f,.014f));
        var rays=new RectTransform[5];
        for(int i=0;i<rays.Length;i++){
            rays[i]=Pic("Light shaft",content,null,width*(.15f+i*.18f),350,12+i*5,1100,new Color(.18f,.62f,.67f,.025f)).rectTransform;
            rays[i].localRotation=Quaternion.Euler(0,0,-22+i*5);
        }
        var motes=new RectTransform[28];
        for(int i=0;i<motes.Length;i++)motes[i]=Pic("Gold dust",content,SeaArt.Disc(),width*(.07f+.86f*(i%7)/6f),230+i*30,2+i%3,2+i%3,new Color(1,.79f,.40f,.17f+i%4*.06f)).rectTransform;
        var studio=Node("Studio lockup",content,0,0,width,height);studio.pivot=new Vector2(0,1);
        var studioFade=studio.gameObject.AddComponent<CanvasGroup>();
        var serif=Resources.Load<Font>("Fonts/Lora");
        var monogram=Label("Studio seal",studio,"Y",width/2,365,140,130,93,gold);monogram.font=serif;monogram.fontStyle=FontStyle.Normal;
        var seal=Pic("Seal top",studio,null,width/2,292,46,2,gold).rectTransform;
        Pic("Seal bottom",studio,null,width/2,438,46,2,gold);
        var name=Label("Yener",studio,"Yener",width/2,570,width-60,155,108,gold);name.font=serif;name.fontStyle=FontStyle.Normal;
        var subtitle=Label("Game Studio",studio,"Game Studio",width/2,687,width-60,70,38,cream);subtitle.font=serif;subtitle.fontStyle=FontStyle.Normal;
        // Remove the gameplay drop shadow for refined title typography.
        foreach(var text in new[]{monogram,name,subtitle}){var shadow=text.GetComponent<Shadow>();if(shadow!=null)Destroy(shadow);}
        var goldName=name.gameObject.AddComponent<StudioGoldText>();
        var goldSeal=monogram.gameObject.AddComponent<StudioGoldText>();
        var rule=Pic("Unfolding gold line",content,null,width/2,770,1,2,new Color(.9f,.67f,.3f)).rectTransform;
        var series=Node("Series credit",content,0,0,width,height);series.pivot=new Vector2(0,1);
        var seriesFade=series.gameObject.AddComponent<CanvasGroup>();
        var seriesName=Label("Ayse Sena series",series,"Ayşe Senanın",width/2,835,width-60,75,39,cream);seriesName.font=serif;seriesName.fontStyle=FontStyle.Normal;
        var seriesDetail=Label("Series detail",series,"Oyun Serisi",width/2,899,width-60,55,25,new Color(.68f,.80f,.81f));seriesDetail.font=serif;seriesDetail.fontStyle=FontStyle.Normal;
        var prompt=Label("Skip intro",content,"GEÇMEK İÇİN DOKUN",width/2,1160,width-60,40,14,new Color(.43f,.58f,.62f));
        var curtain=Pic("Final cinematic fade",overlay,null,width/2,height/2,width,height,new Color(.003f,.014f,.035f,0));
        bool captured=false;
        for(float elapsed=0;elapsed<6.8f;elapsed+=Time.unscaledDeltaTime){
            all.alpha=Mathf.SmoothStep(0,1,elapsed/.9f);
            studioFade.alpha=Mathf.SmoothStep(0,1,(elapsed-.5f)/1.3f);
            studio.anchoredPosition=new Vector2(0,-18*(1-Mathf.SmoothStep(0,1,(elapsed-.5f)/2f)));
            name.rectTransform.localScale=Vector3.one*Mathf.Lerp(1.065f,1,Mathf.Clamp01(elapsed/4.8f));
            sea.localScale=Vector3.one*(1.03f+elapsed*.004f);
            seriesFade.alpha=Mathf.SmoothStep(0,1,(elapsed-2.65f)/1.2f);
            series.anchoredPosition=new Vector2(0,-12*(1-seriesFade.alpha));
            rule.sizeDelta=new Vector2(230*Mathf.SmoothStep(0,1,(elapsed-2.1f)/1.2f),2);
            goldName.SetSweep((elapsed-1.6f)/2f);goldSeal.SetSweep((elapsed-.7f)/1.6f);
            prompt.gameObject.SetActive(elapsed>2.4f);
            for(int i=0;i<motes.Length;i++)motes[i].anchoredPosition=new Vector2(width*(.07f+.86f*(i%7)/6f)+Mathf.Sin(elapsed*.35f+i)*15,-(230+i*30)+elapsed*(5+i%5));
            for(int i=0;i<rays.Length;i++)rays[i].localRotation=Quaternion.Euler(0,0,-22+i*5+Mathf.Sin(elapsed*.3f+i)*3);
            curtain.color=new Color(.003f,.014f,.035f,Mathf.SmoothStep(0,1,(elapsed-6f)/.8f));
            if(preview&&!captured&&elapsed>=4.2f){yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot("/tmp/balik-intro.png");captured=true;}
            if(skip&&elapsed>1.2f)break;
            yield return null;
        }
        ShowHome();if(preview)Debug.Log("BALIK_INTRO_PASS: cinematic title sequence completed and home opened");
    }
    void ClearOverlay(){foreach(Transform c in overlay)Destroy(c.gameObject);overlay.gameObject.SetActive(true);}
    void Scrim(float alpha=.76f){Pic("Shade",overlay,null,width/2,height/2,width,height,new Color(.005f,.04f,.09f,alpha)).raycastTarget=true;}
    void ShowHome(){SaveBest();paused=false;playing=false;hud.gameObject.SetActive(false);ClearOverlay();Scrim(.35f);
        Button("×",overlay,width-50,60+(Screen.height-Screen.safeArea.yMax)*1280f/Screen.height,68,70,ConfirmExit);
        Label("Kicker",overlay,"BİR DENİZ MACERASI",width/2,310,width-40,45,23,gold);
        Label("Title",overlay,"DERİNLİKLERİN\nHAZİNESİ",width/2,420,width-30,190,60);
        Pic("Treasure",overlay,SeaArt.Whole("chest"),width/2,650,230,173);
        Label("Tagline",overlay,"Oltanı sal. Büyük avı yakala.\nSüre dolmadan rekorunu kır!",width/2,805,width-70,95,27);
        Button("MACERAYA BAŞLA",overlay,width/2,940,380,100,StartRun);
        Label("Best",overlay,"EN YÜKSEK SKOR  •  "+best,width/2,1030,width-40,45,24,gold);
        Label("Guide",overlay,"Tek düğme: SAL / ÇEK\nBüyük avlarda art arda dokun!",width/2,1123,width-40,72,22);
        Button(muted?"SES KAPALI":"SES AÇIK",overlay,width/2,1212,235,58,()=>{ToggleSound();ShowHome();});
    }
    public void StartRun(){foreach(var t in targets)Destroy(t.rect.gameObject);targets.Clear();seconds=120;score=coins=0;runTarget=best+1;runElapsed=0;schedule.Reset();newRecord=low30=low10=paused=false;playing=true;hookState=HookState.Ready;caught=null;hookX=(width*.31f+200);hookY=DockY;crowded=false;trafficRemaining=Random.Range(8f,14f);nextSpawn=Random.Range(2f,4f);toastPriority=0;toastUntil=0;overlay.gameObject.SetActive(false);hud.gameObject.SetActive(true);ChooseWind();for(int i=0;i<5;i++)Spawn(true);EnsureFishPresence();UpdateHud();Notify("RASTGELE!",gold,2,1,"start");}
    bool exitWasPaused;
    void ConfirmExit(){
        exitWasPaused=paused;paused=true;SaveBest();ClearOverlay();Scrim();
        Label("Exit title",overlay,"OYUNDAN ÇIK?",width/2,440,width-40,90,42,gold);
        Label("Exit copy",overlay,"En yüksek skorun kaydedildi.",width/2,550,width-50,75,25);
        Button("OYUNDAN ÇIK",overlay,width/2,700,340,90,QuitGame);
        Button("ANA MENÜ",overlay,width/2,815,300,80,ShowHome);
        Button("VAZGEÇ",overlay,width/2,925,300,80,CancelExit);
    }
    void CancelExit(){paused=false;if(playing){overlay.gameObject.SetActive(false);if(exitWasPaused)TogglePause();}else ShowHome();}
    void QuitGame(){SaveBest();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying=false;
#else
        Application.Quit();
#endif
    }
    void ToggleSound(){muted=!muted;PlayerPrefs.SetInt("balik.muted",muted?1:0);PlayerPrefs.Save();ambience.mute=muted;music.mute=muted;if(muted){voice.Stop();effects.Stop();}}
    void TogglePause(){if(!playing)return;paused=!paused;if(!paused){overlay.gameObject.SetActive(false);return;}ClearOverlay();Scrim();Label("Pause title",overlay,"MOLA ZAMANI",width/2,430,width-40,100,47,gold);Label("Pause copy",overlay,"Deniz seni bekler.\nSüren durduruldu.",width/2,550,width-50,100,28);Button("DEVAM ET",overlay,width/2,710,330,90,TogglePause);Button(muted?"SESİ AÇ":"SESİ KAPAT",overlay,width/2,825,330,75,()=>{ToggleSound();paused=false;TogglePause();});Button("OYUNDAN ÇIK",overlay,width/2,940,330,75,ConfirmExit);}
    void OnApplicationPause(bool p){if(p&&playing&&!paused)TogglePause();if(ambience!=null){if(p)ambience.Pause();else ambience.UnPause();}if(music!=null){if(p)music.Pause();else music.UnPause();}SaveBest();}
    void OnApplicationFocus(bool focus){if(!testMode&&!focus&&playing&&!paused)TogglePause();}
    void OnApplicationQuit(){SaveBest();}
    void SaveBest(){if(score>best){best=score;PlayerPrefs.SetInt("balik.best",best);PlayerPrefs.Save();}}
    void ChooseWind(){wind=Random.Range(-1,2);windClock=Random.Range(4f,8f);}
    public void Press(){if(!playing||paused)return;buttonPulse=1;Tone(560,.055f);
        if(hookState==HookState.Ready){castWind=wind;hookState=HookState.Sinking;hookX=(width*.31f+200);hookY=DockY;hintText.text="İSTEDİĞİN AN ÇEK";PlayWaterEffect();}
        else if(hookState==HookState.Fighting){if(Time.time-lastTap<.055f)return;lastTap=Time.time;grip=Mathf.Min(1,grip+.115f);hookY-=20f/(.75f+caught.def.difficulty*.25f);hintText.text="ÇEK! ÇEK! ÇEK!";}
        else hookState=HookState.Reeling;
        actionText.text=hookState==HookState.Ready?"SAL":"ÇEK";
    }
    void Update(){float dt=Mathf.Min(Time.deltaTime,.05f);
        if(!paused){foreach(var b in bubbles){b.y-=b.speed*dt;b.x+=Mathf.Sin(Time.time*.5f+b.speed)*dt*3;if(b.y<275)b.y=1270;b.rect.anchoredPosition=new Vector2(b.x,-b.y);}boat.localRotation=Quaternion.Euler(0,0,Mathf.Sin(Time.time*1.5f)*1.1f);}
        if(!playing){AnimateTargets(dt);return;}if(paused)return;
        seconds-=dt;runElapsed+=dt;voiceCooldown-=dt;windClock-=dt;if(hookState==HookState.Ready&&windClock<=0)ChooseWind();
        if(seconds<=0){seconds=0;EndRun();return;}
        if(seconds<=30&&!low30){low30=true;Notify("SÜREN AZALIYOR!",new Color(1,.6f,.2f),2.5f,3,"time");}
        if(seconds<=10&&!low10){low10=true;Notify("SON 10 SANİYE!",new Color(1,.3f,.2f),2,4,"last10");}
        if(seconds>35)low30=false;if(seconds>15)low10=false;
        trafficRemaining-=dt;
        if(trafficRemaining<=0){crowded=!crowded;trafficRemaining=crowded?Random.Range(8f,13f):Random.Range(8f,14f);nextSpawn=crowded?Random.Range(1f,2f):Random.Range(3f,5f);}
        nextSpawn-=dt;
        if(nextSpawn<=0){int limit=crowded?10:5;int count=crowded&&Random.value<.3f?2:1;for(int i=0;i<count&&targets.Count<limit;i++)Spawn(false);nextSpawn=crowded?Random.Range(1.3f,2.6f):Random.Range(3f,5f);}
        if(schedule.Poll(runElapsed)){SpawnMap();Notify("HARİTA PARÇASI!",gold,2,2,"map");}
        AnimateTargets(dt);
        if(hookState==HookState.Sinking){hookY=Mathf.Min(FloorY,hookY+155*dt);hookX=Mathf.Clamp(hookX+castWind*21*dt,70,width-70);if(hookY>=FloorY)hintText.text="DİBE ULAŞTIN • ÇEK";}
        if(hookState==HookState.Reeling)hookY-=dt*(caught==null?360:250/(1+caught.def.difficulty*.35f));
        if(hookState==HookState.Fighting){grip=FishingRules.GripAfter(grip,dt,caught.def.difficulty);if(Time.time-lastTap<.3f)hookY-=dt*38;
            if(grip<.3f){gripText.text="KAÇIYOR!";gripText.color=new Color(1,.35f,.15f);if(!warning){warning=true;PlayVoice("escaping");}}else{warning=false;gripText.text="ÇEK! ÇEK! ÇEK!";gripText.color=cream;}
            if(grip<=0)Escape();}
        TryCatch();
        if(caught!=null){caught.phase+=dt*14;PositionCaught();}
        if(hookY<=DockY&&hookState!=HookState.Ready)Land();EnsureFishPresence();UpdateHook();UpdateHud();
        buttonPulse=Mathf.Max(0,buttonPulse-dt*5);action.localScale=Vector3.one*(1-buttonPulse*.09f);
        if(Time.time<toastUntil){float t=Time.time-toastStart;float s=1+Mathf.Sin(Mathf.Min(1,t*4)*Mathf.PI)*.17f;toastRoot.localScale=Vector3.one*s;toastText.color=new Color(toastText.color.r,toastText.color.g,toastText.color.b,Mathf.Clamp01((toastUntil-Time.time)*3));}else toastText.text="";
    }
    void Spawn(bool initial,bool reserveFish=false){var d=FishingRules.Pick(rng);if(reserveFish)while(d.sheet!="fish")d=FishingRules.Pick(rng);bool right=Random.value>.5f;float scale=Random.Range(.85f,1.16f),w=d.width*scale;var t=new Target{def=d,x=initial?Random.Range(30,width-30):reserveFish?(right?0:width):(right?-w:width+w),y=Random.Range(d.depthMin,d.depthMax),speed=Random.Range(25,70)*(right?1:-1),phase=Random.Range(0f,6f),scale=scale,ttl=1000};t.rect=Pic(d.name,world,SeaArt.Crop(d.sheet,d.x,d.y,d.w,d.h),t.x,t.y,w,w*d.h/d.w).rectTransform;t.rect.localScale=new Vector3(right?1:-1,1,1);targets.Add(t);hook.SetAsLastSibling();}
    void EnsureFishPresence(){
        // Keep two free fish in view, replacing outgoing fish before they leave.
        // Trash, maps, escaped and hooked targets do not satisfy this reserve.
        int remaining=0;
        foreach(var t in targets){
            if(t==caught||t.escaped||t.def.sheet!="fish"||t.ttl<=2)continue;
            float futureX=t.x+t.speed*2,halfWidth=t.rect.rect.width*.5f;
            if(t.x+halfWidth>0&&t.x-halfWidth<width&&futureX+halfWidth>0&&futureX-halfWidth<width)remaining++;
        }
        while(remaining++<2)Spawn(false,true);
    }
    void SpawnMap(){
        var d=new CatchDefinition("map","Harita parçası","",0,0,1,1,65,180,.4f,550,750,0);
        // Enter fully outside the screen and cross with the current; never teleport into play.
        int direction=wind==0?(Random.value<.5f?-1:1):wind;
        const float margin=60;
        float crossingSeconds=Random.Range(18f,22f);
        var t=new Target{def=d,map=true,x=direction>0?-margin:width+margin,
            y=Random.Range(510f,770f),speed=direction*(width+2*margin)/crossingSeconds,
            phase=Random.Range(0f,6f),ttl=crossingSeconds+1,scale=1};
        t.rect=Pic("Catchable parchment map",world,SeaArt.Whole("map-fragment"),t.x,t.y,86,90).rectTransform;
        targets.Add(t);
    }
    void AnimateTargets(float dt){if(paused)return;for(int i=targets.Count-1;i>=0;i--){var t=targets[i];if(t==caught)continue;t.x+=t.speed*dt;t.phase+=dt*2;t.ttl-=dt;t.rect.anchoredPosition=new Vector2(t.x,-t.y+Mathf.Sin(t.phase)*5);t.rect.localRotation=Quaternion.Euler(0,0,Mathf.Sin(t.phase)*2);if(t.x< -(t.map?60:350)||t.x>width+(t.map?60:350)||t.ttl<=0){Destroy(t.rect.gameObject);targets.RemoveAt(i);}}}
    void Catch(Target t){caught=t;t.phase=0;PositionCaught();hook.SetAsLastSibling();grip=1;warning=false;lastTap=Time.time;hookState=t.def.Heavy?HookState.Fighting:HookState.Reeling;t.rect.SetAsLastSibling();hook.SetAsLastSibling();gripBar.gameObject.SetActive(t.def.Heavy);hintText.text=t.def.Heavy?"HIZLI HIZLI DOKUN!":"AVIN YUKARI GELİYOR";Notify(t.def.Heavy?"ÇEK! ÇEK! ÇEK!":"YAKALADIN!",cream,1.2f,1,null);PlayWaterEffect();}
    void Escape(){var t=caught;caught=null;t.rect.localRotation=Quaternion.identity;t.escaped=true;t.speed=Mathf.Sign(t.speed==0?1:t.speed)*260;t.ttl=2;hookState=HookState.Reeling;gripBar.gameObject.SetActive(false);Notify("KAÇTI!",new Color(1,.5f,.3f),1.8f,2,"escaped");hintText.text="BOŞ OLTAYI ÇEK";}
    void Land(){if(caught!=null){var d=caught.def;int value=Mathf.RoundToInt(d.value*caught.scale);float reward=value<=0?0:Mathf.Max(1,value/45f);float added=FishingRules.RewardTime(seconds,reward)-seconds;seconds+=added;coins+=value;score+=value;if(caught.map)schedule.Collect();
            StartCoroutine(RewardFloat("+"+value+"   +"+added.ToString("0.#")+" SN",new Vector2(hookX,DockY+50)));
            if(!newRecord&&score>best){newRecord=true;Notify("YENİ REKOR!",gold,2,3,"record");}
            else Notify(d.id=="can"?"TENEKE ÇIKTI!":d.id=="sock"?"BU DA NE?":d.id=="shoe"?"TEKİ NEREDE?":d.id=="bag"?"ÇÖP ÇIKTI!":d.Heavy?"BÜYÜK VURGUN!":"GÜZEL AV!",gold,1.7f,1,d.Heavy?"big":"reward");
            SaveBest();targets.Remove(caught);Destroy(caught.rect.gameObject);caught=null;Tone(1050,.18f);
        }
        hookState=HookState.Ready;hookX=(width*.31f+200);hookY=DockY;gripBar.gameObject.SetActive(false);hintText.text="DOKUN • OLTAYI SAL";actionText.text="SAL";}
    Vector2 RodTip(){
        // Last guide ring at source-sheet pixel (1402,549), inside crop (750,525,680,485).
        Vector3 local=new Vector3(((1402f-750)/680-.5f)*rod.rect.width,(.5f-(549f-525)/485)*rod.rect.height,0);
        return world.InverseTransformPoint(rod.TransformPoint(local));
    }
    void LateUpdate(){if(hook!=null){UpdateHook();if(caught!=null)PositionCaught();}}
    Vector2 MouthLocal(Target t){
        // Mouth landmarks in the original sprite sheets (top-left pixel coordinates).
        Vector2 pixel;
        switch(t.def.id){
            case "gold":pixel=new Vector2(540,198);break;
            case "clown":pixel=new Vector2(1250,196);break;
            case "stripe":pixel=new Vector2(617,478);break;
            case "purple":pixel=new Vector2(1337,472);break;
            case "tuna":pixel=new Vector2(753,811);break;
            case "snapper":pixel=new Vector2(1475,817);break;
            case "shark":pixel=new Vector2(1060,361);break;
            case "octopus":pixel=new Vector2(655,930);break;
            case "sword":pixel=new Vector2(810,228);break;
            case "whale":pixel=new Vector2(1150,550);break;
            case "orca":pixel=new Vector2(1030,1125);break;
            default:return Vector2.zero;
        }
        return new Vector2(((pixel.x-t.def.x)/t.def.w-.5f)*t.rect.rect.width,
            (.5f-(pixel.y-t.def.y)/t.def.h)*t.rect.rect.height);
    }
    Vector2 MouthPosition(Target t)=>world.InverseTransformPoint(t.rect.TransformPoint(MouthLocal(t)));
    Vector2 HookPoint()=>new Vector2(hookX-9,-hookY+1);
    void PositionCaught(){
        var t=caught;
        if(t.def.IsCreature){
            float facing=Mathf.Sign(t.rect.localScale.x);
            float hang=t.def.id=="octopus"?0:70*facing;
            t.rect.localRotation=Quaternion.Euler(0,0,hang+Mathf.Sin(t.phase)*18+Mathf.Sin(t.phase*2.3f)*5);
            Vector2 offset=world.InverseTransformVector(t.rect.TransformVector(MouthLocal(t)));
            t.rect.anchoredPosition=HookPoint()-offset;
        }else{
            t.rect.localRotation=Quaternion.Euler(0,0,Mathf.Sin(t.phase*.5f)*5);
            t.rect.anchoredPosition=new Vector2(hookX,-hookY-14);
        }
        t.x=t.rect.anchoredPosition.x;t.y=-t.rect.anchoredPosition.y;
    }
    void TryCatch(){
        // Only the descending hook tip can catch; creatures require head/mouth contact.
        if(caught!=null||hookState!=HookState.Sinking)return;
        foreach(var t in targets){
            if(t.ttl<=0||t.escaped)continue;
            Vector2 p=t.def.IsCreature?MouthPosition(t):t.rect.anchoredPosition;
            float rx=t.def.IsCreature?Mathf.Clamp(t.rect.rect.width*.16f,19,38):t.rect.rect.width*.35f+10;
            float ry=t.def.IsCreature?Mathf.Clamp(t.rect.rect.height*.28f,19,38):t.rect.rect.height*.32f+12;
            Vector2 delta=HookPoint()-p;
            if(delta.x*delta.x/(rx*rx)+delta.y*delta.y/(ry*ry)<1){Catch(t);break;}
        }
    }
    void PlayWaterEffect(){if(!muted){var clip=Resources.Load<AudioClip>("Audio/water-catch");if(clip!=null)effects.PlayOneShot(clip,1.3f);}}
    void UpdateHook(){
        if(hookState==HookState.Ready)hookX=RodTip().x-8;
        hook.anchoredPosition=new Vector2(hookX,-hookY);
        Vector2 tip=RodTip();
        Vector2 end=world.InverseTransformPoint(hook.TransformPoint(new Vector3(8,18,0)));
        Vector2 delta=end-tip;line.anchoredPosition=(tip+end)*.5f;line.sizeDelta=new Vector2(1.7f,delta.magnitude);line.localRotation=Quaternion.Euler(0,0,Mathf.Atan2(delta.y,delta.x)*Mathf.Rad2Deg-90);
        if(caught!=null&&caught.def.Heavy){gripBar.anchoredPosition=new Vector2(Mathf.Clamp(hookX,140,width-140),-Mathf.Clamp(hookY-75,340,1020));gripFill.sizeDelta=new Vector2(205*grip,12);gripFill.GetComponent<Image>().color=grip<.3f?new Color(1,.25f,.12f):new Color(.2f,1,.7f);}}
    void UpdateHud(){int s=Mathf.CeilToInt(seconds);timerText.text=$"{s/60:00}:{s%60:00}";timerText.color=seconds<=10?Color.Lerp(cream,new Color(1,.25f,.12f),.5f+.5f*Mathf.Sin(Time.time*8)):cream;coinText.text="SKOR "+score;scoreText.text=score>=runTarget?"YENİ REKOR!":"HEDEF "+runTarget;recordText.text="REKOR "+best;}

    void Notify(string text,Color color,float duration,int priority,string audio=null){if(Time.time<toastUntil&&priority<toastPriority)return;toastPriority=priority;toastStart=Time.time;toastUntil=Time.time+duration;toastText.text=text;toastText.color=color;if(audio!=null)PlayVoice(audio);}
    IEnumerator RewardFloat(string text,Vector2 p){var label=Label("Reward",hud,text,p.x,p.y,360,60,29,new Color(.6f,1,.65f));for(float t=0;t<1.4f;t+=Time.deltaTime){if(paused){yield return null;continue;}label.rectTransform.anchoredPosition=new Vector2(Mathf.Lerp(p.x,width/2,t/1.4f),-Mathf.Lerp(p.y,90,t/1.4f));yield return null;}Destroy(label.gameObject);}
    void EndRun(){playing=false;paused=false;SaveBest();gripBar.gameObject.SetActive(false);ClearOverlay();Scrim();Label("Result title",overlay,"SÜRE DOLDU!",width/2,360,width-30,150,51,gold);Pic("Result treasure",overlay,SeaArt.Whole("chest"),width/2,555,210,158);Label("Result score",overlay,"TOPLAM SKOR\n"+score,width/2,735,width-30,120,42);Label("Best score",overlay,(newRecord?"YENİ REKOR!  •  ":"EN YÜKSEK SKOR  •  ")+best,width/2,845,width-30,60,26,gold);Button("TEKRAR OYNA",overlay,width/2,1010,350,95,StartRun);Button("ANA MENÜ",overlay,width/2,1130,290,75,ShowHome);PlayVoice("end");}
    void PlayVoice(string name){if(name=="cast"||name=="pull")return;if(muted||voiceCooldown>0)return;var clip=Resources.Load<AudioClip>("Audio/"+name);if(clip==null)return;voice.Stop();voice.clip=clip;voice.Play();voiceCooldown=.6f;}
    void Tone(float frequency,float duration){if(muted)return;string key=frequency.ToString();if(!clips.TryGetValue(key,out var clip)){int n=(int)(22050*duration);float[] samples=new float[n];for(int i=0;i<n;i++)samples[i]=Mathf.Sin(i*frequency*2*Mathf.PI/22050)*Mathf.Sin(Mathf.PI*i/n)*.18f;clip=AudioClip.Create(key,n,1,22050,false);clip.SetData(samples,0);clips[key]=clip;}effects.PlayOneShot(clip);}
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public IEnumerator SmokeTest(){StartRun();yield return null;Press();if(hookState!=HookState.Sinking)throw new Exception("Cast state");Press();if(hookState!=HookState.Reeling)throw new Exception("Reel state");hookY=DockY;Land();
        foreach(var actor in targets)Destroy(actor.rect.gameObject);targets.Clear();
        foreach(int direction in new[]{-1,1}){wind=direction;SpawnMap();var map=targets[targets.Count-1];
            if(map.x>=-43&&map.x<=width+43)throw new Exception("Map appeared inside screen");
            if(Mathf.Sign(map.speed)!=direction)throw new Exception("Map ignored current direction");
            float halfCrossing=(width+120)/Mathf.Abs(map.speed)/2;AnimateTargets(halfCrossing);
            if(map.x<0||map.x>width||!targets.Contains(map))throw new Exception("Map did not drift through screen");
            AnimateTargets(halfCrossing+1);if(targets.Contains(map))throw new Exception("Map did not leave screen");}
        EnsureFishPresence();
        for(int tick=0;tick<2400;tick++){
            AnimateTargets(.05f);
            if(tick%41==0){var removed=targets.Find(a=>a.def.sheet=="fish");if(removed!=null){targets.Remove(removed);Destroy(removed.rect.gameObject);}}
            EnsureFishPresence();
            int visible=targets.FindAll(a=>a.def.sheet=="fish"&&!a.escaped&&a!=caught&&a.x+a.rect.rect.width*.5f>0&&a.x-a.rect.rect.width*.5f<width).Count;
            if(visible<2)throw new Exception("Fish reserve became empty");
        }
        foreach(var actor in targets)Destroy(actor.rect.gameObject);targets.Clear();
        foreach(var definition in FishingRules.Catalog){
            if(definition.IsCreature){foreach(int facing in new[]{-1,1}){
                var fish=new Target{def=definition,x=width/2,y=700,scale=1,ttl=100,
                    rect=Pic("Mouth test",world,SeaArt.Crop(definition.sheet,definition.x,definition.y,definition.w,definition.h),width/2,700,definition.width,definition.width*definition.h/definition.w).rectTransform};
                fish.rect.localScale=new Vector3(facing,1,1);targets.Add(fish);
                Vector2 tail=world.InverseTransformPoint(fish.rect.TransformPoint(new Vector2(-fish.rect.rect.width*.45f,0)));
                hookX=tail.x+9;hookY=1-tail.y;hookState=HookState.Sinking;TryCatch();
                if(caught!=null)throw new Exception("Tail contact caught "+definition.id);
                Vector2 mouthPoint=MouthPosition(fish);hookX=mouthPoint.x+9;hookY=1-mouthPoint.y-18;TryCatch();
                if(caught!=fish)throw new Exception("Mouth contact missed "+definition.id);
                for(int frame=0;frame<20;frame++){fish.phase=frame*.3f;PositionCaught();if(Vector2.Distance(MouthPosition(fish),HookPoint())>.1f)throw new Exception("Mouth detached "+definition.id);}
                caught=null;targets.Remove(fish);Destroy(fish.rect.gameObject);
            }}else if(definition.sheet=="trash"){
                var trash=new Target{def=definition,scale=1,ttl=100,rect=Pic("Trash test",world,null,200,700,60,60).rectTransform};targets.Add(trash);
                int oldScore=score,oldCoins=coins;float oldTime=seconds;Catch(trash);Land();
                if(score!=oldScore||coins!=oldCoins||seconds!=oldTime)throw new Exception("Trash awarded reward "+definition.id);
            }
        }
        var d=FishingRules.Catalog[6];var t=new Target{def=d,x=hookX,y=700,scale=1,ttl=100,rect=Pic("Test shark",world,SeaArt.Crop(d.sheet,d.x,d.y,d.w,d.h),hookX,700,200,90).rectTransform};targets.Add(t);Vector2 mouth=MouthPosition(t);hookX=mouth.x+9;hookY=1-mouth.y;hookState=HookState.Reeling;TryCatch();if(caught!=null)throw new Exception("Empty returning hook caught a target");hookState=HookState.Sinking;TryCatch();if(caught!=t)throw new Exception("Descending hook missed contact");
        foreach(float angle in new[]{-8f,0f,8f}){boat.localRotation=Quaternion.Euler(0,0,angle);UpdateHook();Vector2 endpoint=world.InverseTransformPoint(line.TransformPoint(new Vector3(0,-line.rect.height/2,0)));if(Vector2.Distance(endpoint,RodTip())>.1f)throw new Exception("Line detached from rod");}if(hookState!=HookState.Fighting)throw new Exception("Heavy catch");float oldY=hookY;lastTap=-10;Press();if(hookY>=oldY)throw new Exception("Tap progress");Escape();if(caught!=null)throw new Exception("Escape");hookY=DockY;Land();score=12000;coins=12000;Land();if(!playing)throw new Exception("Score prematurely ended run");score=coins=0;ConfirmExit();if(!paused||!playing)throw new Exception("Exit did not pause run");CancelExit();if(paused||!playing)throw new Exception("Exit cancel did not resume");seconds=0;Update();if(playing)throw new Exception("Timer did not end run");Debug.Log("BALIK_PLAYMODE_PASS: zero trash reward, all creature mouths and tail rejection in both directions, attachment during thrashing, continuous fish presence across 120 simulated seconds and repeated removals, cast, reel, no return catches, descending contact, map enters and exits with current in both directions, rod attachment at three angles, heavy tap, escape, score has no stage cap, exit cancellation, timed finish");ShowHome();}
#endif
}
}
