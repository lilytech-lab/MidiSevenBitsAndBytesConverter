using System.Diagnostics;

namespace LilytechLab.MidiSevenBitsAndBytesConverter.Pages;

public partial class Index {

	private string message = string.Empty;

	private string sevenBitsStr = string.Empty;

	private string eightBitsStr = string.Empty;

	private void OnSevenToEightClicked() {
		this.eightBitsStr = string.Empty;
		if (string.IsNullOrWhiteSpace(this.sevenBitsStr)) return;

		try {
			var sevenBitsBytes = this.GetBytes(this.sevenBitsStr);
			var (eightBitsByteLength, _) = Math.DivRem(sevenBitsBytes.Length * 7, 8);

			Span<byte> eightBitsBytes = new byte[eightBitsByteLength];

			// 7bit文字8byte分→8bit文字7byte分に変換
			for (int i = 0, j = 0; i < sevenBitsBytes.Length; i += 8, j += 7) {
				ReadOnlySpan<byte> source = i + 8 <= sevenBitsBytes.Length ? sevenBitsBytes[i..(i + 8)] : sevenBitsBytes[i..];
				Debug.WriteLine($"source: {BitConverter.ToString(source.ToArray())}");

				Span<byte> dest = j + 7 <= eightBitsByteLength ? eightBitsBytes[j..(j + 7)] : eightBitsBytes[j..];

				for (var k = 1; k < source.Length; k++) {
					// 1byte目の中から該当1bitを抜き出し
					var topBit = (byte)((source[0] & (0x80 >> k)) << k);
					// 抜き出した1bitを頭に付けて8bit分を構成
					dest[k - 1] = (byte)(source[k] | topBit);
				}
			}

			this.eightBitsStr = BitConverter.ToString(eightBitsBytes.ToArray()).Replace('-', ' ');

		} catch (FormatException ex) {
			this.message = ex.Message;
		} catch (Exception ex) {
			this.message = "Something went wrong.";
			Console.WriteLine(ex);
		}
	}

	private void OnEightToSevenClicked() {
		this.sevenBitsStr = string.Empty;
		if (string.IsNullOrWhiteSpace(this.eightBitsStr)) return;

		try {
			var eightBitsBytes = this.GetBytes(this.eightBitsStr);
			var (sevenBitsByteLength, remainder) = Math.DivRem(eightBitsBytes.Length * 8, 7);
			if (remainder > 0)
				sevenBitsByteLength++;

			Span<byte> sevenBitsBytes = new byte[sevenBitsByteLength];

			// 8bit文字7byte分→7bit文字8byte分に変換
			for (int i = 0, j = 0; i < eightBitsBytes.Length; i += 7, j += 8) {
				ReadOnlySpan<byte> source = i + 7 <= eightBitsBytes.Length ? eightBitsBytes[i..(i + 7)] : eightBitsBytes[i..];
				Debug.WriteLine($"source: {BitConverter.ToString(source.ToArray())}");

				Span<byte> dest = j + 8 <= sevenBitsByteLength ? sevenBitsBytes[j..(j + 8)] : sevenBitsBytes[j..];

				for (int k = 0; k < source.Length; k++) {
					// 各byteの1bit目は1byte目に集める
					dest[0] |= (byte)((source[k] & 0x80) >> (k + 1));
					// 残りの7bitのみで1byte分を構成
					dest[k + 1] = (byte)(source[k] & 0x7F);
				}
			}

			this.sevenBitsStr = BitConverter.ToString(sevenBitsBytes.ToArray()).Replace('-', ' ');

		} catch (FormatException ex) {
			this.message = ex.Message;
		} catch (Exception ex) {
			this.message = "Something went wrong.";
			Console.WriteLine(ex);
		}
	}

	private ReadOnlySpan<byte> GetBytes(string value) {
		// ハイフンとスペースを除去
		return Convert.FromHexString(value.Replace(Environment.NewLine, string.Empty).Replace(" ", string.Empty));
	}

}

