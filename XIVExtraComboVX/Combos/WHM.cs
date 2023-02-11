namespace XIVExtraComboVX.Combos;

using Dalamud.Game.ClientState.JobGauge.Types;

internal static class WHM {
	public const byte JobID = 24;

	public const uint
		Raise = 125,
		Cure = 120,
		Medica = 124,
		Cure2 = 135,
		PresenceOfMind = 136,
		Holy = 139,
		Benediction = 140,
		Asylum = 3569,
		Tetragrammaton = 3570,
		Assize = 3571,
		PlenaryIndulgence = 7433,
		AfflatusSolace = 16531,
		AfflatusRapture = 16534,
		AfflatusMisery = 16535,
		Temperance = 16536,
		Holy3 = 25860,
		Aquaveil = 25861,
		LiturgyOfTheBell = 25862;

	public static class Buffs {
		// public const ushort placeholder = 0;
	}

	public static class Debuffs {
		// public const ushort placeholder = 0;
	}

	public static class Levels {
		public const byte
			Cure2 = 30,
			AfflatusSolace = 52,
			AfflatusMisery = 74,
			AfflatusRapture = 76;
	}
}

internal class WhiteMageSwiftcastRaiserFeature: SwiftRaiseCombo {
	public override CustomComboPreset Preset => CustomComboPreset.WhiteMageSwiftcastRaiserFeature;
	public override uint[] ActionIDs { get; } = new[] { WHM.Raise };
}

internal class WhiteMageSolaceMiseryFeature: CustomCombo {
	public override CustomComboPreset Preset => CustomComboPreset.WhiteMageSolaceMiseryFeature;
	public override uint[] ActionIDs { get; } = new[] { WHM.AfflatusSolace };

	protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level) {

		if (level >= WHM.Levels.AfflatusMisery && GetJobGauge<WHMGauge>().BloodLily == 3)
			return WHM.AfflatusMisery;

		return actionID;
	}
}

internal class WhiteMageRaptureMiseryFeature: CustomCombo {
	public override CustomComboPreset Preset => CustomComboPreset.WhiteMageRaptureMiseryFeature;
	public override uint[] ActionIDs { get; } = new[] { WHM.AfflatusRapture };

	protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level) {

		if (level >= WHM.Levels.AfflatusMisery && GetJobGauge<WHMGauge>().BloodLily == 3 && HasTarget)
			return WHM.AfflatusMisery;

		return actionID;
	}
}

internal class WhiteMageHoly: CustomCombo {
	public override CustomComboPreset Preset { get; } = CustomComboPreset.WhiteMageHolyMiseryFeature;
	public override uint[] ActionIDs { get; } = new[] { WHM.Holy, WHM.Holy3 };

	protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level) {

		if (level >= WHM.Levels.AfflatusMisery && GetJobGauge<WHMGauge>().BloodLily == 3 && HasTarget)
			return WHM.AfflatusMisery;

		return actionID;
	}
}

internal class WhiteMageCure2: CustomCombo {
	public override CustomComboPreset Preset => CustomComboPreset.WhiteMageCureFeature;
	public override uint[] ActionIDs { get; } = new[] { WHM.Cure2 };

	protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level) {

		if (IsEnabled(CustomComboPreset.WhiteMageCureFeature)) {
			if (level < WHM.Levels.Cure2)
				return WHM.Cure;
		}

		if (IsEnabled(CustomComboPreset.WhiteMageAfflatusFeature)) {
			WHMGauge gauge = GetJobGauge<WHMGauge>();

			if (IsEnabled(CustomComboPreset.WhiteMageSolaceMiseryFeature)) {
				if (level >= WHM.Levels.AfflatusMisery && gauge.BloodLily == 3)
					return WHM.AfflatusMisery;
			}

			if (level >= WHM.Levels.AfflatusSolace && gauge.Lily > 0)
				return WHM.AfflatusSolace;
		}

		return actionID;
	}
}

internal class WhiteMageMedica: CustomCombo {
	public override CustomComboPreset Preset => CustomComboPreset.WhiteMageAfflatusFeature;
	public override uint[] ActionIDs { get; } = new[] { WHM.Medica };

	protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level) {
		WHMGauge gauge = GetJobGauge<WHMGauge>();

		if (IsEnabled(CustomComboPreset.WhiteMageRaptureMiseryFeature)) {
			if (level >= WHM.Levels.AfflatusMisery && gauge.BloodLily == 3 && HasTarget)
				return WHM.AfflatusMisery;
		}

		if (level >= WHM.Levels.AfflatusRapture && gauge.Lily > 0)
			return WHM.AfflatusRapture;

		return actionID;
	}
}
