using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using System.Linq;

namespace game 
{
    [CreateAssetMenu(fileName = "new move", menuName = "Move")]
    public class move : ScriptableObject, ISerializationCallbackReceiver
    {
        public enum StatusEffect : int {
            poison,
            tetanos,
            regen,
            shock,
            buffAdd,
            buffMult,
            dodge,
            spike,
            debuff,
            buffDiv,
            haste,
            luck,
            magic,
            fire
        }
        public enum MoveType : int {
            attack,
            block,
            buff,
            debuff,
            heal,
            poison,
            shock,
            spike,
            dodge,
            tetanos,
            buffMult,
            multiHit,
            magic,
            energy,
            luck,
            haste,
            fire
        } public static MoveType[] MoveTypes = {
            MoveType.attack, MoveType.block, MoveType.buff, MoveType.heal, MoveType.poison, MoveType.debuff, MoveType.shock, MoveType.spike, MoveType.dodge, MoveType.buffMult, MoveType.tetanos, MoveType.multiHit, MoveType.energy, MoveType.luck, MoveType.haste, MoveType.magic, MoveType.fire
        };
        public static float[][] synergy = {
            new float[]{1, 2, 3, 0, 0, 0, 0, 0, 0, 0, 5, 1, 0, 0, 0, 0, 0},
            new float[]{2, 1, 0, 0, 1, 2, 3, 5, 0, 2, 0, 0, 0, 0, 5, 0, 0},
            new float[]{3, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 5, 0, 0, 0, 1, 0},
            new float[]{0, 0, 0, 1, 0, 0, 2, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            new float[]{0, 1, 0, 0, 3, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 1, 0},
            new float[]{0, 2, 0, 0, 0, 5, 0, 2, 0, 0, 0, 0, 0, 0, 0, 1, 0},
            new float[]{0, 3, 0, 2, 0, 0, 4, 0, 1, 1, 0, 0, 0, 0, 0, 1, 0},
            new float[]{0, 5, 0, 4, 0, 2, 0, 4, 0, 0, 0, 0, 0, 0, 0, 1, 0},
            new float[]{0, 0, 0, 0, 2, 0, 1, 0, 4, 0, 1, 0, 0, 0, 0, 0, 0},
            new float[]{0, 2, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0},
            new float[]{5, 0, 0, 0, 0, 0, 0, 0, 1, 0, 1, 2, 0, 0, 0, 0, 0},
            new float[]{1, 0, 5, 0, 0, 0, 0, 0, 0, 0, 2, 1, 0, 0, 0, 0, 0},
            new float[]{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0},
            new float[]{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0},
            new float[]{0, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0},
            new float[]{0, 0, 1, 0, 1, 1, 1, 1, 0, 1, 0, 0, 1, 1, 1, 0, 1},
            new float[]{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0}
        };
        public enum AppliesTo : int {
            targeted,
            all
        }

        public MoveType type = MoveType.attack;
        [Space(5)]
        public string Name;
        [Space(10)]
        [TextArea(3, 5)] public string description;
        [Space(10)]
        public float chance = 1;
        [Space(5)]
        public int energyCost, rarity, energyGiveBack;
        [Space(5)]
        public int attack, block, healing, selfDamage;
        [Space(5)]
        public AppliesTo appliesTo;
        public bool oncePerBattle;
        private int turns;
        public int usableEveryNturns;
        [NonSerialized] public bool usable = true;
        [Space(5)]
        public int waitTurns, repeat = 1;
        [Space(5)] public List<(StatusEffect, int, bool)> effects = new List<(StatusEffect, int, bool)>();
        [SerializeField] private List<StatusEffect> _effects;
        [SerializeField] private List<int> effectsStrength;
        [SerializeField] private List<bool> effectsTargetEnemy;
        [TextArea(1, 10)] [Space(5)] public string code, codeDesc;

        private actor c, t;

        public static Dictionary<MoveType, Sprite> typeImages;
        public static List<move> allMoves;

        public void setTo(move m) {
            type = m.type;
            Name = m.Name;
            name = m.name;
            description = m.description;
            energyCost = m.energyCost;
            energyGiveBack = m.energyGiveBack;
            attack = m.attack;
            block = m.block;
            healing = m.healing;
            rarity = m.rarity;
            appliesTo = m.appliesTo;
            oncePerBattle = m.oncePerBattle;
            usableEveryNturns = m.usableEveryNturns;
            waitTurns = m.waitTurns;
            repeat = m.repeat;
            effects = new List<(StatusEffect, int, bool)>(m.effects);
            code = m.code;
            codeDesc = m.codeDesc;
            chance = m.chance;
            selfDamage = m.selfDamage;
        }

        public void OnBeforeSerialize() {}
        public void OnAfterDeserialize() {
            effects.Clear();

            int len = Mathf.Min(_effects.Count, effectsStrength.Count, effectsTargetEnemy.Count);

            for (int i = 0; i < len && i < effectsStrength.Count; i++)
                effects.Add((_effects[i], effectsStrength[i], effectsTargetEnemy[i]));

            description = generateDescription();
        }

        public static void Awake() {
            Sprite[] sprites = Resources.LoadAll<Sprite>("icons");
            typeImages = new Dictionary<MoveType, Sprite>();
            foreach (MoveType t in MoveTypes)
                typeImages.Add(t, Array.Find(sprites, s => s.name == t.ToString()));
            allMoves = new List<move>();
            foreach (move m in Resources.LoadAll<move>("moves")) {
                if (m.name.EndsWith("0"))
                    allMoves.Add(m);

                m.description = m.generateDescription();
            }
        }

        public void reset() {
            usable = true;
            turns = 0;
        }

        public move upgrade() =>
            this + combatManager.moveUpgrades[Name.Replace("+", "")];

        public void OnStartTurn(actor caster) {
            if (caster is null) return;

            turns ++;
            if (turns >= usableEveryNturns && !oncePerBattle)
                usable = true;

            if (usableEveryNturns == 0 && !oncePerBattle)
                usable = true;

            if (turns >= waitTurns && waitTurns != 0 && c is not null && t is not null) {
                Use(c, t);
                c = t = null;
            }
        }

        public int ExpectedAttack(actor caster, actor target) {
            int a = attack, s = selfDamage, b = block, h = healing;

            if (code is not null && code != "") {
                string[] lines = code.Split(';', StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < lines.Length; i++) {
                    string cond = lines[i].Split(":")[0], effect = lines[i].Split(":")[1];
                    if (decryptCond(cond, caster, target))
                        decryptEffect(effect, caster, target, out a, out s, out b, out h);
                }
            }

            return a;
        }

        public int ExpectedHeal(actor caster, actor target) {
            int a = attack, s = selfDamage, b = block, h = healing;

            if (code is not null && code != "") {
                string[] lines = code.Split(';', StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < lines.Length; i++) {
                    string cond = lines[i].Split(":")[0], effect = lines[i].Split(":")[1];
                    if (decryptCond(cond, caster, target))
                        decryptEffect(effect, caster, target, out a, out s, out b, out h);
                }
            }

            return h;
        }

        public int ExpectedBlock(actor caster, actor target) {
            int a = attack, s = selfDamage, b = block, h = healing;

            if (code is not null && code != "") {
                string[] lines = code.Split(';', StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < lines.Length; i++) {
                    string cond = lines[i].Split(":")[0], effect = lines[i].Split(":")[1];
                    if (decryptCond(cond, caster, target))
                        decryptEffect(effect, caster, target, out a, out s, out b, out h);
                }
            }

            return b;
        }

        public int value(actor caster, actor target) => type switch {
            MoveType.attack => ExpectedAttack(caster, target),
            MoveType.block => ExpectedBlock(caster, target),
            MoveType.heal => ExpectedHeal(caster, target),
            MoveType.energy => energyGiveBack,
            MoveType.multiHit => ExpectedAttack(caster, target),
            MoveType.buff => effects.Find(x => x.Item1 == StatusEffect.buffAdd).Item2,
            MoveType.buffMult => effects.Find(x => x.Item1 == StatusEffect.buffMult).Item2,
            MoveType.debuff => effects.Find(x => x.Item1 == StatusEffect.debuff).Item2,
            MoveType.dodge => effects.Find(x => x.Item1 == StatusEffect.dodge).Item2,
            MoveType.fire => effects.Find(x => x.Item1 == StatusEffect.fire).Item2,
            MoveType.haste => effects.Find(x => x.Item1 == StatusEffect.haste).Item2,
            MoveType.luck => effects.Find(x => x.Item1 == StatusEffect.luck).Item2,
            MoveType.magic => effects.Find(x => x.Item1 == StatusEffect.magic).Item2,
            MoveType.poison => effects.Find(x => x.Item1 == StatusEffect.poison).Item2,
            MoveType.shock => effects.Find(x => x.Item1 == StatusEffect.shock).Item2,
            MoveType.spike => effects.Find(x => x.Item1 == StatusEffect.spike).Item2,
            MoveType.tetanos => effects.Find(x => x.Item1 == StatusEffect.tetanos).Item2,
            _ => throw new Exception($"u forgor to implement value() for {type}")
        } * repeat;

        private int get(string s, actor caster, actor target) {
            if (s.Contains("+"))
                return get(s.Split('+')[0], caster, target) + get(s.Split('+')[1], caster, target);
            if (s.Contains("-"))
                return get(s.Split('-')[0], caster, target) - get(s.Split('-')[1], caster, target);
            if (s.Contains("*"))
                return get(s.Split('*')[0], caster, target) * get(s.Split('*')[1], caster, target);
            if (s.Contains("/"))
                return get(s.Split('/')[0], caster, target) / get(s.Split('/')[1], caster, target);

            bool isN = false;
            foreach (char c in s)
                isN = isN || Char.IsDigit(c);

            if (isN)
                return Convert.ToInt32(s);

            switch (s.Split(".")[0]) {
                case "target" :
                    switch (s.Split(".")[1]) {
                        case "hp" :
                            return target.health;

                        case "effects" :
                            if (target.statusEffects is null)
                                return 0;

                            switch (s.Split(".")[2]) {
                                case "poison" :
                                    return target.statusEffects.Where(s => s is poison).Select(s => s.value).Sum();
                                case "spike" :
                                    return target.statusEffects.Where(s => s is spike).Select(s => s.value).Sum();
                                case "dodge" :
                                    return target.statusEffects.Where(s => s is dodge).Select(s => s.value).Sum();
                                case "buffAdd" :
                                    return target.statusEffects.Where(s => s is buffAdd).Select(s => s.value).Sum();
                                case "buffDiv" :
                                    return target.statusEffects.Where(s => s is buffDiv).Select(s => s.value).Sum();
                                case "buffSub" :
                                    return target.statusEffects.Where(s => s is buffSub).Select(s => s.value).Sum();
                                case "buffMult" :
                                    return target.statusEffects.Where(s => s is buffMult).Select(s => s.value).Sum();
                                case "shock" :
                                    return target.statusEffects.Where(s => s is shock).Select(s => s.value).Sum();
                                case "tetanos" :
                                    return target.statusEffects.Where(s => s is tetanos).Select(s => s.value).Sum();
                                case "regen" :
                                    return target.statusEffects.Where(s => s is regen).Select(s => s.value).Sum();
                                case "luck" :
                                    return target.statusEffects.Where(s => s is luck).Select(s => s.value).Sum();
                                case "haste" :
                                    return target.statusEffects.Where(s => s is haste).Select(s => s.value).Sum();
                                case "magic" :
                                    return target.statusEffects.Where(s => s is magic).Select(s => s.value).Sum();
                                case "fire" :
                                    return target.statusEffects.Where(s => s is fire).Select(s => s.value).Sum();
                                default :
                                    throw new Exception($"wrong code on {Name} get");
                            }

                        case "energy" :
                            return target.energy;

                        case "block" :
                            return target.block;

                        default :
                            throw new Exception($"wrong code on {Name} get");
                    }

                case "caster" :
                    switch (s.Split(".")[1]) {
                        case "hp" :
                            return caster.health;

                        case "effects" :
                            if (caster.statusEffects is null)
                                return 0;

                            switch (s.Split(".")[2]) {
                                case "poison" :
                                    return caster.statusEffects.Where(s => s is poison).Select(s => s.value).Sum();
                                case "spike" :
                                    return caster.statusEffects.Where(s => s is spike).Select(s => s.value).Sum();
                                case "dodge" :
                                    return caster.statusEffects.Where(s => s is dodge).Select(s => s.value).Sum();
                                case "buffAdd" :
                                    return caster.statusEffects.Where(s => s is buffAdd).Select(s => s.value).Sum();
                                case "buffDiv" :
                                    return caster.statusEffects.Where(s => s is buffDiv).Select(s => s.value).Sum();
                                case "buffSub" :
                                    return caster.statusEffects.Where(s => s is buffSub).Select(s => s.value).Sum();
                                case "buffMult" :
                                    return caster.statusEffects.Where(s => s is buffMult).Select(s => s.value).Sum();
                                case "shock" :
                                    return caster.statusEffects.Where(s => s is shock).Select(s => s.value).Sum();
                                case "tetanos" :
                                    return caster.statusEffects.Where(s => s is tetanos).Select(s => s.value).Sum();
                                case "regen" :
                                    return caster.statusEffects.Where(s => s is regen).Select(s => s.value).Sum();
                                case "fire" :
                                    return caster.statusEffects.Where(s => s is fire).Select(s => s.value).Sum();

                                default :
                                    throw new Exception($"wrong code on {Name} get");
                            }

                        case "energy" :
                            return caster.energy;

                        case "block" :
                            return caster.block;

                        default :
                            throw new Exception($"wrong code on {Name} get");
                    }

                case "attack" :
                    return attack;

                case "block" :
                    return block;

                case "healing" :
                    return healing;
                
                default :
                    throw new Exception($"wrong code on {Name} get");
            }
        }

        private bool decryptCond(string cond, actor caster, actor target) {
            if (cond == "true")
                return true;

            if (cond.Contains("<"))
                return get(cond.Split("<")[0], caster, target) < get(cond.Split("<")[1], caster, target);
            else if (cond.Contains("=="))
                return get(cond.Split("==")[0], caster , target) == get(cond.Split("==")[1], caster, target);
            else if (cond.Contains(">"))
                return get(cond.Split(">")[0], caster , target) > get(cond.Split(">")[1], caster, target);

            throw new Exception($"wrong code on {Name} ({cond}), couldn't find condition");
        }

        private void decryptEffect(string effect, actor caster, actor target, out int totalAttack, out int totalSelfDamage, out int totalBlock, out int totalHealing, bool allowEfx = false) {
            string left = effect.Split("=")[0];
            string right = effect.Split("=")[1];

            totalAttack = attack;
            totalBlock = block;
            totalHealing = healing;
            totalSelfDamage = selfDamage;

            int var = get(right, caster, target);

            switch (left.Split(".")[0]) {
                case "attack" :
                    totalAttack = var;
                break;

                case "selfDamage" :
                    totalSelfDamage = var;
                break;

                case "block" :
                    totalBlock = var;
                break;

                case "heal" :
                    totalHealing = var;
                break;

                case "caster" :
                    if (!allowEfx)
                        return;
                    
                    if (left.Split(".")[1] == "effects") {
                        switch (left.Split(".")[2]) {
                            case "poison" :
                                caster.AddEffect(new poison(var));
                            break;
                            case "spike" :
                                caster.AddEffect(new spike(var));
                            break;
                            case "buffAdd" :
                                caster.AddEffect(new buffAdd(var));
                            break;
                            case "buffSub" :
                                caster.AddEffect(new buffSub(var));
                            break;
                            case "buffMult" :
                                caster.AddEffect(new buffMult(var));
                            break;
                            case "buffDiv" :
                                caster.AddEffect(new buffDiv(var));
                            break;
                            case "shock" :
                                caster.AddEffect(new shock(var));
                            break;
                            case "dodge" :
                                caster.AddEffect(new dodge(var));
                            break;
                            case "regen" :
                                caster.AddEffect(new regen(var));
                            break;
                            case "tetanos" :
                                caster.AddEffect(new tetanos(var));
                            break;
                            case "luck" : 
                                caster.AddEffect(new luck(var));
                            break;
                            case "magic" : 
                                caster.AddEffect(new magic(var));
                            break;
                            case "haste" : 
                                caster.AddEffect(new haste(var));
                            break;
                            case "fire" : 
                                caster.AddEffect(new fire(var));
                            break;
                            default :
                                throw new Exception($"wrong code on {Name} effect statusEffects");
                        }
                    }
                break;

                case "target" :
                    if (!allowEfx)
                        return;

                    if (left.Split(".")[1] == "effects") {
                        switch (left.Split(".")[2]) {
                            case "poison" :
                                target.AddEffect(new poison(var));
                            break;
                            case "spike" :
                                target.AddEffect(new spike(var));
                            break;
                            case "buffAdd" :
                                target.AddEffect(new buffAdd(var));
                            break;
                            case "buffSub" :
                                target.AddEffect(new buffSub(var));
                            break;
                            case "buffMult" :
                                target.AddEffect(new buffMult(var));
                            break;
                            case "buffDiv" :
                                target.AddEffect(new buffDiv(var));
                            break;
                            case "shock" :
                                target.AddEffect(new shock(var));
                            break;
                            case "dodge" :
                                target.AddEffect(new dodge(var));
                            break;
                            case "regen" :
                                target.AddEffect(new regen(var));
                            break;
                            case "tetanos" :
                                target.AddEffect(new tetanos(var));
                            break;
                            case "luck" : 
                                target.AddEffect(new luck(var));
                            break;
                            case "magic" : 
                                target.AddEffect(new magic(var));
                            break;
                            case "haste" : 
                                target.AddEffect(new haste(var));
                            break;
                            case "fire" : 
                                target.AddEffect(new fire(var));
                            break;
                            default :
                                throw new Exception($"wrong code on {Name} effect statusEffects");
                        }
                    }
                break;

                case "all" :
                    if (!allowEfx)
                        return;

                    if (left.Split(".")[1] == "effects") {
                        foreach (enemy a in combatManager.current.Values) {
                            if (a.GetType() == caster.GetType())
                                continue;

                            switch (left.Split(".")[2]) {
                                case "poison" :
                                    a.AddEffect(new poison(var));
                                break;
                                case "spike" :
                                    a.AddEffect(new spike(var));
                                break;
                                case "buffAdd" :
                                    a.AddEffect(new buffAdd(var));
                                break;
                                case "buffSub" :
                                    a.AddEffect(new buffSub(var));
                                break;
                                case "buffMult" :
                                    a.AddEffect(new buffMult(var));
                                break;
                                case "buffDiv" :
                                    a.AddEffect(new buffDiv(var));
                                break;
                                case "shock" :
                                    a.AddEffect(new shock(var));
                                break;
                                case "dodge" :
                                    a.AddEffect(new dodge(var));
                                break;
                                case "regen" :
                                    a.AddEffect(new regen(var));
                                break;
                                case "tetanos" :
                                    a.AddEffect(new tetanos(var));
                                break;
                                case "luck" : 
                                    a.AddEffect(new luck(var));
                                break;
                                case "magic" : 
                                    a.AddEffect(new magic(var));
                                break;
                                case "haste" : 
                                    a.AddEffect(new haste(var));
                                break;
                                case "fire" : 
                                    a.AddEffect(new fire(var));
                                break;
                                default :
                                    throw new Exception($"wrong code on {Name} effect statusEffects");
                            }
                        }
                    }
                break;

                default :
                    throw new Exception($"wrong code on {Name} effect");
            }
        }
        
        public void Use(actor caster, actor target) {
            int totalBlock = block, totalAttack = attack, totalHealing = healing, totalSelfDamage = selfDamage;

            if (code is not null && code != "") {
                string[] lines = code.Split(';', StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < lines.Length; i++) {
                    if (lines[i] == "")
                        continue;
                    string cond = lines[i].Split(":")[0], effect = lines[i].Split(":")[1];
                    if (decryptCond(cond, caster, target))
                        decryptEffect(effect, caster, target, out totalAttack, out totalSelfDamage, out totalBlock, out totalHealing, true);
                }
            }

            
            for (int i = 0; i < repeat; i++) 
            {
                if (!caster.statusEffects.Exists(x => x is luck) && UnityEngine.Random.Range(0, 1f) > chance)
                {
                    combatManager.sfx("unlucky");

                    continue;
                }

                caster.energy += energyGiveBack;

                if (totalAttack != 0) 
                {
                    if (appliesTo == AppliesTo.targeted)
                        caster.Attack(target, totalAttack);
                    else 
                    {
                        if (caster is player)
                            foreach (enemy a in combatManager.current.Values)
                                caster.Attack(a, totalAttack);
                        else
                            caster.Attack(combatManager.player, totalAttack);
                    }
                        
                }

                if (totalSelfDamage != 0)
                    caster.attacked(caster, totalSelfDamage, false);

                if (totalBlock != 0) 
                    caster.Block(totalBlock);

                if (totalHealing != 0)
                    caster.Heal(totalHealing);

                foreach ((StatusEffect, int, bool) s in effects) {
                    statusEffect p = efx(s);

                    if (s.Item3) {
                        if (appliesTo == AppliesTo.targeted)
                            target.AddEffect(p);
                        else 
                            foreach (enemy a in combatManager.current.Values)
                                if (caster.GetType() != a.GetType()) {
                                    a.AddEffect(p);
                                    p = efx(s);
                                }
                    } else
                        caster.AddEffect(p);
                }
            }
        }

        public static statusEffect efx((StatusEffect, int, bool) s) => s.Item1 switch {
            StatusEffect.poison => new poison(s.Item2),
            StatusEffect.spike => new spike(s.Item2),
            StatusEffect.dodge => new dodge(s.Item2),
            StatusEffect.shock => new shock(s.Item2),
            StatusEffect.buffAdd => new buffAdd(s.Item2),
            StatusEffect.debuff => new buffSub(s.Item2),
            StatusEffect.buffMult => new buffMult(s.Item2),
            StatusEffect.buffDiv => new buffDiv(s.Item2),
            StatusEffect.tetanos => new tetanos(s.Item2),
            StatusEffect.regen => new regen(s.Item2),
            StatusEffect.haste => new haste(s.Item2),
            StatusEffect.luck => new luck(s.Item2),
            StatusEffect.magic => new magic(s.Item2),
            StatusEffect.fire => new fire(s.Item2),
            _ => throw new ArgumentException("Invalid status effect: " + s)
        };
        
        public void use(actor caster, actor target) {
            caster.energy -= energyCost;

            if (usableEveryNturns != 0) {
                turns = 0;
                usable = false;
            } else if (oncePerBattle)
                usable = false;
            
            if (waitTurns == 0)
                Use(caster, target);
            else {
                turns = 0;
                c = caster;
                t = target;
            }
        }

        public bool Usable(int energy) => 
            energy >= energyCost && usable;

        public string generateDescription() {
            string s = "";

            if (chance < 1)    
                s += $" {chance * 100}% activation chance\n";
            if (usableEveryNturns != 0) 
                s += $"  once every {usableEveryNturns} turn\n";
            else if (oncePerBattle)
                s += "  once per battle\n";

            if (repeat > 1) {
                s += $"* activates {repeat} times";
                if (waitTurns > 0)
                    s += $"after {waitTurns} turns\n";
                else
                    s += "\n";
            } else if (waitTurns > 0)
                s += $"* activates after {waitTurns} turns\n";
                
            if (attack > 0)
                s += $"- Deals {attack} damage to {(appliesTo == AppliesTo.all? "all enemies" : "target")}\n";
            if (selfDamage > 0)
                s += $"- Deals {selfDamage} damage to the user\n";
            if (block > 0)
                s += $"- add {block} block to user\n";
            if (healing > 0)
                s += $"- heals {healing} hp to user\n";
            if (energyGiveBack > 0)
                s += $"- give {energyGiveBack} energy to user\n";

            foreach ((StatusEffect, int, bool) st in effects) {
                if (st.Item3) {
                    if (appliesTo == AppliesTo.targeted)
                        s += $"- inflicts {st.Item1} {st.Item2} to targeted enemy\n";
                    else 
                        s += $"- inflicts {st.Item1} {st.Item2} to all enemies\n";
                } else
                    s += $"- inflicts {st.Item1} {st.Item2} to user\n";
            }

            s += codeDesc;

            return s;
        }

        public override bool Equals(object obj) =>
            obj is move move && move == this;

        public override int GetHashCode() =>
            name.Replace("+", "").GetHashCode();

        public static move operator +(move m1, move m2) {
            move m = ScriptableObject.CreateInstance<move>();

            m.setTo(m1);

            m.Name += "+";
            m.attack += m2.attack;
            m.block += m2.block;
            m.healing += m2.healing;
            m.rarity += m2.rarity;
            m.energyCost += m2.energyCost;
            m.energyGiveBack += m2.energyGiveBack;
            m.waitTurns += m2.waitTurns;
            m.repeat += m2.repeat;
            m.code += m2.code;
            m2.selfDamage += m2.selfDamage;
            m.chance += m2.chance;
            m.codeDesc += "\n" + m2.codeDesc;
            int count = Mathf.Min(m.effects.Count, m2.effects.Count);
            foreach ((StatusEffect, int, bool) s in m2.effects) {
                int i = m.effects.FindIndex(S => S.GetType() == s.GetType());
                if (i != -1)
                    m.effects[i] = (m.effects[i].Item1, m.effects[i].Item2 + s.Item2, m.effects[i].Item3);
                else
                    m.effects.Add(s);
            }
            
            m.description = m.generateDescription();

            return m;
        }

        public static bool operator ==(move m1, move m2) =>
            m1.name.Replace("+", "") == m2.name.Replace("+", "");
        public static bool operator !=(move m1, move m2) =>
            m1.name.Replace("+", "") != m2.name.Replace("+", "");
    }
}