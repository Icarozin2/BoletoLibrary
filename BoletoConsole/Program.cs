using System;
using BoletoLibrary;

namespace BoletoConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var result = new FiltroBoletoConveniado
            {
              
                IdBoletoTOTVS = 19025,
                DataVencimento = "10/10/2025",   
                ValorBoleto = 18446.89m,
                CodColigada = 18,
                CodFilial = 2,
                CodFornecedorTotvs = "00017598",
                PercentualMulta = "1",
                NossoNumero = "02381127-1",      
                DataDocumento = "10/11/2025",
                NumeroDocumento = "023547125",
                EscolaId = 3972,
                NomePagador = "XS TECNOLOGIA",
                DocumentoPagador = "06.071.210/0001-57",
                EnderecoPagadorLogradouro = "Rua XPTO",
                EnderecoPagadorNumero = "25",
                PagadorEnderecoBairro = "Bairro XPTO",
                EnderecoPagadorCidade = "Salvador",
                EnderecoPagadorUF = "BA",
                EnderecoPagadorCep = "40275495",
                NomeBeneficiario = "GRUPO SALTA",
                AgenciaBeneficiario = "0281",
                ContaBeneficiario = "17616",
                ContaDigitoBeneficiario = "4",
                CarteiraBeneficiario = "109",
                CodigoBanco = "341",
                DocumentoBeneficiario = "17765891000170",
                LocalPagamento = "ATÉ O VENCIMENTO, PREFERENCIALMENTE NO ITAÚ. APÓS O VENCIMENTO, SOMENTE NO ITAÚ",
                MensagemLivre = "Aqui entram instruções e notas fiscais"
            };

            var service = new BoletoService();
            string caminhoSaida = "boleto.pdf";
            service.GerarBoleto(result, caminhoSaida);
            Console.WriteLine($"Boleto gerado com sucesso em: {caminhoSaida}");
        }
    }
}
