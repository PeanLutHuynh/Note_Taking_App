using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;
using NoteTakingApp.Data;


namespace NoteTakingApp
{
    public partial class NoteForm : Form
    {
        SinglyLinkedList noteList = new SinglyLinkedList();
        List<TextBox> noteTextBoxes = new List<TextBox>();
        List<Button> noteLabels = new List<Button>();

        int x = 20;
        private Panel kanbanBoard;
        private Button selectedLabel = null;

        public NoteForm()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.FormClosing += NoteForm_FormClosing;
            this.DoubleBuffered = true;

            InitializeKanbanBoard();


        }

        private void InitializeKanbanBoard()
        {
            kanbanBoard = new Panel
            {
                AutoScroll = true,
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(10, 150),
                Size = new Size(this.ClientSize.Width - 20, this.ClientSize.Height - 200),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            this.Controls.Add(kanbanBoard);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (kanbanBoard != null)
            {
                kanbanBoard.Size = new Size(this.ClientSize.Width - 20, this.ClientSize.Height - 200);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (var brush = new LinearGradientBrush(
                ClientRectangle,
                Color.DeepSkyBlue,  // Màu xanh lam ở đầu
                Color.White, // Màu trắng ở cuối
                LinearGradientMode.Vertical)) // Gradient theo chiều dọc
            {
                e.Graphics.FillRectangle(brush, ClientRectangle);
            }
        }

        private void NoteForm_Load(object sender, EventArgs e)
        {
            LoadDataFromJsonFile();
        }

        private void LoadDataFromJsonFile()
        {
            string filePath = "storage.json";
            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, "[]");
            }

            string json = File.ReadAllText(filePath);
            var notes = JsonConvert.DeserializeObject<List<NoteEntry>>(json);
            if (notes != null)
            {
                foreach (var note in notes)
                {
                    noteList.Add(note);
                    AddNoteToKanban(note);

                }
            }
        }

        private void AddNoteToKanban(NoteEntry note)
        {
            // Tính lại x dựa trên note cuối cùng còn lại
            if (noteLabels.Count > 0)
            {
                // Lấy note cuối cùng và tính x dựa trên tọa độ X của nó
                var lastLabel = noteLabels.Last();
                x = lastLabel.Location.X + 240; // 220 (kích thước) + 20 (khoảng cách)
            }
            else
            {
                // Nếu không có note nào, đặt x về vị trí mặc định ban đầu
                x = 20;
            }

            // Tạo TextBox cho ghi chú
            var noteBox = new TextBox
            {
                Text = note.Message,
                Name = "noteTextBox",
                BackColor = SystemColors.GradientInactiveCaption,
                Multiline = true,
                Location = new Point(x, 40),
                Size = new Size(220, 100),
                ReadOnly = true
            };

            // Tạo Label cho ghi chú
            var label = new Button
            {
                Text = note.Title,
                Location = new Point(x, 10),
                Size = new Size(220, 30),
                Tag = note
            };

            int buttonWidth = 64;
            int buttonSpacing = 10;

            // Tạo các nút chức năng
            var readButton = new Button
            {
                Text = "Read",
                Location = new Point(x, 150),
                Size = new Size(buttonWidth, 25),
                Tag = note
            };

            var addFileButton = new Button
            {
                Text = "Add File",
                Location = new Point(x + buttonWidth + buttonSpacing, 150),
                Size = new Size(buttonWidth, 25),
                Tag = note
            };

            var openFileButton = new Button
            {
                Text = "Open File",
                Location = new Point(x + 2 * (buttonWidth + buttonSpacing), 150),
                Size = new Size(buttonWidth + 10, 25),
                Tag = note
            };

            // Gán sự kiện cho các nút
            readButton.Click += (sender, e) => EditNoteContent(note, noteBox);
            addFileButton.Click += (sender, e) => AddFileToNote(note);
            openFileButton.Click += (sender, e) => OpenFilesOfNote(note);

            label.Click += (sender, e) =>
            {
                var clickedLabel = (Button)sender;
                if (clickedLabel.BackColor == Color.LightCoral)
                {
                    clickedLabel.BackColor = SystemColors.Control;
                    SwapBtn.Enabled = false;
                }
                else
                {
                    clickedLabel.BackColor = Color.LightCoral;
                    var selectedLabels = kanbanBoard.Controls.OfType<Button>().Where(b => b.BackColor == Color.LightCoral).ToList();
                    SwapBtn.Enabled = selectedLabels.Count == 2;
                }
            };

            // Thêm các điều khiển vào kanbanBoard
            noteTextBoxes.Add(noteBox);
            noteLabels.Add(label);
            kanbanBoard.Controls.Add(noteBox);
            kanbanBoard.Controls.Add(label);
            kanbanBoard.Controls.Add(readButton);
            kanbanBoard.Controls.Add(addFileButton);
            kanbanBoard.Controls.Add(openFileButton);

            // Cập nhật vị trí các nút
            UpdateButtonPositions(label);

            // Hiển thị tệp đính kèm nếu có
            ShowAttachments(note);
        }

        private void AddFileToNote(NoteEntry note)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Multiselect = true;
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    foreach (var filePath in openFileDialog.FileNames)
                    {
                        // Kiểm tra nếu file là hình ảnh
                        if (filePath.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                            filePath.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                            filePath.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                            filePath.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase) ||
                            filePath.EndsWith(".gif", StringComparison.OrdinalIgnoreCase))
                        {
                            // Kiểm tra xem file đã có trong danh sách ImagePaths chưa
                            if (!note.ImagePaths.Contains(filePath))
                            {
                                note.ImagePaths.Add(filePath);  // Thêm vào ImagePaths
                            }
                        }
                        // Kiểm tra nếu file là âm thanh
                        else if (filePath.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase) ||
                                 filePath.EndsWith(".wav", StringComparison.OrdinalIgnoreCase) ||
                                 filePath.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase))
                        {
                            // Kiểm tra xem file đã có trong danh sách MusicPaths chưa
                            if (!note.MusicPaths.Contains(filePath))
                            {
                                note.MusicPaths.Add(filePath);  // Thêm vào MusicPaths
                                //var audioPlayerForm = new AudioPlayerForm(filePath);
                                //audioPlayerForm.Show();
                            }
                        }
                    }
                    // Cập nhật lại nội dung của note trên Kanban
                    string imageFiles = string.Join(", ", note.ImagePaths.Select(Path.GetFileName));
                    string musicFiles = string.Join(", ", note.MusicPaths.Select(Path.GetFileName));
                    // Xóa các dòng cũ trong note.Message trước khi thêm lại các tệp mới
                    note.Message = note.Message.Split(new[] { "Images:", "Music:" }, StringSplitOptions.None).FirstOrDefault();
                    // Thêm thông tin tệp hình ảnh và âm thanh
                    if (!string.IsNullOrEmpty(imageFiles))
                    {
                        note.Message += Environment.NewLine + "Images: " + imageFiles;
                    }
                    if (!string.IsNullOrEmpty(musicFiles))
                    {
                        note.Message += Environment.NewLine + "Music: " + musicFiles;
                    }
                    var noteBox = noteTextBoxes[noteLabels.IndexOf(kanbanBoard.Controls.OfType<Button>().First(b => b.Tag == note))];
                    noteBox.Text = note.Message;

                    ShowAttachments(note);  // Cập nhật lại giao diện với các tệp đính kèm
                }
            }
        }

        private void OpenFilesOfNote(NoteEntry note)
        {
            if (note.ImagePaths != null && note.ImagePaths.Count > 0)
            {
                foreach (var imagePath in note.ImagePaths)
                {
                    OpenImageFile(imagePath);  // Mở hình ảnh
                }
            }
            if (note.MusicPaths != null && note.MusicPaths.Count > 0)
            {
                foreach (var musicPath in note.MusicPaths)
                {
                    // Mở cửa sổ AudioPlayerForm khi có file nhạc
                    var audioPlayerForm = new AudioPlayerForm(musicPath);
                    audioPlayerForm.Show(); // Hiển thị cửa sổ phát nhạc
                }
            }
        }

        private void OpenImageFile(string filePath)
        {
            try
            {
                var image = Image.FromFile(filePath);
                // Tạo PictureBox và thiết lập thuộc tính để giữ ảnh gốc
                var pictureBox = new PictureBox
                {
                    SizeMode = PictureBoxSizeMode.Zoom,  // Giữ tỷ lệ gốc của ảnh
                    Image = image
                };
                // Tính toán tỷ lệ thu nhỏ để vừa với kích thước cửa sổ tối đa 800x600
                double scaleFactor = Math.Min(1.0, Math.Min(800.0 / image.Width, 600.0 / image.Height)); // Giới hạn kích thước cửa sổ là 800x600
                int width = (int)(image.Width * scaleFactor);
                int height = (int)(image.Height * scaleFactor);
                // Đặt kích thước ban đầu cho PictureBox
                pictureBox.Size = new Size(width, height);
                // Tạo form để hiển thị ảnh
                var form = new Form
                {
                    Text = "Image Viewer",
                    StartPosition = FormStartPosition.CenterScreen,
                    ClientSize = new Size(width + 20, height + 40),  // Thêm một chút padding cho đẹp
                    MinimumSize = new Size(400, 300) // Đặt kích thước tối thiểu của form
                };
                // Tạo panel để chứa PictureBox và cho phép cuộn nếu ảnh quá lớn
                var panel = new Panel
                {
                    AutoScroll = true,  // Kích hoạt cuộn nếu ảnh quá lớn
                    Dock = DockStyle.Fill
                };
                // Đảm bảo panel có cùng kích thước với form
                panel.Size = form.ClientSize;
                // Đặt vị trí ban đầu của pictureBox ở giữa panel
                pictureBox.Location = new Point((panel.Width - pictureBox.Width) / 2,
                                                  (panel.Height - pictureBox.Height) / 2);
                // Đưa pictureBox vào trong panel
                panel.Controls.Add(pictureBox);
                // Đặt panel vào form
                form.Controls.Add(panel);
                // Xử lý sự kiện khi thay đổi kích thước cửa sổ form để giữ ảnh ở giữa
                form.Resize += (sender, e) =>
                {
                    // Tính toán lại vị trí của pictureBox để căn giữa trong form
                    pictureBox.Location = new Point((panel.Width - pictureBox.Width) / 2,
                                                      (panel.Height - pictureBox.Height) / 2);
                };
                // Hiển thị form
                form.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error displaying image: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowAttachments(NoteEntry note)
        {
            // Tạo một nhãn để hiển thị các tệp đính kèm (Hình ảnh và Nhạc)
            var label = kanbanBoard.Controls.OfType<Label>().FirstOrDefault(lbl => lbl.Tag == note);
            if (label != null)
            {
                string attachmentInfo = "";
                // Hiển thị các đường dẫn hình ảnh
                if (note.ImagePaths.Any())
                {
                    attachmentInfo += "Images: " + string.Join(", ", note.ImagePaths.Select(Path.GetFileName)) + Environment.NewLine;
                }
                // Hiển thị các đường dẫn nhạc
                if (note.MusicPaths.Any())
                {
                    attachmentInfo += "Audio: " + string.Join(", ", note.MusicPaths.Select(Path.GetFileName)) + Environment.NewLine;
                }
                // Cập nhật nhãn với thông tin tệp đính kèm
                label.Text = attachmentInfo;
            }
        }

        private void ReorganizeKanban()
        {
            int startX = 20; // Vị trí X ban đầu
            int spacing = 240; // Khoảng cách giữa các note

            // Duyệt qua các label và cập nhật lại vị trí
            for (int i = 0; i < noteLabels.Count; i++)
            {
                // Đặt lại vị trí cho Label và TextBox
                noteLabels[i].Location = new Point(startX, 10);
                noteTextBoxes[i].Location = new Point(startX, 40);

                // Cập nhật lại vị trí cho các nút liên quan
                UpdateButtonPositions(noteLabels[i]);

                // Di chuyển sang vị trí tiếp theo
                startX += spacing; // Cập nhật x cho note tiếp theo
            }
        }

        private void EditNoteContent(NoteEntry note, TextBox noteBox)
        {
            // Điền thông tin ghi chú vào các ô nhập liệu
            TitleEntryBox.Text = note.Title;
            MessageEntryBox.Text = note.Message;
            // Lưu trữ ghi chú đang được chỉnh sửa
            TitleEntryBox.Tag = note;
            MessageEntryBox.Tag = note;
        }

        private void NewBtn_Click(object sender, EventArgs e)
        {
            TitleEntryBox.Clear();
            MessageEntryBox.Clear();
        }

        private void SaveBtn_Click(object sender, EventArgs e)
        {
            string title = TitleEntryBox.Text;
            string message = MessageEntryBox.Text;
            if (string.IsNullOrWhiteSpace(title) || noteList.Contains(title))
            {
                MessageBox.Show("Please enter a valid title.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            var note = new NoteEntry(title, message);
            noteList.Add(note);
            AddNoteToKanban(note);
            TitleEntryBox.Clear();
            MessageEntryBox.Clear();
        }

        private void DeleteBtn_Click(object sender, EventArgs e)
        {
            var selectedLabels = kanbanBoard.Controls.OfType<Button>().Where(b => b.BackColor == Color.LightCoral).ToList();
            if (selectedLabels.Count == 0)
            {
                MessageBox.Show("Please select a label to delete or swap.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            // Lưu vị trí cuộn ban đầu
            int currentScrollPositionX = kanbanBoard.HorizontalScroll.Value;

            foreach (var label in selectedLabels)
            {
                var note = (NoteEntry)label.Tag;
                var noteBox = noteTextBoxes[noteLabels.IndexOf(label)];

                // Tìm và xóa các nút liên quan
                var relatedButtons = kanbanBoard.Controls.OfType<Button>().Where(b => b.Tag == note).ToList();

                if (MessageBox.Show($"Are you sure you want to delete '{note.Title}'?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    // Nếu note bị che khuất, cuộn thanh cuộn sang phải
                    if (label.Right > kanbanBoard.Width)
                    {
                        kanbanBoard.HorizontalScroll.Value = label.Right - kanbanBoard.Width;
                    }

                    // Xóa note khỏi danh sách
                    noteList.Remove(note.Title);

                    // Xóa các điều khiển khỏi kanbanBoard
                    kanbanBoard.Controls.Remove(label);
                    kanbanBoard.Controls.Remove(noteBox);
                    foreach (var btn in relatedButtons)
                        kanbanBoard.Controls.Remove(btn);

                    // Cập nhật danh sách
                    noteTextBoxes.Remove(noteBox);
                    noteLabels.Remove(label);
                }
            }
            // Cuộn thanh cuộn về vị trí ban đầu
            kanbanBoard.HorizontalScroll.Value = 0;
            // Tái tổ chức lại sau khi xóa
            ReorganizeKanban();  // Gọi lại phương thức để cập nhật vị trí các note
            // Đảm bảo rằng không còn khoảng trống
            kanbanBoard.PerformLayout();
            SwapBtn.Enabled = false;
        }

        private void UpdateButtonPositions(Button specificLabel = null)
        {
            int buttonWidth = 64;
            int buttonSpacing = 10;
            int yOffset = 150; // Vị trí Y cố định cho các nút

            // Danh sách label cần cập nhật
            var labelsToUpdate = specificLabel != null ? new List<Button> { specificLabel } : noteLabels;

            foreach (var label in labelsToUpdate)
            {
                int xPosition = label.Location.X; // Lấy tọa độ X của label
                var note = (NoteEntry)label.Tag;  // Lấy note từ Tag

                // Tìm các nút liên quan đến note này
                var readButton = kanbanBoard.Controls.OfType<Button>().FirstOrDefault(b => b.Text == "Read" && b.Tag == note);
                var addFileButton = kanbanBoard.Controls.OfType<Button>().FirstOrDefault(b => b.Text == "Add File" && b.Tag == note);
                var openFileButton = kanbanBoard.Controls.OfType<Button>().FirstOrDefault(b => b.Text == "Open File" && b.Tag == note);

                // Cập nhật vị trí của các nút liên quan
                if (readButton != null)
                    readButton.Location = new Point(xPosition, yOffset);

                if (addFileButton != null)
                    addFileButton.Location = new Point(xPosition + buttonWidth + buttonSpacing, yOffset);

                if (openFileButton != null)
                    openFileButton.Location = new Point(xPosition + 2 * (buttonWidth + buttonSpacing), yOffset);
            }
        }

        private void NoteForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveDataToJsonFile();
        }

        private void SaveDataToJsonFile()
        {
            string json = JsonConvert.SerializeObject(noteList.GetAllNotes(), Formatting.Indented);
            File.WriteAllText("storage.json", json);
        }

        private void MessageEntryBox_TextChanged(object sender, EventArgs e)
        {
        }

        private void Update_Click(object sender, EventArgs e)
        {
            string title = TitleEntryBox.Text;
            string message = MessageEntryBox.Text;

            // Kiểm tra xem title có trống không
            if (string.IsNullOrWhiteSpace(title))
            {
                MessageBox.Show("Please enter a title.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Lấy ghi chú đang được chỉnh sửa từ Tag của TitleEntryBox
            var note = (NoteEntry)TitleEntryBox.Tag;
            if (note != null)
            {
                // Xóa các tên tệp trong phần message nếu có
                string imageFiles = string.Join(", ", note.ImagePaths.Select(Path.GetFileName));
                string musicFiles = string.Join(", ", note.MusicPaths.Select(Path.GetFileName));

                // Loại bỏ tên tệp trong message nếu có
                note.Message = note.Message
                    .Replace("Images: " + imageFiles, "")
                    .Replace("Music: " + musicFiles, "");

                // Cập nhật lại title và message mới
                note.Title = title;
                note.Message = message;

                // Cập nhật lại nội dung TextBox trên Kanban
                TextBox noteBox = noteTextBoxes[noteLabels.IndexOf(noteLabels.Find(x => x.Tag == note))];
                noteBox.Text = message;

                // Cập nhật lại Label (Button) trên Kanban với title mới
                Button label = noteLabels.Find(x => x.Tag == note);
                label.Text = title;

                // Cập nhật lại Tag của label và noteBox với title mới
                label.Tag = note;
                noteBox.Tag = note;

                // Cập nhật lại danh sách tệp đính kèm (xóa tệp không còn trong message)
                note.ImagePaths.RemoveAll(imagePath => !note.Message.Contains(Path.GetFileName(imagePath)));
                note.MusicPaths.RemoveAll(musicPath => !note.Message.Contains(Path.GetFileName(musicPath)));

                MessageBox.Show("Note updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Note not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void SwapBtn_Click(object sender, EventArgs e)
        {
            // Lấy các ghi chú đã chọn
            var selectedNotes = kanbanBoard.Controls.OfType<Button>().Where(b => b.BackColor == Color.LightCoral).ToList();
            if (selectedNotes.Count == 2)
            {
                // Lấy hai ghi chú đã chọn
                var note1 = (NoteEntry)selectedNotes[0].Tag;
                var note2 = (NoteEntry)selectedNotes[1].Tag;
                // Hoán đổi dữ liệu của hai ghi chú trực tiếp
                (note1.Title, note2.Title) = (note2.Title, note1.Title);
                (note1.Message, note2.Message) = (note2.Message, note1.Message);
                // Cập nhật lại hiển thị trên Kanban (Label)
                selectedNotes[0].Text = note1.Title;
                selectedNotes[1].Text = note2.Title;
                // Cập nhật lại nội dung ghi chú trong TextBox tương ứng
                var textBox1 = noteTextBoxes[noteLabels.IndexOf(selectedNotes[0])];
                var textBox2 = noteTextBoxes[noteLabels.IndexOf(selectedNotes[1])];
                textBox1.Text = note1.Message;
                textBox2.Text = note2.Message;
                // Cập nhật lại Tag của các Button và TextBox
                selectedNotes[0].Tag = note1;
                selectedNotes[1].Tag = note2;
                // Xóa màu nền của các ghi chú đã chọn
                selectedNotes[0].BackColor = SystemColors.Control;
                selectedNotes[1].BackColor = SystemColors.Control;
                // Vô hiệu hóa nút Swap sau khi hoán đổi xong
                SwapBtn.Enabled = false;
                // Cập nhật lại giao diện Kanban (vị trí của các ghi chú trên board)
                ReorganizeKanban();
                // Cập nhật lại vị trí các nút sau khi hoán đổi
                UpdateButtonPositions();
                // Đặt lại selectedLabel để người dùng có thể chọn ghi chú khác
                selectedLabel = null;
                // Hủy bỏ bất kỳ sự chọn lựa văn bản nào trong các TextBox để tránh bôi đen
                textBox1.SelectionLength = 0;
                textBox2.SelectionLength = 0;
                // Xóa focus khỏi TextBox để tránh bị bôi đen
                kanbanBoard.Focus();
            }
        }
    }
}
