namespace XIVExtraComboVX.Combos;

using Dalamud.Game.ClientState.JobGauge.Types;

internal static class SCH {
	public const byte JobID = 28;

	public const uint
		Resurrection = 173,
		Aetherflow = 166,
		EnergyDrain = 167,
		SacredSoil = 188,
		Lustrate = 189,
		Indomitability = 3583,
		DeploymentTactics = 3585,
		EmergencyTactics = 3586,
		Dissipation = 3587,
		Excogitation = 7434,
		ChainStratagem = 7436,
		Aetherpact = 7437,
		WhisperingDawn = 16537,
		FeyIllumination = 16538,
		Recitation = 16542,
		FeyBless = 16543,
		SummonSeraph = 16545,
		Consolation = 16546,
		SummonEos = 17215,
		SummonSelene = 17216,
		Ruin2 = 17870;

	public static class Buffs {
		public const ushort
			Dissipation = 791,
			Recitation = 1896;
	}

	public static class Debuffs {
		// public const ushort placeholder = 0;
	}

	public static class Levels {
		public const byte
			Aetherflow = 45,
			Lustrate = 45,
			Excogitation = 62,
			ChainStratagem = 66,
			Recitation = 74,
			Consolation = 80,
			SummonSeraph = 80;
	}
}

internal class ScholarSwiftcastRaiserFeature: SwiftRaiseCombo {
	public override CustomComboPreset Preset => CustomComboPreset.ScholarSwiftcastRaiserFeature;
	public override uint[] ActionIDs { get; } = new[] { SCH.Resurrection };
}

internal class ScholarFeyBless: CustomCombo {
	public override CustomComboPreset Preset => CustomComboPreset.ScholarSeraphConsolationFeature;
	public override uint[] ActionIDs { get; } = new[] { SCH.FeyBless };

	protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level) {

		if (level >= SCH.Levels.Consolation && GetJobGauge<SCHGauge>().SeraphTimer > 0)
			return SCH.Consolation;

		return actionID;
	}
}

internal class ScholarExcogitation: CustomCombo {
	public override CustomComboPreset Preset => CustomComboPreset.ScholarExcogFallbackFeature;
	public override uint[] ActionIDs { get; } = new[] { SCH.Excogitation };

	protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level) {

		if (IsEnabled(CustomComboPreset.ScholarExcogitationRecitationFeature)) {
			if (level >= SCH.Levels.Recitation && IsOffCooldown(SCH.Recitation))
				return SCH.Recitation;
		}

		if (IsEnabled(CustomComboPreset.ScholarExcogFallbackFeature)) {
			if (level < SCH.Levels.Excogitation || IsOnCooldown(SCH.Excogitation))
				return SCH.Lustrate;
		}

		return actionID;
	}
}

internal class ScholarEnergyDrain: CustomCombo {
	public override CustomComboPreset Preset => CustomComboPreset.ScholarEnergyDrainAetherflowFeature;
	public override uint[] ActionIDs { get; } = new[] { SCH.EnergyDrain };

	protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level) {

		if (level >= SCH.Levels.Aetherflow && GetJobGauge<SCHGauge>().Aetherflow == 0)
			return SCH.Aetherflow;

		return actionID;
	}
}

internal class ScholarLustrate: CustomCombo {
	public override CustomComboPreset Preset { get; } = CustomComboPreset.SmnAny;
	public override uint[] ActionIDs { get; } = new[] { SCH.Lustrate };

	protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level) {

		if (IsEnabled(CustomComboPreset.ScholarLustrateRecitationFeature)) {
			if (level >= SCH.Levels.Recitation && IsOffCooldown(SCH.Recitation))
				return SCH.Recitation;
		}

		if (IsEnabled(CustomComboPreset.ScholarLustrateExcogitationFeature)) {
			if (level >= SCH.Levels.Excogitation && IsOffCooldown(SCH.Excogitation))
				return SCH.Excogitation;
		}

		if (IsEnabled(CustomComboPreset.ScholarLustrateAetherflowFeature)) {
			if (level >= SCH.Levels.Aetherflow && GetJobGauge<SCHGauge>().Aetherflow == 0)
				return SCH.Aetherflow;
		}

		return actionID;
	}
}

internal class ScholarIndomitability: CustomCombo {
	public override CustomComboPreset Preset { get; } = CustomComboPreset.ScholarIndomAetherflowFeature;
	public override uint[] ActionIDs { get; } = new[] { SCH.EnergyDrain };

	protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level) {

		if (level >= SCH.Levels.Aetherflow && GetJobGauge<SCHGauge>().Aetherflow == 0 && !SelfHasEffect(SCH.Buffs.Recitation))
			return SCH.Aetherflow;

		return actionID;
	}
}

internal class ScholarSummon: CustomCombo {
	public override CustomComboPreset Preset { get; } = CustomComboPreset.ScholarSeraphFeature;
	public override uint[] ActionIDs { get; } = new[] { SCH.SummonEos, SCH.SummonSelene };

	protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level) {
		SCHGauge gauge = GetJobGauge<SCHGauge>();

		if (gauge.SeraphTimer > 0 || HasPetPresent)
			return OriginalHook(SCH.SummonSeraph);

		return actionID;
	}
}
