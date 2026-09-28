using System;
using UnityEngine;

namespace Turris
{
    /// <summary>
    /// Jawna tabela przejść między akcjami postaci.
    ///
    ///  z \ do            | Lekki/Ciężki/Riposta | Czar | Blok | Parowanie | Unik | Flaszka
    ///  ------------------+----------------------+------+------+-----------+------+--------
    ///  Brak (Idle)       |          tak         | tak  | tak  |    tak    | tak  |  tak
    ///  Blok (trzymany)   |          tak         | tak  |  —   |    tak    | tak  |  NIE (najpierw opuść gardę)
    ///  Atak / Czar       | tylko po punkcie przerwania w fazie regeneracji (cancelAfter); flaszka – nigdy
    ///  Riposta, Parowanie, Unik, Flaszka, Drgnięcie, Przełamanie gardy – NIE (akcja musi się zakończyć)
    ///  Śmierć            | nic
    ///
    /// Drgnięcie, przełamanie gardy i śmierć są wymuszane (Force) przez system trafień i przerywają wszystko.
    /// Dzięki temu nie da się jednocześnie atakować, parować, blokować, robić uniku ani pić flaszki.
    /// </summary>
    public static class ActionRules
    {
        public static bool CanTransition(ActionType from, ActionType to, bool pastCancelPoint)
        {
            if (to == ActionType.None || to == ActionType.Flinch || to == ActionType.GuardBroken || to == ActionType.Dead)
                return false; // te stany tylko przez Force()

            switch (from)
            {
                case ActionType.None:
                    return true;
                case ActionType.Block:
                    return to != ActionType.Flask && to != ActionType.Block;
                case ActionType.LightAttack:
                case ActionType.HeavyAttack:
                case ActionType.Cast:
                    return pastCancelPoint && to != ActionType.Flask;
                default:
                    return false;
            }
        }
    }

    /// <summary>
    /// Czysta (bez MonoBehaviour) maszyna stanów akcji z fazami Startup → Active → Recovery.
    /// Właściciel reaguje na zdarzenie PhaseChanged (włącza hitbox, okno parowania, niewrażliwość itp.).
    /// </summary>
    public class ActionController
    {
        public ActionType Current { get; private set; } = ActionType.None;
        public ActionPhase Phase { get; private set; } = ActionPhase.Finished;
        public float Elapsed { get; private set; }
        public int ComboIndex { get; private set; }

        float startup, active, recovery, cancelAt;

        /// <summary>(akcja, nowa faza). Faza Finished oznacza powrót do Idle.</summary>
        public event Action<ActionType, ActionPhase> PhaseChanged;

        public bool IsIdle => Current == ActionType.None;
        public bool IsBlocking => Current == ActionType.Block;
        public bool IsDead => Current == ActionType.Dead;
        public bool PastCancelPoint => Elapsed >= cancelAt;
        public float TotalDuration => startup + active + recovery;

        public bool CanStart(ActionType type) => ActionRules.CanTransition(Current, type, PastCancelPoint);

        /// <summary>Rozpoczyna akcję czasową. cancelAfterRecovery – ile sekund fazy regeneracji musi minąć, zanim można ją przerwać.</summary>
        public bool TryStart(ActionType type, float startupTime, float activeTime, float recoveryTime, float cancelAfterRecovery = float.PositiveInfinity)
        {
            if (!CanStart(type)) return false;
            bool chained = type == ActionType.LightAttack && Current == ActionType.LightAttack;
            ComboIndex = chained ? ComboIndex + 1 : 0;
            Begin(type, startupTime, activeTime, recoveryTime, cancelAfterRecovery);
            return true;
        }

        public bool TryStartBlock()
        {
            if (Current == ActionType.Block) return true;
            if (!CanStart(ActionType.Block)) return false;
            Current = ActionType.Block;
            Phase = ActionPhase.Active;
            Elapsed = 0;
            startup = 0; active = float.PositiveInfinity; recovery = 0; cancelAt = 0;
            PhaseChanged?.Invoke(ActionType.Block, ActionPhase.Active);
            return true;
        }

        public void EndBlock()
        {
            if (Current != ActionType.Block) return;
            Finish();
        }

        /// <summary>Wymuszone przejście (trafienie, przełamanie gardy, śmierć) – przerywa każdą akcję poza śmiercią.</summary>
        public void Force(ActionType type, float duration)
        {
            if (Current == ActionType.Dead) return;
            ComboIndex = 0;
            if (type == ActionType.Dead)
            {
                Current = ActionType.Dead;
                Phase = ActionPhase.Active;
                Elapsed = 0;
                startup = 0; active = float.PositiveInfinity; recovery = 0; cancelAt = float.PositiveInfinity;
                PhaseChanged?.Invoke(type, ActionPhase.Active);
                return;
            }
            Begin(type, 0f, 0f, duration, float.PositiveInfinity);
        }

        public void Reset()
        {
            Current = ActionType.None;
            Phase = ActionPhase.Finished;
            Elapsed = 0;
            ComboIndex = 0;
        }

        void Begin(ActionType type, float s, float a, float r, float cancelAfterRecovery)
        {
            Current = type;
            startup = Mathf.Max(0, s); active = Mathf.Max(0, a); recovery = Mathf.Max(0, r);
            cancelAt = startup + active + cancelAfterRecovery;
            Elapsed = 0;
            Phase = ActionPhase.Startup;
            PhaseChanged?.Invoke(type, ActionPhase.Startup);
            AdvancePhases();
        }

        public void Tick(float dt)
        {
            if (Current == ActionType.None || Current == ActionType.Dead || Current == ActionType.Block) return;
            Elapsed += dt;
            AdvancePhases();
        }

        void AdvancePhases()
        {
            // Pętla, by przy dużym dt nie pominąć zdarzeń fazy aktywnej.
            for (int guard = 0; guard < 4; guard++)
            {
                if (Phase == ActionPhase.Startup && Elapsed >= startup)
                {
                    Phase = ActionPhase.Active;
                    PhaseChanged?.Invoke(Current, ActionPhase.Active);
                    continue;
                }
                if (Phase == ActionPhase.Active && Elapsed >= startup + active)
                {
                    Phase = ActionPhase.Recovery;
                    PhaseChanged?.Invoke(Current, ActionPhase.Recovery);
                    continue;
                }
                if (Phase == ActionPhase.Recovery && Elapsed >= startup + active + recovery)
                {
                    Finish();
                }
                break;
            }
        }

        void Finish()
        {
            var ended = Current;
            Current = ActionType.None;
            Phase = ActionPhase.Finished;
            Elapsed = 0;
            PhaseChanged?.Invoke(ended, ActionPhase.Finished);
        }

        public bool InActivePhase => Phase == ActionPhase.Active;

        /// <summary>Postęp 0..1 w bieżącej fazie (dla animacji zsynchronizowanej z walką).</summary>
        public float PhaseProgress
        {
            get
            {
                switch (Phase)
                {
                    case ActionPhase.Startup: return startup > 0 ? Mathf.Clamp01(Elapsed / startup) : 1f;
                    case ActionPhase.Active: return active > 0 && !float.IsInfinity(active) ? Mathf.Clamp01((Elapsed - startup) / active) : 0f;
                    case ActionPhase.Recovery: return recovery > 0 ? Mathf.Clamp01((Elapsed - startup - active) / recovery) : 1f;
                    default: return 0f;
                }
            }
        }

        /// <summary>Czas trwania akcji (bez nieskończonych faz trzymanych).</summary>
        public float FiniteDuration => (float.IsInfinity(active) ? 0f : active) + startup + recovery;
    }

    /// <summary>Pula zasobu (życie, mana, wytrzymałość) z opóźnieniem regeneracji po wydatku.</summary>
    public class ResourcePool
    {
        public float Current { get; private set; }
        public float Max { get; private set; }
        float delayTimer;

        public ResourcePool(float max) { Max = max; Current = max; }

        public float Fraction => Max <= 0 ? 0 : Current / Max;

        public void SetMax(float max, bool keepFraction)
        {
            float f = Fraction;
            Max = Mathf.Max(0, max);
            Current = keepFraction ? f * Max : Mathf.Min(Current, Max);
        }

        public void SetCurrent(float v) => Current = Mathf.Clamp(v, 0, Max);

        public bool TrySpend(float amount, float regenDelay = 0f)
        {
            if (Current + 0.001f < amount) return false;
            Current = Mathf.Max(0, Current - amount);
            delayTimer = Mathf.Max(delayTimer, regenDelay);
            return true;
        }

        public void Drain(float amount, float regenDelay = 0f)
        {
            Current = Mathf.Max(0, Current - amount);
            delayTimer = Mathf.Max(delayTimer, regenDelay);
        }

        public void Restore(float amount) => Current = Mathf.Min(Max, Current + amount);

        public void Tick(float dt, float regenPerSecond)
        {
            if (delayTimer > 0) { delayTimer -= dt; return; }
            if (regenPerSecond > 0) Restore(regenPerSecond * dt);
        }
    }
}
