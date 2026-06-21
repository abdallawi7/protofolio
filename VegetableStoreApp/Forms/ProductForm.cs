using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Windows.Forms;
using VegetableStoreApp.Database;
using VegetableStoreApp.Models;

namespace VegetableStoreApp.Forms
{
    public class ProductForm : Form
    {
        private DataGridView dgvProducts;
        private TextBox txtProductName, txtPrice, txtQuantity;
        private ComboBox cmbSupplier;
        private Button btnAdd, btnUpdate, btnDelete;
        private List<Product> products;
        private List<Supplier> suppliers;

        public ProductForm()
        {
            InitializeComponent();
            LoadSuppliers();
            LoadProducts();
        }

        private void InitializeComponent()
        {
            this.Text = "إدارة المنتجات";
            this.Size = new System.Drawing.Size(900, 550);
            this.StartPosition = FormStartPosition.CenterParent;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;

            // DataGridView
            dgvProducts = new DataGridView
            {
                Location = new System.Drawing.Point(10, 120),
                Size = new System.Drawing.Size(860, 350),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true
            };
            dgvProducts.CellClick += DgvProducts_CellClick;
            this.Controls.Add(dgvProducts);

            // Labels and TextBoxes
            Label lblProductName = new Label { Text = "اسم المنتج:", Location = new System.Drawing.Point(10, 10) };
            txtProductName = new TextBox { Location = new System.Drawing.Point(100, 10), Width = 200 };
            this.Controls.Add(lblProductName);
            this.Controls.Add(txtProductName);

            Label lblSupplier = new Label { Text = "المورد:", Location = new System.Drawing.Point(320, 10) };
            cmbSupplier = new ComboBox { Location = new System.Drawing.Point(400, 10), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            this.Controls.Add(lblSupplier);
            this.Controls.Add(cmbSupplier);

            Label lblPrice = new Label { Text = "السعر:", Location = new System.Drawing.Point(10, 50) };
            txtPrice = new TextBox { Location = new System.Drawing.Point(100, 50), Width = 200 };
            this.Controls.Add(lblPrice);
            this.Controls.Add(txtPrice);

            Label lblQuantity = new Label { Text = "الكمية:", Location = new System.Drawing.Point(320, 50) };
            txtQuantity = new TextBox { Location = new System.Drawing.Point(400, 50), Width = 200 };
            this.Controls.Add(lblQuantity);
            this.Controls.Add(txtQuantity);

            // Buttons
            btnAdd = new Button { Text = "إضافة", Location = new System.Drawing.Point(10, 90), Width = 80 };
            btnAdd.Click += BtnAdd_Click;
            this.Controls.Add(btnAdd);

            btnUpdate = new Button { Text = "تعديل", Location = new System.Drawing.Point(100, 90), Width = 80 };
            btnUpdate.Click += BtnUpdate_Click;
            this.Controls.Add(btnUpdate);

            btnDelete = new Button { Text = "حذف", Location = new System.Drawing.Point(190, 90), Width = 80 };
            btnDelete.Click += BtnDelete_Click;
            this.Controls.Add(btnDelete);
        }

        private void LoadSuppliers()
        {
            suppliers = new List<Supplier>();
            try
            {
                using (SQLiteConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT * FROM Suppliers";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                suppliers.Add(new Supplier
                                {
                                    SupplierID = Convert.ToInt32(reader["SupplierID"]),
                                    SupplierName = reader["SupplierName"].ToString()
                                });
                            }
                        }
                    }
                }
                cmbSupplier.DataSource = suppliers;
                cmbSupplier.DisplayMember = "SupplierName";
                cmbSupplier.ValueMember = "SupplierID";
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
                                    SupplierID = Convert.ToInt32(reader["SupplierID"]),
                                    Price = Convert.ToDecimal(reader["Price"]),
                                    Quantity = Convert.ToInt32(reader["Quantity"])
                                });
                            }
                        }
                    }
                }
                RefreshGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ: {ex.Message}");
            }
        }

        private void RefreshGrid()
        {
            dgvProducts.DataSource = products;
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtProductName.Text) || string.IsNullOrEmpty(txtPrice.Text))
            {
                MessageBox.Show("يرجى إدخال اسم المنتج والسعر");
                return;
            }

            try
            {
                using (SQLiteConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string query = "INSERT INTO Products (ProductName, SupplierID, Price, Quantity) VALUES (@name, @supplier, @price, @quantity)";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@name", txtProductName.Text);
                        cmd.Parameters.AddWithValue("@supplier", cmbSupplier.SelectedValue ?? 0);
                        cmd.Parameters.AddWithValue("@price", decimal.Parse(txtPrice.Text));
                        cmd.Parameters.AddWithValue("@quantity", int.Parse(txtQuantity.Text ?? "0"));
                        cmd.ExecuteNonQuery();
                    }
                }
                MessageBox.Show("تمت الإضافة بنجاح");
                ClearFields();
                LoadProducts();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ: {ex.Message}");
            }
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (dgvProducts.SelectedRows.Count == 0)
            {
                MessageBox.Show("يرجى اختيار منتج للتعديل");
                return;
            }

            try
            {
                int productID = Convert.ToInt32(dgvProducts.SelectedRows[0].Cells[0].Value);
                using (SQLiteConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string query = "UPDATE Products SET ProductName=@name, SupplierID=@supplier, Price=@price, Quantity=@quantity WHERE ProductID=@id";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@name", txtProductName.Text);
                        cmd.Parameters.AddWithValue("@supplier", cmbSupplier.SelectedValue ?? 0);
                        cmd.Parameters.AddWithValue("@price", decimal.Parse(txtPrice.Text));
                        cmd.Parameters.AddWithValue("@quantity", int.Parse(txtQuantity.Text ?? "0"));
                        cmd.Parameters.AddWithValue("@id", productID);
                        cmd.ExecuteNonQuery();
                    }
                }
                MessageBox.Show("تم التعديل بنجاح");
                ClearFields();
                LoadProducts();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ: {ex.Message}");
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvProducts.SelectedRows.Count == 0)
            {
                MessageBox.Show("يرجى اختيار منتج للحذف");
                return;
            }

            try
            {
                int productID = Convert.ToInt32(dgvProducts.SelectedRows[0].Cells[0].Value);
                using (SQLiteConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string query = "DELETE FROM Products WHERE ProductID=@id";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", productID);
                        cmd.ExecuteNonQuery();
                    }
                }
                MessageBox.Show("تم الحذف بنجاح");
                ClearFields();
                LoadProducts();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ: {ex.Message}");
            }
        }

        private void DgvProducts_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                txtProductName.Text = dgvProducts.Rows[e.RowIndex].Cells[1].Value?.ToString() ?? "";
                cmbSupplier.SelectedValue = dgvProducts.Rows[e.RowIndex].Cells[2].Value ?? 0;
                txtPrice.Text = dgvProducts.Rows[e.RowIndex].Cells[3].Value?.ToString() ?? "";
                txtQuantity.Text = dgvProducts.Rows[e.RowIndex].Cells[4].Value?.ToString() ?? "";
            }
        }

        private void ClearFields()
        {
            txtProductName.Clear();
            txtPrice.Clear();
            txtQuantity.Clear();
            cmbSupplier.SelectedIndex = 0;
        }
    }
}