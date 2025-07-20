using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using iTextSharp.text.pdf;
using iTextSharp.text;
using System.IO;

public partial class Report : System.Web.UI.Page
{
    public static String CS = ConfigurationManager.ConnectionStrings["MyShoppingDB"].ConnectionString;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            if (Session["Username"] != null)
            {
                bindGrid1();
                bindGrid2();
            }
            else
            {
                Response.Redirect("Signin.aspx");
            }
        } 
    }

    private void bindGrid1()
    {

        SqlConnection con = new SqlConnection(CS);
        string qr = "select t1.OrderID,t3.Name,t2.PName,t1.Quantity as QtySell,t4.Quantity as StockOpening,t4.Quantity-t1.Quantity as Available  from tblOrderProducts as t1 inner join tblProducts as t2 on t2.PID=t1.PID inner join tblUsers as t3 on t3.Uid=t1.UserID inner join tblProductSizeQuantity as t4 on t4.PID=t1.PID";
        SqlCommand cmd = new SqlCommand(qr, con);
        SqlDataAdapter da = new SqlDataAdapter(cmd);
        DataTable dt = new DataTable();
        da.Fill(dt);
        con.Close();
        GridView1.DataSource = dt;
        GridView1.DataBind();
    }

    private void bindGrid2()
    {

        SqlConnection con = new SqlConnection(CS);
        string qr = "select  distinct t2.PName,t1.Quantity from tblProductSizeQuantity as t1 inner join tblProducts as t2 on t2.PID=t1.PID";
        SqlCommand cmd = new SqlCommand(qr, con);
        SqlDataAdapter da = new SqlDataAdapter(cmd);
        DataTable dt = new DataTable();
        da.Fill(dt);
        con.Close();
        GridView2.DataSource = dt;
        GridView2.DataBind();
    }
    public class SalesItem
    {
        public string Product { get; set; }
        public int Sold { get; set; }
    }

    public class YearlySalesItem
    {
        public string Month { get; set; }
        public decimal TotalSales { get; set; }
    }

    protected void Button1_Click(object sender, EventArgs e)
    {
        List<SalesItem> salesData = new List<SalesItem>
    {
        new SalesItem { Product = "Shirt", Sold = 50 },
        new SalesItem { Product = "Pants", Sold = 30 },
        new SalesItem { Product = "Shoes", Sold = 20 }
    };

        Document doc = new Document(PageSize.A4);
        MemoryStream ms = new MemoryStream();
        PdfWriter.GetInstance(doc, ms);
        doc.Open();

        // 🔥 Add the logo at the top
        string logoPath = Server.MapPath("~/Images/royal-logo.png");
        if (File.Exists(logoPath))
        {
            iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(logoPath);
            logo.ScaleAbsolute(300, 100); // resize logo
            logo.Alignment = Element.ALIGN_CENTER;
            doc.Add(logo);
        }

        // 📄 Title
        var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
        Paragraph title = new Paragraph("Sales Report", titleFont)
        {
            Alignment = Element.ALIGN_CENTER
        };
        doc.Add(title);

        doc.Add(new Paragraph("Generated on: " + DateTime.Now.ToString("dd MMM yyyy")));
        doc.Add(new Paragraph(" ")); // spacer

        // 📊 Table
        PdfPTable table = new PdfPTable(2);
        table.WidthPercentage = 100;
        table.AddCell("Product");
        table.AddCell("Sold");

        foreach (var item in salesData)
        {
            table.AddCell(item.Product);
            table.AddCell(item.Sold.ToString());
        }

        doc.Add(table);
        doc.Close();

        // 📨 Return the PDF
        byte[] bytes = ms.ToArray();
        Response.Clear();
        Response.ContentType = "application/pdf";
        Response.AddHeader("Content-Disposition", "attachment; filename=SalesReport.pdf");
        Response.BinaryWrite(bytes);
        Response.End();
    }

    private List<YearlySalesItem> GetYearlySalesData(int year)
    {
        return new List<YearlySalesItem>
    {
        new YearlySalesItem { Month = "October", TotalSales = 11500 },
        new YearlySalesItem { Month = "November", TotalSales = 13200 },
        new YearlySalesItem { Month = "December", TotalSales = 14500 },
        new YearlySalesItem { Month = "January", TotalSales = 12000 },
        new YearlySalesItem { Month = "February", TotalSales = 9800 },
    };
    }

    protected void Button2_Click(object sender, EventArgs e)
    {
        int year = DateTime.Now.Year; // or set dynamically
        List<YearlySalesItem> salesData = GetYearlySalesData(year); // simulate from DB

        string logoPath = Server.MapPath("~/Images/royal-logo.png");
        
        Document doc = new Document(PageSize.A4, 25, 25, 30, 30);
        MemoryStream ms = new MemoryStream();
        PdfWriter.GetInstance(doc, ms);
        doc.Open();

        // 🖼 Add Logo
        if (File.Exists(logoPath))
        {
            var logo = iTextSharp.text.Image.GetInstance(logoPath);
            logo.ScaleAbsolute(300, 100);
            logo.Alignment = Element.ALIGN_CENTER;
            doc.Add(logo);
        }

        // 📄 Title
        Font titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
        Paragraph title = new Paragraph("Monthly Sales Report", titleFont)
        {
            Alignment = Element.ALIGN_CENTER
        };
        doc.Add(title);

        doc.Add(new Paragraph("Generated on: " + DateTime.Now.ToString("dd MMMM yyyy")));
        doc.Add(new Paragraph(" "));

        // 📊 Table
        PdfPTable table = new PdfPTable(2);
        table.WidthPercentage = 100;
        table.SetWidths(new float[] { 5, 3 });

        Font headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
        table.AddCell(new PdfPCell(new Phrase("Month", headerFont)));
        table.AddCell(new PdfPCell(new Phrase("Total Sales", headerFont)));

        Font cellFont = FontFactory.GetFont(FontFactory.HELVETICA, 11);
        decimal grandTotal = 0;

        foreach (var item in salesData)
        {
            table.AddCell(new PdfPCell(new Phrase(item.Month, cellFont)));
            table.AddCell(new PdfPCell(new Phrase(item.TotalSales.ToString("N2"), cellFont)));
            grandTotal += item.TotalSales;
        }

        doc.Add(table);

        // 📌 Summary
        Paragraph totalPara = new Paragraph("\nGrand Total: ₹" + grandTotal.ToString("N2"), titleFont)
        {
            Alignment = Element.ALIGN_RIGHT
        };
        doc.Add(totalPara);

        doc.Close();

        byte[] bytes = ms.ToArray();
        Response.Clear();
        Response.ContentType = "application/pdf";
        Response.AddHeader("Content-Disposition", "attachment; filename=MonthlySalesReport.pdf");
        Response.BinaryWrite(bytes);
        Response.End();
    }
}