using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Windows.Forms;
using VegetableStoreApp.Database;
using VegetableStoreApp.Models;

namespace VegetableStoreApp.Forms
{
    public class InvoiceForm : Form
    {
        private DataGridView dgvInvoices;
        private ComboBox cmbCustomer, cmbProduct;
        private TextBox txtQuantity, txtPrice;
        private Button btnNewInvoice, btnAddItem, btnComplete, btnCancel;
        private Label lblTotal;
        private List<Customer> customers;
        private List<Product> products;
        private List<InvoiceDetail> currentInvoiceDetails;
        private int currentInvoiceID = 0;
        private decimal invoiceTotal = 0;

        public InvoiceForm()
        {
            InitializeComponent();
            LoadCustomers();
            LoadProducts();
        }

        private void InitializeComponent()
        {
            this.Text = "إدارة الفواتير";
            this.Size = new System.Drawing.Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;

            // Customers ComboBox
            Label lblCustomer = new Label { Text = "الزبون:", Location = new System.Drawing.Point(10, 10) };
            cmbCustomer = new ComboBox { Location = new System.Drawing.Point(100, 10), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            this.Controls.Add(lblCustomer);
            this.Controls.Add(cmbCustomer);

            // Product ComboBox
            Label lblProduct = new Label { Text = "المنتج:", Location = new System.Drawing.Point(320, 10) };
            cmbProduct = new ComboBox { Location = new System.Drawing.Point(400, 10), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            this.Controls.Add(lblProduct);
            this.Controls.Add(cmbProduct);

            // Quantity
            Label lblQuantity = new Label { Text = "الكمية:", Location = new System.Drawing.Point(10, 50) };
            txtQuantity = new TextBox { Location = new System.Drawing.Point(100, 50), Width = 200 };
            this.Controls.Add(lblQuantity);
            this.Controls.Add(txtQuantity);

            // Price
            Label lblPrice = new Label { Text = "السعر:", Location = new System.Drawing.Point(320, 50) };
            txtPrice = new TextBox { Location = new System.Drawing.Point(400, 50), Width = 200, ReadOnly = true };
            this.Controls.Add(lblPrice);
            this.Controls.Add(txtPrice);

            // Buttons
            btnNewInvoice = new Button { Text = "فاتورة جديدة", Location = new System.Drawing.Point(10, 90), Width = 100 };
            btnNewInvoice.Click += BtnNewInvoice_Click;
            this.Controls.Add(btnNewInvoice);

            btnAddItem = new Button { Text = "إضافة صنف", Location = new System.Drawing.Point(120, 90), Width = 100 };
            btnAddItem.Click += BtnAddItem_Click;
            this.Controls.Add(btnAddItem);

            btnComplete = new Button { Text = "إنهاء الفاتورة", Location = new System.Drawing.Point(230, 90), Width = 100 };
            btnComplete.Click += BtnComplete_Click;
            this.Controls.Add(btnComplete);

            btnCancel = new Button { Text = "إلغاء", Location = new System.Drawing.Point(340, 90), Width = 100 };
            btnCancel.Click += BtnCancel_Click;
            this.Controls.Add(btnCancel);

            // DataGridView
            dgvInvoices = new DataGridView
            {
                Location = new System.Drawing.Point(10, 130),
                Size = new System.Drawing.Size(860, 350),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true
            };
            this.Controls.Add(dgvInvoices);

            // Total Label
            lblTotal = new Label { Text = "الإجمالي: 0 د.أ", Location = new System.Drawing.Point(10, 490), Font = new System.Drawing.Font("Arial", 14, System.Drawing.FontStyle.Bold) };
            this.Controls.Add(lblTotal);
        }

        private void LoadCustomers()
        {
            customers = new List<Customer>();
            try
            {
                using (SQLiteConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT * FROM Customers";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                customers.Add(new Customer
                                {
                                    CustomerID = Convert.ToInt32(reader["CustomerID"]),
                                    CustomerName = reader["CustomerName"].ToString()
                                });
                            }
                        }
                    }
                }
                cmbCustomer.DataSource = customers;
                cmbCustomer.DisplayMember = "CustomerName";
                cmbCustomer.ValueMember = "CustomerID";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ: {ex.Message}");
            }
        }

        private void LoadProducts()
        {
            products = new List<Product>();
            try
            {
                using (SQLiteConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT * FROM Products";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                products.Add(new Product
                                {
                                    ProductID = Convert.ToInt32(reader["ProductID"]),
                                    ProductName = reader["ProductName"].ToString(),
                                    Price = Convert.ToDecimal(reader["Price"]),
                                    Quantity = Convert.ToInt32(reader["Quantity"])
                                });
                            }
                        }
                    }
                }
                cmbProduct.DataSource = products;
                cmbProduct.DisplayMember = "ProductName";
                cmbProduct.ValueMember = "ProductID";
                cmbProduct.SelectedIndexChanged += CmbProduct_SelectedIndexChanged;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ: {ex.Message}");
            }
        }

        private void CmbProduct_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbProduct.SelectedItem is Product product)
            {
                txtPrice.Text = product.Price.ToString();
            }
        }

        private void BtnNewInvoice_Click(object sender, EventArgs e)
        {
            currentInvoiceDetails = new List<InvoiceDetail>();
            invoiceTotal = 0;
            dgvInvoices.DataSource = currentInvoiceDetails;
            lblTotal.Text = "الإجمالي: 0 د.أ";
            MessageBox.Show("فاتورة جديدة - ابدأ بإضافة الأصناف");
        }

        private void BtnAddItem_Click(object sender, EventArgs e)
        {
            if (currentInvoiceDetails == null)
            {
                MessageBox.Show("يرجى إنشاء فاتورة جديدة أولاً");
                return;
            }

            if (string.IsNullOrEmpty(txtQuantity.Text))
            {
                MessageBox.Show("يرجى إدخال الكمية");
                return;
            }

            try
            {
                int quantity = int.Parse(txtQuantity.Text);
                decimal price = decimal.Parse(txtPrice.Text);
                decimal total = quantity * price;

                InvoiceDetail detail = new InvoiceDetail
                {
                    ProductID = Convert.ToInt32(cmbProduct.SelectedValue),
                    Quantity = quantity,
                    Price = price,
                    Total = total
                };

                currentInvoiceDetails.Add(detail);
                invoiceTotal += total;
                dgvInvoices.DataSource = new List<InvoiceDetail>(currentInvoiceDetails);
                lblTotal.Text = $"الإجمالي: {invoiceTotal} د.أ";
                txtQuantity.Clear();
                MessageBox.Show("تمت إضافة الصنف");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ: {ex.Message}");
            }
        }

        private void BtnComplete_Click(object sender, EventArgs e)
        {
            if (currentInvoiceDetails == null || currentInvoiceDetails.Count == 0)
            {
                MessageBox.Show("يرجى إضافة أصناف للفاتورة");
                return;
            }

            try
            {
                int customerID = Convert.ToInt32(cmbCustomer.SelectedValue);
                using (SQLiteConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    
                    // إدراج الفاتورة
                    string invoiceQuery = "INSERT INTO Invoices (CustomerID, TotalAmount) VALUES (@customer, @total); SELECT last_insert_rowid();";
                    using (SQLiteCommand cmd = new SQLiteCommand(invoiceQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@customer", customerID);
                        cmd.Parameters.AddWithValue("@total", invoiceTotal);
                        currentInvoiceID = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    // إدراج تفاصيل الفاتورة
                    string detailQuery = "INSERT INTO InvoiceDetails (InvoiceID, ProductID, Quantity, Price, Total) VALUES (@invoice, @product, @quantity, @price, @total)";
                    foreach (var detail in currentInvoiceDetails)
                    {
                        using (SQLiteCommand cmd = new SQLiteCommand(detailQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@invoice", currentInvoiceID);
                            cmd.Parameters.AddWithValue("@product", detail.ProductID);
                            cmd.Parameters.AddWithValue("@quantity", detail.Quantity);
                            cmd.Parameters.AddWithValue("@price", detail.Price);
                            cmd.Parameters.AddWithValue("@total", detail.Total);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                MessageBox.Show($"تم حفظ الفاتورة رقم {currentInvoiceID} بنجاح");
                BtnNewInvoice_Click(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ: {ex.Message}");
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            currentInvoiceDetails = null;
            invoiceTotal = 0;
            dgvInvoices.DataSource = null;
            lblTotal.Text = "الإجمالي: 0 د.أ";
            txtQuantity.Clear();
        }
    }
}