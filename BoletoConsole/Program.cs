using System;
using BoletoLibrary;

namespace BoletoConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            // Criar objeto mocado
            var result = new FiltroBoletoConveniado()
            {
                IdBoletoTOTVS = 19025,
                DataVencimento = "10/12/2025",
                ValorBoleto = 225.65,
                CodColigada = 18,
                CodFilial = 2,
                CodFornecedorTotvs = "00017598",
                PercentualMulta = "1",
                NossoNumero = "109/" + "077785-4",
                DataDocumento = "10/11/2025",
                NumeroDocumento = "023547125",
                EscolaId = 3972,
                NomePagador = "XS TECNOLOGIA ",
                DocumentoPagador = "06.071.210/0001-57",
                EnderecoPagadorLogradouro = "Rua XPTO",
                EnderecoPagadorNumero = "25",
                EnderecoPagadorCidade = "Salvador",
                EnderecoPagadorCep = "40.275-495",
                EnderecoPagadorUF = "BA",
                PagadorEnderecoBairro = "Bairro XPTO",
                NomeBeneficiario = "GRUPO SALTA",
                CodigoBeneficiario = "0281/17616-4",
                AgenciaBeneficiario = "0281",
                CarteiraBeneficiario = "109",
                CodigoBanco = "341",
                DocumentoBeneficiario = "17765891000170",
                LocalPagamento = "ATÉ O VENCIMENTO, PREFERENCIALMENTE NO ITAÚ. APÓS O VENCIMENTO, SOMENTE NO ITAÚ",
                MensagemLivre = "Aqui entram instruções e notas fiscais"
            };

            // Criar boleto
            var service = new BoletoService();
            string caminhoSaida = "boleto.pdf";
            service.GerarBoleto(result, caminhoSaida);

            Console.WriteLine($"Boleto gerado com sucesso em: {caminhoSaida}");
        }
    }
}