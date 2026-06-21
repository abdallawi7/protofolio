using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Windows.Forms;
using VegetableStoreApp.Database;

namespace VegetableStoreApp.Forms
{
    public class ReportForm : Form
    {
        private DataGridView dgvReport;
        private ComboBox cmbReportType;
        private Button btnGenerate;
        private Label lblSummary;

        public ReportForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "التقارير";
            this.Size = new System.Drawing.Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;

            // Report Type ComboBox
            Label lblReportType = new Label { Text = "نوع التقرير:", Location = new System.Drawing.Point(10, 10) };
            cmbReportType = new ComboBox
            {
                Location = new System.Drawing.Point(100, 10),
                Width = 300,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbReportType.Items.Add("مبيعات حسب الزبون");
            cmbReportType.Items.Add("مبيعات حسب المنتج");
            cmbReportType.Items.Add("المخزون الحالي");
            cmbReportType.Items.Add("إجمالي المبيعات");
            cmbReportType.SelectedIndex = 0;
            this.Controls.Add(lblReportType);
            this.Controls.Add(cmbReportType);

            // Generate Button
            btnGenerate = new Button { Text = "إنشاء التقرير", Location = new System.Drawing.Point(420, 10), Width = 120 };
            btnGenerate.Click += BtnGenerate_Click;
            this.Controls.Add(btnGenerate);

            // DataGridView
            dgvReport = new DataGridView
            {
                Location = new System.Drawing.Point(10, 50),
                Size = new System.Drawing.Size(860, 400),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true
            };
            this.Controls.Add(dgvReport);

            // Summary Label
            lblSummary = new Label
            {
                Location = new System.Drawing.Point(10, 460),
                Size = new System.Drawing.Size(860, 100),
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(5)
            };
            this.Controls.Add(lblSummary);
        }

        private void BtnGenerate_Click(object sender, EventArgs e)
        {
            try
            {
                switch (cmbReportType.SelectedIndex)
                {
                    case 0:
                        GenerateSalesByCustomer();
                        break;
                    case 1:
                        GenerateSalesByProduct();
                        break;
                    case 2:
                        GenerateInventoryReport();
                        break;
                    case 3:
                        GenerateTotalSales();
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ: {ex.Message}");
            }
        }

        private void GenerateSalesByCustomer()
        {
            List<dynamic> data = new List<dynamic>();
            decimal totalSales = 0;

            using (SQLiteConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string query = @"
                    SELECT c.CustomerName, COUNT(i.InvoiceID) as [عدد الفواتير], SUM(i.TotalAmount) as [إجمالي المبيعات]
                    FROM Customers c
                    LEFT JOIN Invoices i ON c.CustomerID = i.CustomerID
                    GROUP BY c.CustomerID, c.CustomerName
                    ORDER BY SUM(i.TotalAmount) DESC";

                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                {
                    using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd))
                    {
                        System.Data.DataTable dt = new System.Data.DataTable();
                        adapter.Fill(dt);
                        dgvReport.DataSource = dt;

                        foreach (System.Data.DataRow row in dt.Rows)
                        {
                            if (row[2] != DBNull.Value)
                                totalSales += Convert.ToDecimal(row[2]);
                        }
                    }
                }
            }

            lblSummary.Text = $"إجمالي المبيعات: {totalSales} د.أ";
        }

        private void GenerateSalesByProduct()
        {
            List<dynamic> data = new List<dynamic>();
            decimal totalSales = 0;

            using (SQLiteConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string query = @"
                    SELECT p.ProductName, SUM(id.Quantity) as [الكمية المباعة], SUM(id.Total) as [إجمالي المبيعات]
                    FROM Products p
                    LEFT JOIN InvoiceDetails id ON p.ProductID = id.ProductID
                    GROUP BY p.ProductID, p.ProductName
                    ORDER BY SUM(id.Total) DESC";

                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                {
                    using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd))
                    {
                        System.Data.DataTable dt = new System.Data.DataTable();
                        adapter.Fill(dt);
                        dgvReport.DataSource = dt;

                        foreach (System.Data.DataRow row in dt.Rows)
                        {
                            if (row[2] != DBNull.Value)
                                totalSales += Convert.ToDecimal(row[2]);
                        }
                    }
                }
            }

            lblSummary.Text = $"إجمالي المبيعات: {totalSales} د.أ";
        }

        private void GenerateInventoryReport()
        {
            using (SQLiteConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string query = @"
                    SELECT p.ProductName, s.SupplierName, p.Quantity as [الكمية الحالية], p.Price as [السعر]
                    FROM Products p
                    LEFT JOIN Suppliers s ON p.SupplierID = s.SupplierID
                    ORDER BY p.Quantity ASC";

                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                {
                    using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd))
                    {
                        System.Data.DataTable dt = new System.Data.DataTable();
                        adapter.Fill(dt);
                        dgvReport.DataSource = dt;
                    }
                }
            }

            lblSummary.Text = "تقرير المخزون الحالي";
        }

        private void GenerateTotalSales()
        {
            decimal totalSales = 0;
            int totalInvoices = 0;
            decimal averageSale = 0;

            using (SQLiteConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string query = @"
                    SELECT InvoiceID, CustomerID, InvoiceDate, TotalAmount
                    FROM Invoices
                    ORDER BY InvoiceDate DESC";

                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                {
                    using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd))
                    {
                        System.Data.DataTable dt = new System.Data.DataTable();
                        adapter.Fill(dt);
                        dgvReport.DataSource = dt;

                        totalInvoices = dt.Rows.Count;
                        foreach (System.Data.DataRow row in dt.Rows)
                        {
                            totalSales += Convert.ToDecimal(row[3]);
                        }

                        if (totalInvoices > 0)
                            averageSale = totalSales / totalInvoices;
                    }
                }
            }

            lblSummary.Text = $"إجمالي الفواتير: {totalInvoices}\nإجمالي المبيعات: {totalSales} د.أ\nمتوسط الفاتورة: {averageSale:F2} د.أ";
        }
    }
}