using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

namespace game 
{
    public abstract class actor : ScriptableObject, ISerializationCallbackReceiver
    {
        public List<move> Moves = new List<move>();
        public List<statusEffect> statusEffects = new List<statusEffect>(),
                                  attackModifyingEffects = new List<statusEffect>(),
                                  blockModifyingEffects = new List<statusEffect>(),
                                  efxModifyingEffects = new List<statusEffect>();
        public Dictionary<Stats, int> stats;
        [SerializeField] private int stat_hp, stat_energy;
        private int stat_attack, stat_defense;
        public Sprite sprite;
        public string domain;

        public void OnBeforeSerialize() {}

        public void OnAfterDeserialize() {
            stats = new Dictionary<Stats, int> {
                {Stats.attack, stat_attack},
                {Stats.defense, stat_defense},
                {Stats.hp, stat_hp},
                {Stats.energy, stat_energy}
            };
        }

        [NonSerialized] public int health, maxHealth, block, energy, maxEnergy;

        public string Name;

        public enum Stats : int {
            hp,
            attack,
            defense,
            energy
        } public static Stats[] StatsArray = {Stats.hp, Stats.attack, Stats.defense, Stats.energy};

        public void attacked(actor caster, int damage, bool triggerEffects = true) {
            bool b = false;

            if (triggerEffects)
                foreach (statusEffect s in statusEffects) 
                    b = b || s.OnTakeDamage(this, caster, damage);

            if (b)
                return;

            if (damage < 0)
                damage = 0;

            if (block != 0) {
                combatManager.Anim("block", this, -Mathf.Min(block, damage));
                block -= damage;
            } else
                block -= damage;

            if (block < 0) {
                health += block;
                combatManager.Anim("attack", this, -block);
                block = 0;
            }

            if (health <= 0) {
                if (this is enemy e)
                    combatManager.ded.Add(combatManager.current.KeyOf(e));
                else
                    combatManager.Reset();
            }
        }

        public virtual void OnStartTurn(int j) {
            block -= 3;
            if (block < 0)  
                block = 0;

            for (int i = 0; i < statusEffects.Count; i++)
                if (statusEffects[i].removeCond(true)) {
                    statusEffects.RemoveAt(i);
                    i --;
                }

            foreach (statusEffect s in statusEffects)
                s.OnStartTurn(this);

            if (this is enemy e && combatManager.ded.Contains(j))
                return;

            foreach (move m in Moves)
                m.OnStartTurn(this);
        }

        public virtual void OnEndTurn() {
            energy = maxEnergy;

            for (int i = 0; i < statusEffects.Count; i++)
                if (statusEffects[i].removeCond(false)) {
                    statusEffects.RemoveAt(i);
                    i --;
                }
        }

        public void useMove(move m, actor target) {
            if (energy >= m.energyCost && m.usable) {
                foreach (statusEffect s in statusEffects)
                    s.OnDoAMove(this);

                m.use(this, target);
            }
        }

        public void Block(int block) {
            foreach (statusEffect s in statusEffects)
                block = s.influenceBlock(block);

            combatManager.Anim("block", this, block);

            this.block += block;
        }

        public void Attack(actor target, int attack) {
            foreach (statusEffect s in statusEffects)
                attack = s.influence(attack);

            ScreenShakerUI.ShakeRandPos(10 * attack, combatManager.canvas);

            target.attacked(this, attack);
        }

        public void Heal(int healing) {
            combatManager.Anim("heal", this, healing);
            
            health += healing;
            health = Math.Min(health, maxHealth);
        }

        public void AddEffect(statusEffect s) {
            switch (s) {
                case regen:
                    combatManager.sfx("heal");
                    break;
                case buffMult:
                    combatManager.sfx("buff");
                    break;
                case buffDiv:
                    combatManager.sfx("debuff");
                    break;
                case shock:
                    combatManager.sfx("shock");
                    break;
                case poison:
                    combatManager.sfx("poison");
                    break;
                case spike:
                    combatManager.sfx("spike");
                    break;
                case buffAdd:
                    combatManager.sfx("buff");
                    break;
                case dodge:
                    combatManager.sfx("dodge");
                    break;
                case buffSub:
                    combatManager.sfx("debuff");
                    break;
                case tetanos:
                    combatManager.sfx("tetanos");
                    break;
                case haste:
                    combatManager.sfx("haste");
                    break;
                case luck:
                    combatManager.sfx("luck");
                    break;
                case magic:
                    combatManager.sfx("magic");
                    break;
                case fire:
                    combatManager.sfx("fire");
                    break;
            }

            foreach (statusEffect S in efxModifyingEffects)
                s.value = S.influenceEfx(s.value);

            int i = statusEffects.FindIndex(S => S.GetType() == s.GetType());
            if (i != -1)
                statusEffects[i].value += s.value;
            else
            {
                statusEffects.Add(s);
                switch (s) {
                    case buffMult or buffDiv or buffSub or buffAdd:
                        attackModifyingEffects.Add(s);
                        break;
                    case magic:
                        efxModifyingEffects.Add(s);
                        break;
                    case haste:
                        blockModifyingEffects.Add(s);
                        break;       
                }
            }
        }
    }
}