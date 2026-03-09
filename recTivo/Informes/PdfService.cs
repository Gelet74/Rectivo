using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using recTivo.Backend.Modelos;
using recTivo.MVVM;
using System.IO;

namespace recTivo.Informes
{
    /// <summary>
    /// Servicio central para generar informes PDF en recTivo.
    /// Requiere el paquete NuGet: QuestPDF
    /// </summary>
    public static class PdfService
    {
        static PdfService()
        {
            // Licencia gratuita Community (proyectos no comerciales)
            QuestPDF.Settings.License = LicenseType.Community;
        }

        // ================================================================
        //  1. LISTADO DE ÓRDENES
        // ================================================================

        public static string GenerarListadoOrdenes(
            IEnumerable<OrdenViewModel> ordenes,
            string? rutaDestino = null)
        {
            var lista = ordenes.ToList();
            rutaDestino ??= RutaDescargas($"Ordenes_{Fecha()}.pdf");

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    ConfigurarPagina(page);
                    page.Header().Element(c => Cabecera(c, "LISTADO DE ÓRDENES"));

                    page.Content().PaddingTop(10).Table(tabla =>
                    {
                        tabla.ColumnsDefinition(cols =>
                        {
                            cols.ConstantColumn(60);
                            cols.RelativeColumn(3);
                            cols.ConstantColumn(55);
                            cols.ConstantColumn(85);
                            cols.ConstantColumn(65);
                        });

                        tabla.Header(h =>
                        {
                            CeldaCabecera(h.Cell(), "CÓDIGO");
                            CeldaCabecera(h.Cell(), "DESCRIPCIÓN");
                            CeldaCabecera(h.Cell(), "CANT.");
                            CeldaCabecera(h.Cell(), "FECHA FIN");
                            CeldaCabecera(h.Cell(), "ESTADO");
                        });

                        bool par = false;
                        foreach (var o in lista)
                        {
                            var bg = par ? Colors.Grey.Lighten4 : Colors.White;
                            par = !par;

                            CeldaDato(tabla.Cell(), o.Codigo, bg);
                            CeldaDato(tabla.Cell(), o.Descripcion, bg);
                            CeldaDato(tabla.Cell(), o.Cantidad.ToString(), bg, Alineacion.Centro);
                            CeldaDato(tabla.Cell(), o.FechaFin, bg, Alineacion.Centro);
                            CeldaEstado(tabla.Cell(), o.Estado, bg);
                        }
                    });

                    page.Footer().Element(Pie);
                });
            })
            .GeneratePdf(rutaDestino);

            return rutaDestino;
        }

        // ================================================================
        //  2. DETALLE DE UNA ORDEN (con fases)
        // ================================================================

        public static string GenerarDetalleOrden(
            OrdenViewModel orden,
            IEnumerable<OrdenFase> fases,
            string? rutaDestino = null)
        {
            rutaDestino ??= RutaDescargas($"Orden_{orden.Codigo}_{Fecha()}.pdf");

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    ConfigurarPagina(page);
                    page.Header().Element(c => Cabecera(c, $"ORDEN DE FABRICACIÓN · {orden.Codigo}"));

                    page.Content().Column(col =>
                    {
                        col.Spacing(12);

                        // ── Datos generales ──────────────────────────────
                        col.Item()
                            .Border(1).BorderColor(Colors.Grey.Lighten2)
                            .Padding(10)
                            .Column(datos =>
                            {
                                datos.Item()
                                    .Text("DATOS DE LA ORDEN")
                                    .Bold().FontSize(9).FontColor(ColorPrimario);

                                datos.Item().PaddingTop(6).Row(row =>
                                {
                                    FilaDato(row.RelativeItem(), "Código", orden.Codigo);
                                    FilaDato(row.RelativeItem(), "Descripción", orden.Descripcion);
                                });

                                datos.Item().PaddingTop(4).Row(row =>
                                {
                                    FilaDato(row.RelativeItem(), "Cantidad", orden.Cantidad.ToString());
                                    FilaDato(row.RelativeItem(), "Fecha fin", orden.FechaFin);
                                    FilaDato(row.RelativeItem(), "Estado", orden.Estado);
                                });

                                if (!string.IsNullOrWhiteSpace(orden.Descrip2))
                                {
                                    datos.Item().PaddingTop(4);
                                    FilaDatoSimple(datos.Item(), "Descripción 2", orden.Descrip2);
                                }
                            });

                        // ── Fases ────────────────────────────────────────
                        var listaFases = fases.ToList();

                        if (listaFases.Any())
                        {
                            col.Item()
                                .Text("FASES DE FABRICACIÓN")
                                .Bold().FontSize(9).FontColor(ColorPrimario);

                            col.Item().Table(tabla =>
                            {
                                tabla.ColumnsDefinition(cols =>
                                {
                                    cols.RelativeColumn(3);
                                    cols.ConstantColumn(60);
                                    cols.ConstantColumn(55);
                                    cols.ConstantColumn(55);
                                    cols.ConstantColumn(85);
                                    cols.ConstantColumn(65);
                                });

                                tabla.Header(h =>
                                {
                                    CeldaCabecera(h.Cell(), "FASE");
                                    CeldaCabecera(h.Cell(), "ENTRADA");
                                    CeldaCabecera(h.Cell(), "OK");
                                    CeldaCabecera(h.Cell(), "DEFECTO");
                                    CeldaCabecera(h.Cell(), "FECHA FIN");
                                    CeldaCabecera(h.Cell(), "ESTADO");
                                });

                                bool par = false;
                                foreach (var fase in listaFases.OrderBy(f => f.NumeroFase))
                                {
                                    var bg = par ? Colors.Grey.Lighten4 : Colors.White;
                                    par = !par;

                                    CeldaDato(tabla.Cell(), fase.NombreFaseTexto, bg);
                                    CeldaDato(tabla.Cell(), fase.CantidadEntrada.ToString(), bg, Alineacion.Centro);
                                    CeldaDato(tabla.Cell(), fase.CantidadOK?.ToString() ?? "—", bg, Alineacion.Centro);
                                    CeldaDato(tabla.Cell(), fase.CantidadDefecto?.ToString() ?? "—", bg, Alineacion.Centro);
                                    CeldaDato(tabla.Cell(), fase.FechaFin?.ToString("dd/MM/yyyy") ?? "—", bg, Alineacion.Centro);
                                    CeldaEstado(tabla.Cell(), fase.EstadoTexto, bg);
                                }
                            });
                        }
                        else
                        {
                            col.Item()
                                .Text("Esta orden no tiene fases asociadas.")
                                .FontSize(9).Italic().FontColor(Colors.Grey.Medium);
                        }
                    });

                    page.Footer().Element(Pie);
                });
            })
            .GeneratePdf(rutaDestino);

            return rutaDestino;
        }

        // ================================================================
        //  3. LISTADO DE ARTÍCULOS / STOCK
        //     Formato apaisado (Landscape) para incluir la columna Ubicación
        // ================================================================

        public static string GenerarListadoArticulos(
            IEnumerable<Articulo> articulos,
            string? rutaDestino = null)
        {
            var lista = articulos.ToList();
            rutaDestino ??= RutaDescargas($"Articulos_{Fecha()}.pdf");

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    // Apaisado para que quepan todas las columnas
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1.5f, Unit.Centimetre);
                    page.DefaultTextStyle(t => t.FontSize(8).FontFamily("Arial"));

                    page.Header().Element(c => Cabecera(c, "LISTADO DE ARTÍCULOS"));

                    page.Content().PaddingTop(10).Table(tabla =>
                    {
                        tabla.ColumnsDefinition(cols =>
                        {
                            cols.ConstantColumn(65);    // Código
                            cols.RelativeColumn(2);     // Descripción
                            cols.RelativeColumn(1.5f);  // Descripción 2
                            cols.ConstantColumn(50);    // Stock
                            cols.ConstantColumn(65);    // PVP
                            cols.ConstantColumn(75);    // P. Compra
                            cols.RelativeColumn(2);     // Ubicación
                        });

                        tabla.Header(h =>
                        {
                            CeldaCabecera(h.Cell(), "CÓDIGO");
                            CeldaCabecera(h.Cell(), "DESCRIPCIÓN");
                            CeldaCabecera(h.Cell(), "DESCRIPCIÓN 2");
                            CeldaCabecera(h.Cell(), "STOCK");
                            CeldaCabecera(h.Cell(), "PVP");
                            CeldaCabecera(h.Cell(), "P. COMPRA");
                            CeldaCabecera(h.Cell(), "UBICACIÓN");
                        });

                        bool par = false;
                        foreach (var a in lista.OrderBy(x => x.Codigo))
                        {
                            var bg = par ? Colors.Grey.Lighten4 : Colors.White;
                            par = !par;

                            CeldaDato(tabla.Cell(), a.Codigo, bg, Alineacion.Centro);
                            CeldaDato(tabla.Cell(), a.descrip ?? "—", bg, Alineacion.Centro);
                            CeldaDato(tabla.Cell(), a.descrip2 ?? "—", bg, Alineacion.Centro);
                            CeldaDato(tabla.Cell(), a.StockTotal.ToString(), bg, Alineacion.Centro);
                            CeldaDato(tabla.Cell(), a.Pvp.HasValue ? $"{a.Pvp:F2} €" : "—", bg, Alineacion.Centro);
                            CeldaDato(tabla.Cell(), a.PrecioCompra.HasValue ? $"{a.PrecioCompra:F2} €" : "—", bg, Alineacion.Centro);
                            CeldaDato(tabla.Cell(), a.UbicacionesResumen, bg, Alineacion.Centro);
                        }
                    });

                    page.Footer().Element(Pie);
                });
            })
            .GeneratePdf(rutaDestino);

            return rutaDestino;
        }

        // ================================================================
        //  4. ESCANDALLO DE UN PRODUCTO
        // ================================================================

        public static string GenerarEscandallo(
            Escandallo escandallo,
            IEnumerable<ComponenteEscandallo> componentes,
            string? rutaDestino = null)
        {
            rutaDestino ??= RutaDescargas($"Escandallo_{escandallo.CodigoProducto}_{Fecha()}.pdf");

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    ConfigurarPagina(page);
                    page.Header().Element(c =>
                        Cabecera(c, $"ESCANDALLO · {escandallo.CodigoProducto}"));

                    page.Content().Column(col =>
                    {
                        col.Spacing(12);

                        // ── Cabecera producto ────────────────────────────
                        col.Item()
                            .Border(1).BorderColor(Colors.Grey.Lighten2)
                            .Padding(10)
                            .Column(d =>
                            {
                                d.Item()
                                    .Text("PRODUCTO")
                                    .Bold().FontSize(9).FontColor(ColorPrimario);

                                d.Item().PaddingTop(6).Row(row =>
                                {
                                    FilaDato(row.RelativeItem(), "Código", escandallo.CodigoProducto);
                                    FilaDato(row.RelativeItem(), "Descripción", escandallo.Descrip);
                                });

                                if (!string.IsNullOrWhiteSpace(escandallo.Descrip2))
                                {
                                    d.Item().PaddingTop(4);
                                    FilaDatoSimple(d.Item(), "Descripción 2", escandallo.Descrip2);
                                }
                            });

                        // ── Componentes ──────────────────────────────────
                        col.Item()
                            .Text("COMPONENTES")
                            .Bold().FontSize(9).FontColor(ColorPrimario);

                        var listaComp = componentes.ToList();

                        if (listaComp.Any())
                        {
                            col.Item().Table(tabla =>
                            {
                                tabla.ColumnsDefinition(cols =>
                                {
                                    cols.ConstantColumn(80);
                                    cols.RelativeColumn(3);
                                    cols.ConstantColumn(65);
                                    cols.ConstantColumn(80);
                                });

                                tabla.Header(h =>
                                {
                                    CeldaCabecera(h.Cell(), "CÓDIGO");
                                    CeldaCabecera(h.Cell(), "DESCRIPCIÓN");
                                    CeldaCabecera(h.Cell(), "CANTIDAD");
                                    CeldaCabecera(h.Cell(), "P. UNITARIO");
                                });

                                bool par = false;
                                foreach (var comp in listaComp)
                                {
                                    var bg = par ? Colors.Grey.Lighten4 : Colors.White;
                                    par = !par;

                                    CeldaDato(tabla.Cell(), comp.CodigoArticulo, bg);
                                    CeldaDato(tabla.Cell(), comp.Descripcion ?? comp.NombreComponente ?? "—", bg);
                                    CeldaDato(tabla.Cell(), comp.Cantidad?.ToString("F2") ?? "—", bg, Alineacion.Centro);
                                    CeldaDato(tabla.Cell(), comp.PrecioUnitario.HasValue ? $"{comp.PrecioUnitario:F2} €" : "—", bg, Alineacion.Derecha);
                                }
                            });

                            // Total si hay precios
                            var total = listaComp
                                .Where(c => c.PrecioUnitario.HasValue && c.Cantidad.HasValue)
                                .Sum(c => c.PrecioUnitario!.Value * c.Cantidad!.Value);

                            if (total > 0)
                            {
                                col.Item().AlignRight()
                                    .Text($"Coste total componentes:  {total:F2} €")
                                    .Bold().FontSize(10);
                            }
                        }
                        else
                        {
                            col.Item()
                                .Text("Este escandallo no tiene componentes.")
                                .FontSize(9).Italic().FontColor(Colors.Grey.Medium);
                        }
                    });

                    page.Footer().Element(Pie);
                });
            })
            .GeneratePdf(rutaDestino);

            return rutaDestino;
        }

        // ================================================================
        //  HELPERS PRIVADOS
        // ================================================================

        private const string ColorPrimario = "#1565C0";

        private static void ConfigurarPagina(PageDescriptor page)
        {
            page.Size(PageSizes.A4);
            page.Margin(1.5f, Unit.Centimetre);
            page.DefaultTextStyle(t => t.FontSize(8).FontFamily("Arial"));
        }

        private static void Cabecera(IContainer c, string titulo)
        {
            c.BorderBottom(2).BorderColor(ColorPrimario).PaddingBottom(6).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("recTivo").Bold().FontSize(16).FontColor(ColorPrimario);
                    col.Item().Text(titulo).Bold().FontSize(11);
                });
                row.ConstantItem(130).AlignRight().AlignMiddle()
                    .Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}")
                    .FontSize(7).FontColor(Colors.Grey.Medium);
            });
        }

        private static void Pie(IContainer c)
        {
            c.BorderTop(1).BorderColor(Colors.Grey.Lighten2).PaddingTop(4).Row(row =>
            {
                row.RelativeItem()
                    .Text("recTivo · Sistema de gestión de producción")
                    .FontSize(7).FontColor(Colors.Grey.Medium);

                row.ConstantItem(80).AlignRight().Text(x =>
                {
                    x.Span("Página ").FontSize(7).FontColor(Colors.Grey.Medium);
                    x.CurrentPageNumber().FontSize(7).FontColor(Colors.Grey.Medium);
                    x.Span(" de ").FontSize(7).FontColor(Colors.Grey.Medium);
                    x.TotalPages().FontSize(7).FontColor(Colors.Grey.Medium);
                });
            });
        }

        private static void CeldaCabecera(IContainer c, string texto)
        {
            c.Background(ColorPrimario).Padding(5)
             .Text(texto).Bold().FontSize(7.5f).FontColor(Colors.White).AlignCenter();
        }

        private enum Alineacion { Izquierda, Centro, Derecha }

        private static void CeldaDato(IContainer c, string texto, string bg,
            Alineacion align = Alineacion.Izquierda)
        {
            var t = c.Background(bg)
                     .BorderBottom(1).BorderColor(Colors.Grey.Lighten3)
                     .Padding(4)
                     .Text(texto ?? "").FontSize(8);

            switch (align)
            {
                case Alineacion.Centro: t.AlignCenter(); break;
                case Alineacion.Derecha: t.AlignRight(); break;
            }
        }

        private static void CeldaEstado(IContainer c, string estado, string bg)
        {
            string color = estado switch
            {
                "Cerrada" => Colors.Green.Darken2,
                "En curso" => Colors.Orange.Darken2,
                _ => Colors.Grey.Darken1
            };

            c.Background(bg)
             .BorderBottom(1).BorderColor(Colors.Grey.Lighten3)
             .Padding(4).AlignCenter()
             .Text(estado).FontSize(7.5f).FontColor(color).Bold();
        }

        private static void FilaDato(IContainer c, string etiqueta, string valor)
        {
            c.Column(col =>
            {
                col.Item().Text(etiqueta.ToUpper()).FontSize(7).FontColor(Colors.Grey.Medium);
                col.Item().Text(valor ?? "—").Bold().FontSize(9);
            });
        }

        private static void FilaDatoSimple(IContainer c, string etiqueta, string valor)
        {
            c.Row(row =>
            {
                row.ConstantItem(80).Text(etiqueta + ":").FontSize(8).FontColor(Colors.Grey.Medium);
                row.RelativeItem().Text(valor ?? "—").FontSize(8).Bold();
            });
        }

        private static string Fecha() => DateTime.Now.ToString("yyyyMMdd_HHmm");

        private static string RutaDescargas(string nombre)
        {
            var carpeta = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "recTivo", "Informes");
            Directory.CreateDirectory(carpeta);
            return Path.Combine(carpeta, nombre);
        }
    }
}