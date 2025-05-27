// AddEditShoeWindow.xaml.cs
using System;
using System.Text.RegularExpressions; 
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ShoesFactory.Models;

namespace ShoesFactory
{
    public partial class AddEditShoeWindow : Window
    {
        public Shoe CurrentShoe { get; private set; } // Властивість для передачі/отримання об'єкта взуття
        private bool _isEditMode; 

        // Конструктор для додавання нового взуття
        public AddEditShoeWindow()
        {
            InitializeComponent();
            _isEditMode = false;
            this.Title = "Додати нове взуття";
        }

        // Конструктор для редагування існуючого взуття
        public AddEditShoeWindow(Shoe shoeToEdit)
        {
            InitializeComponent();
            _isEditMode = true;
            this.Title = "Редагувати взуття";
            CurrentShoe = shoeToEdit;
            LoadShoeData(shoeToEdit); 
        }

       
        private void LoadShoeData(Shoe shoe)
        {
            ArticleTextBox.Text = shoe.Article;
            ModelNameTextBox.Text = shoe.ModelName;
            SizeTextBox.Text = shoe.Size.ToString();
            QuantityTextBox.Text = shoe.QuantityInStock.ToString();
            PriceTextBox.Text = shoe.Price.ToString();
            

            ArticleTextBox.IsEnabled = false;
            ModelNameTextBox.IsEnabled = false;
            SizeTextBox.IsEnabled = false;
        }

        
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Валідація введених даних
            if (string.IsNullOrWhiteSpace(ArticleTextBox.Text) ||
                string.IsNullOrWhiteSpace(ModelNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(SizeTextBox.Text) ||
                string.IsNullOrWhiteSpace(QuantityTextBox.Text) ||
                string.IsNullOrWhiteSpace(PriceTextBox.Text))
            {
                MessageBox.Show("Будь ласка, заповніть усі обов'язкові поля.", "Помилка валідації", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(SizeTextBox.Text, out int size) || size <= 0)
            {
                MessageBox.Show("Розмір повинен бути позитивним цілим числом.", "Помилка валідації", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!int.TryParse(QuantityTextBox.Text, out int quantity) || quantity < 0)
            {
                MessageBox.Show("Кількість на складі повинна бути невід'ємним цілим числом.", "Помилка валідації", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!decimal.TryParse(PriceTextBox.Text.Replace('.', ','), out decimal price) || price < 0) // Замінюємо крапку на кому для коректного парсингу десяткових чисел
            {
                MessageBox.Show("Ціна повинна бути невід'ємним числом.", "Помилка валідації", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Перевірка формату артикулу (Д/Ч/П)
            string article = ArticleTextBox.Text.ToUpper();
            if (article != "Д" && article != "Ч" && article != "П")
            {
                MessageBox.Show("Артикул повинен бути 'Д', 'Ч' або 'П'.", "Помилка валідації", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }


            // Створюємо або оновлюємо об'єкт Shoe
            if (!_isEditMode)
            {
                CurrentShoe = new Shoe();
            }

            CurrentShoe.Article = article;
            CurrentShoe.ModelName = ModelNameTextBox.Text;
            CurrentShoe.Size = size;
            CurrentShoe.QuantityInStock = quantity;
            CurrentShoe.Price = price;
            

            this.DialogResult = true; // Встановлюємо DialogResult для закриття вікна з успіхом
            this.Close();
        }

        
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false; // Встановлюємо DialogResult для закриття вікна без успіху
            this.Close();
        }

        
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+"); // Дозволяє тільки цифри
            e.Handled = regex.IsMatch(e.Text);
        }

        
        private void DecimalValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            // Дозволяє цифри та одну кому або крапку
            Regex regex = new Regex("[^0-9.,]+");
            if (regex.IsMatch(e.Text))
            {
                e.Handled = true;
                return;
            }

            TextBox textBox = sender as TextBox;
            if (textBox != null && (e.Text == "." || e.Text == ","))
            {
                // Дозволяємо лише одну кому/крапку
                if (textBox.Text.Contains(".") || textBox.Text.Contains(","))
                {
                    e.Handled = true;
                }
            }
        }
    }
}
