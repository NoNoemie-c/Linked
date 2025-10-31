using UnityEngine;
using System.Collections.Generic;

namespace game 
{
    public abstract class statusEffect 
    {
        public int value;
        public string name;
        public Sprite sprite;
        public static Dictionary<move.StatusEffect, float> worthness = new Dictionary<move.StatusEffect, float>{
            {move.StatusEffect.poison, -1},
            {move.StatusEffect.spike, 3},
            {move.StatusEffect.tetanos, -5},
            {move.StatusEffect.shock, -1},
            {move.StatusEffect.dodge, 4},
            {move.StatusEffect.buffMult, 0},
            {move.StatusEffect.buffAdd, 0},
            {move.StatusEffect.buffDiv, -3},
            {move.StatusEffect.debuff, -3},
            {move.StatusEffect.regen, 2},
            {move.StatusEffect.haste, 2},
            {move.StatusEffect.luck, 2},
            {move.StatusEffect.magic, 2},
            {move.StatusEffect.fire, -1}
        };
        
        public virtual void OnStartTurn(actor user) {}
        public virtual bool OnTakeDamage(actor target, actor user, int damage) => false;
        public virtual int influence(int attack) => attack;
        public virtual void OnEndTurn(actor user) {}
        public virtual void OnDoAMove(actor user) {}
        public virtual int influenceEfx(int val) => val;
        public virtual int influenceBlock(int val) => val;
        
        public abstract bool removeCond(bool isStart);
    }

    public sealed class poison : statusEffect 
    {
        public poison(int val) {
            name = "poison";
            value = val;
            sprite = Resources.Load<Sprite>("icons/poison");
        }

        public override void OnStartTurn(actor target) {
            target.attacked(target, value, false);
            value --;
        }

        public override bool removeCond(bool isStart) => 
            value <= 0;
    }

    public sealed class spike : statusEffect  
    {
        public spike(int val) {
            name = "spike";
            value = val;
            sprite = Resources.Load<Sprite>("icons/spike");
        }

        public override bool OnTakeDamage(actor target, actor user, int damage) {
            user.attacked(target, value, false);

            return false;
        }

        public override void OnStartTurn(actor user) =>
            value --;

        public override bool removeCond(bool isStart) => 
            value <= 0;
    }

    public sealed class tetanos : statusEffect 
    {
        public tetanos(int val) {
            name = "tetanos";
            value = val;
            sprite = Resources.Load<Sprite>("icons/tetanos");
        }

        public override void OnStartTurn(actor user) {
            if (value == 0)
                user.attacked(user, 100000, false);
            value --;
        }

        public override bool removeCond(bool isStart) => 
            combatManager.current.Count == 1;
    }

    public sealed class shock : statusEffect
    {
        public shock(int val) {
            name = "shock";
            value = val;
            sprite = Resources.Load<Sprite>("icons/shock");
        }

        public override void OnStartTurn(actor user) {
            user.energy -= value;
            if (user.energy < 0)
                user.energy = 0;
            value --;
        }

        public override bool removeCond(bool isStart) => 
            value <= 0;
    }

    public sealed class dodge : statusEffect
    {
        public dodge(int val) {
            name = "dodge";
            value = val;
            sprite = Resources.Load<Sprite>("icons/dodge");
        }

        public override bool OnTakeDamage(actor user, actor target, int damage) {
            if (value > 0) {
                value --;

                return true;
            }

            return false;
        }

        public override bool removeCond(bool isStart) => 
            value <= 0;
    }

    public sealed class buffMult : statusEffect
    {
        public buffMult(int val) {
            name = "buffMult";
            value = val;
            sprite = Resources.Load<Sprite>("icons/buffMult");
        }

        public override int influence(int attack) =>
            attack * value;

        public override bool removeCond(bool isStart) => 
            true;
    }

    public sealed class buffAdd : statusEffect
    {
        public buffAdd(int val) {
            name = "buffAdd";
            value = val;
            sprite = Resources.Load<Sprite>("icons/buff");
        }

         public override int influence(int attack) =>
            attack != 0? attack + value : 0;

        public override bool removeCond(bool isStart) => 
            true;
    }

    public sealed class buffDiv : statusEffect
    {
        public buffDiv(int val) {
            name = "buffDiv";
            value = val;
            sprite = Resources.Load<Sprite>("icons/buffDiv");
        }

        public override int influence(int attack) =>
            Mathf.FloorToInt(attack / (float) value);

        public override bool removeCond(bool isStart) => 
            !isStart;
    }

    public sealed class buffSub : statusEffect
    {
        public buffSub(int val) {
            name = "buffSub";
            value = val;
            sprite = Resources.Load<Sprite>("icons/debuff");
        }

        public override int influence(int attack) =>
            attack - value;

        public override bool removeCond(bool isStart) => 
            !isStart;
    }

    public sealed class regen : statusEffect
    {
        public regen(int val) {
            name = "regen";
            value = val;
            sprite = Resources.Load<Sprite>("icons/heal");
        }

        public override bool removeCond(bool isStart) => 
            value <= 0;

        public override void OnStartTurn(actor user) {
            user.Heal(value);
            value --;
        }
    }

    public sealed class haste : statusEffect
    {
        public haste(int val) {
            name = "haste";
            value = val;
            sprite = Resources.Load<Sprite>("icons/haste");
        }

        public override void OnStartTurn(actor user) => 
            value --;

        public override bool removeCond(bool isStart) =>
            value <= 0;

        public override int influenceBlock(int val) => 
            val == 0? 0 : val + value;
    }

    public sealed class luck : statusEffect
    {
        public luck(int val) {
            name = "luck";
            value = val;
            sprite = Resources.Load<Sprite>("icons/luck");
        }

        public override void OnStartTurn(actor user) => 
            value --;

        public override bool removeCond(bool isStart) =>
            value <= 0;
    }

    public sealed class magic : statusEffect
    {
        public magic(int val) {
            name = "magic";
            value = val;
            sprite = Resources.Load<Sprite>("icons/magic");
        }

        public override bool removeCond(bool isStart) =>
            !isStart;

        public override int influenceEfx(int val) => 
            val == 0? 0 : val + value;
    }

    public sealed class fire : statusEffect 
    {
        public fire(int val) {
            name = "fire";
            value = val;
            sprite = Resources.Load<Sprite>("icons/fire");
        }

        public override bool removeCond(bool isStart) =>
            !isStart;

        public override void OnDoAMove(actor user) =>
            user.attacked(user, value, false);
    }
}