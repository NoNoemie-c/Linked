using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace game 
{
    [CreateAssetMenu(fileName = "new enemy", menuName = "enemy")]
    public sealed class enemy : actor
    {
        public List<move> nextMoves = new List<move>(), prevMoves = new List<move>();

        public List<int> movesRarity;

        public static List<move> enemyMoves;

        public int rarity;

        public void setTo(enemy e) {
            health = maxHealth = e.stats[Stats.hp];
            energy = maxEnergy = e.stats[Stats.energy];
            block = 0;
            Name = e.Name;
            sprite = e.sprite;
            statusEffects = new List<statusEffect>();
            domain = e.domain;
            Moves = new List<move>();
            if (e.Name == "Tardichad" || e.Name == "Herve")
                Moves = new List<move>(enemyMoves);

            var availableMoves = new List<move>(enemyMoves);

            for (int i = 0; i < e.Moves.Count; i++) {
                if (e.Moves[i] is null) {
                    int r = e.movesRarity[i];
                    while (!enemyMoves.Exists(m => m.rarity == r && m.energyCost < energy)) {
                        r --;
                        if (r < 0)
                            break;
                    }

                    if (r < 0)
                        continue;

                    move m = ScriptableObject.CreateInstance<move>();
                    var v = pickMove(Moves, availableMoves, r, energy);
                    if (v is not null)
                    {
                        m.setTo(v);
                        if (combatManager.gamer)
                            m = m.upgrade();
                        Moves.Add(m);
                        availableMoves.Remove(m);
                    }
                } else {
                    move m = ScriptableObject.CreateInstance<move>();
                    m.setTo(e.Moves[i]);
                    if (combatManager.gamer)
                        m = m.upgrade();
                    Moves.Add(m);
                    availableMoves.Remove(m);
                }
            }
        }

        public static move pickMove(List<move> Moves, List<move> availableMoves, int rarity, int energy) {
            float best = 0;
            move picked = null;
            foreach (move m in availableMoves) {
                if (m.rarity != rarity || m.energyCost > energy)
                    continue;
                
                float Count = 0;
                foreach (move M in Moves)
                    Count += move.synergy[(int)M.type][(int)m.type];
            
                if (Count > best || (Count == best && UnityEngine.Random.Range(0, 1f) > .5f)) {
                    best = Count;
                    picked = m;
                }
            }
            
            return picked;
        }

        public void calculateNextMoves() {
            nextMoves.Clear();
            int energyLeft = energy;
            foreach (statusEffect s in statusEffects)
                if (s is shock)
                    energyLeft -= s.value;

            List<move> usableMoves = Moves.FindAll(m => m.Usable(energyLeft));
            Stack<move> current = new (), nonRepeat = new ();
            List<statusEffect> Efx = new List<statusEffect>(statusEffects),
                               attEfx = new List<statusEffect>(attackModifyingEffects),
                               efxEfx = new List<statusEffect>(efxModifyingEffects),
                               blockEfx = new List<statusEffect>(blockModifyingEffects);

            string prev = "";

            var picked = new List<move>();
            float best = -200;

            int healthGain = 0, damageGain = 0, blockGain = 0;
            float effectsGain = 0;
            int maxEnergy = 100;

            void calculateNext(List<move> usableMoves) {
                foreach (move m in usableMoves) {
                    List<statusEffect> added = new List<statusEffect>();
                    int d = 0, b = 0, h = 0;
                    float efxGain = 0;

                    if (m.usableEveryNturns != 0 || m.oncePerBattle)
                        nonRepeat.Push(m);
                    energyLeft -= m.energyCost;
                    energyLeft += m.energyGiveBack;

                    for (int i = 0; i < m.repeat; i++) {
                        healthGain += h = m.ExpectedHeal(this, combatManager.player);

                        b += m.ExpectedBlock(this, combatManager.player);
                        if (b != 0) {
                            foreach (statusEffect s in blockEfx)
                                b = s.influenceBlock(b);
                            blockGain += b;
                        }

                        d = m.ExpectedAttack(this, combatManager.player);
                        if (d != 0) {
                            foreach (statusEffect s in attEfx)
                                d = s.influence(d);
                            damageGain += d;
                        }
                        
                        foreach ((move.StatusEffect, int, bool) s in m.effects)
                            if (!s.Item3) {
                                statusEffect S = move.efx(s);
                                foreach (statusEffect ss in efxEfx)
                                    S.value = ss.influenceEfx(S.value);
                                added.Add(S);
                                Efx.Add(S);
                                switch (S) {
                                    case buffMult or buffDiv or buffSub or buffAdd:
                                        attEfx.Add(S);
                                        break;
                                    case magic:
                                        efxEfx.Add(S);
                                        break;
                                    case haste:
                                        blockEfx.Add(S);
                                        break;       
                                }
                            }

                        efxGain = 0;
                        foreach ((move.StatusEffect, int, bool) s in m.effects)
                            efxGain += statusEffect.worthness[s.Item1] * s.Item2 * (s.Item3? -1 : 1) * (
                                s.Item1 == move.StatusEffect.regen && health == maxHealth ||
                                (s.Item1 == move.StatusEffect.tetanos && combatManager.player.statusEffects.Exists(M => M is tetanos))? 0 : 1 
                            );
                        effectsGain += efxGain;
                    }

                    int prevMax = maxEnergy;
                    maxEnergy = m.energyCost;

                    List<move> newUsableMoves = Moves.FindAll(m => m.energyCost <= maxEnergy && m.Usable(energyLeft) && !nonRepeat.Contains(m) && (!current.Contains(m) || prev == m.Name));

                    current.Push(m);
                   
                    if (newUsableMoves.Count == 0) {
                        float avrg = (damageGain + effectsGain + healthGain + blockGain) / 4;
                        float score = ((healthGain * (health == maxHealth? 0 : 1.6f) + 
                            blockGain * (block == 0? 1.1f : .5f) + 
                            damageGain * ((combatManager.player.block < 5 && combatManager.player.statusEffects.Find(s => s is dodge)?.value < 1)? 1.15f : 1) + 
                            effectsGain * 1 
                            - energyLeft * 3
                            + avrg
                            - prevMoves.FindAll(M => current.Contains(M)).Count * 3) 
                            / (m.waitTurns + 1))
                            * m.chance;

                        if (score > best || (score == best && UnityEngine.Random.value > .5f)) {
                            best = score;

                            picked = new(current);
                        }
                    } else {
                        prev = m.name;
                        calculateNext(newUsableMoves);
                    }

                    if (m.usableEveryNturns != 0 || m.oncePerBattle)
                        nonRepeat.Pop();

                    foreach (statusEffect s in added)
                    {
                        Efx.Remove(s);
                        switch (s) {
                            case buffMult or buffDiv or buffSub or buffAdd:
                                attEfx.Remove(s);
                                break;
                            case magic:
                                efxEfx.Remove(s);
                                break;
                            case haste:
                                blockEfx.Remove(s);
                                break;       
                        }
                    }

                    maxEnergy = prevMax;

                    current.Pop();
                    energyLeft += m.energyCost;
                    energyLeft -= m.energyGiveBack;
                    healthGain -= h;
                    blockGain -= b;
                    damageGain -= d;
                    effectsGain -= efxGain;
                }
            }
            
            calculateNext(usableMoves);

            nextMoves = new List<move>(picked);

            prevMoves = new List<move>(nextMoves);
        }
    }
}
