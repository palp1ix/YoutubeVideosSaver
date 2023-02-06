using System;
using System.Windows.Forms;
using VideoLibrary;
using System.IO;
using System.Linq;
using System.Net.Http;

namespace YoutubeVideosSaver
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            try
            {
                string link = textBox1.Text;
                int temp = 0;
                int k = 0;
                int selectedResolution = Convert.ToInt32(comboBox1.Text);
                var youtube = YouTube.Default;
                var videoInfos = youtube.GetAllVideosAsync(link).GetAwaiter().GetResult();
                var videoResolution = videoInfos.First(i => i.Resolution == selectedResolution);
                var video = youtube.GetVideo(link);
                var client = new HttpClient();
                string dir;
                long? totalByte = 0;

                //Меню выбора директроии
                DialogResult result = folderBrowserDialog1.ShowDialog();
                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(folderBrowserDialog1.SelectedPath))
                    dir = folderBrowserDialog1.SelectedPath;
                else
                {
                    MessageBox.Show("Вы не выбрали директорию!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }


                using (Stream output = File.OpenWrite(dir + "\\" + video.FullName))
                {
                    using (var request = new HttpRequestMessage(HttpMethod.Head, videoResolution.Uri))
                        totalByte = client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).Result.Content.Headers.ContentLength;

                    using (var input = await client.GetStreamAsync(videoResolution.Uri))
                    {
                        byte[] buffer = new byte[16 * 1024];
                        int read;
                        progressBar1.Minimum = 0;
                        progressBar1.Maximum = (int)totalByte;
                        while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            output.Write(buffer, 0, read);
                            progressBar1.Value += read;
                            k++;
                        }
                        temp = progressBar1.Value;
                        label6.Text = ((temp / k) / 1048576.347892466).ToString("f2");
                        MessageBox.Show("Загрузка завершена!", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        progressBar1.Value = 0;
                    }

                }
            }
            catch(Exception ex)
            {
                MessageBox.Show($"{ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

            private void textBox1_Leave(object sender, EventArgs e)
            {
            if (textBox1.Text == "")
                    return;
                var youTube = YouTube.Default;  // starting point for YouTube actions
                try
                {
                    var video = youTube.GetVideo(textBox1.Text); // gets a Video object with info about the video
                    label4.Text = video.Title;
                }
                catch
                {
                    label4.Text = "Ошибка";
                }

            }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = 3;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(comboBox1.SelectedIndex == 0)
                MessageBox.Show("При скачке видео с качеством 1080p в нём будет отсутствовать звук!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
