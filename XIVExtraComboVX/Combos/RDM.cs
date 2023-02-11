namespace XIVExtraComboVX.Combos;

using System;

using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.JobGauge.Types;

internal static class RDM {
	public const byte JobID = 35;

	public const int
		ManaCostRiposte = 20,
		ManaCostZwerchhau = 15,
		ManaCostRedoublement = 15;

	public const uint
		Verraise = 7523,
		Verthunder = 7505,
		Corpsacorps = 7506,
		Veraero = 7507,
		Scatter = 7509,
		Displacement = 7515,
		Veraero2 = 16525,
		Verthunder2 = 16524,
		Impact = 16526,
		Embolden = 7520,
		Manafication = 7521,
		Redoublement = 7516,
		EnchantedRedoublement = 7529,
		Zwerchhau = 7512,
		EnchantedZwerchhau = 7528,
		Riposte = 7504,
		EnchantedRiposte = 7527,
		Jolt = 7503,
		Verstone = 7511,
		Verfire = 7510,
		Moulinet = 7513,
		Fleche = 7517,
		Acceleration = 7518,
		ContreSixte = 7519,
		Jolt2 = 7524,
		Verholy = 7526,
		Verflare = 7525,
		Engagement = 16527,
		Scorch = 16530,
		Resolution = 25858;

	public static class Buffs {
		public const ushort
			VerfireReady = 1234,
			VerstoneReady = 1235,
			Acceleration = 1238,
			Dualcast = 1249;
	}

	public static class Debuffs {
		// public const ushort placeholder = 0;
	}

	public static class Levels {
		public const byte
			Jolt = 2,
			Verthunder = 4,
			Corpsacorps = 6,
			Veraero = 10,
			Scatter = 15,
			Swiftcast = 18,
			Verthunder2 = 18,
			Veraero2 = 22,
			Zwerchhau = 35,
			Displacement = 40,
			Engagement = 40,
			Fleche = 45,
			Redoublement = 50,
			Acceleration = 50,
			Vercure = 54,
			ContreSixte = 56,
			Embolden = 58,
			Manafication = 60,
			Jolt2 = 62,
			Verraise = 64,
			Impact = 66,
			Verflare = 68,
			Verholy = 70,
			Scorch = 80,
			Resolution = 90;
	}
}

internal class RedMageSwiftcastRaiserFeature: SwiftRaiseCombo {
	public override CustomComboPreset Preset => CustomComboPreset.RedMageSwiftcastRaiserFeature;
	public override uint[] ActionIDs { get; } = new[] { RDM.Verraise };
}

internal class RedMageAoECombo: CustomCombo {
	public override CustomComboPreset Preset => CustomComboPreset.RedMageAoECombo;
	public override uint[] ActionIDs { get; } = new[] { RDM.Veraero2, RDM.Verthunder2 };

	protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level) {

		if (lastComboMove is RDM.Scorch && level >= RDM.Levels.Resolution)
			return RDM.Resolution;

		if (lastComboMove is RDM.Verflare or RDM.Verholy && level >= RDM.Levels.Scorch)
			return RDM.Scorch;

		if (level >= RDM.Levels.Scatter && (IsFastcasting || SelfHasEffect(RDM.Buffs.Acceleration)))
			return OriginalHook(RDM.Scatter);

		return actionID;
	}
}

internal class RedMageMeleeCombo: CustomCombo {
	public override CustomComboPreset Preset => CustomComboPreset.RedMageMeleeCombo;
	public override uint[] ActionIDs { get; } = new[] { RDM.Redoublement, RDM.Moulinet };

	protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level) {
		const int
			FINISHER_DELTA = 11,
			IMBALANCE_DIFF_MAX = 30;

		if (IsEnabled(CustomComboPreset.RedMageMeleeComboPlus)) {
			RDMGauge gauge = GetJobGauge<RDMGauge>();
			int black = gauge.BlackMana;
			int white = gauge.WhiteMana;
			bool isFinishing1 = gauge.ManaStacks == 3;
			bool isFinishing2 = comboTime > 0 && lastComboMove is RDM.Verholy or RDM.Verflare;
			bool isFinishing3 = comboTime > 0 && lastComboMove is RDM.Scorch;
			bool canFinishWhite = level >= RDM.Levels.Verholy;
			bool canFinishBlack = level >= RDM.Levels.Verflare;
			int blackThreshold = white + IMBALANCE_DIFF_MAX;
			int whiteThreshold = black + IMBALANCE_DIFF_MAX;
			bool verfireUp = SelfHasEffect(RDM.Buffs.VerfireReady);
			bool verstoneUp = SelfHasEffect(RDM.Buffs.VerstoneReady);

			if (isFinishing3 && level >= RDM.Levels.Resolution)
				return RDM.Resolution;
			if (isFinishing2 && level >= RDM.Levels.Scorch)
				return RDM.Scorch;

			if (isFinishing1 && canFinishBlack) {

				if (black >= white && canFinishWhite) {

					if (verstoneUp && !verfireUp && (black + FINISHER_DELTA <= blackThreshold))
						return RDM.Verflare;

					return RDM.Verholy;
				}

				if (verfireUp && !verstoneUp && canFinishWhite && (white + FINISHER_DELTA <= whiteThreshold))
					return RDM.Verholy;

				return RDM.Verflare;
			}
		}

		if (actionID is RDM.Redoublement) {

			if (IsEnabled(CustomComboPreset.RedMageMeleeComboCloser)) {
				if (HasTarget && !InMeleeRange)
					return RDM.Corpsacorps;
			}

			if (lastComboMove is RDM.Zwerchhau or RDM.EnchantedZwerchhau && level >= RDM.Levels.Redoublement)
				return OriginalHook(RDM.Redoublement);

			if (lastComboMove is RDM.Riposte or RDM.EnchantedRiposte && level >= RDM.Levels.Zwerchhau)
				return OriginalHook(RDM.Zwerchhau);

			return OriginalHook(RDM.Riposte);
		}

		return OriginalHook(actionID);
	}
}

internal class RedMageVerprocCombo: CustomCombo {
	public override CustomComboPreset Preset => CustomComboPreset.RedMageVerprocCombo;
	public override uint[] ActionIDs { get; } = new[] { RDM.Verstone, RDM.Verfire };

	protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level) {

		if (lastComboMove == RDM.Scorch && level >= RDM.Levels.Resolution)
			return RDM.Resolution;

		if (lastComboMove is RDM.Verflare or RDM.Verholy && level >= RDM.Levels.Scorch)
			return RDM.Scorch;

		RDMGauge gauge = GetJobGauge<RDMGauge>();

		if (actionID is RDM.Verstone) {

			if (gauge.ManaStacks == 3 && level >= RDM.Levels.Verholy)
				return RDM.Verholy;

			if (IsEnabled(CustomComboPreset.RedMageVerprocComboPlus)
				&& level >= RDM.Levels.Veraero
				&& (IsFastcasting || SelfHasEffect(RDM.Buffs.Acceleration))
			) {
				return OriginalHook(RDM.Veraero);
			}

			if (IsEnabled(CustomComboPreset.RedMageVeraeroOpenerFeature)
				&& level >= RDM.Levels.Veraero
				&& !HasCondition(ConditionFlag.InCombat)
				&& !SelfHasEffect(RDM.Buffs.VerstoneReady)
			) {
				return OriginalHook(RDM.Veraero);
			}

			if (SelfHasEffect(RDM.Buffs.VerstoneReady))
				return RDM.Verstone;

		}
		else if (actionID is RDM.Verfire) {

			if (gauge.ManaStacks == 3 && level >= RDM.Levels.Verflare)
				return RDM.Verflare;

			if (IsEnabled(CustomComboPreset.RedMageVerprocComboPlus)
				&& level >= RDM.Levels.Verthunder
				&& (IsFastcasting || SelfHasEffect(RDM.Buffs.Acceleration))
			) {
				return OriginalHook(RDM.Verthunder);
			}

			if (IsEnabled(CustomComboPreset.RedMageVerthunderOpenerFeature)
				&& level >= RDM.Levels.Verthunder
				&& !HasCondition(ConditionFlag.InCombat)
				&& !SelfHasEffect(RDM.Buffs.VerfireReady)
			) {
				return OriginalHook(RDM.Verthunder);
			}

			if (SelfHasEffect(RDM.Buffs.VerfireReady))
				return RDM.Verfire;

		}

		return OriginalHook(RDM.Jolt2);
	}
}

internal class RedMageContreFlecheFeature: CustomCombo {
	public override CustomComboPreset Preset { get; } = CustomComboPreset.RedMageContreFlecheFeature;
	public override uint[] ActionIDs { get; } = new[] { RDM.Fleche, RDM.ContreSixte };

	protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level) {

		if (level >= RDM.Levels.ContreSixte)
			return PickByCooldown(actionID, RDM.Fleche, RDM.ContreSixte);

		if (level >= RDM.Levels.Fleche)
			return RDM.Fleche;

		return actionID;
	}
}

internal class RedMageSmartcastAoECombo: CustomCombo {
	public override CustomComboPreset Preset => CustomComboPreset.RedMageSmartcastAoEFeature;
	public override uint[] ActionIDs { get; } = new[] { RDM.Veraero2, RDM.Verthunder2 };

	protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level) {
		const int
			DELTA = 7,
			FINISHER_DELTA = 11,
			IMBALANCE_DIFF_MAX = 30;

		RDMGauge gauge = GetJobGauge<RDMGauge>();
		int black = gauge.BlackMana;
		int white = gauge.WhiteMana;
		bool isFinishing1 = gauge.ManaStacks == 3;
		bool isFinishing2 = comboTime > 0 && lastComboMove is RDM.Verholy or RDM.Verflare;
		bool isFinishing3 = comboTime > 0 && lastComboMove is RDM.Scorch;
		bool canFinishWhite = level >= RDM.Levels.Verholy;
		bool canFinishBlack = level >= RDM.Levels.Verflare;
		int blackThreshold = white + IMBALANCE_DIFF_MAX;
		int whiteThreshold = black + IMBALANCE_DIFF_MAX;

		// There is never a reason to NOT use the finishers when you have them.
		if (isFinishing3 && level >= RDM.Levels.Resolution)
			return RDM.Resolution;
		if (isFinishing2 && level >= RDM.Levels.Scorch)
			return RDM.Scorch;
		if (isFinishing1 && canFinishBlack) {
			bool verfireUp = SelfHasEffect(RDM.Buffs.VerfireReady);
			bool verstoneUp = SelfHasEffect(RDM.Buffs.VerstoneReady);

			if (black >= white && canFinishWhite) {

				// If we can already Verstone, but we can't Verfire, and Verflare WON'T imbalance us, use Verflare
				if (verstoneUp && !verfireUp && (black + FINISHER_DELTA <= blackThreshold))
					return RDM.Verflare;

				return RDM.Verholy;
			}

			// If we can already Verfire, but we can't Verstone, and we can use Verholy, and it WON'T imbalance us, use Verholy
			if (verfireUp && !verstoneUp && canFinishWhite && (white + FINISHER_DELTA <= whiteThreshold))
				return RDM.Verholy;

			return RDM.Verflare;
		}

		bool fastCast = IsFastcasting;

		if ((IsEnabled(CustomComboPreset.RedMageSmartcastAoEWeave) && CanWeave(actionID)) || (IsEnabled(CustomComboPreset.RedMageSmartcastAoEMovement) && IsMoving && !fastCast)) {
			if (level >= RDM.Levels.ContreSixte) {
				if (IsEnabled(CustomComboPreset.RedMageContreFlecheFeature)) {
					uint chosen = PickByCooldown(RDM.ContreSixte, RDM.Fleche, RDM.ContreSixte);
					if (IsOffCooldown(chosen))
						return chosen;
				}
				else if (IsOffCooldown(RDM.ContreSixte)) {
					return RDM.ContreSixte;
				}
			}
			else if (level >= RDM.Levels.Fleche && IsEnabled(CustomComboPreset.RedMageContreFlecheFeature)) {
				if (IsOffCooldown(RDM.Fleche))
					return RDM.Fleche;
			}
		}

		if (fastCast || SelfHasEffect(RDM.Buffs.Acceleration) || level < RDM.Levels.Verthunder2)
			return OriginalHook(RDM.Impact);

		if (level < RDM.Levels.Veraero2)
			return RDM.Verthunder2;

		if (white < black || Math.Min(100, black + DELTA) == white)
			return RDM.Veraero2;

		if (black < white || Math.Min(100, white + DELTA) == black)
			return RDM.Verthunder2;

		return actionID;
	}
}

internal class RedmageSmartcastSingleComboOpener: CustomCombo {
	public override CustomComboPreset Preset => CustomComboPreset.RedMageSmartcastSingleFeature;
	public override uint[] ActionIDs { get; } = new[] { RDM.Veraero, RDM.Verthunder };

	protected override uint Invoke(uint actionID, uint lastComboActionId, float comboTime, byte level) {
		const int LONG_DELTA = 6;
		RDMGauge gauge = GetJobGauge<RDMGauge>();
		int black = gauge.BlackMana;
		int white = gauge.WhiteMana;

		if (level < RDM.Levels.Verthunder)
			return RDM.Jolt;

		if (level is < RDM.Levels.Veraero and >= RDM.Levels.Verthunder)
			return OriginalHook(RDM.Verthunder);

		// This is for the long opener only, so we're not bothered about fast casting or finishers or anything like that
		// However, we DO want to prevent the mana levels from being perfectly even, cause that fucks up Manafication into melee as an opener

		if (black < white || Math.Min(100, white + LONG_DELTA) == black)
			return OriginalHook(RDM.Verthunder);

		if (white < black || Math.Min(100, black + LONG_DELTA) == white)
			return OriginalHook(RDM.Veraero);

		return actionID;
	}
}

internal class RedmageSmartcastSingleComboFull: CustomCombo {
	public override CustomComboPreset Preset => CustomComboPreset.RedMageSmartcastSingleFeature;
	public override uint[] ActionIDs { get; } = new[] { RDM.Verstone, RDM.Verfire };

	private static uint noCastingSubCheck(byte level, bool engageCheck, bool holdOneEngageCharge, bool engageEarly, bool canMelee, bool allowAccelerate) {

		if (allowAccelerate) {
			bool canAccelerate = level >= RDM.Levels.Acceleration && CanUse(RDM.Acceleration);
			bool canSwiftcast = IsEnabled(CustomComboPreset.RedMageSmartcastSingleAccelerationSwiftcast) && CanUse(Common.Swiftcast);

			if (canSwiftcast && IsEnabled(CustomComboPreset.RedMageSmartcastSingleAccelerationSwiftcastFirst))
				return Common.Swiftcast;
			if (canAccelerate)
				return RDM.Acceleration;
			if (canSwiftcast)
				return Common.Swiftcast;
		}

		ushort engageCharges = GetCooldown(RDM.Engagement).RemainingCharges;
		bool canEngage = canMelee && engageCharges > 0;

		bool shouldEngage = canEngage
			&& engageCheck
			&& (!holdOneEngageCharge || engageCharges > 1);
		bool engagePre = shouldEngage && engageEarly;

		if (canEngage && engagePre)
			return RDM.Engagement;

		if (level >= RDM.Levels.Fleche) {
			if (IsEnabled(CustomComboPreset.RedMageContreFlecheFeature) && level >= RDM.Levels.ContreSixte) {
				uint chosen = PickByCooldown(RDM.Fleche, RDM.Fleche, RDM.ContreSixte);
				if (IsOffCooldown(chosen))
					return chosen;
			}
			if (IsOffCooldown(RDM.Fleche))
				return RDM.Fleche;
		}

		if (shouldEngage)
			return RDM.Engagement;

		return 0;
	}

	protected override uint Invoke(uint actionID, uint lastComboActionId, float comboTime, byte level) {
		const int
			LONG_DELTA = 6,
			PROC_DELTA = 5,
			FINISHER_DELTA = 11,
			IMBALANCE_DIFF_MAX = 30;

		// This algorithm is a bit messy, because.. well, there's no clean way to do it, really.
		// The same conditions need to be checked at different stages in the flow because of priorities.
		// As a result, we kind of have to preload a bunch of checks, or else we'll be repeating them.
		// Plus, some of the conditions get complex, so it's better to split things up a little like this.
		// Just let the compiler handle optimisations :)

		bool fastCasting = IsFastcasting;
		bool accelerated = SelfHasEffect(RDM.Buffs.Acceleration);
		bool instacasting = fastCasting || accelerated;
		bool weaving = CanWeave(actionID);
		bool moving = IsMoving;
		bool targeting = HasTarget;
		bool fighting = InCombat;
		bool isClose = InMeleeRange;

		RDMGauge gauge = GetJobGauge<RDMGauge>();

		int black = gauge.BlackMana;
		int white = gauge.WhiteMana;
		int blackThreshold = white + IMBALANCE_DIFF_MAX;
		int whiteThreshold = black + IMBALANCE_DIFF_MAX;

		int minManaForEnchantedMelee = RDM.ManaCostRiposte + (level >= RDM.Levels.Zwerchhau ? RDM.ManaCostZwerchhau : 0) + (level >= RDM.Levels.Redoublement ? RDM.ManaCostRedoublement : 0);
		bool hasMeleeMana = black >= minManaForEnchantedMelee && white >= minManaForEnchantedMelee && (black != white || black is 100);

		bool verfireUp = SelfHasEffect(RDM.Buffs.VerfireReady);
		bool verstoneUp = SelfHasEffect(RDM.Buffs.VerstoneReady);
		bool isFinishing1 = gauge.ManaStacks == 3; // Mana stacks are only unlocked at the same level as Verflare, so we don't need a check for that here
		bool isFinishing2 = lastComboActionId is RDM.Verholy or RDM.Verflare && level >= RDM.Levels.Scorch;
		bool isFinishing3 = lastComboActionId is RDM.Scorch && level >= RDM.Levels.Resolution;
		bool isFinishingAny = isFinishing1 || isFinishing2 || isFinishing3;
		bool canFinishWhite = level >= RDM.Levels.Verholy;

		bool meleeCombo = IsEnabled(CustomComboPreset.RedMageSmartcastSingleMeleeCombo)
			&& lastComboActionId is RDM.EnchantedRiposte or RDM.Riposte or RDM.EnchantedZwerchhau or RDM.Zwerchhau;
		bool startMelee = IsEnabled(CustomComboPreset.RedMageSmartcastSingleMeleeComboStarter)
			&& targeting && isClose && hasMeleeMana;
		bool shouldCloseGap = IsEnabled(CustomComboPreset.RedMageSmartcastSingleMeleeComboStarterCloser)
			&& targeting && !isClose && hasMeleeMana;

		bool smartWeave = IsEnabled(CustomComboPreset.RedMageSmartcastSingleWeave) && weaving;
		bool smartMove = IsEnabled(CustomComboPreset.RedMageSmartcastSingleMovement) && moving;

		bool accelerate = level >= Common.Levels.Swiftcast
			&& !instacasting
			&& !isFinishingAny
			&& !meleeCombo
			&& !verfireUp
			&& !verstoneUp
			&& IsEnabled(CustomComboPreset.RedMageSmartcastSingleAcceleration);
		bool accelLimitCombat = IsEnabled(CustomComboPreset.RedMageSmartcastSingleAccelerationCombat);
		bool allowAccel = accelerate && (fighting || !accelLimitCombat);
		bool accelWeave = allowAccel && IsEnabled(CustomComboPreset.RedMageSmartcastSingleAccelerationWeave);
		bool accelMove = allowAccel && IsEnabled(CustomComboPreset.RedMageSmartcastSingleAccelerationMoving);
		bool accelNoNormal = IsEnabled(CustomComboPreset.RedMageSmartcastSingleAccelerationNoOverride);

		if (smartWeave) {
			// This is basically universal.
			// I know it's a mess. Moving it into another method was basically the best I could do, since the whole thing is duplicated for weaving and moving but with different variables.
			bool
				engageCheck = IsEnabled(CustomComboPreset.RedMageSmartcastSingleWeaveMelee) && weaving,
				holdOneEngageCharge = IsEnabled(CustomComboPreset.RedMageSmartcastSingleWeaveMeleeHoldOne),
				engageEarly = IsEnabled(CustomComboPreset.RedMageSmartcastSingleWeaveMeleeFirst);
			uint alt = noCastingSubCheck(level, engageCheck, holdOneEngageCharge, engageEarly, targeting && isClose, accelWeave);
			if (alt > 0)
				return alt;
		}
		if (instacasting) {
			// TODO: need to account for hardcasting spells with no cast time!

			if (level is < RDM.Levels.Veraero and >= RDM.Levels.Verthunder)
				return RDM.Verthunder;

			if (verfireUp == verstoneUp) {

				// Either both procs are already up or neither is - use whatever gives us the mana we need
				if (black < white || Math.Min(100, white + LONG_DELTA) == black)
					return OriginalHook(RDM.Verthunder);

				if (white < black || Math.Min(100, black + LONG_DELTA) == white)
					return OriginalHook(RDM.Veraero);

				// If mana levels are equal, prioritise the colour that the original button was
				return actionID is RDM.Verstone
					? OriginalHook(RDM.Veraero)
					: OriginalHook(RDM.Verthunder);
			}

			if (verfireUp) {

				// If Veraero is feasible, use it
				if (white + LONG_DELTA <= whiteThreshold && Math.Min(100, white + LONG_DELTA) != black)
					return OriginalHook(RDM.Veraero);

				return OriginalHook(RDM.Verthunder);
			}

			if (verstoneUp) {

				// If Verthunder is feasible, use it
				if (black + LONG_DELTA <= blackThreshold && Math.Min(100, black + LONG_DELTA) != white)
					return OriginalHook(RDM.Verthunder);

				return OriginalHook(RDM.Veraero);
			}
		}
		if (meleeCombo) {
			// If we're out of range while in the combo, become Corps-a-corps to get back in range. Otherwise, just run the combo.

			if (!(targeting && isClose))
				return RDM.Corpsacorps;

			if (lastComboActionId is RDM.EnchantedZwerchhau or RDM.Zwerchhau && level >= RDM.Levels.Redoublement && black >= RDM.ManaCostRedoublement && white >= RDM.ManaCostRedoublement)
				return OriginalHook(RDM.EnchantedRedoublement);
			if (lastComboActionId is RDM.EnchantedRiposte or RDM.Riposte && level >= RDM.Levels.Zwerchhau && black >= RDM.ManaCostZwerchhau && white >= RDM.ManaCostZwerchhau)
				return OriginalHook(RDM.EnchantedZwerchhau);
		}
		if (shouldCloseGap) {
			// If this is the case, then startMelee CANNOT be, because one requires isClose and one requires !isClose, so the order of these two doesn't really matter.
			// I decided to put it here because logically, you need to close before you can melee.
			return RDM.Corpsacorps;
		}
		if (isFinishing1) {
			// First finisher - have to make a Smart Decision here. Remember, we do the thinking so you don't have to! :P

			if (black >= white && canFinishWhite) {

				// If we can already Verstone, but we can't Verfire, and Verflare WON'T imbalance us, use Verflare
				if (verstoneUp && !verfireUp && (black + FINISHER_DELTA <= blackThreshold))
					return RDM.Verflare;

				return RDM.Verholy;
			}

			// If we can already Verfire, but we can't Verstone, and we can use Verholy, and it WON'T imbalance us, use Verholy
			if (verfireUp && !verstoneUp && canFinishWhite && (white + FINISHER_DELTA <= whiteThreshold))
				return RDM.Verholy;

			// If all else fails, just Verflare
			return RDM.Verflare;
		}
		if (isFinishing2) {
			// Finisher combo, simple chain.
			return RDM.Scorch;
		}
		if (isFinishing3) {
			// Second verse, same as the first!
			return RDM.Resolution;
		}
		if (startMelee) {
			// Do we allow becoming spells from within the combo? They'll break the combo, but sometimes the boss moves out of melee range of the whole arena.
			// As it's coded now, you can do it, so you have to pay attention to your distance.
			return RDM.EnchantedRiposte;
		}
		if (smartMove) {
			// Can't slowcast spells if you're moving, so we have to fall back to instants.
			// I know it's a mess. Moving it into another method was basically the best I could do, since the whole thing is duplicated for weaving and moving but with different variables.
			bool
				engageCheck = IsEnabled(CustomComboPreset.RedMageSmartcastSingleMovementMelee) && moving,
				holdOneEngageCharge = IsEnabled(CustomComboPreset.RedMageSmartcastSingleMovementMeleeHoldOne),
				engageEarly = IsEnabled(CustomComboPreset.RedMageSmartcastSingleMovementMeleeFirst);
			uint alt = noCastingSubCheck(level, engageCheck, holdOneEngageCharge, engageEarly, targeting && isClose, accelMove);
			if (alt > 0)
				return alt;
		}

		// Stand fast, slow cast!

		if (verfireUp && verstoneUp) {

			// Decide by mana levels
			if (black < white || Math.Min(100, white + PROC_DELTA) == black)
				return RDM.Verfire;

			if (white < black || Math.Min(100, black + PROC_DELTA) == white)
				return RDM.Verstone;

			// If mana levels are equal, prioritise the original button
			return actionID;
		}

		// Only use Verfire if it won't imbalance us
		if (verfireUp && black + PROC_DELTA <= blackThreshold)
			return RDM.Verfire;

		// Only use Verstone if it won't imbalance us
		if (verstoneUp && white + PROC_DELTA <= whiteThreshold)
			return RDM.Verstone;

		// If there's NOTHING up right to use, should we override with Accleration (or Swiftcast)?
		if (allowAccel && !accelNoNormal) {
			bool canAccelerate = level >= RDM.Levels.Acceleration && CanUse(RDM.Acceleration);
			bool canSwiftcast = IsEnabled(CustomComboPreset.RedMageSmartcastSingleAccelerationSwiftcast) && CanUse(Common.Swiftcast);

			if (canSwiftcast && IsEnabled(CustomComboPreset.RedMageSmartcastSingleAccelerationSwiftcastFirst))
				return Common.Swiftcast;
			if (canAccelerate)
				return RDM.Acceleration;
			if (canSwiftcast)
				return Common.Swiftcast;
		}

		// Finally, if all else fails, become Jolt (II)
		return OriginalHook(RDM.Jolt2);
	}
}

internal class RedMageAcceleration: CustomCombo {
	public override CustomComboPreset Preset { get; } = CustomComboPreset.RedMageAccelerationSwiftcastFeature;
	public override uint[] ActionIDs => new[] { RDM.Acceleration };

	protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level) {

		if (level >= RDM.Levels.Acceleration) {
			bool canAccelerate = CanUse(RDM.Acceleration);
			bool canSwiftcast = CanUse(Common.Swiftcast);

			if (IsEnabled(CustomComboPreset.RedMageAccelerationSwiftcastOption) && canAccelerate && canSwiftcast)
				return Common.Swiftcast;

			if (canAccelerate)
				return RDM.Acceleration;

			if (canSwiftcast)
				return Common.Swiftcast;

			return RDM.Acceleration;
		}

		return Common.Swiftcast;
	}
}

internal class RedMageManafication: CustomCombo {
	public override CustomComboPreset Preset { get; } = CustomComboPreset.RedMageManaficationFeature;
	public override uint[] ActionIDs => new[] { RDM.Manafication };

	protected override uint Invoke(uint actionID, uint lastComboActionId, float comboTime, byte level) {
		RDMGauge gauge = GetJobGauge<RDMGauge>();
		byte
			black = gauge.BlackMana,
			white = gauge.WhiteMana;
		bool
			combo = lastComboActionId is RDM.EnchantedRiposte or RDM.Riposte or RDM.EnchantedZwerchhau or RDM.Zwerchhau;

		if (combo || (black > 50 && white > 50) || (IsEnabled(CustomComboPreset.RedMageManaficationFeatureConservative) && (black > 50 || white > 50))) {
			if (IsEnabled(CustomComboPreset.RedMageMeleeComboCloser) && HasTarget && !InMeleeRange)
				return RDM.Corpsacorps;

			if (lastComboActionId is RDM.EnchantedZwerchhau or RDM.Zwerchhau && level >= RDM.Levels.Redoublement && black >= RDM.ManaCostRedoublement && white >= RDM.ManaCostRedoublement)
				return OriginalHook(RDM.EnchantedRedoublement);
			if (lastComboActionId is RDM.EnchantedRiposte or RDM.Riposte && level >= RDM.Levels.Zwerchhau && black >= RDM.ManaCostZwerchhau && white >= RDM.ManaCostZwerchhau)
				return OriginalHook(RDM.EnchantedZwerchhau);

			return OriginalHook(RDM.EnchantedRiposte);
		}

		return actionID;
	}
}

internal class RedMageGapControl: CustomCombo {
	public override CustomComboPreset Preset { get; } = CustomComboPreset.RdmAny;
	public override uint[] ActionIDs => new[] { RDM.Corpsacorps, RDM.Displacement };

	protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level) {
		if (level < RDM.Levels.Displacement)
			return actionID;

		if ((actionID is RDM.Corpsacorps && IsEnabled(CustomComboPreset.RedMageMeleeGapReverserBackstep)) || (actionID is RDM.Displacement && IsEnabled(CustomComboPreset.RedMageMeleeGapReverserLunge))) {
			if (HasTarget)
				return InMeleeRange ? RDM.Displacement : RDM.Corpsacorps;
		}

		return actionID;
	}
}
