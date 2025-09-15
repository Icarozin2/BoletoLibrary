using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoletoLibrary
{
    public class FiltroBoletoConveniado
    {
  
        public int IdBoletoTOTVS { get; set; }
        public string DataVencimento { get; set; }
        public decimal ValorBoleto { get; set; }

       
        public int CodColigada { get; set; }
        public int CodFilial { get; set; }
        public string CodFornecedorTotvs { get; set; }
        public string PercentualMulta { get; set; }
        public string NossoNumero { get; set; }
        public string DataDocumento { get; set; }
        public string NumeroDocumento { get; set; }

       
        public int EscolaId { get; set; }
        public string NomePagador { get; set; }
        public string DocumentoPagador { get; set; }
        public string EnderecoPagadorLogradouro { get; set; }
        public string EnderecoPagadorNumero { get; set; }
        public string EnderecoPagadorCidade { get; set; }
        public string EnderecoPagadorCep { get; set; }
        public string EnderecoPagadorUF { get; set; }
        public string PagadorEnderecoBairro { get; set; }

       
        public string NomeBeneficiario { get; set; }
        public string CodigoBeneficiario { get; set; }
        public string AgenciaBeneficiario { get; set; }
        public string AgenciaDigitoBeneficiario { get; set; }
        public string ContaBeneficiario { get; set; }
        public string ContaDigitoBeneficiario { get; set; }
        public string CarteiraBeneficiario { get; set; }
        public string CodigoBanco { get; set; }
        public string DocumentoBeneficiario { get; set; }
        public string LocalPagamento { get; set; }
        public string MensagemLivre { get; set; }
    }
}
