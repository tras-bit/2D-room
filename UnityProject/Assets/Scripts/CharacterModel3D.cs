using UnityEngine;

namespace Subsistence
{
    /// <summary>Simple volumetric side-view survivor rig. The 2D rigidbody remains the movement authority.</summary>
    public sealed class CharacterModel3D : MonoBehaviour
    {
        Transform visual;
        Transform leftArm,rightArm,leftLeg,rightLeg;
        GameObject helmet,vest,pack,hazmat;
        Material skin,cloth,jacket,pants,boot,leather,armor,black,eye;
        float runAmount,airAmount,attackAmount,threatAmount,phase;
        int facing=1;
        bool built;

        void Awake(){Build();}
        public void Build()
        {
            if(built)return;built=true;RuntimeArtFactory.Initialize();
            visual=new GameObject("3D side-view character").transform;visual.SetParent(transform,false);
            skin=Mat(new Color(.65f,.48f,.35f),.08f,0);cloth=Mat(new Color(.18f,.21f,.20f),.05f,0);
            jacket=Mat(new Color(.34f,.39f,.27f),.08f,0);pants=Mat(new Color(.28f,.31f,.27f),.06f,0);
            boot=Mat(new Color(.24f,.17f,.12f),.15f,.05f);leather=Mat(new Color(.30f,.23f,.16f),.05f,0);
            armor=Mat(new Color(.25f,.28f,.26f),.22f,.24f);black=Mat(new Color(.055f,.065f,.06f),.18f,.06f);
            eye=Mat(new Color(.12f,.085f,.06f),.06f,0);
            RuntimeArtFactory.Capsule(visual,"Work trousers",new Vector3(0,.70f,0),new Vector3(.31f,.31f,.28f),pants);
            Transform torso=RuntimeArtFactory.Capsule(visual,"Field jacket",new Vector3(0,1.20f,0),new Vector3(.40f,.40f,.33f),jacket).transform;
            RuntimeArtFactory.Cube(visual,"Jacket front panel",new Vector3(0,1.19f,-.168f),new Vector3(.26f,.48f,.025f),leather);
            RuntimeArtFactory.Cube(visual,"Zipper",new Vector3(-.012f,1.19f,-.185f),new Vector3(.012f,.38f,.009f),trimMaterial());
            RuntimeArtFactory.Cube(visual,"Left pocket",new Vector3(-.13f,1.10f,-.188f),new Vector3(.10f,.10f,.025f),jacket);
            RuntimeArtFactory.Cube(visual,"Right pocket",new Vector3(.13f,1.10f,-.188f),new Vector3(.10f,.10f,.025f),jacket);
            RuntimeArtFactory.Capsule(visual,"Neck",new Vector3(0,1.54f,0),new Vector3(.12f,.12f,.12f),skin);
            RuntimeArtFactory.Sphere(visual,"Head",new Vector3(0,1.75f,0),new Vector3(.32f,.37f,.30f),skin);
            RuntimeArtFactory.Capsule(visual,"Hair",new Vector3(0,1.91f,.015f),new Vector3(.31f,.105f,.29f),black);
            RuntimeArtFactory.Cube(visual,"Ear",new Vector3(.16f,1.73f,-.005f),new Vector3(.055f,.09f,.09f),skin);
            RuntimeArtFactory.Sphere(visual,"Nose",new Vector3(0,1.71f,-.145f),new Vector3(.075f,.085f,.08f),skin);
            RuntimeArtFactory.Sphere(visual,"Eye",new Vector3(-.075f,1.79f,-.145f),new Vector3(.035f,.04f,.022f),eye);
            RuntimeArtFactory.Sphere(visual,"Eye",new Vector3(.075f,1.79f,-.145f),new Vector3(.035f,.04f,.022f),eye);
            RuntimeArtFactory.Cube(visual,"Scarf",new Vector3(0,1.52f,-.05f),new Vector3(.31f,.11f,.27f),leather);
            // Arms and legs pivot at the shoulder/hip so even low-poly clothing has a readable gait.
            leftArm=MakeLimb("Left arm",-.27f,1.39f,jacket,.19f,.34f);
            rightArm=MakeLimb("Right arm",.27f,1.39f,jacket,.19f,.34f);
            leftLeg=MakeLimb("Left leg",-.14f,.82f,pants,.25f,.34f);
            rightLeg=MakeLimb("Right leg",.14f,.82f,pants,.25f,.34f);
            RuntimeArtFactory.Cube(visual,"Left boot",new Vector3(-.16f,.13f,-.035f),new Vector3(.22f,.19f,.34f),boot);
            RuntimeArtFactory.Cube(visual,"Right boot",new Vector3(.16f,.13f,-.035f),new Vector3(.22f,.19f,.34f),boot);
            pack=RuntimeArtFactory.Cube(visual,"Canvas backpack",new Vector3(0,1.15f,.25f),new Vector3(.40f,.55f,.24f),leather);
            RuntimeArtFactory.Cube(pack.transform,"Pack flap",new Vector3(0,.10f,.51f),new Vector3(.27f,.20f,.04f),jacket);
            helmet=RuntimeArtFactory.Cube(visual,"Steel helmet",new Vector3(0,1.97f,0),new Vector3(.40f,.16f,.35f),RuntimeArtFactory.Metal);
            RuntimeArtFactory.Cube(helmet.transform,"Helmet brim",new Vector3(0,-.42f,-.08f),new Vector3(.52f,.06f,.41f),RuntimeArtFactory.Metal);
            RuntimeArtFactory.Sphere(helmet.transform,"Helmet lamp",new Vector3(0,-.1f,-.48f),new Vector3(.12f,.10f,.08f),RuntimeArtFactory.Fluorescent);
            vest=RuntimeArtFactory.Cube(visual,"Armoured vest",new Vector3(0,1.22f,-.07f),new Vector3(.49f,.54f,.34f),armor);
            RuntimeArtFactory.Cube(vest.transform,"Vest plate",new Vector3(0,.02f,-.53f),new Vector3(.32f,.32f,.08f),RuntimeArtFactory.Metal);
            hazmat=RuntimeArtFactory.Capsule(visual,"Hazmat outer suit",new Vector3(0,1.18f,-.015f),new Vector3(.44f,.43f,.36f),Mat(new Color(.70f,.62f,.20f),.04f,0));
            helmet.SetActive(false);vest.SetActive(false);pack.SetActive(true);hazmat.SetActive(false);
            SetGear(null);
        }
        Material trimMaterial(){return RuntimeArtFactory.Trim;}
        Transform MakeLimb(string name,float x,float y,Material mat,float width,float length)
        {
            var pivot=new GameObject(name+" pivot").transform;pivot.SetParent(visual,false);pivot.localPosition=new Vector3(x,y,0);
            RuntimeArtFactory.Capsule(pivot,name+" upper",new Vector3(0,-length*.24f,0),new Vector3(width,length*.25f,width*.86f),mat);
            RuntimeArtFactory.Sphere(pivot,name+" joint",new Vector3(0,-length*.47f,0),new Vector3(width*.78f,width*.8f,width*.78f),skin);
            RuntimeArtFactory.Capsule(pivot,name+" lower",new Vector3(0,-length*.73f,0),new Vector3(width*.82f,length*.23f,width*.78f),mat);
            return pivot;
        }
        static Material Mat(Color c,float smooth,float metal)
        {
            Shader s=Shader.Find("Standard");var m=new Material(s){color=c};if(m.HasProperty("_Glossiness"))m.SetFloat("_Glossiness",smooth);if(m.HasProperty("_Metallic"))m.SetFloat("_Metallic",metal);return m;
        }
        public void SetGear(ItemStack[] gear)
        {
            if(!built){Build();return;}bool hasHelmet=false,hasVest=false,hasPack=false,isHazmat=false;
            if(gear!=null&&gear.Length>=InventorySystem.GearSize)
            {
                hasHelmet=gear[(int)GearSlot.Head].id==ItemId.Helmet;
                hasVest=gear[(int)GearSlot.Chest].id==ItemId.ArmorVest;
                isHazmat=gear[(int)GearSlot.Chest].id==ItemId.HazmatSuit;
                hasPack=gear[(int)GearSlot.Back].id==ItemId.Backpack;
            }
            helmet.SetActive(hasHelmet);vest.SetActive(hasVest);pack.SetActive(hasPack);hazmat.SetActive(isHazmat);
        }
        public void SetMotion(float speed,bool grounded,bool jumping,int direction,bool threat=false)
        {runAmount=Mathf.Clamp01(speed);airAmount=grounded?0:1;facing=direction;threatAmount=Mathf.MoveTowards(threatAmount,threat?1:0,Time.deltaTime*2.2f);}
        public void Attack(){attackAmount=1;}
        void Update()
        {
            if(!built)return;phase+=Time.deltaTime*(5f+runAmount*8f);
            visual.localScale=new Vector3(facing>=0?1:-1,1,1);
            float swing=Mathf.Sin(phase)*runAmount*.68f;
            leftArm.localRotation=Quaternion.Euler(0,0,swing+attackAmount*48f);
            rightArm.localRotation=Quaternion.Euler(0,0,-swing+attackAmount*12f);
            leftLeg.localRotation=Quaternion.Euler(0,0,-swing*.8f);rightLeg.localRotation=Quaternion.Euler(0,0,swing*.8f);
            visual.localPosition=new Vector3(0,Mathf.Abs(Mathf.Sin(phase*2))*runAmount*.035f,0);
            attackAmount=Mathf.MoveTowards(attackAmount,0,Time.deltaTime*2.4f);
        }
        public void SetHostileAppearance()
        {
            skin.color=new Color(.31f,.34f,.29f);jacket.color=new Color(.20f,.22f,.18f);pants.color=new Color(.15f,.17f,.15f);eye.color=new Color(.66f,.075f,.025f);
            if(eye.HasProperty("_EmissionColor")){eye.EnableKeyword("_EMISSION");eye.SetColor("_EmissionColor",new Color(.75f,.045f,.012f));}
        }
        public void SetThreat(bool value){threatAmount=Mathf.MoveTowards(threatAmount,value?1:0,Time.deltaTime*2);}
    }
}
