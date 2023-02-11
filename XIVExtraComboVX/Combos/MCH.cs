namespace XIVExtraComboVX.Combos;

using Dalamud.Game.ClientState.JobGauge.Types;

internal static class MCH {
	public const byte JobID = 31;

	public const uint
		// Single target
		CleanShot = 2873,
		HeatedCleanShot = 7413,
		SplitShot = 2866,
		HeatedSplitShot = 7411,
		SlugShot = 2868,
		HeatedSlugshot = 7412,
		// Charges
		GaussRound = 2874,
		Ricochet = 2890,
		// AoE
		SpreadShot = 2870,
		AutoCrossbow = 16497,
		Scattergun = 25786,
		// Rook
		RookAutoturret = 2864,
		RookOverdrive = 7415,
		AutomatonQueen = 16501,
		QueenOverdrive = 16502,
		// Other
		Wildfire = 2878,
		Detonator = 16766,
		Hypercharge = 17209,
		HeatBlast = 7410,
		HotShot = 2872,
		Drill = 16498,
		Bioblaster = 16499,
		AirAnchor = 16500,
		Chainsaw = 25788;

	public static class Buffs {
		// public const ushort placeholder = 0;
	}

	public static class Debuffs {
		// public const ushort placeholder = 0;
	}

	public static class Levels {
		public const byte
			SlugShot = 2,
			GaussRound = 15,
			CleanShot = 26,
			Hypercharge = 30,
			HeatBlast = 35,
			RookOverdrive = 40,
			Wildfire = 45,
			Ricochet = 50,
			AutoCrossbow = 52,
			HeatedSplitShot = 54,
			Drill = 58,
			HeatedSlugshot = 60,
			HeatedCleanShot = 64,
			ChargedActionMastery = 74,
			AirAnchor = 76,
			QueenOverdrive = 80,
			Chainsaw = 90;
	}
}

internal class MachinistCleanShot: CustomCombo {
	public override CustomComboPreset Preset => CustomComboPreset.MachinistMainCombo;
	public override uint[] ActionIDs { get; } = new[] { MCH.CleanShot, MCH.HeatedCleanShot };

	protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level) {

		if (level >= MCH.Levels.HeatBlast && IsEnabled(CustomComboPreset.MachinistHypercombo) && GetJobGauge<MCHGauge>().IsOverheated)
			return MCH.HeatBlast;

		if (comboTime > 0) {

			if (lastComboMove is MCH.SplitShot or MCH.HeatedSplitShot && level >= MCH.Levels.SlugShot)
				return OriginalHook(MCH.SlugShot);

			if (lastComboMove is MCH.SlugShot or MCH.HeatedSlugshot && level >= MCH.Levels.CleanShot)
				return OriginalHook(MCH.CleanShot);

		}

		return OriginalHook(MCH.SplitShot);
	}
}

internal class MachinistGaussRoundRicochet: CustomCombo {
	public override CustomComboPreset Preset => CustomComboPreset.MachinistGaussRoundRicochet;
	public override uint[] ActionIDs { get; } = new[] { MCH.GaussRound, MCH.Ricochet };

	protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level) {

		if (IsEnabled(CustomComboPreset.MachinistGaussRoundRicochetLimiter) && !GetJobGauge<MCHGauge>().IsOverheated)
			return actionID;

		if (level >= MCH.Levels.Ricochet)
			return PickByCooldown(actionID, MCH.Ricochet, MCH.GaussRound);

		return MCH.GaussRound;
	}
}

internal class MachinistHyperfire: CustomCombo {
	public override CustomComboPreset Preset { get; } = CustomComboPreset.MachinistHyperfire;
	public override uint[] ActionIDs { get; } = new[] { MCH.Hypercharge };

	protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level) {

		if (level >= MCH.Levels.Wildfire && IsOffCooldown(MCH.Wildfire) && HasTarget)
			return MCH.Wildfire;

		if (level >= MCH.Levels.Wildfire && IsOnCooldown(MCH.Hypercharge) && !IsOriginal(MCH.Wildfire))
			return MCH.Detonator;

		return actionID;
	}
}

internal class MachinistHeatBlastAutoCrossbow: CustomCombo {
	public override CustomComboPreset Preset => CustomComboPreset.MachinistOverheat;
	public override uint[] ActionIDs { get; } = new[] { MCH.HeatBlast, MCH.AutoCrossbow };

	protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level) {

		if (level >= MCH.Levels.Wildfire && IsEnabled(CustomComboPreset.MachinistHyperfire) && IsOffCooldown(MCH.Wildfire) && HasTarget)
			return MCH.Wildfire;

		if (level >= MCH.Levels.Hypercharge && !GetJobGauge<MCHGauge>().IsOverheated)
			return MCH.Hypercharge;

		if (level < MCH.Levels.AutoCrossbow)
			return MCH.HeatBlast;

		return actionID;
	}
}

internal class MachinistSpreadShotFeature: CustomCombo {
	public override CustomComboPreset Preset => CustomComboPreset.MachinistSpreadShot;
	public override uint[] ActionIDs { get; } = new[] { MCH.SpreadShot, MCH.Scattergun };

	protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level) {

		if (level >= MCH.Levels.AutoCrossbow && GetJobGauge<MCHGauge>().IsOverheated)
			return MCH.AutoCrossbow;

		return actionID;
	}
}

internal class MachinistOverdriveFeature: CustomCombo {
	public override CustomComboPreset Preset => CustomComboPreset.MachinistOverdrive;
	public override uint[] ActionIDs { get; } = new[] { MCH.RookAutoturret, MCH.AutomatonQueen };

	protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level) {

		if (level >= MCH.Levels.RookOverdrive && GetJobGauge<MCHGauge>().IsRobotActive)
			return OriginalHook(MCH.RookOverdrive);

		return actionID;
	}
}

internal class MachinistDrillAirAnchorFeature: CustomCombo {
	public override CustomComboPreset Preset => CustomComboPreset.MachinistDrillAirAnchor;
	public override uint[] ActionIDs { get; } = new[] { MCH.HotShot, MCH.AirAnchor, MCH.Drill };

	protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level) {

		if (level >= MCH.Levels.Chainsaw && IsEnabled(CustomComboPreset.MachinistDrillAirAnchorPlus)) {
			return GetJobGauge<MCHGauge>().Battery > 80
				? PickByCooldown(MCH.Drill, MCH.Chainsaw, MCH.AirAnchor, MCH.Drill)
				: PickByCooldown(MCH.AirAnchor, MCH.Chainsaw, MCH.Drill, MCH.AirAnchor);
		}

		if (level >= MCH.Levels.AirAnchor) {
			return GetJobGauge<MCHGauge>().Battery > 80
				? PickByCooldown(MCH.Drill, MCH.AirAnchor, MCH.Drill)
				: PickByCooldown(MCH.AirAnchor, MCH.Drill, MCH.AirAnchor);
		}

		if (level >= MCH.Levels.Drill)
			return PickByCooldown(actionID, MCH.HotShot, MCH.Drill);

		return MCH.HotShot;
	}
}
