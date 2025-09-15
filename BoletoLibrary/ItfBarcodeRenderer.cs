using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace BoletoLibrary
{
    public static class ItfBarcodeRenderer
    {
        
        private static readonly Dictionary<char, bool[]> DigitPattern = new()
        {
            ['0'] = new[] { false, false, true, true, false }, // n n W W n
            ['1'] = new[] { true, false, false, false, true }, // W n n n W
            ['2'] = new[] { false, true, false, false, true }, // n W n n W
            ['3'] = new[] { true, true, false, false, false }, // W W n n n
            ['4'] = new[] { false, false, true, false, true }, // n n W n W
            ['5'] = new[] { true, false, true, false, false }, // W n W n n
            ['6'] = new[] { false, true, true, false, false }, // n W W n n
            ['7'] = new[] { false, false, false, true, true }, // n n n W W
            ['8'] = new[] { true, false, false, true, false }, // W n n W n
            ['9'] = new[] { false, true, false, true, false }  // n W n W n
        };

        /// <summary>
        /// Renderiza um ITF (Interleaved 2 of 5) puro em PNG.
        /// </summary>
        /// <param name="digits">Somente dígitos (deve ter quantidade PAR). Sem start/stop, sem checksum extra.</param>
        /// <param name="outPath">Caminho destino do PNG.</param>
        /// <param name="xModulePx">Largura do módulo estreito (px). 2–3 px é ótimo.</param>
        /// <param name="heightPx">Altura das barras (px).</param>
        /// <param name="wideRatio">Multiplicador do largo sobre o estreito (2.0–3.0). Padrão 2.5.</param>
        /// <param name="quietModules">Quiet zone (margem branca) em módulos estreitos, cada lado. Recomendo ≥ 10.</param>
        public static void RenderToPng(string digits, string outPath,
                                       int xModulePx = 3, int heightPx = 90,
                                       double wideRatio = 2.5, int quietModules = 10)
        {
            if (string.IsNullOrWhiteSpace(digits))
                throw new ArgumentException("Dados do ITF não informados.");
            if (!IsAllDigits(digits))
                throw new ArgumentException("ITF aceita apenas dígitos (0-9).");
            if ((digits.Length % 2) != 0)
                throw new ArgumentException("ITF exige quantidade PAR de dígitos.");

            // Constrói a sequência de (isBar, widthInModules) incluindo start/stop e quiet zones
            var seq = BuildItfSequence(digits, wideRatio, quietModules);

            // Calcula largura total em pixels
            int totalModules = 0;
            foreach (var (isBar, w) in seq) totalModules += w;
            int totalWidthPx = totalModules * xModulePx;

            using var bmp = new Bitmap(totalWidthPx, heightPx, PixelFormat.Format24bppRgb);
            using var g = Graphics.FromImage(bmp);
            g.Clear(Color.White);

            int x = 0;
            foreach (var (isBar, w) in seq)
            {
                int wpx = w * xModulePx;
                if (isBar)
                    g.FillRectangle(Brushes.Black, x, 0, wpx, heightPx);
                x += wpx;
            }

            bmp.Save(outPath, ImageFormat.Png);
        }

        private static bool IsAllDigits(string s)
        {
            foreach (var c in s) if (c < '0' || c > '9') return false;
            return true;
        }

        private static List<(bool isBar, int widthModules)> BuildItfSequence(string digits, double wideRatio, int quietModules)
        {
            int N = 1; 
            int W = (int)Math.Round(N * wideRatio); if (W < 2) W = 2;

            var seq = new List<(bool, int)>();

         
            seq.Add((false, quietModules));

         
            seq.Add((true, N)); seq.Add((false, N));
            seq.Add((true, N)); seq.Add((false, N));

           
            for (int i = 0; i < digits.Length; i += 2)
            {
                var bars = DigitPattern[digits[i]];
                var spaces = DigitPattern[digits[i + 1]];

                for (int k = 0; k < 5; k++)
                {
                    seq.Add((true, bars[k] ? W : N));  
                    seq.Add((false, spaces[k] ? W : N));   
                }
            }

         
            seq.Add((true, W));
            seq.Add((false, N));
            seq.Add((true, N));

         
            seq.Add((false, quietModules));

            return seq;
        }
    }
}
