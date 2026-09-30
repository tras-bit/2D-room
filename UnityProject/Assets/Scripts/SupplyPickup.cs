using UnityEngine;

namespace Subsistence
{
    [RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
    public sealed class SupplyPickup : MonoBehaviour
    {
        public enum Kind { Scrap, Cloth, Food, Water }
        public Kind Type { get; private set; }
        public bool Collected { get; private set; }
        float baseY;
        float phase;
        SpriteRenderer spriteRenderer;

        public void Initialize(Kind kind)
        {
            Type = kind;
            baseY = transform.position.y;
            phase = Random.value * 6.28f;
        }

        void Awake() { spriteRenderer = GetComponent<SpriteRenderer>(); }

        void Update()
        {
            if (Collected) return;
            transform.position = new Vector3(transform.position.x, baseY + Mathf.Sin(Time.time * 2.4f + phase) * .055f, transform.position.z);
            if (spriteRenderer != null) spriteRenderer.color = Color.Lerp(Color.white, new Color(1f,.88f,.59f), .12f + Mathf.Sin(Time.time * 3f + phase) * .08f);
        }

        public void Collect()
        {
            if (Collected || RunState.Instance == null) return;
            Collected = true;
            RunState.Instance.Collect(Type);
            AudioDirector.Instance?.Play(Type == Kind.Scrap ? "metal" : Type == Kind.Water ? "water" : "cloth", .7f);
            gameObject.SetActive(false);
        }

        public void ResetForNewRun()
        {
            Collected = false;
            gameObject.SetActive(true);
            transform.position = new Vector3(transform.position.x, baseY, transform.position.z);
        }
    }
}
