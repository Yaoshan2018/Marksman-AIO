﻿#region Licensing
// ---------------------------------------------------------------------
// <copyright file="PermaActive.cs" company="EloBuddy">
// 
// Marksman Master
// Copyright (C) 2016 by gero
// All rights reserved
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see http://www.gnu.org/licenses/. 
// </copyright>
// <summary>
// 
// Email: geroelobuddy@gmail.com
// PayPal: geroelobuddy@gmail.com
// </summary>
// ---------------------------------------------------------------------
#endregion
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Spells;
using Marksman_Master.Utils;

namespace Marksman_Master.Plugins.Varus.Modes
{
    internal class PermaActive : Varus
    {
        public static void Execute()
        {
            if (Settings.Misc.EnableKillsteal)
            {
                if(Q.IsCharging && EntityManager.Heroes.Enemies.Any(x=>x.IsValidTarget(Q.Range) && x.TotalHealthWithShields() <= Damage.GetQDamage(x) + Damage.GetWDamage(x)))
                {
                    Q.CastMinimumHitchance(
                        EntityManager.Heroes.Enemies.First(
                            x => !x.IsDead &&
                                x.IsValidTarget(Q.Range) &&
                                x.TotalHealthWithShields() <= Damage.GetQDamage(x) + Damage.GetWDamage(x)), HitChance.Medium);
                } else if (E.IsReady() && EntityManager.Heroes.Enemies.Any(x => x.IsValidTarget(E.Range) && x.TotalHealthWithShields() <= Player.Instance.GetSpellDamage(x, SpellSlot.E) + Damage.GetWDamage(x)))
                {
                    E.CastMinimumHitchance(EntityManager.Heroes.Enemies.First(x => !x.IsDead && x.IsValidTarget(E.Range) && x.TotalHealthWithShields() <= Player.Instance.GetSpellDamage(x, SpellSlot.E) + Damage.GetWDamage(x)), HitChance.Medium);
                }
            }

            if (Settings.Harass.AutoHarassWithQ && !Player.Instance.IsRecalling() && !Player.Instance.Position.IsVectorUnderEnemyTower() && Q.IsReady() &&
                Player.Instance.ManaPercent >= Settings.Harass.MinManaQ)
            {
                if (EntityManager.Heroes.Enemies.Any(x => x.IsValidTarget(Q.MaximumRange-100) && Settings.Harass.IsAutoHarassEnabledFor(x) && Q.GetPrediction(x).HitChancePercent > 50))
                {
                    if (!Q.IsCharging && !IsPreAttack && !EntityManager.Heroes.Enemies.Any(x =>
                                                                          x.IsValidTarget(Settings.Combo.QMinDistanceToTarget)))
                    {
                        Q.StartCharging();
                    } else if (Q.IsCharging && Q.IsFullyCharged)
                    {
                        foreach (var target in EntityManager.Heroes.Enemies.Where(x => x.IsValidTarget(Q.Range) && Settings.Harass.IsAutoHarassEnabledFor(x) && Q.GetPrediction(x).HitChancePercent > 60).TakeWhile(target => Q.IsReady()))
                        {
                            Q.CastMinimumHitchance(target, 60);
                        }
                    }
                }
            }

            if (!R.IsReady())
                return;

            var t = TargetSelector.GetTarget(R.Range, DamageType.Physical);

            if (t == null || !Settings.Combo.RKeybind)
                return;

            var rPrediciton = Prediction.Manager.GetPrediction(new Prediction.Manager.PredictionInput
            {
                CollisionTypes = new HashSet<CollisionType> { CollisionType.ObjAiMinion },
                Delay = 250,
                From = Player.Instance.Position,
                Radius = 115,
                Range = R.Range,
                RangeCheckFrom = Player.Instance.Position,
                Speed = R.Speed,
                Target = t,
                Type = SkillShotType.Linear
            });

            if (rPrediciton.HitChance >= HitChance.Medium)
            {
                R.Cast(rPrediciton.CastPosition);
            }
        }
    }
}