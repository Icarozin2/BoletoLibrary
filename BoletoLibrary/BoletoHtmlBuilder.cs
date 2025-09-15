using System;

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
                    * {{ box-sizing: border-box; }}
                    html, body {{ margin: 0; padding: 0; }}
                    body {{ font-family: Arial, sans-serif; font-size: 11px; }}

                    /* Página e impressão: não deixar o engine “ajustar” escala */
                    @page {{
                        margin: 20mm;
                        size: A4;
                    }}
                    @media print {{
                        /* Largura segura para não forçar shrink em A4 com margens */
                        .boleto-container {{ width: 180mm; }}
                        img {{ -webkit-print-color-adjust: exact; print-color-adjust: exact; }}
                    }}

                    /* Largura segura também no modo tela (evita shrink ao converter) */
                    .boleto-container {{
                        width: 700px;               /* 680-720px é uma faixa segura */
                        margin: 20px auto;
                        border: 1px solid #000;
                        padding: 10px;
                    }}

                    .header {{ display: flex; align-items: center; justify-content: space-between; margin-bottom: 10px; gap: 8px; }}
                    .logo {{ width: 150px; }}
                    .banco-info {{ border: 1px solid #000; padding: 5px; text-align: center; font-weight: bold; font-size: 14px; width: 100px; }}
                    .linha-digitavel {{ font-size: 16px; font-weight: bold; text-align: right; }}
                    .data-field {{ border: 1px solid #000; padding: 4px; margin-bottom: 5px; }}
                    .data-label {{ font-size: 10px; color: #555; }}
                    .data-value {{ font-size: 12px; font-weight: bold; }}
                    .table-row {{ display: flex; gap: 0; }}
                    .table-cell {{ flex: 1; border: 1px solid #000; padding: 4px; }}
                    .table-cell-2 {{ flex: 2; border: 1px solid #000; padding: 4px; }}
                    .campo-menor {{ flex: 0.5; }}
                    .pagador-info {{ border: 1px solid #000; padding: 5px; margin-top: 10px; }}
                    .pagador-title {{ font-size: 10px; color: #555; margin-bottom: 5px; }}

                    /* BLOCO DO BARCODE – NÃO ESCALAR! */
                    .barcode {{ text-align: center; margin-top: 14px; }}
                    .barcode-img {{
                        display: block;
                        margin: 0 auto;
                        /* NÃO force width/height – preserve tamanho nativo do arquivo */
                        width: auto !important;
                        height: auto !important;
                        max-width: none !important;
                        max-height: none !important;

                        /* evitar interpolação / reamostragem */
                        image-rendering: pixelated;
                        image-rendering: crisp-edges;
                        -ms-interpolation-mode: nearest-neighbor;
                    }}

                    .cutline {{ text-align: center; margin-top: 8px; }}
                </style>
            </head>
            <body>
                <div class='boleto-container'>
                    <div class='header'>
                        <img src={dados.LogoSrc} class='logo' />
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
                            <div class='data-value'>{DateTime.Now:dd/MM/yyyy}</div>
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
                        <img src='{dados.CaminhoCodigoBarras}' class='barcode-img' alt='Código de barras do boleto' />
                    </div>
                    <p class='cutline'>Corte na linha pontilhada</p>
                </div>
            </body>
            </html>";
        }
    }
}
