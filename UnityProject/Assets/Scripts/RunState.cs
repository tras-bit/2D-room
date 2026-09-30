using UnityEngine;

namespace Subsistence
{
    public sealed class RunState : MonoBehaviour
    {
        public static RunState Instance { get; private set; }
        public float Health { get; private set; } = 100f;
        public float Hunger { get; private set; } = 82f;
        public float Thirst { get; private set; } = 68f;
        public int Scrap { get; private set; }
        public int Cloth { get; private set; }
        public int SuppliesFound { get; private set; }
        public float ShiftSeconds { get; private set; }
        public string Notice { get; private set; } = "Сигнал потерян. Найди припасы и держись в тени.";
        public float NoticeTime { get; private set; } = 4f;
        public bool IsDead { get; private set; }
        public bool HasEscaped { get; private set; }
        public const float ExitX = 74f;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Update()
        {
            if (!GameHUD.IsPlaying || IsDead || HasEscaped) return;
            ShiftSeconds += Time.deltaTime;
            Hunger = Mathf.Max(0, Hunger - Time.deltaTime * .22f);
            Thirst = Mathf.Max(0, Thirst - Time.deltaTime * .34f);
            if (Hunger <= 0 || Thirst <= 0) TakeDamage(Time.deltaTime * (Hunger <= 0 && Thirst <= 0 ? 2.2f : 1.2f));
            NoticeTime = Mathf.Max(0, NoticeTime - Time.deltaTime);
        }

        public void Collect(SupplyPickup.Kind kind)
        {
            SuppliesFound++;
            switch (kind)
            {
                case SupplyPickup.Kind.Scrap: Scrap++; Notify("Подобран ржавый металл."); break;
                case SupplyPickup.Kind.Cloth: Cloth++; Notify("Сухая ткань добавлена в рюкзак."); break;
                case SupplyPickup.Kind.Food: Hunger = Mathf.Min(100, Hunger + 28); Notify("Консервы. +28 сытости."); break;
                case SupplyPickup.Kind.Water: Thirst = Mathf.Min(100, Thirst + 34); Notify("Вода. +34 запаса влаги."); break;
            }
        }

        public void TakeDamage(float amount)
        {
            if (IsDead || HasEscaped) return;
            Health = Mathf.Max(0, Health - amount);
            if (Health <= 0) { IsDead = true; Notify("Смена завершена. Сигнал прерван.", 99f); }
        }

        public void Heal(float amount) { Health = Mathf.Min(100, Health + amount); }
        public bool CraftBandage()
        {
            if (Cloth < 2) { Notify("Для перевязки нужны 2 единицы ткани."); return false; }
            Cloth -= 2; Heal(36); Notify("Перевязка готова. +36 здоровья."); AudioDirector.Instance?.Play("ui", .8f); return true;
        }

        public void ResetForNewRun()
        {
            Health=100;Hunger=82;Thirst=68;Scrap=0;Cloth=0;SuppliesFound=0;ShiftSeconds=0;
            IsDead=false;HasEscaped=false;Notify("Сигнал потерян. Найди припасы и держись в тени.",4f);
        }

        public void CheckExit(float x)
        {
            if (x >= ExitX && SuppliesFound >= 3 && Scrap >= 1)
            {
                HasEscaped = true;
                Notify("Ты выбрался. Но сигнал всё ещё зовёт.", 99f);
            }
            else if (x >= ExitX && Scrap < 1) Notify("Выход обесточен. Найди металлолом.");
            else if (x >= ExitX && SuppliesFound < 3) Notify("Сначала найди три припаса.");
        }

        public void Notify(string message, float duration = 2.2f) { Notice = message; NoticeTime = duration; }
    }
}
