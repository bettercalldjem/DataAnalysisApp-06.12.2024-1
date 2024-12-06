using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Maui.Controls;
using Microcharts;
using SkiaSharp;

namespace DataAnalysisApp
{
    public partial class MainPage : ContentPage
    {
        private string _filePath = string.Empty;

        public List<UserAction> UserData { get; set; } = new List<UserAction>();

        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;
        }

        public class UserAction
        {
            public string UserName { get; set; }
            public DateTime ActionDate { get; set; }
            public string Action { get; set; }
            public string Category { get; set; }
        }

        private async void OnLoadCsvFileClicked(object sender, EventArgs e)
        {
            var file = await FilePicker.PickAsync();
            if (file != null)
            {
                _filePath = file.FullPath;
                LoadDataFromCsv(_filePath);
                AnalyzeData();
            }
        }

        private void LoadDataFromCsv(string filePath)
        {
            var lines = File.ReadAllLines(filePath);
            var data = new List<UserAction>();

            foreach (var line in lines.Skip(1))
            {
                var columns = line.Split(',');

                if (columns.Length == 4)
                {
                    data.Add(new UserAction
                    {
                        UserName = columns[0],
                        ActionDate = DateTime.Parse(columns[1]),
                        Action = columns[2],
                        Category = columns[3]
                    });
                }
            }

            UserData = data;
            DataTable.ItemsSource = UserData;
        }

        private void AnalyzeData()
        {
            var activeUsers = UserData
                .GroupBy(u => u.UserName)
                .Select(g => new { User = g.Key, Count = g.Count() })
                .OrderByDescending(u => u.Count)
                .Take(5)
                .ToList();

            var actionCategories = UserData
                .GroupBy(u => u.Category)
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .ToList();

            var userActivityEntries = activeUsers.Select(u => new ChartEntry(u.Count)
            {
                Label = u.User,
                ValueLabel = u.Count.ToString(),
                Color = SKColor.Parse("#FF5733")
            }).ToList();

            UserActivityChart.Chart = new BarChart { Entries = userActivityEntries };

            var categoryEntries = actionCategories.Select(c => new ChartEntry(c.Count)
            {
                Label = c.Category,
                ValueLabel = c.Count.ToString(),
                Color = SKColor.Parse("#33FF57")
            }).ToList();

            CategoryDistributionChart.Chart = new PieChart { Entries = categoryEntries };
        }
    }
}
