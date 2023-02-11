namespace XIVExtraComboVX.Combos;

using System;
using System.Linq;

using Dalamud.Game.ClientState.JobGauge.Enums;
using Dalamud.Game.ClientState.JobGauge.Types;

internal static class MNK {
	public const byte ClassID = 2;
	public const byte JobID = 20;

	public const uint
		Bootshine = 53,
		TrueStrike = 54,
		SnapPunch = 56,
		TwinSnakes = 61,
		ArmOfTheDestroyer = 62,
		Demolish = 66,
		PerfectBalance = 69,
		Rockbreaker = 70,
		DragonKick = 74,
		Meditation = 3546,
		RiddleOfEarth = 7394,
		RiddleOfFire = 7395,
		Brotherhood = 7396,
		Bloodbath = 7542,
		FourPointFury = 16473,
		Enlightenment = 16474,
		HowlingFist = 25763,
		MasterfulBlitz = 25764,
		RiddleOfWind = 25766,
		ShadowOfTheDestroyer = 25767;

	public static class Buffs {
		public const ushort
			OpoOpoForm = 107,
			RaptorForm = 108,
			CoerlForm = 109,
			PerfectBalance = 110,
			FifthChakra = 797,
			LeadenFist = 1861,
			Brotherhood = 1185,
			RiddleOfFire = 1181,
			FormlessFist = 2513,
			DisciplinedFist = 3001;
	}

	public static class Debuffs {
		public const ushort
			Demolish = 246;
	}

	public static class Levels {
		public const byte
			TrueStrike = 4,
			SnapPunch = 6,
			Bloodbath = 12,
			Meditation = 15,
			TwinSnakes = 18,
			ArmOfTheDestroyer = 26,
			Rockbreaker = 30,
			Demolish = 30,
			FourPointFury = 45,
			HowlingFist = 40,
			DragonKick = 50,
			PerfectBalance = 50,
			FormShift = 52,
			EnhancedPerfectBalance = 60,
			MasterfulBlitz = 60,
			RiddleOfEarth = 64,
			RiddleOfFire = 68,
			Brotherhood = 70,
			Enlightenment = 70,
			RiddleOfWind = 72,
			ShadowOfTheDestroyer = 82;
	}
}

internal class MonkAoECombo: CustomCombo {
	public override CustomComboPreset Preset { get; } = CustomComboPreset.MonkAoECombo;

	protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level) {

		if (actionID is MNK.ArmOfTheDestroyer or MNK.ShadowOfTheDestroyer) {
			if (!IsEnabled(CustomComboPreset.MonkAoECombo_Destroyers))
				return actionID;
		}
		else if (actionID is MNK.MasterfulBlitz) {
			if (!IsEnabled(CustomComboPreset.MonkAoECombo_MasterBlitz))
				return actionID;
		}
		else if (actionID is MNK.Rockbreaker) {
			if (!IsEnabled(CustomComboPreset.MonkAoECombo_Rockbreaker))
				return actionID;
		}
		else {
			return actionID;
		}

		if (level >= MNK.Levels.HowlingFist) {
			if (InCombat && HasTarget && CanWeave(actionID) && SelfHasEffect(MNK.Buffs.FifthChakra)) {
				uint real = OriginalHook(MNK.HowlingFist);
				if (CanUse(real))
					return real;
			}
		}

		MNKGauge gauge = GetJobGauge<MNKGauge>();

		// Blitz
		if (level >= MNK.Levels.MasterfulBlitz && !gauge.BeastChakra.Contains(BeastChakra.NONE))
			return OriginalHook(MNK.MasterfulBlitz);

		if (level >= MNK.Levels.PerfectBalance && SelfHasEffect(MNK.Buffs.PerfectBalance)) {

			// Solar
			if (level >= MNK.Levels.EnhancedPerfectBalance && !gauge.Nadi.HasFlag(Nadi.SOLAR)) {
				if (level >= MNK.Levels.FourPointFury && !gauge.BeastChakra.Contains(BeastChakra.RAPTOR))
					return MNK.FourPointFury;

				if (level >= MNK.Levels.Rockbreaker && !gauge.BeastChakra.Contains(BeastChakra.COEURL))
					return MNK.Rockbreaker;

				if (level >= MNK.Levels.ArmOfTheDestroyer && !gauge.BeastChakra.Contains(BeastChakra.OPOOPO))
					// Shadow of the Destroyer
					return OriginalHook(MNK.ArmOfTheDestroyer);

				return level >= MNK.Levels.ShadowOfTheDestroyer
					? MNK.ShadowOfTheDestroyer
					: MNK.Rockbreaker;
			}

			// Lunar.  Also used if we have both Nadi as Tornado Kick/Phantom Rush isn't picky, or under 60.
			return level >= MNK.Levels.ShadowOfTheDestroyer
				? MNK.ShadowOfTheDestroyer
				: MNK.Rockbreaker;
		}

		// FPF with FormShift
		if (level >= MNK.Levels.FormShift && SelfHasEffect(MNK.Buffs.FormlessFist)) {
			if (level >= MNK.Levels.FourPointFury)
				return MNK.FourPointFury;
		}

		// 1-2-3 combo
		if (level >= MNK.Levels.FourPointFury && SelfHasEffect(MNK.Buffs.RaptorForm))
			return MNK.FourPointFury;

		if (level >= MNK.Levels.ArmOfTheDestroyer && SelfHasEffect(MNK.Buffs.OpoOpoForm))
			// Shadow of the Destroyer
			return OriginalHook(MNK.ArmOfTheDestroyer);

		if (level >= MNK.Levels.Rockbreaker && SelfHasEffect(MNK.Buffs.CoerlForm))
			return MNK.Rockbreaker;

		// Shadow of the Destroyer
		return OriginalHook(MNK.ArmOfTheDestroyer);
	}
}

internal class MonkSTCombo: CustomCombo {
	public override CustomComboPreset Preset { get; } = CustomComboPreset.MonkSTCombo;
	public override uint[] ActionIDs => new[] { MNK.Bootshine };

	// All credit to Evolutious on the github - they wrote the code themselves and sent it to me.
	// All I did was adjust the style to better fit the rest of the plugin, and change a few hardcoded values to adjustable ones.
	// Update post-6.2: I've also integrated a few other combos better and added one that was missing.
	protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level) {
		MNKGauge gauge = GetJobGauge<MNKGauge>();

		// *** oGCD Rotation ***
		if (CanWeave(MNK.Bootshine)) {

			// Bloodbath
			if (level >= MNK.Levels.Bloodbath) {
				if (IsOffCooldown(MNK.Bloodbath) && PlayerHealthPercentage < Service.Configuration.MonkBloodbathHealthPercentage)
					return MNK.Bloodbath;
			}

			// Riddle of Earth
			if (level >= MNK.Levels.RiddleOfEarth) {
				if (IsOffCooldown(MNK.RiddleOfEarth) && PlayerHealthPercentage < Service.Configuration.MonkRiddleOfEarthHealthPercentage)
					return MNK.RiddleOfEarth;
			}

			// Steel Peak/The Forbidden Chakra
			if (level >= MNK.Levels.Meditation) {
				if (gauge.Chakra == 5 && InCombat)
					return OriginalHook(MNK.Meditation);
			}

			// Perfect Balance
			if (level >= MNK.Levels.PerfectBalance && !SelfHasEffect(MNK.Buffs.PerfectBalance)) {
				// These all go to the same codepath, but combining them would be some eldritch frankencondition, so they're kept separate for readability. -PrincessRTFM
				if (level >= MNK.Levels.Brotherhood) {
					if (SelfHasEffect(MNK.Buffs.RiddleOfFire) && SelfEffectDuration(MNK.Buffs.RiddleOfFire) < 10.0 && !SelfHasEffect(MNK.Buffs.FormlessFist) && SelfHasEffect(MNK.Buffs.DisciplinedFist) && SelfHasEffect(MNK.Buffs.Brotherhood))
						return MNK.PerfectBalance;
				}
				else if (level >= MNK.Levels.RiddleOfFire) {
					if (SelfHasEffect(MNK.Buffs.RiddleOfFire) && SelfEffectDuration(MNK.Buffs.RiddleOfFire) >= 10.0 && !SelfHasEffect(MNK.Buffs.FormlessFist) && SelfHasEffect(MNK.Buffs.DisciplinedFist))
						return MNK.PerfectBalance;
				}
				else if (level >= MNK.Levels.FormShift) {
					if (level < MNK.Levels.RiddleOfFire && !SelfHasEffect(MNK.Buffs.FormlessFist) && SelfHasEffect(MNK.Buffs.DisciplinedFist))
						return MNK.PerfectBalance;
				}
				else if (level < MNK.Levels.FormShift) {
					if (SelfHasEffect(MNK.Buffs.DisciplinedFist))
						return MNK.PerfectBalance;
				}
			}

			// Riddle of Fire
			if (level >= MNK.Levels.RiddleOfFire) {
				if (!IsOnCooldown(MNK.RiddleOfFire) && SelfHasEffect(MNK.Buffs.DisciplinedFist))
					return MNK.RiddleOfFire;
			}

			// Brotherhood
			if (level >= MNK.Levels.Brotherhood) {
				if (SelfHasEffect(MNK.Buffs.PerfectBalance) && !IsOnCooldown(MNK.Brotherhood) && (gauge.BeastChakra.Contains(BeastChakra.RAPTOR) || gauge.BeastChakra.Contains(BeastChakra.COEURL) || gauge.BeastChakra.Contains(BeastChakra.OPOOPO)))
					return MNK.Brotherhood;
			}

			// Riddle of Wind
			if (level >= MNK.Levels.RiddleOfWind) {
				if (!IsOnCooldown(MNK.RiddleOfWind) && IsOnCooldown(MNK.RiddleOfFire))
					return MNK.RiddleOfWind;
			}

		}

		// Masterful Blitz
		if (level >= MNK.Levels.MasterfulBlitz) {
			if (!gauge.BeastChakra.Contains(BeastChakra.NONE))
				return OriginalHook(MNK.MasterfulBlitz);
		}

		// Master's Gauge
		if (level >= MNK.Levels.PerfectBalance) {
			if (SelfHasEffect(MNK.Buffs.PerfectBalance)) {

				// Solar
				if (level >= MNK.Levels.EnhancedPerfectBalance) {
					if (!gauge.Nadi.HasFlag(Nadi.SOLAR)) {

						if (!gauge.BeastChakra.Contains(BeastChakra.RAPTOR)) {
							return IsEnabled(CustomComboPreset.MonkTwinSnakesFeature) && SelfEffectDuration(MNK.Buffs.DisciplinedFist) > Service.Configuration.MonkTwinSnakesBuffTime
								? MNK.TrueStrike
								: MNK.TwinSnakes;
						}

						if (!gauge.BeastChakra.Contains(BeastChakra.COEURL)) {
							return level < MNK.Levels.Demolish || (IsEnabled(CustomComboPreset.MonkDemolishFeature) && TargetOwnEffectDuration(MNK.Debuffs.Demolish) > Service.Configuration.MonkDemolishDebuffTime)
								? MNK.SnapPunch
								: MNK.Demolish;
						}

						if (!gauge.BeastChakra.Contains(BeastChakra.OPOOPO)) {
							return level < MNK.Levels.DragonKick || SelfHasEffect(MNK.Buffs.LeadenFist)
								? MNK.Bootshine
								: MNK.DragonKick;
						}

						return level >= MNK.Levels.DragonKick
							? MNK.DragonKick
							: MNK.Bootshine;
					}
				}

				// Lunar.  Also used if we have both Nadi as Tornado Kick/Phantom Rush isn't picky, or under 60.
				return level < MNK.Levels.DragonKick || SelfHasEffect(MNK.Buffs.LeadenFist)
					? MNK.Bootshine
					: MNK.DragonKick;
			}
		}

		// 1-2-3 combo
		if (level >= MNK.Levels.TrueStrike) {
			if (SelfHasEffect(MNK.Buffs.RaptorForm) || SelfHasEffect(MNK.Buffs.FormlessFist)) {
				return level < MNK.Levels.TwinSnakes || (IsEnabled(CustomComboPreset.MonkTwinSnakesFeature) && SelfEffectDuration(MNK.Buffs.DisciplinedFist) > Service.Configuration.MonkTwinSnakesBuffTime)
					? MNK.TrueStrike
					: MNK.TwinSnakes;
			}
		}

		if (level >= MNK.Levels.SnapPunch) {
			if (SelfHasEffect(MNK.Buffs.CoerlForm)) {
				return level < MNK.Levels.Demolish || (IsEnabled(CustomComboPreset.MonkDemolishFeature) && TargetOwnEffectDuration(MNK.Debuffs.Demolish) > Service.Configuration.MonkDemolishDebuffTime)
					? MNK.SnapPunch
					: MNK.Demolish;
			}
		}

		if (SelfHasEffect(MNK.Buffs.OpoOpoForm)) {
			return level < MNK.Levels.DragonKick || SelfHasEffect(MNK.Buffs.LeadenFist)
				? MNK.Bootshine
				: MNK.DragonKick;
		}

		// Dragon Kick
		return level < MNK.Levels.DragonKick || SelfHasEffect(MNK.Buffs.LeadenFist)
			? MNK.Bootshine
			: MNK.DragonKick;
	}
}

internal class MonkHowlingFistEnlightenment: CustomCombo {
	public override CustomComboPreset Preset { get; } = CustomComboPreset.MonkHowlingFistMeditationFeature;
	public override uint[] ActionIDs => new[] { MNK.HowlingFist, MNK.Enlightenment };

	protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level) {
		MNKGauge gauge = GetJobGauge<MNKGauge>();

		if (level >= MNK.Levels.Meditation && gauge.Chakra < 5)
			return MNK.Meditation;

		return actionID;
	}
}

internal class MonkDragonKick: CustomCombo {
	public override CustomComboPreset Preset { get; } = CustomComboPreset.MnkAny;
	public override uint[] ActionIDs => new[] { MNK.DragonKick };

	protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level) {
		MNKGauge gauge = GetJobGauge<MNKGauge>();

		if (IsEnabled(CustomComboPreset.MonkDragonKickMeditationFeature)) {
			if (level >= MNK.Levels.Meditation && gauge.Chakra < 5 && !InCombat)
				return MNK.Meditation;
		}

		if (IsEnabled(CustomComboPreset.MonkDragonKickSteelPeakFeature)) {
			if (level >= MNK.Levels.Meditation && gauge.Chakra == 5 && InCombat)
				return OriginalHook(MNK.Meditation);
		}

		if (IsEnabled(CustomComboPreset.MonkDragonKickBalanceFeature)) {
			if (level >= MNK.Levels.MasterfulBlitz && !gauge.BeastChakra.Contains(BeastChakra.NONE))
				return OriginalHook(MNK.MasterfulBlitz);
		}

		if (IsEnabled(CustomComboPreset.MonkBootshineFeature)) {
			if (level < MNK.Levels.DragonKick || SelfHasEffect(MNK.Buffs.LeadenFist))
				return MNK.Bootshine;
		}

		return actionID;
	}
}

internal class MonkTwinSnakes: CustomCombo {
	public override CustomComboPreset Preset { get; } = CustomComboPreset.MonkTwinSnakesFeature;
	public override uint[] ActionIDs => new[] { MNK.TwinSnakes };

	protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level) {

		if (level < MNK.Levels.TwinSnakes || SelfEffectDuration(MNK.Buffs.DisciplinedFist) > Service.Configuration.MonkTwinSnakesBuffTime)
			return MNK.TrueStrike;

		return actionID;
	}
}

internal class MonkDemolish: CustomCombo {
	public override CustomComboPreset Preset { get; } = CustomComboPreset.MonkDemolishFeature;
	public override uint[] ActionIDs => new[] { MNK.Demolish };

	protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level) {

		if (level < MNK.Levels.Demolish || TargetOwnEffectDuration(MNK.Debuffs.Demolish) > Service.Configuration.MonkDemolishDebuffTime)
			return MNK.SnapPunch;

		return actionID;
	}
}

internal class MonkPerfectBalance: CustomCombo {
	public override CustomComboPreset Preset { get; } = CustomComboPreset.MonkPerfectBalanceFeature;
	public override uint[] ActionIDs => new[] { MNK.PerfectBalance };

	protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level) {

		if (level >= MNK.Levels.MasterfulBlitz && (!GetJobGauge<MNKGauge>().BeastChakra.Contains(BeastChakra.NONE) || SelfHasEffect(MNK.Buffs.PerfectBalance)))
			// Chakra actions
			return OriginalHook(MNK.MasterfulBlitz);

		return actionID;
	}
}

internal class MonkRiddleOfFire: CustomCombo {
	public override CustomComboPreset Preset { get; } = CustomComboPreset.MnkAny;
	public override uint[] ActionIDs => new[] { MNK.RiddleOfFire };

	protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level) {

		if (IsEnabled(CustomComboPreset.MonkBrotherlyFire)) {
			if (level >= MNK.Levels.Brotherhood && IsOffCooldown(MNK.Brotherhood) && IsOnCooldown(MNK.RiddleOfFire))
				return MNK.Brotherhood;
		}

		if (IsEnabled(CustomComboPreset.MonkFireWind)) {
			if (level >= MNK.Levels.RiddleOfWind && IsOffCooldown(MNK.RiddleOfWind) && IsOnCooldown(MNK.RiddleOfFire))
				return MNK.RiddleOfWind;
		}

		return actionID;
	}
}
