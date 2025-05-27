// MainWindow.xaml.cs
using System;
using System.Collections.Generic;
using System.Linq; // Для LINQ (Where)
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input; // Для TextCompositionEventArgs
using ShoesFactory.Data;
using ShoesFactory.Models;
using System.Text.RegularExpressions; // Для валідації вводу
using ShoesFactory; // Додано для доступу до AddEditShoeWindow

namespace ShoesFactory
{
    public partial class MainWindow : Window
    {
        private DatabaseHelper _dbHelper;
        private List<Shoe> _allShoes; // Зберігаємо повний список взуття

        public MainWindow()
        {
            InitializeComponent();
            _dbHelper = new DatabaseHelper();
            this.Loaded += MainWindow_Loaded; // Підписуємося на подію завантаження вікна
        }

        /// <summary>
        /// Обробник події завантаження головного вікна.
        /// Завантажує дані з бази даних одразу при запуску вікна.
        /// </summary>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadShoesData();
        }

        /// <summary>
        /// Завантажує дані про взуття з бази даних та відображає їх у DataGrid.
        /// </summary>
        private void LoadShoesData()
        {
            try
            {
                _allShoes = _dbHelper.LoadShoes();
                ShoesDataGrid.ItemsSource = _allShoes;
            }
            catch (Exception ex)
            {
                // Відображаємо помилку, якщо завантаження даних не вдалося
                MessageBox.Show($"Помилка завантаження даних: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                // Можливо, закрити програму або відобразити порожню таблицю
                this.Close(); // Закриваємо програму, якщо не вдалося завантажити дані
            }
        }

        /// <summary>
        /// Обробник натискання кнопки "Додати".
        /// </summary>
        private void AddShoe_Click(object sender, RoutedEventArgs e)
        {
            // Створюємо нове вікно для додавання взуття
            AddEditShoeWindow addShoeWindow = new AddEditShoeWindow();
            if (addShoeWindow.ShowDialog() == true) // Відкриваємо вікно як діалог
            {
                Shoe newShoe = addShoeWindow.CurrentShoe; // Отримуємо об'єкт взуття з вікна
                try
                {
                    // Перевіряємо, чи вже існує взуття з таким артикулом, моделлю та розміром
                    bool exists = _allShoes.Any(s =>
                        s.Article == newShoe.Article &&
                        s.ModelName == newShoe.ModelName &&
                        s.Size == newShoe.Size);

                    if (exists)
                    {
                        MessageBox.Show("Взуття з таким артикулом, моделлю та розміром вже існує. Будь ласка, відредагуйте існуючий запис.", "Помилка додавання", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Додаємо взуття до бази даних
                    if (_dbHelper.AddShoe(newShoe))
                    {
                        LoadShoesData(); // Перезавантажуємо дані після успішного додавання
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка при додаванні взуття: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Обробник натискання кнопки "Редагувати".
        /// </summary>
        private void EditShoe_Click(object sender, RoutedEventArgs e)
        {
            if (ShoesDataGrid.SelectedItem is Shoe selectedShoe) // Перевіряємо, чи вибрано елемент
            {
                // Створюємо нове вікно для редагування, передаючи вибраний об'єкт
                AddEditShoeWindow editShoeWindow = new AddEditShoeWindow(selectedShoe);
                if (editShoeWindow.ShowDialog() == true) // Відкриваємо вікно як діалог
                {
                    Shoe updatedShoe = editShoeWindow.CurrentShoe; // Отримуємо оновлений об'єкт
                    try
                    {
                        _dbHelper.UpdateShoe(updatedShoe); // Оновлюємо в базі даних
                        LoadShoesData(); // Перезавантажуємо дані після успішного оновлення
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Помилка при редагуванні взуття: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Будь ласка, виберіть взуття для редагування.", "Вибір елемента", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Обробник натискання кнопки "Шукати" (пошук).
        /// </summary>
        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            ApplySearchFilter();
        }

        /// <summary>
        /// Обробник натискання кнопки "Очистити фільтр".
        /// </summary>
        private void ClearFilterButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = ""; // Очищаємо поле пошуку за моделлю/артикулом
            SizeSearchTextBox.Text = ""; // Очищаємо поле пошуку за розміром
            LoadShoesData(); // Перезавантажуємо всі дані
        }

        /// <summary>
        /// Застосовує фільтр пошуку за моделлю/артикулом та розміром.
        /// </summary>
        private void ApplySearchFilter()
        {
            string modelArticleFilterText = SearchTextBox.Text.ToLower();
            int? sizeFilter = null;

            // Спроба розпарсити розмір, якщо введено
            if (int.TryParse(SizeSearchTextBox.Text, out int parsedSize))
            {
                sizeFilter = parsedSize;
            }

            var filteredList = _allShoes.AsEnumerable(); // Починаємо з повного списку

            // Застосовуємо фільтр за моделлю/артикулом, якщо він не порожній
            if (!string.IsNullOrWhiteSpace(modelArticleFilterText))
            {
                filteredList = filteredList.Where(s =>
                    s.ModelName.ToLower().Contains(modelArticleFilterText) ||
                    s.Article.ToLower().Contains(modelArticleFilterText)
                );
            }

            // Застосовуємо фільтр за розміром, якщо він вказаний
            if (sizeFilter.HasValue)
            {
                filteredList = filteredList.Where(s => s.Size == sizeFilter.Value);
            }

            ShoesDataGrid.ItemsSource = filteredList.ToList();
        }

        /// <summary>
        /// Валідація вводу для числових полів (цілі числа).
        /// Цей метод буде використовуватися у AddEditShoeWindow та для SizeSearchTextBox.
        /// </summary>
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+"); // Дозволяє тільки цифри
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
