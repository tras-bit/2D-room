using UnityEngine;

namespace Subsistence
{
    /// <summary>Simple readable survivor model with visible clothing and armour driven by equipped inventory items.</summary>
    public sealed class CharacterModel3D : MonoBehaviour
    {
        Transform visual,leftArm,rightArm,leftLeg,rightLeg;
        GameObject jacket,vest,helmet,backpack,hazmatSuit,hazmatHood,kneePadsLeft,kneePadsRight,workBootsLeft,workBootsRight;
        Renderer[] armClothing,legClothing;
        Material skin,shirt,jacketMaterial,pantsMaterial,fieldPantsMaterial,bootMaterial,workBootMaterial,vestMaterial,hazmatMaterial,hairMaterial,eyeMaterial;
        float runAmount,airAmount,attackAmount,threatAmount,phase;
        int facing=1;
        bool built;

        void Awake(){Build();}

        public void Build()
        {
            if(built)return;
            built=true;
            RuntimeArtFactory.Initialize();
            visual=new GameObject("Survivor model").transform;
            visual.SetParent(transform,false);

            skin=Mat(new Color(.62f,.45f,.32f),.06f,0);
            shirt=Mat(new Color(.36f,.38f,.34f),.04f,0);
            jacketMaterial=Mat(new Color(.34f,.39f,.27f),.05f,0);
            pantsMaterial=Mat(new Color(.20f,.22f,.21f),.03f,0);
            fieldPantsMaterial=Mat(new Color(.34f,.37f,.29f),.04f,0);
            bootMaterial=Mat(new Color(.11f,.12f,.11f),.16f,.02f);
            workBootMaterial=Mat(new Color(.29f,.22f,.15f),.18f,.04f);
            vestMaterial=Mat(new Color(.24f,.27f,.24f),.15f,.20f);
            hazmatMaterial=Mat(new Color(.71f,.61f,.19f),.04f,0);
            hairMaterial=Mat(new Color(.12f,.105f,.085f),.02f,0);
            eyeMaterial=Mat(new Color(.10f,.075f,.05f),.02f,0);

            // Plain human silhouette: shirt, trousers, simple head and four articulated limbs.
            RuntimeArtFactory.Capsule(visual,"Shirt",new Vector3(0,1.18f,0),new Vector3(.36f,.39f,.30f),shirt);
            RuntimeArtFactory.Capsule(visual,"Neck",new Vector3(0,1.51f,0),new Vector3(.11f,.12f,.11f),skin);
            RuntimeArtFactory.Sphere(visual,"Head",new Vector3(0,1.73f,0),new Vector3(.29f,.34f,.27f),skin);
            RuntimeArtFactory.Sphere(visual,"Short hair",new Vector3(0,1.91f,.015f),new Vector3(.29f,.13f,.27f),hairMaterial);
            RuntimeArtFactory.Sphere(visual,"Ear",new Vector3(-.16f,1.72f,-.025f),new Vector3(.055f,.085f,.065f),skin);
            RuntimeArtFactory.Sphere(visual,"Profile nose",new Vector3(.17f,1.70f,-.045f),new Vector3(.075f,.075f,.09f),skin);
            RuntimeArtFactory.Sphere(visual,"Visible eye",new Vector3(.065f,1.79f,-.126f),new Vector3(.031f,.035f,.022f),eyeMaterial);
            RuntimeArtFactory.Cube(visual,"Mouth",new Vector3(.105f,1.63f,-.132f),new Vector3(.065f,.012f,.012f),hairMaterial);

            leftArm=MakeLimb("Left arm",-.27f,1.38f,shirt,.15f,.34f,out Renderer[] leftArmParts);
            rightArm=MakeLimb("Right arm",.27f,1.38f,shirt,.15f,.34f,out Renderer[] rightArmParts);
            armClothing=new[]{leftArmParts[0],leftArmParts[1],rightArmParts[0],rightArmParts[1]};
            leftLeg=MakeLimb("Left leg",-.14f,.82f,pantsMaterial,.14f,.36f,out Renderer[] leftLegParts);
            rightLeg=MakeLimb("Right leg",.14f,.82f,pantsMaterial,.14f,.36f,out Renderer[] rightLegParts);
            legClothing=new[]{leftLegParts[0],leftLegParts[1],rightLegParts[0],rightLegParts[1]};

            // Clothing is layered over the basic figure. Equipping an item switches the corresponding layer/material.
            jacket=CreateGroup("Equipped field jacket");
            RuntimeArtFactory.Capsule(jacket.transform,"Jacket body",new Vector3(0,1.18f,-.012f),new Vector3(.395f,.415f,.33f),jacketMaterial);
            RuntimeArtFactory.Cube(jacket.transform,"Jacket zipper",new Vector3(0,1.18f,-.19f),new Vector3(.016f,.35f,.018f),RuntimeArtFactory.Trim);
            RuntimeArtFactory.Cube(jacket.transform,"Jacket left pocket",new Vector3(-.13f,1.09f,-.19f),new Vector3(.10f,.10f,.025f),jacketMaterial);
            RuntimeArtFactory.Cube(jacket.transform,"Jacket right pocket",new Vector3(.13f,1.09f,-.19f),new Vector3(.10f,.10f,.025f),jacketMaterial);

            vest=CreateGroup("Equipped armour vest");
            RuntimeArtFactory.Cube(vest.transform,"Vest body",new Vector3(0,1.19f,-.035f),new Vector3(.42f,.47f,.32f),vestMaterial);
            RuntimeArtFactory.Cube(vest.transform,"Front ballistic plate",new Vector3(0,1.20f,-.22f),new Vector3(.28f,.27f,.07f),RuntimeArtFactory.Metal);
            RuntimeArtFactory.Cube(vest.transform,"Chest strap",new Vector3(0,1.00f,-.22f),new Vector3(.35f,.045f,.045f),RuntimeArtFactory.Trim);
            RuntimeArtFactory.Cube(vest.transform,"Left shoulder strap",new Vector3(-.14f,1.39f,-.17f),new Vector3(.09f,.23f,.04f),vestMaterial);
            RuntimeArtFactory.Cube(vest.transform,"Right shoulder strap",new Vector3(.14f,1.39f,-.17f),new Vector3(.09f,.23f,.04f),vestMaterial);

            hazmatSuit=CreateGroup("Equipped hazmat suit");
            RuntimeArtFactory.Capsule(hazmatSuit.transform,"Hazmat outer layer",new Vector3(0,1.18f,-.022f),new Vector3(.41f,.43f,.35f),hazmatMaterial);
            RuntimeArtFactory.Cube(hazmatSuit.transform,"Hazmat front seam",new Vector3(0,1.18f,-.207f),new Vector3(.018f,.40f,.018f),RuntimeArtFactory.Trim);
            RuntimeArtFactory.Cube(hazmatSuit.transform,"Hazmat chest patch",new Vector3(.12f,1.31f,-.21f),new Vector3(.12f,.09f,.025f),RuntimeArtFactory.Metal);
            hazmatHood=CreateGroup("Hazmat hood");
            RuntimeArtFactory.Sphere(hazmatHood.transform,"Hood shell",new Vector3(0,1.75f,.005f),new Vector3(.34f,.38f,.31f),hazmatMaterial);
            RuntimeArtFactory.Cube(hazmatHood.transform,"Hood face shield",new Vector3(.07f,1.69f,-.15f),new Vector3(.27f,.19f,.035f),RuntimeArtFactory.Rubber);
            RuntimeArtFactory.Cube(hazmatHood.transform,"Respirator",new Vector3(.17f,1.55f,-.12f),new Vector3(.12f,.10f,.08f),RuntimeArtFactory.Metal);

            kneePadsLeft=RuntimeArtFactory.Cube(leftLeg,"Left trouser knee pad",new Vector3(0,-.22f,-.12f),new Vector3(.14f,.12f,.05f),RuntimeArtFactory.Rubber);
            kneePadsRight=RuntimeArtFactory.Cube(rightLeg,"Right trouser knee pad",new Vector3(0,-.22f,-.12f),new Vector3(.14f,.12f,.05f),RuntimeArtFactory.Rubber);
            RuntimeArtFactory.Cube(visual,"Left shoe",new Vector3(-.15f,.11f,-.025f),new Vector3(.22f,.17f,.30f),bootMaterial);
            RuntimeArtFactory.Cube(visual,"Right shoe",new Vector3(.15f,.11f,-.025f),new Vector3(.22f,.17f,.30f),bootMaterial);
            workBootsLeft=MakeBoot("Left equipped work boot",-.15f);
            workBootsRight=MakeBoot("Right equipped work boot",.15f);

            backpack=CreateGroup("Equipped canvas backpack");
            RuntimeArtFactory.Cube(backpack.transform,"Pack body",new Vector3(0,1.14f,.235f),new Vector3(.39f,.53f,.22f),jacketMaterial);
            RuntimeArtFactory.Cube(backpack.transform,"Pack flap",new Vector3(0,1.24f,.35f),new Vector3(.27f,.18f,.04f),jacketMaterial);
            RuntimeArtFactory.Cube(backpack.transform,"Pack side pocket",new Vector3(.20f,1.09f,.24f),new Vector3(.14f,.23f,.35f),bootMaterial);

            helmet=CreateGroup("Equipped hard helmet");
            RuntimeArtFactory.Sphere(helmet.transform,"Helmet shell",new Vector3(0,1.98f,0),new Vector3(.35f,.17f,.31f),RuntimeArtFactory.Metal);
            RuntimeArtFactory.Cube(helmet.transform,"Helmet brim",new Vector3(0,1.91f,-.025f),new Vector3(.48f,.045f,.38f),RuntimeArtFactory.Metal);
            RuntimeArtFactory.Cube(helmet.transform,"Helmet front lamp",new Vector3(.08f,1.97f,-.16f),new Vector3(.11f,.09f,.07f),RuntimeArtFactory.Fluorescent);

            helmet.SetActive(false);
            jacket.SetActive(false);
            vest.SetActive(false);
            backpack.SetActive(false);
            hazmatSuit.SetActive(false);
            hazmatHood.SetActive(false);
            kneePadsLeft.SetActive(false);
            kneePadsRight.SetActive(false);
            workBootsLeft.SetActive(false);
            workBootsRight.SetActive(false);
            SetGear(null);
        }

        GameObject MakeBoot(string name,float x)
        {
            var boot=CreateGroup(name);
            RuntimeArtFactory.Cube(boot.transform,"Boot body",new Vector3(x,.13f,-.04f),new Vector3(.25f,.21f,.34f),workBootMaterial);
            RuntimeArtFactory.Cube(boot.transform,"Boot sole",new Vector3(x,.025f,-.055f),new Vector3(.27f,.035f,.35f),RuntimeArtFactory.Rubber);
            RuntimeArtFactory.Cube(boot.transform,"Boot toe",new Vector3(x+.025f,.145f,-.055f),new Vector3(.18f,.105f,.28f),workBootMaterial);
            return boot;
        }

        GameObject CreateGroup(string name)
        {
            var group=new GameObject(name);
            group.transform.SetParent(visual,false);
            return group;
        }

        Transform MakeLimb(string name,float x,float y,Material material,float width,float length,out Renderer[] clothing)
        {
            var pivot=new GameObject(name+" pivot").transform;
            pivot.SetParent(visual,false);
            pivot.localPosition=new Vector3(x,y,0);
            var upper=RuntimeArtFactory.Capsule(pivot,name+" upper",new Vector3(0,-length*.24f,0),new Vector3(width,length*.25f,width*.86f),material);
            RuntimeArtFactory.Sphere(pivot,name+" joint",new Vector3(0,-length*.47f,0),new Vector3(width*.78f,width*.8f,width*.78f),skin);
            var lower=RuntimeArtFactory.Capsule(pivot,name+" lower",new Vector3(0,-length*.73f,0),new Vector3(width*.82f,length*.23f,width*.78f),material);
            clothing=new[]{upper.GetComponent<Renderer>(),lower.GetComponent<Renderer>()};
            return pivot;
        }

        static Material Mat(Color color,float smoothness,float metallic)
        {
            Shader shader=Shader.Find("Standard");
            if(shader==null)shader=Shader.Find("Diffuse");
            var material=new Material(shader){color=color};
            if(material.HasProperty("_Glossiness"))material.SetFloat("_Glossiness",smoothness);
            if(material.HasProperty("_Metallic"))material.SetFloat("_Metallic",metallic);
            return material;
        }

        static void SetMaterials(Renderer[] renderers,Material material)
        {
            if(renderers==null)return;
            foreach(var renderer in renderers)if(renderer!=null)renderer.sharedMaterial=material;
        }

        public void SetGear(ItemStack[] gear)
        {
            if(!built){Build();return;}
            ItemId head=ItemId.None,chest=ItemId.None,legs=ItemId.None,feet=ItemId.None,back=ItemId.None;
            if(gear!=null&&gear.Length>=InventorySystem.GearSize)
            {
                head=gear[(int)GearSlot.Head].id;
                chest=gear[(int)GearSlot.Chest].id;
                legs=gear[(int)GearSlot.Legs].id;
                feet=gear[(int)GearSlot.Feet].id;
                back=gear[(int)GearSlot.Back].id;
            }
            bool wearingJacket=chest==ItemId.FieldJacket;
            bool wearingVest=chest==ItemId.ArmorVest;
            bool wearingHazmat=chest==ItemId.HazmatSuit;
            bool wearingPants=legs==ItemId.FieldPants;
            bool wearingBoots=feet==ItemId.WorkBoots;

            jacket.SetActive(wearingJacket);
            vest.SetActive(wearingVest);
            hazmatSuit.SetActive(wearingHazmat);
            hazmatHood.SetActive(wearingHazmat);
            helmet.SetActive(head==ItemId.Helmet);
            backpack.SetActive(back==ItemId.Backpack);
            kneePadsLeft.SetActive(wearingPants&&!wearingHazmat);
            kneePadsRight.SetActive(wearingPants&&!wearingHazmat);
            workBootsLeft.SetActive(wearingBoots);
            workBootsRight.SetActive(wearingBoots);

            Material armMaterial=wearingHazmat?hazmatMaterial:wearingJacket?jacketMaterial:shirt;
            Material legMaterial=wearingHazmat?hazmatMaterial:wearingPants?fieldPantsMaterial:pantsMaterial;
            SetMaterials(armClothing,armMaterial);
            SetMaterials(legClothing,legMaterial);
        }

        public void SetMotion(float speed,bool grounded,bool jumping,int direction,bool threat=false)
        {
            runAmount=Mathf.Clamp01(speed);
            airAmount=grounded?0:1;
            facing=direction;
            threatAmount=Mathf.MoveTowards(threatAmount,threat?1:0,Time.deltaTime*2.2f);
        }

        public void Attack(){attackAmount=1;}

        void Update()
        {
            if(!built)return;
            phase+=Time.deltaTime*(5f+runAmount*8f);
            visual.localScale=new Vector3(facing>=0?1:-1,1,1);
            visual.localRotation=Quaternion.Euler(0,0,-airAmount*2f-threatAmount*5f);
            float swing=Mathf.Sin(phase)*runAmount*.66f;
            leftArm.localRotation=Quaternion.Euler(0,0,swing+attackAmount*45f);
            rightArm.localRotation=Quaternion.Euler(0,0,-swing+attackAmount*12f);
            leftLeg.localRotation=Quaternion.Euler(0,0,-swing*.8f);
            rightLeg.localRotation=Quaternion.Euler(0,0,swing*.8f);
            visual.localPosition=new Vector3(0,Mathf.Abs(Mathf.Sin(phase*2))*runAmount*.03f,0);
            attackAmount=Mathf.MoveTowards(attackAmount,0,Time.deltaTime*2.4f);
        }

        public void SetHostileAppearance()
        {
            skin.color=new Color(.31f,.34f,.29f);
            shirt.color=new Color(.17f,.19f,.17f);
            jacketMaterial.color=new Color(.20f,.22f,.18f);
            pantsMaterial.color=new Color(.14f,.16f,.15f);
            fieldPantsMaterial.color=new Color(.18f,.20f,.17f);
            hazmatMaterial.color=new Color(.30f,.32f,.24f);
            eyeMaterial.color=new Color(.66f,.075f,.025f);
            if(eyeMaterial.HasProperty("_EmissionColor"))
            {
                eyeMaterial.EnableKeyword("_EMISSION");
                eyeMaterial.SetColor("_EmissionColor",new Color(.75f,.045f,.012f));
            }
        }

        public void SetThreat(bool value)
        {
            threatAmount=Mathf.MoveTowards(threatAmount,value?1:0,Time.deltaTime*2f);
        }
    }
}
