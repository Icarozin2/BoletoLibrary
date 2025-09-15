using System;
using System.Linq;

namespace BoletoLibrary
{
    public class BoletoHtmlBuilder
    {
        public static string MontarHtml(BoletoViewModel dados)
        {
            return $@"
            <html>
            <head>
                <meta charset='UTF-8'>
                <style>
                    body {{ font-family: 'Arial', sans-serif; font-size: 11px; margin: 0; padding: 0; }}
                    .boleto-container {{ width: 700px; margin: 20px auto; border: 1px solid #000; padding: 10px; }}
                    .header {{ display: flex; align-items: center; justify-content: space-between; margin-bottom: 10px; }}
                    .logo {{ width: 150px; }}
                    .banco-info {{ border: 1px solid #000; padding: 5px; text-align: center; font-weight: bold; font-size: 14px; width: 100px; }}
                    .linha-digitavel {{ font-size: 16px; font-weight: bold; text-align: right; }}
                    .data-field {{ border: 1px solid #000; padding: 4px; margin-bottom: 5px; }}
                    .data-label {{ font-size: 10px; color: #555; }}
                    .data-value {{ font-size: 12px; font-weight: bold; }}
                    .table-row {{ display: flex; }}
                    .table-cell {{ flex: 1; border: 1px solid #000; padding: 4px; }}
                    .table-cell-2 {{ flex: 2; border: 1px solid #000; padding: 4px; }}
                    .campo-menor {{ flex: 0.5; }}
                    .barcode {{ text-align: center; margin-top: 10px; }}
                    .barcode-img {{ width: 100%; height: 60px; }}
                    .pagador-info {{ border: 1px solid #000; padding: 5px; margin-top: 10px; }}
                    .pagador-title {{ font-size: 10px; color: #555; margin-bottom: 5px; }}
                </style>
            </head>
            <body>
                <div class='boleto-container'>
                    <div class='header'>
                        <img src='https://www.itau.com.br/arquivos/itau.gif' class='logo' />
                        <div class='banco-info'>341-7</div>
                        <div class='linha-digitavel'>{dados.LinhaDigitavel}</div>
                    </div>
                    
                    <div class='table-row'>
                        <div class='table-cell-2 data-field'>
                            <div class='data-label'>Local de pagamento</div>
                            <div class='data-value'>{dados.LocalPagamento}</div>
                        </div>
                        <div class='table-cell data-field'>
                            <div class='data-label'>Vencimento</div>
                            <div class='data-value'>{dados.DataVencimento}</div>
                        </div>
                    </div>

                    <div class='table-row'>
                        <div class='table-cell-2 data-field'>
                            <div class='data-label'>Beneficiário</div>
                            <div class='data-value'>{dados.NomeBeneficiario}</div>
                        </div>
                        <div class='table-cell data-field'>
                            <div class='data-label'>Agência / Código do Beneficiário</div>
                            <div class='data-value'>{dados.AgenciaCodigoBeneficiario}</div>
                        </div>
                    </div>
                    
                    <div class='table-row'>
                        <div class='table-cell campo-menor data-field'>
                            <div class='data-label'>Data do documento</div>
                            <div class='data-value'>{dados.DataDocumento}</div>
                        </div>
                        <div class='table-cell campo-menor data-field'>
                            <div class='data-label'>Nº do documento</div>
                            <div class='data-value'>{dados.NumeroDocumento}</div>
                        </div>
                        <div class='table-cell campo-menor data-field'>
                            <div class='data-label'>Espécie doc.</div>
                            <div class='data-value'>-</div>
                        </div>
                        <div class='table-cell campo-menor data-field'>
                            <div class='data-label'>Aceite</div>
                            <div class='data-value'>N</div>
                        </div>
                        <div class='table-cell campo-menor data-field'>
                            <div class='data-label'>Data processamento</div>
                            <div class='data-value'>{DateTime.Now.ToString("dd/MM/yyyy")}</div>
                        </div>
                        <div class='table-cell data-field'>
                            <div class='data-label'>Carteira / Nosso número</div>
                            <div class='data-value'>{dados.CarteiraNossoNumero}</div>
                        </div>
                        <div class='table-cell data-field'>
                            <div class='data-label'>(=) Valor do documento</div>
                            <div class='data-value'>{dados.ValorBoletoFormatado}</div>
                        </div>
                    </div>

                    <div class='table-row'>
                        <div class='table-cell-2 data-field'>
                            <div class='data-label'>Instruções (Texto de responsabilidade do beneficiário)</div>
                            <div class='data-value'>{dados.MensagemLivre}</div>
                        </div>
                        <div class='table-cell data-field'>
                            <div class='data-label'>(-) Desconto / Abatimento</div>
                            <div class='data-value'></div>
                        </div>
                    </div>
                    <div class='table-row'>
                        <div class='table-cell-2 data-field' style='border-top:none;'>
                            <div class='data-value'></div>
                        </div>
                        <div class='table-cell data-field' style='border-top:none;'>
                            <div class='data-label'>(+) Mora / Multa</div>
                            <div class='data-value'></div>
                        </div>
                    </div>
                    <div class='table-row'>
                        <div class='table-cell-2 data-field' style='border-top:none;'>
                            <div class='data-value'></div>
                        </div>
                        <div class='table-cell data-field' style='border-top:none;'>
                            <div class='data-label'>(+) Outros acréscimos</div>
                            <div class='data-value'></div>
                        </div>
                    </div>
                    <div class='table-row'>
                        <div class='table-cell-2 data-field' style='border-top:none;'>
                            <div class='data-value'></div>
                        </div>
                        <div class='table-cell data-field' style='border-top:none;'>
                            <div class='data-label'>(=) Valor cobrado</div>
                            <div class='data-value'></div>
                        </div>
                    </div>
                    
                    <div class='pagador-info'>
                        <div class='pagador-title'>Pagador</div>
                        <div><b>{dados.NomePagador}</b> - {dados.DocumentoPagadorFormatado}</div>
                        <div>{dados.EnderecoPagadorCompleto}</div>
                    </div>
                    
                    <div class='barcode'>
                        <img src='{dados.CaminhoCodigoBarras}' class='barcode-img' />
                    </div>
                    <p style='text-align: center; margin-top: 5px;'>Corte na linha pontilhada</p>
                </div>
            </body>
            </html>";
        }

        // Este método não é mais necessário aqui, pois a formatação já é feita no BoletoService
        // private static string FormatDocumento(string documento) { ... }
    }
}