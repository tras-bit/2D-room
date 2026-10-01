using UnityEngine;

namespace Subsistence
{
    /// <summary>Inventory portrait backed by the same pixel sprite as the side-view character.</summary>
    public sealed class CharacterPreview : MonoBehaviour
    {
        public Texture Texture{get;private set;}
        ItemStack[] equipment;
        void Awake(){Refresh();}
        public void SetGear(ItemStack[] gear){equipment=gear==null?null:(ItemStack[])gear.Clone();Refresh();}
        void Refresh(){Texture=PixelArtFactory.Survivor(0,equipment).texture;}
    }
}
