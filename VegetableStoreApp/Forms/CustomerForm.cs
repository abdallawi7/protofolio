using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Windows.Forms;
using VegetableStoreApp.Database;
using VegetableStoreApp.Models;

namespace VegetableStoreApp.Forms
{
    public class CustomerForm : Form
    {
        private DataGridView dgvCustomers;
        private TextBox txtName, txtPhone, txtAddress;
        private Button btnAdd, btnUpdate, btnDelete;
        private List<Customer> customers;

        public CustomerForm()
        {
            InitializeComponent();
            LoadCustomers();
        }

        private void InitializeComponent()
        {
            this.Text = "إدارة الزبائن";
            this.Size = new System.Drawing.Size(800, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;

            // DataGridView
            dgvCustomers = new DataGridView
            {
                Location = new System.Drawing.Point(10, 120),
                Size = new System.Drawing.Size(760, 300),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true
            };
            dgvCustomers.CellClick += DgvCustomers_CellClick;
            this.Controls.Add(dgvCustomers);

            // Labels and TextBoxes
            Label lblName = new Label { Text = "اسم الزبون:", Location = new System.Drawing.Point(10, 10) };
            txtName = new TextBox { Location = new System.Drawing.Point(100, 10), Width = 200 };
            this.Controls.Add(lblName);
            this.Controls.Add(txtName);

            Label lblPhone = new Label { Text = "الهاتف:", Location = new System.Drawing.Point(320, 10) };
            txtPhone = new TextBox { Location = new System.Drawing.Point(400, 10), Width = 200 };
            this.Controls.Add(lblPhone);
            this.Controls.Add(txtPhone);

            Label lblAddress = new Label { Text = "العنوان:", Location = new System.Drawing.Point(10, 50) };
            txtAddress = new TextBox { Location = new System.Drawing.Point(100, 50), Width = 500 };
            this.Controls.Add(lblAddress);
            this.Controls.Add(txtAddress);

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
                                    CustomerName = reader["CustomerName"].ToString(),
                                    Phone = reader["Phone"].ToString(),
                                    Address = reader["Address"].ToString(),
                                    CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
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
            dgvCustomers.DataSource = customers;
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtName.Text))
            {
                MessageBox.Show("يرجى إدخال اسم الزبون");
                return;
            }

            try
            {
                using (SQLiteConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string query = "INSERT INTO Customers (CustomerName, Phone, Address) VALUES (@name, @phone, @address)";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@name", txtName.Text);
                        cmd.Parameters.AddWithValue("@phone", txtPhone.Text ?? "");
                        cmd.Parameters.AddWithValue("@address", txtAddress.Text ?? "");
                        cmd.ExecuteNonQuery();
                    }
                }
                MessageBox.Show("تمت الإضافة بنجاح");
                ClearFields();
                LoadCustomers();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ: {ex.Message}");
            }
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (dgvCustomers.SelectedRows.Count == 0)
            {
                MessageBox.Show("يرجى اختيار زبون للتعديل");
                return;
            }

            try
            {
                int customerID = Convert.ToInt32(dgvCustomers.SelectedRows[0].Cells[0].Value);
                using (SQLiteConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string query = "UPDATE Customers SET CustomerName=@name, Phone=@phone, Address=@address WHERE CustomerID=@id";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@name", txtName.Text);
                        cmd.Parameters.AddWithValue("@phone", txtPhone.Text ?? "");
                        cmd.Parameters.AddWithValue("@address", txtAddress.Text ?? "");
                        cmd.Parameters.AddWithValue("@id", customerID);
                        cmd.ExecuteNonQuery();
                    }
                }
                MessageBox.Show("تم التعديل بنجاح");
                ClearFields();
                LoadCustomers();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ: {ex.Message}");
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvCustomers.SelectedRows.Count == 0)
            {
                MessageBox.Show("يرجى اختيار زبون للحذف");
                return;
            }

            try
            {
                int customerID = Convert.ToInt32(dgvCustomers.SelectedRows[0].Cells[0].Value);
                using (SQLiteConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string query = "DELETE FROM Customers WHERE CustomerID=@id";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", customerID);
                        cmd.ExecuteNonQuery();
                    }
                }
                MessageBox.Show("تم الحذف بنجاح");
                ClearFields();
                LoadCustomers();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ: {ex.Message}");
            }
        }

        private void DgvCustomers_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                txtName.Text = dgvCustomers.Rows[e.RowIndex].Cells[1].Value?.ToString() ?? "";
                txtPhone.Text = dgvCustomers.Rows[e.RowIndex].Cells[2].Value?.ToString() ?? "";
                txtAddress.Text = dgvCustomers.Rows[e.RowIndex].Cells[3].Value?.ToString() ?? "";
            }
        }

        private void ClearFields()
        {
            txtName.Clear();
            txtPhone.Clear();
            txtAddress.Clear();
        }
    }
}