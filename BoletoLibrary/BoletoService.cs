using Aspose.BarCode.Generation;
using Aspose.Pdf;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace BoletoLibrary
{
    public class BoletoService
    {
        // Este é o método principal que será chamado pelo Program.cs
        public void GerarBoleto(FiltroBoletoConveniado dados, string caminhoSaida)
        {
            // 1. Gerar o Código de Barras e a Linha Digitável
            var codigoBarras = GerarCodigoDeBarras(dados);
            var linhaDigitavel = GerarLinhaDigitavel(codigoBarras);

            // 2. Gerar a imagem do código de barras
            string barcodePath = Path.Combine(Path.GetTempPath(), "barcode.png");
            using (var generator = new BarcodeGenerator(EncodeTypes.Interleaved2of5, codigoBarras))
            {
                generator.Parameters.Barcode.XDimension.Pixels = 2;
                generator.Parameters.Barcode.BarHeight.Pixels = 50;
                generator.Save(barcodePath);
            }

            // 3. Montar o BoletoViewModel com todos os dados calculados e formatados
            var viewModel = MontarBoletoViewModel(dados, linhaDigitavel, barcodePath);

            // 4. Montar HTML baseado no BoletoViewModel
            string htmlContent = BoletoHtmlBuilder.MontarHtml(viewModel);

            // 5. Converter HTML para PDF
            var options = new HtmlLoadOptions();
            var document = new Document(new MemoryStream(Encoding.UTF8.GetBytes(htmlContent)), options);
            document.Save(caminhoSaida);
        }

        private BoletoViewModel MontarBoletoViewModel(FiltroBoletoConveniado dados, string linhaDigitavel, string barcodePath)
        {
            var viewModel = new BoletoViewModel
            {
                // Dados calculados
                LinhaDigitavel = linhaDigitavel,
                CaminhoCodigoBarras = barcodePath,

                // Dados de exibição formatados
                LocalPagamento = dados.LocalPagamento,
                DataVencimento = dados.DataVencimento,
                NomeBeneficiario = dados.NomeBeneficiario,
                AgenciaCodigoBeneficiario = $"{dados.AgenciaBeneficiario}/{dados.CodigoBeneficiario}",
                DataDocumento = dados.DataDocumento,
                NumeroDocumento = dados.NumeroDocumento,
                CarteiraNossoNumero = $"{dados.CarteiraBeneficiario} / {dados.NossoNumero}",
                ValorBoletoFormatado = dados.ValorBoleto.ToString("C2"),
                MensagemLivre = dados.MensagemLivre,
                NomePagador = dados.NomePagador,
                DocumentoPagadorFormatado = FormatDocumento(dados.DocumentoPagador),
                EnderecoPagadorCompleto = $"{dados.EnderecoPagadorLogradouro}, {dados.EnderecoPagadorNumero} - {dados.PagadorEnderecoBairro} - {dados.EnderecoPagadorCidade} - {dados.EnderecoPagadorUF} - CEP: {dados.EnderecoPagadorCep}"
            };

            return viewModel;
        }

        // --- Métodos de Apoio ---
        private string GerarCodigoDeBarras(FiltroBoletoConveniado dados)
        {
            // Posições 01-03: Código do Banco (Ex: 001 para BB, 341 para Itaú)
            string codigoBanco = dados.CodigoBanco.PadLeft(3, '0');
            // Posição 04: Moeda (9 para Real)
            string codigoMoeda = "9";
            // Posição 05: Dígito Verificador Geral (Calculado depois)
            string dvGeral = "";
            // Posições 06-09: Fator de Vencimento
            string fatorVencimento = CalcularFatorVencimento(dados.DataVencimento).ToString().PadLeft(4, '0');
            // Posições 10-19: Valor do documento
            string valorBoleto = string.Format("{0:N2}", dados.ValorBoleto).Replace(",", "").Replace(".", "").PadLeft(10, '0');
            // Posições 20-44: Campo Livre (25 posições)
            string campoLivre = GerarCampoLivre(dados);

            // Montar o código de barras sem o DV geral
            string codigoParcial = codigoBanco + codigoMoeda + fatorVencimento + valorBoleto + campoLivre;
            dvGeral = CalcularDvMod11(codigoParcial);

            // Montar o código de barras completo de 44 posições
            string codigoBarrasCompleto = codigoBanco + codigoMoeda + dvGeral + fatorVencimento + valorBoleto + campoLivre;

            return codigoBarrasCompleto;
        }

        private string GerarLinhaDigitavel(string codigoBarras)
        {
            string campoLivre = codigoBarras.Substring(19);

            // Bloco 1: Código do banco + moeda + 5 posições do campo livre + DV
            string bloco1 = codigoBarras.Substring(0, 4) + campoLivre.Substring(0, 5);
            string dv1 = CalcularDvMod10(bloco1);

            // Bloco 2: 10 posições do campo livre + DV
            string bloco2 = campoLivre.Substring(5, 10);
            string dv2 = CalcularDvMod10(bloco2);

            // Bloco 3: 10 posições do campo livre + DV
            string bloco3 = campoLivre.Substring(15, 10);
            string dv3 = CalcularDvMod10(bloco3);

            // Bloco 4: Dígito verificador geral do código de barras
            string bloco4 = codigoBarras.Substring(4, 1);

            // Bloco 5: Fator de vencimento e valor do boleto
            string bloco5 = codigoBarras.Substring(5, 4) + codigoBarras.Substring(9, 10);

            return $"{bloco1}.{dv1} {bloco2}.{dv2} {bloco3}.{dv3} {bloco4} {bloco5}";
        }

        private int CalcularFatorVencimento(string dataVencimentoStr)
        {
            DateTime dataVencimento = DateTime.Parse(dataVencimentoStr);
            DateTime dataBase = new DateTime(1997, 10, 07);
            TimeSpan ts = dataVencimento - dataBase;
            return ts.Days;
        }

        private string GerarCampoLivre(FiltroBoletoConveniado dados)
        {
            // Baseado na imagem do boleto, o campo livre do Itaú é de 25 posições
            // Agência (4) + Código do Beneficiário (8) + Nosso Número (11) + Carteira (2)
            string agencia = dados.AgenciaBeneficiario.PadLeft(4, '0');
            string codigoBeneficiario = dados.CodigoBeneficiario.Replace("/", "").Replace("-", "").PadLeft(8, '0');
            string nossoNumeroLimpo = dados.NossoNumero.Replace("/", "").Replace("-", "").PadLeft(11, '0');
            string carteira = dados.CarteiraBeneficiario.PadLeft(2, '0');

            // Concatenação
            return $"{agencia}{codigoBeneficiario}{nossoNumeroLimpo}{carteira}";
        }

        private string CalcularDvMod11(string sequencia)
        {
            int soma = 0;
            int multiplicador = 2;
            for (int i = sequencia.Length - 1; i >= 0; i--)
            {
                soma += int.Parse(sequencia[i].ToString()) * multiplicador;
                multiplicador++;
                if (multiplicador > 9)
                {
                    multiplicador = 2;
                }
            }
            int resto = soma % 11;
            int dv = 11 - resto;

            if (dv == 0 || dv > 9)
            {
                return "1";
            }
            return dv.ToString();
        }

        private string CalcularDvMod10(string sequencia)
        {
            int soma = 0;
            string sequenciaInvertida = new string(sequencia.ToCharArray().Reverse().ToArray());

            for (int i = 0; i < sequenciaInvertida.Length; i++)
            {
                int digito = int.Parse(sequenciaInvertida[i].ToString());
                int valorMultiplicado = (i % 2 == 0) ? digito * 2 : digito;

                if (valorMultiplicado > 9)
                {
                    valorMultiplicado = valorMultiplicado.ToString().Sum(c => int.Parse(c.ToString()));
                }
                soma += valorMultiplicado;
            }

            int dv = (soma % 10 == 0) ? 0 : 10 - (soma % 10);
            return dv.ToString();
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
    }
}