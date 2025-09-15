
using PuppeteerSharp;
using PuppeteerSharp.Media;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace BoletoLibrary
{
    public class BoletoService
    {
   

        public void GerarBoleto(FiltroBoletoConveniado dados, string caminhoSaida)
        {
         
            NormalizarInsumos(dados);

           
            string codigo44 = MontarCodigoBarras44_Itau109(dados);

          
            string linha47 = MontarLinhaDigitavelFrom44(codigo44);

           
            string barcodePath = GerarImagemCodigoBarras(codigo44);
            string barcodeDataUri = ToDataUri(barcodePath);
            string logoPath = ResolvePath("assets/logo.jpeg");
            string logoDataUri = ToDataUri(logoPath);

            var viewModel = MontarBoletoViewModel(dados, linha47, barcodeDataUri, logoDataUri);

        
            Console.WriteLine("Código de barras (44): " + codigo44);
            Console.WriteLine("Linha digitável (47):  " + linha47);

            
            string htmlContent = BoletoHtmlBuilder.MontarHtml(viewModel);

            SalvarPdfComPuppeteer(htmlContent, caminhoSaida); // síncrono (bloqueia internamente)

            // 7) Sanity check 44 ↔ 47
            if (!Validar44e47(codigo44, linha47, out var motivo))
                Console.WriteLine("⚠️ Inconsistência 44↔47: " + motivo);
            else
                Console.WriteLine("✅ 44 e 47 consistentes.");
        }

        // ------ helper: data URI ------
        private static string ToDataUri(string imagePath)
        {
            var bytes = File.ReadAllBytes(imagePath);
            var ext = (Path.GetExtension(imagePath) ?? "").ToLowerInvariant();
            var mime = ext switch
            {
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                _ => "application/octet-stream"
            };
            return $"data:{mime};base64,{Convert.ToBase64String(bytes)}";
        }

        private static string ResolvePath(params string[] candidates)
        {
            foreach (var c in candidates)
            {
                var p = Path.IsPathRooted(c) ? c : Path.Combine(AppContext.BaseDirectory, c);
                if (File.Exists(p)) return p;
            }
            throw new FileNotFoundException("Logo não encontrada. Ex.: assets/logo.png",
                string.Join(", ", candidates));
        }

     
        private void SalvarPdfComPuppeteer(string html, string caminhoSaida)
        {
           
            SalvarPdfComPuppeteerAsync(html, caminhoSaida).GetAwaiter().GetResult();
        }

        private async System.Threading.Tasks.Task SalvarPdfComPuppeteerAsync(string html, string caminhoSaida)
        {
         
            var fetcher = new BrowserFetcher();
            await fetcher.DownloadAsync(); 

            var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                Args = new[] { "--no-sandbox", "--disable-gpu" }
            });

            try
            {
                var page = await browser.NewPageAsync();

               
                await page.SetContentAsync(
                    html,
                    new NavigationOptions { WaitUntil = new[] { WaitUntilNavigation.Networkidle0 } }
                );

               
                await page.EmulateMediaTypeAsync(MediaType.Screen);

               
                await page.PdfAsync(caminhoSaida, new PdfOptions
                {
                    Format = PaperFormat.A4,
                    PrintBackground = true,
                    MarginOptions = new MarginOptions
                    {
                        Top = "20mm",
                        Right = "20mm",
                        Bottom = "20mm",
                        Left = "20mm"
                    }
                });
            }
            finally
            {
                await browser.CloseAsync();
            }
        }




        private BoletoViewModel MontarBoletoViewModel(FiltroBoletoConveniado dados, string linhaDigitavel, string barcodePath, string logoSrc)
        {
            return new BoletoViewModel
            {
                LinhaDigitavel = linhaDigitavel,
                CaminhoCodigoBarras = barcodePath,
                LogoSrc = logoSrc,
                LocalPagamento = dados.LocalPagamento,
                DataVencimento = dados.DataVencimento,
                NomeBeneficiario = dados.NomeBeneficiario,
                AgenciaCodigoBeneficiario = $"{dados.AgenciaBeneficiario}/{dados.ContaBeneficiario}-{dados.ContaDigitoBeneficiario}",
                DataDocumento = dados.DataDocumento,
                NumeroDocumento = dados.NumeroDocumento,
                CarteiraNossoNumero = $"{dados.CarteiraBeneficiario} / {dados.NossoNumero}",
                ValorBoletoFormatado = FormatarMoeda(dados.ValorBoleto),
                MensagemLivre = dados.MensagemLivre,
                NomePagador = dados.NomePagador,
                DocumentoPagadorFormatado = FormatDocumento(dados.DocumentoPagador),
                EnderecoPagadorCompleto = $"{dados.EnderecoPagadorLogradouro}, {dados.EnderecoPagadorNumero} - {dados.PagadorEnderecoBairro} - {dados.EnderecoPagadorCidade} - {dados.EnderecoPagadorUF} - CEP: {FormatCep(dados.EnderecoPagadorCep)}"
            };
        }


        private bool Validar44e47(string codigo44, string linha47, out string motivo)
        {
            motivo = "";
            string l = new string((linha47 ?? "").Where(char.IsDigit).ToArray());
            if (codigo44 == null || codigo44.Length != 44) { motivo = "Código 44 não tem 44 dígitos."; return false; }
            if (l.Length != 47) { motivo = "Linha digitável não tem 47 dígitos."; return false; }

        
            string banco = l.Substring(0, 3), moeda = l.Substring(3, 1);
            string c1 = l.Substring(0, 9); int dv1 = l[9] - '0';
            string c2 = l.Substring(10, 10); int dv2 = l[20] - '0';
            string c3 = l.Substring(21, 10); int dv3 = l[31] - '0';
            string dvGeral = l.Substring(32, 1);
            string fator = l.Substring(33, 4);
            string valor10 = l.Substring(37, 10);

            if (DvMod10(c1) != dv1) { motivo = "DV do campo 1 inválido."; return false; }
            if (DvMod10(c2) != dv2) { motivo = "DV do campo 2 inválido."; return false; }
            if (DvMod10(c3) != dv3) { motivo = "DV do campo 3 inválido."; return false; }

            string campoLivre = c1.Substring(4, 5) + c2 + c3; 
            string semDv = banco + moeda + fator + valor10 + campoLivre; 
            if ((dvGeral[0] - '0') != DvGeralMod11(semDv)) { motivo = "DV geral (mód.11) inválido."; return false; }

            string reconstruido44 = banco + moeda + dvGeral + fator + valor10 + campoLivre;
            if (reconstruido44 != codigo44) { motivo = "Linha não reconstrói o mesmo 44."; return false; }

            return true;
        }



        private static string FormatarMoeda(decimal valor) =>
            string.Format(new CultureInfo("pt-BR"), "{0:C2}", valor);

        

        private string MontarCodigoBarras44_Itau109(FiltroBoletoConveniado dados)
        {
         
            string banco = DigitsOnly(dados.CodigoBanco).PadLeft(3, '0');     
            string moeda = "9";

            string fator = FatorVencimento(dados.DataVencimento);
            string valor10 = ValorEm10Digitos(dados.ValorBoleto);

            string campoLivre = CampoLivre_Itau_109(
                carteira: DigitsOnly(dados.CarteiraBeneficiario).PadLeft(3, '0'),
                nossoNumero: ExtrairNossoNumero_Itau(dados.NossoNumero, out string nnDV), 
                nnDV: nnDV,
                agencia4: DigitsOnly(dados.AgenciaBeneficiario).PadLeft(4, '0'),
                conta5: DigitsOnly(dados.ContaBeneficiario).PadLeft(5, '0'),
                contaDV: DigitsOnly(dados.ContaDigitoBeneficiario).PadLeft(1, '0')
            );

            string semDv = banco + moeda + fator + valor10 + campoLivre; 
            string dvGeral = DvGeralMod11(semDv).ToString();

            return banco + moeda + dvGeral + fator + valor10 + campoLivre;  
        }

        private string MontarLinhaDigitavelFrom44(string codigo44)
        {
            if (codigo44 == null || codigo44.Length != 44)
                throw new ArgumentException("Código de barras deve ter 44 dígitos.");

            string banco = codigo44.Substring(0, 3);
            string moeda = codigo44.Substring(3, 1);
            string dvGeral = codigo44.Substring(4, 1);
            string fator = codigo44.Substring(5, 4);
            string valor10 = codigo44.Substring(9, 10);
            string campoLivre = codigo44.Substring(19, 25);

            string c1SemDv = banco + moeda + campoLivre.Substring(0, 5);
            string c2SemDv = campoLivre.Substring(5, 10);
            string c3SemDv = campoLivre.Substring(15, 10);

            int dv1 = DvMod10(c1SemDv);
            int dv2 = DvMod10(c2SemDv);
            int dv3 = DvMod10(c3SemDv);

            string campo1 = c1SemDv.Insert(5, ".") + dv1; 
            string campo2 = c2SemDv.Insert(5, ".") + dv2; 
            string campo3 = c3SemDv.Insert(5, ".") + dv3; 
            string campo4 = dvGeral;
            string campo5 = fator + valor10;

            return $"{campo1} {campo2} {campo3} {campo4} {campo5}";
        }

        private string GerarImagemCodigoBarras(string codigo44)
        {
            string path = Path.Combine(Path.GetTempPath(), "barcode.png");
           
            ItfBarcodeRenderer.RenderToPng(
                digits: codigo44,
                outPath: path,
                xModulePx: 2,          
                heightPx: 90,         
                wideRatio: 2.0,       
                quietModules: 10     
            );
            return path;
        }

  
        private string CampoLivre_Itau_109(string carteira, string nossoNumero, string nnDV,
                                           string agencia4, string conta5, string contaDV)
        {
            if (carteira.Length != 3) throw new ArgumentException("Carteira deve ter 3 dígitos.");
            if (nossoNumero.Length != 8) throw new ArgumentException("Nosso Número (sem DV) deve ter 8 dígitos no Itaú.");
            if (nnDV.Length != 1) throw new ArgumentException("DV do Nosso Número deve ter 1 dígito.");
            if (agencia4.Length != 4) throw new ArgumentException("Agência deve ter 4 dígitos.");
            if (conta5.Length != 5) throw new ArgumentException("Conta deve ter 5 dígitos.");
            if (contaDV.Length != 1) throw new ArgumentException("DV da Conta deve ter 1 dígito.");

            return carteira + nossoNumero + nnDV + agencia4 + conta5 + contaDV + "000";
        }

       
        private string ExtrairNossoNumero_Itau(string bruto, out string nnDV)
        {
            string d = DigitsOnly(bruto);
            if (d.Length < 9) d = d.PadLeft(9, '0');
            if (d.Length > 9) d = d.Substring(d.Length - 9);

            string nn = d.Substring(0, 8);
            nnDV = d.Substring(8, 1);
            return nn;
        }

      

        private static string DigitsOnly(string s) => new string((s ?? "").Where(char.IsDigit).ToArray());

        private string FatorVencimento(string dataVencStr)
        {
            if (string.IsNullOrWhiteSpace(dataVencStr)) return "0000";

            if (!DateTime.TryParseExact(
                    dataVencStr.Trim(),
                    new[] { "dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd" },
                    new System.Globalization.CultureInfo("pt-BR"),
                    System.Globalization.DateTimeStyles.None,
                    out DateTime venc))
            {
                return "0000";
            }

            var cutoff = new DateTime(2025, 2, 22);
            if (venc.Date >= cutoff)
            {
                int diasNovo = (int)(venc.Date - cutoff).TotalDays; 
                int fatorNovo = 1000 + diasNovo;                    
                return fatorNovo.ToString("0000");                
            }
            else
            {
                var baseAntiga = new DateTime(1997, 10, 7);
                int diasAntigo = (int)(venc.Date - baseAntiga).TotalDays;
                if (diasAntigo < 0) diasAntigo = 0;
                return diasAntigo.ToString("0000");
            }
        }

        private string ValorEm10Digitos(decimal valor)
        {
            long centavos = (long)Math.Round(valor * 100m, 0, MidpointRounding.AwayFromZero);
            if (centavos < 0) centavos = 0;
            return centavos.ToString("0000000000");
        }

        
        private int DvGeralMod11(string numeros43)
        {
            int peso = 2, soma = 0;
            for (int i = numeros43.Length - 1; i >= 0; i--)
            {
                int n = numeros43[i] - '0';
                soma += n * peso;
                peso = (peso == 9) ? 2 : (peso + 1);
            }
            int resto = soma % 11;
            int dv = 11 - resto;
            if (dv == 0 || dv == 10 || dv == 11) dv = 1;
            return dv;
        }

       
        private int DvMod10(string numeros)
        {
            int soma = 0;
            bool peso2 = true;
            for (int i = numeros.Length - 1; i >= 0; i--)
            {
                int n = numeros[i] - '0';
                int prod = n * (peso2 ? 2 : 1);
                soma += (prod > 9) ? (prod - 9) : prod; 
                peso2 = !peso2;
            }
            return (10 - (soma % 10)) % 10;
        }

       

        private void NormalizarInsumos(FiltroBoletoConveniado dados)
        {
            
            dados.DocumentoPagador = DigitsOnly(dados.DocumentoPagador);
            dados.EnderecoPagadorCep = DigitsOnly(dados.EnderecoPagadorCep);

           
            if (!string.IsNullOrWhiteSpace(dados.CodigoBeneficiario) &&
                (string.IsNullOrWhiteSpace(dados.ContaBeneficiario) || string.IsNullOrWhiteSpace(dados.ContaDigitoBeneficiario)))
            {
                TryDecomposeCodigoBeneficiario(dados);
            }

            dados.CodigoBanco = DigitsOnly(dados.CodigoBanco);
            dados.CarteiraBeneficiario = DigitsOnly(dados.CarteiraBeneficiario);
           
        }

        private void TryDecomposeCodigoBeneficiario(FiltroBoletoConveniado dados)
        {
           
            string raw = dados.CodigoBeneficiario ?? "";
            string d = DigitsOnly(raw);

           
            if (d.Length >= 10)
            {
                string ag = d.Substring(0, 4);
                string conta = d.Substring(4, 5);
                string dv = d.Substring(9, 1);
                dados.AgenciaBeneficiario = string.IsNullOrWhiteSpace(dados.AgenciaBeneficiario) ? ag : dados.AgenciaBeneficiario;
                dados.ContaBeneficiario = conta;
                dados.ContaDigitoBeneficiario = dv;
            }
        }

       

        private string FormatDocumento(string documento)
        {
            if (string.IsNullOrEmpty(documento)) return string.Empty;
            string docLimpo = new string(documento.Where(char.IsDigit).ToArray());
            if (docLimpo.Length == 14)
            {
                return $"{docLimpo.Substring(0, 2)}.{docLimpo.Substring(2, 3)}.{docLimpo.Substring(5, 3)}/{docLimpo.Substring(8, 4)}-{docLimpo.Substring(12, 2)}";
            }
            if (docLimpo.Length == 11)
            {
                return $"{docLimpo.Substring(0, 3)}.{docLimpo.Substring(3, 3)}.{docLimpo.Substring(6, 3)}-{docLimpo.Substring(9, 2)}";
            }
            return documento;
        }

        private string FormatCep(string cep)
        {
            string d = new string((cep ?? "").Where(char.IsDigit).ToArray());
            if (d.Length == 8) return $"{d.Substring(0, 5)}-{d.Substring(5, 3)}";
            return cep ?? "";
        }
    }
}
