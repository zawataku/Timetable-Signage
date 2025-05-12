using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TrainDepartureDisplay
{
    public partial class MainForm : Form
    {
        private Timer timer;
        private List<Departure> timetable;
        private const string CsvFileName = "timetable.csv";

        public MainForm()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.Black;
            this.ForeColor = Color.White;
            this.Font = new Font("Meiryo", 32, FontStyle.Bold);
            this.DoubleBuffered = true;

            LoadTimetable();
            SetupTimer();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // CSVファイルの存在を確認
            if (!File.Exists(CsvFileName))
            {
                MessageBox.Show($"時刻表ファイル「{CsvFileName}」が見つかりません。\nアプリを終了します。",
                                "エラー",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                Application.Exit();
                return;
            }

            // CSVの読み込み処理に続く
            LoadTimetable();
        }

        private void LoadTimetable()
        {
            timetable = new List<Departure>();
            var lines = File.ReadAllLines(CsvFileName, Encoding.GetEncoding("shift_jis"));
            foreach (var line in lines.Skip(1)) // Skip header
            {
                var parts = line.Split(',');
                if (parts.Length < 4) continue;

                if (TimeSpan.TryParse(parts[0], out var time))
                {
                    timetable.Add(new Departure
                    {
                        Time = time,
                        Destination = parts[1],
                        Platform = parts[2],
                        Type = parts[3]
                    });
                }
            }
        }

        private void SetupTimer()
        {
            timer = new Timer();
            timer.Interval = 1000; // 1秒ごとに更新
            timer.Tick += (s, e) => this.Invalidate();
            timer.Start();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var now = DateTime.Now.TimeOfDay;
            var nextTrains = timetable.Where(t => t.Time >= now).Take(3).ToList();

            var g = e.Graphics;
            int y = 100;
            int rowHeight = 120;

            if (nextTrains.Count == 0)
            {
                g.DrawString("本日の運行は終了しました", this.Font, Brushes.White, 100, y);
                return;
            }

            foreach (var train in nextTrains)
            {
                string text = $"{train.Time:hh\\:mm}　{train.Destination}行き　{train.Platform}番線　{train.Type}";
                g.DrawString(text, this.Font, Brushes.White, 100, y);
                y += rowHeight;
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // ESCで終了
            if (keyData == Keys.Escape)
            {
                Application.Exit();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }

    public class Departure
    {
        public TimeSpan Time { get; set; }
        public string Destination { get; set; }
        public string Platform { get; set; }
        public string Type { get; set; }
    }
}
