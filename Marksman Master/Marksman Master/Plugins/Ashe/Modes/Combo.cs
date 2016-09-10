﻿#region Licensing
// ---------------------------------------------------------------------
// <copyright file="Combo.cs" company="EloBuddy">
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

namespace Marksman_Master.Plugins.Ashe.Modes
{
    internal class Combo : Ashe
    {
        public static void Execute()
        {
            if (Q.IsReady() && IsPreAttack && Settings.Combo.UseQ)
            {
                var target = TargetSelector.GetTarget(Player.Instance.GetAutoAttackRange(), DamageType.Physical);

                if (target != null)
                {
                    if (EntityManager.Heroes.Enemies.Any(x => x.IsValidTarget(Player.Instance.GetAutoAttackRange() - 50)))
                    {
                        Q.Cast();
                    }
                }
            }

            if (W.IsReady() && Settings.Combo.UseW && Player.Instance.Mana - 50 > 100)
            {
                var possibleTargets =
                    EntityManager.Heroes.Enemies.Where(
                        x => x.IsValidTarget(W.Range) && !x.HasSpellShield() && GetWPrediction(x) != null && GetWPrediction(x).HitChance >= HitChance.Medium)
                        .ToList();

                if (possibleTargets.Any())
                {
                    var target = TargetSelector.GetTarget(possibleTargets, DamageType.Physical);

                    if (target != null)
                    {
                        var wPrediction = GetWPrediction(target);

                        if (wPrediction != null && wPrediction.HitChance >= HitChance.Medium)
                        {
                            W.Cast(wPrediction.CastPosition);
                        }

                    }
                }
            }

            if (E.IsReady() && Settings.Combo.UseE)
            {
                foreach (var source in EntityManager.Heroes.Enemies.Where(x=> !x.IsDead && x.IsUserInvisibleFor(500)))
                {
                    var data = source.GetVisibilityTrackerData();

                    if (data.LastHealthPercent < 25 && data.LastPosition.Distance(Player.Instance) < 3000)
                    {
                        E.Cast(data.LastPath);
                    }
                }
            }

            if (R.IsReady() && Settings.Combo.UseR)
            {
                var target = TargetSelector.GetTarget(Settings.Combo.RMaximumRange, DamageType.Physical);

                if (target != null && !target.IsUnderTurret() && !target.HasSpellShield() && !target.HasUndyingBuffA() && target.Distance(Player.Instance) > Settings.Combo.RMinimumRange && target.Health - IncomingDamage.GetIncomingDamage(target) > 100)
                {
                    var damage = 0f;

                    if (Player.Instance.Mana > 200 && target.IsValidTarget(W.Range))
                    {
                        damage = Player.Instance.GetSpellDamage(target, SpellSlot.R) +
                                 Player.Instance.GetSpellDamage(target, SpellSlot.W) +
                                 Player.Instance.GetAutoAttackDamage(target)*4;
                    }
                    else if (Player.Instance.Mana > 150 && target.IsValidTarget(W.Range))
                        damage = Player.Instance.GetSpellDamage(target, SpellSlot.R) +
                                 Player.Instance.GetAutoAttackDamage(target)*4;

                   var rPrediction = Prediction.Manager.GetPrediction(new Prediction.Manager.PredictionInput
                    {
                        CollisionTypes = new HashSet<CollisionType> { CollisionType.ObjAiMinion },
                        Delay = 250,
                        From = Player.Instance.Position,
                        Radius = 120,
                        Range = Settings.Combo.RMaximumRange,
                        RangeCheckFrom = Player.Instance.Position,
                        Speed = R.Speed,
                        Target = target,
                        Type = SkillShotType.Linear
                    });

                    if (damage > target.TotalHealthWithShields() && (rPrediction.HitChancePercent >= 65))
                    {
                        R.Cast(rPrediction.CastPosition);
                    }
                }
            }
        }
    }
}