// DatabaseHelper.cs
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration; 
using ShoesFactory.Models; 

namespace ShoesFactory.Data
{
    public class DatabaseHelper
    {
        private string _connectionString;
        private const int MAX_SHOES_LIMIT = 100; // Максимальна кількість записів взуття

        public DatabaseHelper()
        {
            // Отримуємо рядок підключення з файлу конфігурації App.config
            _connectionString = ConfigurationManager.ConnectionStrings["ShoesDbConnection"]?.ConnectionString;

            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new ConfigurationErrorsException("Не знайдено рядок підключення 'ShoesDbConnection' у файлі конфігурації.");
            }
        }

      
        private MySqlConnection GetConnection()
        {
            MySqlConnection connection = new MySqlConnection(_connectionString);
            try
            {
                connection.Open();
                return connection;
            }
            catch (MySqlException ex)
            {
                System.Windows.MessageBox.Show($"Помилка підключення до бази даних MySQL: {ex.Message}", "Помилка підключення", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return null;
            }
        }

        
        public List<Shoe> LoadShoes()
        {
            List<Shoe> shoes = new List<Shoe>();
            using (MySqlConnection connection = GetConnection())
            {
                if (connection != null)
                {
                    string sql = "SELECT article, model, size, quantity, price FROM asortument;";
                    using (MySqlCommand cmd = new MySqlCommand(sql, connection))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            shoes.Add(new Shoe
                            {
                                Article = reader.GetString("article"),
                                ModelName = reader.GetString("model"),
                                Size = reader.GetInt32("size"),
                                QuantityInStock = reader.GetInt32("quantity"),
                                Price = reader.GetDecimal("price")
                                
                            });
                        }
                    }
                }
            }
            return shoes;
        }

        
        public bool AddShoe(Shoe shoe)
        {
            using (MySqlConnection connection = GetConnection())
            {
                if (connection != null)
                {
                    // Перевіряємо кількість записів перед додаванням
                    int currentCount = GetShoesCount(connection);
                    if (currentCount >= MAX_SHOES_LIMIT)
                    {
                        System.Windows.MessageBoxResult result = System.Windows.MessageBox.Show(
                            $"Досягнуто максимальної кількості взуття ({MAX_SHOES_LIMIT}). Видалити найстаріший запис, щоб додати новий?",
                            "Обмеження кількості", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question);

                        if (result == System.Windows.MessageBoxResult.Yes)
                        {
                            
                        }
                        else
                        {
                            return false; 
                        }
                    }

                    string sql = "INSERT INTO asortument (article, model, size, quantity, price) VALUES (@Article, @ModelName, @Size, @QuantityInStock, @Price);";
                    using (MySqlCommand cmd = new MySqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@Article", shoe.Article);
                        cmd.Parameters.AddWithValue("@ModelName", shoe.ModelName);
                        cmd.Parameters.AddWithValue("@Size", shoe.Size);
                        cmd.Parameters.AddWithValue("@QuantityInStock", shoe.QuantityInStock);
                        cmd.Parameters.AddWithValue("@Price", shoe.Price);
                        cmd.ExecuteNonQuery();
                        System.Windows.MessageBox.Show("Взуття успішно додано!", "Успіх", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                        return true;
                    }
                }
                return false;
            }
        }

        
        public void UpdateShoe(Shoe shoe)
        {
            using (MySqlConnection connection = GetConnection())
            {
                if (connection != null)
                {
                    
                    string sql = "UPDATE asortument SET quantity = @QuantityInStock, price = @Price WHERE article = @Article AND model = @ModelName AND size = @Size;";
                    using (MySqlCommand cmd = new MySqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@QuantityInStock", shoe.QuantityInStock);
                        cmd.Parameters.AddWithValue("@Price", shoe.Price);
                        cmd.Parameters.AddWithValue("@Article", shoe.Article);
                        cmd.Parameters.AddWithValue("@ModelName", shoe.ModelName);
                        cmd.Parameters.AddWithValue("@Size", shoe.Size);
                        cmd.ExecuteNonQuery();
                        System.Windows.MessageBox.Show("Інформацію про взуття оновлено успішно!", "Успіх", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                    }
                }
            }
        }

        /// <summary>
        /// Видаляє запис взуття з бази даних.
        /// </summary>
        /// <param name="article">Артикул взуття для видалення.</param>
        /// <param name="modelName">Назва моделі взуття для видалення.</param>
        /// <param name="size">Розмір взуття для видалення.</param>
        public void DeleteShoe(string article, string modelName, int size)
        {
            using (MySqlConnection connection = GetConnection())
            {
                if (connection != null)
                {
                    string sql = "DELETE FROM asortument WHERE article = @Article AND model = @ModelName AND size = @Size;";
                    using (MySqlCommand cmd = new MySqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@Article", article);
                        cmd.Parameters.AddWithValue("@ModelName", modelName);
                        cmd.Parameters.AddWithValue("@Size", size);
                        cmd.ExecuteNonQuery();
                        System.Windows.MessageBox.Show("Взуття успішно видалено!", "Успіх", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                    }
                }
            }
        }

        /// <summary>
        /// Отримує поточну кількість записів у таблиці взуття.
        /// </summary>
        /// <param name="connection">Існуюче підключення до БД.</param>
        /// <returns>Кількість записів.</returns>
        private int GetShoesCount(MySqlConnection connection)
        {
            string sql = "SELECT COUNT(*) FROM asortument;";
            using (MySqlCommand cmd = new MySqlCommand(sql, connection))
            {
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }
    }
}
