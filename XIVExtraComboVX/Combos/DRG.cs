namespace XIVExtraComboVX.Combos;

using Dalamud.Game.ClientState.JobGauge.Types;

internal static class DRG {
	public const byte JobID = 22;

	public const uint
		// Single Target
		TrueThrust = 75,
		VorpalThrust = 78,
		Disembowel = 87,
		FullThrust = 84,
		ChaosThrust = 88,
		HeavensThrust = 25771,
		ChaoticSpring = 25772,
		WheelingThrust = 3556,
		FangAndClaw = 3554,
		RaidenThrust = 16479,
		// AoE
		DoomSpike = 86,
		SonicThrust = 7397,
		CoerthanTorment = 16477,
		DraconianFury = 25770,
		// Combined
		Geirskogul = 3555,
		Nastrond = 7400,
		// Jumps
		Jump = 92,
		SpineshatterDive = 95,
		DragonfireDive = 96,
		HighJump = 16478,
		MirageDive = 7399,
		// Dragon
		Stardiver = 16480,
		WyrmwindThrust = 25773;

	public static class Buffs {
		public const ushort
			SharperFangAndClaw = 802,
			EnhancedWheelingThrust = 803,
			DiveReady = 1243,
			PowerSurge = 2720;
	}

	public static class Debuffs {
		// public const ushort placeholder = 0;
	}

	public static class Levels {
		public const byte
			VorpalThrust = 4,
			Disembowel = 18,
			FullThrust = 26,
			SpineshatterDive = 45,
			DragonfireDive = 50,
			ChaosThrust = 50,
			HeavensThrust = 86,
			ChaoticSpring = 86,
			FangAndClaw = 56,
			WheelingThrust = 58,
			Geirskogul = 60,
			SonicThrust = 62,
			MirageDive = 68,
			LifeOfTheDragon = 70,
			CoerthanTorment = 72,
			HighJump = 74,
			RaidenThrust = 76,
			Stardiver = 80;
	}
}

internal class DragoonCoerthanTorment: CustomCombo {
	public override CustomComboPreset Preset => CustomComboPreset.DrgAny;
	public override uint[] ActionIDs { get; } = new[] { DRG.CoerthanTorment };

	protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level) {

		if (IsEnabled(CustomComboPreset.DragoonCoerthanWyrmwindFeature)) {
			if (GetJobGauge<DRGGauge>().FirstmindsFocusCount == 2)
				return DRG.WyrmwindThrust;
		}

		if (IsEnabled(CustomComboPreset.DragoonCoerthanTormentCombo)) {

			if (comboTime > 0) {

				if (level >= DRG.Levels.SonicThrust && lastComboMove is DRG.DoomSpike or DRG.DraconianFury)
					return DRG.SonicThrust;

				if (level >= DRG.Levels.CoerthanTorment && lastComboMove is DRG.SonicThrust)
					return DRG.CoerthanTorment;
			}

			return OriginalHook(DRG.DoomSpike);
		}

		return actionID;
	}
}

internal class DragoonChaosThrust: CustomCombo {
	public override CustomComboPreset Preset => CustomComboPreset.DrgAny;
	public override uint[] ActionIDs { get; } = new[] { DRG.ChaosThrust, DRG.ChaoticSpring };

	protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level) {

		if (IsEnabled(CustomComboPreset.DragoonFangThrustFeature)) {
			if (level >= DRG.Levels.FangAndClaw) {
				if (SelfHasEffect(DRG.Buffs.SharperFangAndClaw) || SelfHasEffect(DRG.Buffs.EnhancedWheelingThrust))
					return DRG.WheelingThrust;
			}
		}

		if (IsEnabled(CustomComboPreset.DragoonChaosThrustCombo)) {

			if (level >= DRG.Levels.FangAndClaw && SelfHasEffect(DRG.Buffs.SharperFangAndClaw))
				return DRG.FangAndClaw;

			if (level >= DRG.Levels.WheelingThrust && SelfHasEffect(DRG.Buffs.EnhancedWheelingThrust))
				return DRG.WheelingThrust;

			if (comboTime > 0) {

				if (lastComboMove is DRG.Disembowel && level >= DRG.Levels.ChaosThrust)
					return OriginalHook(DRG.ChaosThrust);

				if (lastComboMove is DRG.TrueThrust or DRG.RaidenThrust && level >= DRG.Levels.Disembowel)
					return DRG.Disembowel;

			}

			return IsEnabled(CustomComboPreset.DragoonChaosThrustLateOption)
				? DRG.Disembowel
				: OriginalHook(DRG.TrueThrust);
		}

		return OriginalHook(actionID);
	}
}

internal class DragoonFullThrustCombo: CustomCombo {
	public override CustomComboPreset Preset => CustomComboPreset.DrgAny;
	public override uint[] ActionIDs { get; } = new[] { DRG.FullThrust, DRG.HeavensThrust };

	protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level) {

		if (IsEnabled(CustomComboPreset.DragoonFangThrustFeature)
			&& level >= DRG.Levels.FangAndClaw
			&& (SelfHasEffect(DRG.Buffs.SharperFangAndClaw) || SelfHasEffect(DRG.Buffs.EnhancedWheelingThrust))
		) {
			return DRG.FangAndClaw;
		}

		if (IsEnabled(CustomComboPreset.DragoonFullThrustCombo)) {

			if (level >= DRG.Levels.WheelingThrust && SelfHasEffect(DRG.Buffs.EnhancedWheelingThrust))
				return DRG.WheelingThrust;

			if (level >= DRG.Levels.FangAndClaw && SelfHasEffect(DRG.Buffs.SharperFangAndClaw))
				return DRG.FangAndClaw;

			if (comboTime > 0) {

				if (lastComboMove is DRG.TrueThrust or DRG.RaidenThrust) {
					if (IsEnabled(CustomComboPreset.DragoonFullThrustBuffSaver)) {
						if (level >= DRG.Levels.Disembowel) {
							if (SelfEffectDuration(DRG.Buffs.PowerSurge) < Service.Configuration.DragoonFullThrustBuffSaverBuffTime)
								return DRG.Disembowel;
						}
					}
					if (level >= DRG.Levels.VorpalThrust)
						return DRG.VorpalThrust;
				}

				if (lastComboMove is DRG.VorpalThrust) {
					if (level >= DRG.Levels.FullThrust)
						return OriginalHook(DRG.FullThrust);
				}

			}

			return IsEnabled(CustomComboPreset.DragoonFullThrustLateOption)
				? DRG.VorpalThrust
				: OriginalHook(DRG.TrueThrust);
		}

		return OriginalHook(actionID);
	}
}

internal class DragoonStardiver: CustomCombo {
	public override CustomComboPreset Preset { get; } = CustomComboPreset.DrgAny;
	public override uint[] ActionIDs { get; } = new[] { DRG.Stardiver };

	protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level) {
		DRGGauge gauge = GetJobGauge<DRGGauge>();

		if (IsEnabled(CustomComboPreset.DragoonStardiverNastrondFeature)) {
			if (level >= DRG.Levels.Geirskogul && (!gauge.IsLOTDActive || IsOffCooldown(DRG.Nastrond) || IsOnCooldown(DRG.Stardiver)))
				return OriginalHook(DRG.Geirskogul);
		}

		if (IsEnabled(CustomComboPreset.DragoonStardiverDragonfireDiveFeature)) {
			if (level < DRG.Levels.Stardiver || !gauge.IsLOTDActive || IsOnCooldown(DRG.Stardiver) || (IsOffCooldown(DRG.DragonfireDive) && gauge.LOTDTimer > 7.5))
				return DRG.DragonfireDive;
		}

		return actionID;
	}
}

internal class DragoonDiveFeature: CustomCombo {
	public override CustomComboPreset Preset { get; } = CustomComboPreset.DragoonDiveFeature;
	public override uint[] ActionIDs { get; } = new[] { DRG.SpineshatterDive, DRG.DragonfireDive, DRG.Stardiver };

	protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level) {

		if (level >= DRG.Levels.Stardiver && GetJobGauge<DRGGauge>().IsLOTDActive)
			return PickByCooldown(actionID, DRG.SpineshatterDive, DRG.DragonfireDive, DRG.Stardiver);

		if (level >= DRG.Levels.DragonfireDive)
			return PickByCooldown(actionID, DRG.SpineshatterDive, DRG.DragonfireDive);

		return DRG.SpineshatterDive;
	}
}

internal class DragoonMirageJumpFeature: CustomCombo {
	public override CustomComboPreset Preset { get; } = CustomComboPreset.DragoonMirageJumpFeature;
	public override uint[] ActionIDs { get; } = new[] { DRG.Jump, DRG.HighJump };

	protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level) {

		if (level >= DRG.Levels.MirageDive && SelfHasEffect(DRG.Buffs.DiveReady))
			return DRG.MirageDive;

		return actionID;
	}
}
