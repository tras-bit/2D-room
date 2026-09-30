using UnityEngine;

namespace Subsistence
{
    public sealed class WorkbenchStation : MonoBehaviour
    {
        public int Tier { get; private set; }
        public string StationName { get; private set; }
        public void Initialize(int tier,string stationName)
        {
            Tier=Mathf.Clamp(tier,1,3);StationName=stationName;
            var trigger=gameObject.AddComponent<BoxCollider2D>();trigger.isTrigger=true;trigger.size=new Vector2(2.7f,1.65f);trigger.offset=new Vector2(0,.82f);
            var worktop=gameObject.AddComponent<BoxCollider2D>();worktop.isTrigger=false;worktop.size=new Vector2(2.3f,.88f);worktop.offset=new Vector2(0,.44f);
            RuntimeArtFactory.Initialize();var root=new GameObject("3D workbench · level "+Tier).transform;root.SetParent(transform,false);root.localPosition=new Vector3(0,0,2.7f);
            Material top=Tier==1?RuntimeArtFactory.Wood:RuntimeArtFactory.Metal;
            RuntimeArtFactory.Cube(root,"Workbench table",new Vector3(0,.87f,0),new Vector3(2.4f,.18f,1.15f),top);
            for(int x=-1;x<=1;x+=2)RuntimeArtFactory.Cube(root,"Workbench leg",new Vector3(x*.9f,.42f,0),new Vector3(.13f,.85f,.16f),RuntimeArtFactory.Metal);
            RuntimeArtFactory.Cube(root,"Tool tray",new Vector3(-.35f,1.03f,-.02f),new Vector3(.72f,.09f,.64f),RuntimeArtFactory.Rubber);
            RuntimeArtFactory.Cube(root,"Workbench vise",new Vector3(.66f,1.08f,-.05f),new Vector3(.24f,.20f,.32f),RuntimeArtFactory.Metal);
            RuntimeArtFactory.Cube(root,"Bench tier badge",new Vector3(0,1.15f,-.61f),new Vector3(.50f,.18f,.035f),RuntimeArtFactory.Concrete);
            var label=new GameObject("Station label").AddComponent<TextMesh>();label.transform.SetParent(root,false);label.transform.localPosition=new Vector3(0,1.15f,-.64f);label.transform.localRotation=Quaternion.Euler(0,180,0);label.text="BENCH  /  "+Tier;label.characterSize=.045f;label.fontSize=28;label.anchor=TextAnchor.MiddleCenter;label.alignment=TextAlignment.Center;label.color=new Color(.84f,.80f,.63f);
            RuntimeArtFactory.PointLight(root,"Bench lamp",new Vector3(0,2.2f,0),new Color(.90f,.70f,.38f),.35f,4);
        }
    }
}
