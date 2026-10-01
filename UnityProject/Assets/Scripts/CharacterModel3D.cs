using UnityEngine;

namespace Subsistence
{
    /// <summary>Legacy component name retained for project compatibility; delegates entirely to the 2D sprite renderer.</summary>
    [System.Obsolete("Use CharacterVisual2D. This compatibility wrapper creates no 3D model.")]
    public sealed class CharacterModel3D : MonoBehaviour
    {
        CharacterVisual2D visual;
        void Awake(){EnsureVisual();}
        void EnsureVisual(){if(visual==null)visual=GetComponent<CharacterVisual2D>()??gameObject.AddComponent<CharacterVisual2D>();}
        public void Build()=>EnsureVisual();
        public void SetGear(ItemStack[] gear){EnsureVisual();visual.SetGear(gear);}
        public void SetMotion(float speed,bool grounded,bool jumping,int direction,bool threat=false){EnsureVisual();visual.SetMotion(speed,grounded,jumping,direction,threat);}
        public void SetHostileAppearance(){EnsureVisual();visual.SetHostileAppearance();}
        public void SetThreat(bool value){EnsureVisual();visual.SetMotion(0,true,false,1,value);}
        public void Attack(){EnsureVisual();visual.Attack();}
    }
}
