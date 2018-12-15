using ImageProcessor;
using OnnxConsumer;
using OnnxConsumer.Models;
using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace TheApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private void btnLoad_Click(object sender, EventArgs e)
        {
            var url = txtImageUrl.Text;
            var imageFilePath = Path.GetTempFileName();

            DownloadImage(url, imageFilePath);

            ResizeImage(imageFilePath);

            var consumer = new ModelConsumer();
            var response = consumer.PredictObjects(imageFilePath);            
            RenderResult(imageFilePath, response);
        }

        private static void DownloadImage(string url, string imageFilePath)
        {
            using (var wc = new WebClient())
            {
                wc.DownloadFile(url, imageFilePath);
            }
        }

        private static void ResizeImage(string imageFilePath)
        {
            using (var factory = new ImageFactory())
            {
                factory.Load(imageFilePath);

                var originalWidth = factory.Image.Width;
                var originalHeight = factory.Image.Height;

                var imageWidth = 416;
                var imageHeight = 416;
                using (var resizedImage = factory.Resize(new Size(imageWidth, imageHeight)))
                {
                    resizedImage.Save(imageFilePath);
                }
            }
        }

        private void RenderResult(string imageFilePath, YoloResponse parsedResponse)
        {
            var topObjectsFound = parsedResponse.GetTopObjectsDetected(20);

            Image image = Image.FromFile(imageFilePath);

            var builder = new StringBuilder();
            var random = new Random();

            using (Graphics g = Graphics.FromImage(image))
            {
                topObjectsFound
                   .ForEach(a =>
                   {
                       Color customColor = Color.FromArgb(random.Next(255), random.Next(255), random.Next(255));
                       var pen = new Pen(customColor, 2);
                       var x = a.X - a.Width / 2;
                       var y = a.Y - a.Height / 2;

                       builder.AppendLine($"{a.ObjectLabel} with confidence {a.Confidence:p}. Position : x = {a.X}, y = {a.Y}");
                       g.DrawRectangle(pen, x, y, a.Width, a.Height);

                       var brush = new SolidBrush(Color.Yellow);
                       var font = new Font(FontFamily.GenericSansSerif, 12, FontStyle.Bold);
                       g.DrawString(a.ObjectLabel, font, brush, x, y-20);

                       g.DrawString("X", font, new SolidBrush(customColor), a.X, a.Y);
                   });

                txtResults.Text = builder.ToString();
                pictureBox.Image = image;
            }
        }        
    }
}
