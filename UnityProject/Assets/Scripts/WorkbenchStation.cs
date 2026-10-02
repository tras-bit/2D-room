using UnityEngine;

namespace Subsistence
{
    public sealed class WorkbenchStation : MonoBehaviour
    {
        public int Tier{get;private set;}
        public string StationName{get;private set;}
        public bool PlayerPlaced{get;private set;}
        public void Initialize(int tier,string stationName,bool playerPlaced=false)
        {
            Tier=Mathf.Clamp(tier,1,3);StationName=stationName;PlayerPlaced=playerPlaced;
            var trigger=GetComponent<BoxCollider2D>();if(trigger==null)trigger=gameObject.AddComponent<BoxCollider2D>();
            trigger.isTrigger=true;if(trigger.size.x<1f)trigger.size=new Vector2(2.7f,1.65f);if(trigger.offset.y==0)trigger.offset=new Vector2(0,.82f);
            var renderer=GetComponent<SpriteRenderer>();if(renderer==null)renderer=gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite=PixelArtFactory.Workbench2D(Tier);renderer.sortingOrder=6;RendererLibrary2D.Configure(renderer);
        }
    }
}
