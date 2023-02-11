namespace XIVExtraComboVX.Combos;

using Dalamud.Game.ClientState.JobGauge.Enums;
using Dalamud.Game.ClientState.JobGauge.Types;

using global::XIVExtraComboVX.GameData;

internal static class AST {
	public const byte JobID = 33;

	public const uint
		Ascend = 3603,
		Draw = 3590,
		Benefic = 3594,
		Malefic = 3596,
		Malefic2 = 3598,
		Lightspeed = 3606,
		Benefic2 = 3610,
		Synastry = 3612,
		CollectiveUnconscious = 3613,
		Gravity = 3615,
		Balance = 4401,
		Bole = 4404,
		Arrow = 4402,
		Spear = 4403,
		Ewer = 4405,
		Spire = 4406,
		EarthlyStar = 7439,
		Malefic3 = 7442,
		MinorArcana = 7443,
		SleeveDraw = 7448,
		Divination = 16552,
		CelestialOpposition = 16553,
		Malefic4 = 16555,
		Horoscope = 16557,
		NeutralSect = 16559,
		Play = 17055,
		CrownPlay = 25869,
		Astrodyne = 25870,
		FallMalefic = 25871,
		Gravity2 = 25872,
		Exaltation = 25873,
		Macrocosmos = 25874;

	public static class Buffs {
		public const ushort
			LordOfCrownsDrawn = 2054,
			LadyOfCrownsDrawn = 2055;
	}

	public static class Debuffs {
		// public const ushort placeholder = 0;
	}

	public static class Levels {
		public const byte
			Benefic2 = 26,
			Draw = 30,
			Astrodyne = 50,
			MinorArcana = 70,
			CrownPlay = 70;
	}
}

internal class AstrologianSwiftcastRaiserFeature: SwiftRaiseCombo {
	public override CustomComboPreset Preset => CustomComboPreset.AstrologianSwiftcastRaiserFeature;
	public override uint[] ActionIDs { get; } = new[] { AST.Ascend };
}

internal class AstrologianPlay: CustomCombo {
	public override CustomComboPreset Preset => CustomComboPreset.AstAny;
	public override uint[] ActionIDs { get; } = new[] { AST.Play };

	protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level) {

		ASTGauge gauge = GetJobGauge<ASTGauge>();

		if (IsEnabled(CustomComboPreset.AstrologianPlayAstrodyneFeature)) {
			if (level >= AST.Levels.Astrodyne && !gauge.ContainsSeal(SealType.NONE))
				return AST.Astrodyne;
		}

		if (IsEnabled(CustomComboPreset.AstrologianPlayDrawFeature)) {

			if (IsEnabled(CustomComboPreset.AstrologianPlayDrawAstrodyneFeature)) {
				CooldownData draw = GetCooldown(AST.Draw);

				if (level >= AST.Levels.Astrodyne && !gauge.ContainsSeal(SealType.NONE) && draw.RemainingCharges == 0)
					return AST.Astrodyne;
			}

			if (level >= AST.Levels.Draw && gauge.DrawnCard is CardType.NONE)
				return AST.Draw;
		}

		return actionID;
	}
}

internal class AstrologianDraw: CustomCombo {
	public override CustomComboPreset Preset { get; } = CustomComboPreset.AstrologianDrawLockoutFeature;
	public override uint[] ActionIDs { get; } = new[] { AST.Draw };

	protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level) {

		ASTGauge gauge = GetJobGauge<ASTGauge>();

		if (gauge.DrawnCard is not CardType.NONE)
			return OriginalHook(AST.Malefic);

		return actionID;
	}
}

internal class AstrologianBeneficFeature: CustomCombo {
	public override CustomComboPreset Preset => CustomComboPreset.AstrologianBeneficFeature;
	public override uint[] ActionIDs { get; } = new[] { AST.Benefic2 };

	protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level) {

		if (level < AST.Levels.Benefic2)
			return AST.Benefic;

		return actionID;
	}
}
