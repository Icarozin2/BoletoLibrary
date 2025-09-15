namespace BoletoLibrary
{
    public class BoletoViewModel
    {
        public string LinhaDigitavel { get; set; }
        public string CaminhoCodigoBarras { get; set; }
        public string LocalPagamento { get; set; }
        public string DataVencimento { get; set; }
        public string NomeBeneficiario { get; set; }
        public string AgenciaCodigoBeneficiario { get; set; }
        public string DataDocumento { get; set; }
        public string NumeroDocumento { get; set; }
        public string CarteiraNossoNumero { get; set; }
        public string ValorBoletoFormatado { get; set; }
        public string MensagemLivre { get; set; }
        public string NomePagador { get; set; }
        public string DocumentoPagadorFormatado { get; set; }
        public string EnderecoPagadorCompleto { get; set; }
    }
}