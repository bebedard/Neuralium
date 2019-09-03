using System;
using Neuralia.Blockchains.Core.General.Types.Specialized;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Tools {
	public static class NeuraliumUtilities {

		public const int NEURALIUM_PRECISION = 9;
		public static readonly decimal MinValue = RoundNeuraliumsPrecision(0.000000000000001);

		public static decimal RoundNeuraliumsPrecision(decimal value) {
			return Math.Round(value, NEURALIUM_PRECISION);
		}

		public static Amount RoundNeuraliumsPrecision(Amount value) {
			return RoundNeuraliumsPrecision(value.Value);
		}

		public static decimal Cap(decimal value) {
			return Math.Max(value, MinValue);
		}

		public static Amount Cap(Amount value) {
			return Cap(value.Value);
		}
	}
}