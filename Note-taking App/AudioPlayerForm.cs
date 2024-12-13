using NAudio.Wave;
using System.Windows.Forms;
using System;
using System.Drawing;
using System.IO;

public partial class AudioPlayerForm : Form
{
    private WaveOut waveOutDevice;
    private AudioFileReader audioFileReader;
    private bool isPlaying = false;

    private Timer progressTimer;
    private Button btnPlayPause;
    private Button btnSkipBackward;
    private Button btnSkipForward;
    private TrackBar volumeTrackBar;
    private Button btnStop;
    private ProgressBar progressBar1;
    private Label lblFileName;
    private Label lblCurrentTime;
    private Label lblTotalTime;

    public AudioPlayerForm(string filePath)
    {
        InitializeComponent();
        InitializeAudioControls(filePath);
        this.MaximizeBox = false;
    }

    private void InitializeAudioControls(string filePath)
    {
        // Khởi tạo WaveOut và AudioFileReader
        waveOutDevice = new WaveOut();
        audioFileReader = new AudioFileReader(filePath);

        // Cập nhật progressBar tối đa bằng chiều dài của file audio
        progressBar1.Maximum = (int)audioFileReader.Length;

        // Cập nhật thời lượng tổng của bài hát
        lblTotalTime.Text = TimeSpan.FromSeconds(audioFileReader.TotalTime.TotalSeconds).ToString(@"hh\:mm\:ss");

        // Cập nhật label với tên file (không có đuôi tệp)
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
        lblFileName.Text = fileNameWithoutExtension;

        // Khởi tạo Timer để cập nhật progress bar
        progressTimer = new Timer();
        progressTimer.Interval = 100; // Cập nhật mỗi 100ms
        progressTimer.Tick += (sender, e) => UpdateProgressBar();

        // Các sự kiện cho các điều khiển
        btnPlayPause.Click += (sender, e) => TogglePlayPause();
        btnStop.Click += (sender, e) => StopPlayback();
        btnSkipBackward.Click += (sender, e) => SkipBackward();
        btnSkipForward.Click += (sender, e) => SkipForward();
        volumeTrackBar.Scroll += (sender, e) => AdjustVolume(volumeTrackBar.Value);
        progressBar1.MouseDown += (sender, e) => OnProgressBarMouseDown(e); // Kéo thả thanh progress bar
    }


    private void InitializeComponent()
    {
        // Khởi tạo các điều khiển trong phương thức này
        this.btnPlayPause = new Button { Text = "Play", Location = new Point(211, 74), Size = new Size(75, 23) };
        this.btnSkipBackward = new Button { Text = "<<", Location = new Point(75, 74), Size = new Size(75, 23) };
        this.btnSkipForward = new Button { Text = ">>", Location = new Point(343, 74), Size = new Size(75, 23) };
        this.volumeTrackBar = new TrackBar { Location = new Point(127, 166), Size = new Size(104, 45), Minimum = 0, Maximum = 100, Value = 50 };
        this.btnStop = new Button { Text = "Replay", Location = new Point(277, 166), Size = new Size(75, 23) };
        this.progressBar1 = new ProgressBar { Location = new Point(75, 118), Size = new Size(343, 23) };
        this.lblFileName = new Label { Location = new Point(75, 40), Size = new Size(343, 23), Text = "File Name" };
        this.lblCurrentTime = new Label { Location = new Point(75, 150), Size = new Size(50, 23), Text = "00:00:00" };
        this.lblTotalTime = new Label { Location = new Point(370, 150), Size = new Size(50, 23), Text = "00:00:00" };

        // Thêm vào form
        this.Controls.Add(lblCurrentTime);
        this.Controls.Add(lblTotalTime);

        // Thêm vào form
        this.Controls.Add(btnPlayPause);
        this.Controls.Add(btnSkipBackward);
        this.Controls.Add(btnSkipForward);
        this.Controls.Add(volumeTrackBar);
        this.Controls.Add(btnStop);
        this.Controls.Add(progressBar1);
        this.Controls.Add(lblFileName);

        this.ClientSize = new Size(493, 261);
        this.Name = "AudioPlayerForm";
    }

    // Play/Pause toggle
    private void TogglePlayPause()
    {
        if (isPlaying)
        {
            waveOutDevice.Pause();
            btnPlayPause.Text = "Play";
            progressTimer.Stop(); // Dừng Timer khi tạm dừng phát nhạc
        }
        else
        {
            waveOutDevice.Init(audioFileReader);
            waveOutDevice.Play();
            btnPlayPause.Text = "Pause";
            progressTimer.Start(); // Bắt đầu Timer khi phát nhạc
        }
        isPlaying = !isPlaying;
    }

    // Dừng phát nhạc
    private void StopPlayback()
    {
        waveOutDevice.Stop();
        btnPlayPause.Text = "Play";
        progressTimer.Stop(); // Dừng Timer khi dừng phát nhạc
        audioFileReader.Position = 0; // Đặt lại vị trí về đầu file
        progressBar1.Value = 0;       // Reset progress bar
        lblCurrentTime.Text = "00:00:00"; // Reset thời gian phát hiện tại
        isPlaying = false;
    }

    // Tua lại 10 giây
    private void SkipBackward()
    {
        long skipBytes = 10 * audioFileReader.WaveFormat.AverageBytesPerSecond;
        audioFileReader.Position = Math.Max(0, audioFileReader.Position - skipBytes);
    }

    // Tua tới 10 giây
    private void SkipForward()
    {
        long skipBytes = 10 * audioFileReader.WaveFormat.AverageBytesPerSecond;
        audioFileReader.Position = Math.Min(audioFileReader.Length, audioFileReader.Position + skipBytes);
    }

    // Điều chỉnh âm lượng
    private void AdjustVolume(int volume)
    {
        float volumeLevel = volume / 100f;
        waveOutDevice.Volume = volumeLevel;
    }

    // Cập nhật progress bar
    private void UpdateProgressBar()
    {
        // Tính tỷ lệ phần trăm giữa vị trí hiện tại và tổng chiều dài của file
        int progressValue = (int)((double)audioFileReader.Position / audioFileReader.Length * progressBar1.Maximum);

        // Đảm bảo giá trị nằm trong giới hạn cho phép của ProgressBar
        progressBar1.Value = Math.Min(progressBar1.Maximum, Math.Max(progressBar1.Minimum, progressValue));

        // Cập nhật thời gian phát hiện hiện tại
        lblCurrentTime.Text = TimeSpan.FromSeconds(audioFileReader.CurrentTime.TotalSeconds).ToString(@"hh\:mm\:ss");

        // Cập nhật thời gian tổng bài hát
        lblTotalTime.Text = TimeSpan.FromSeconds(audioFileReader.TotalTime.TotalSeconds).ToString(@"hh\:mm\:ss");
    }


    // Xử lý khi người dùng kéo thả thanh progress bar
    private void OnProgressBarMouseDown(MouseEventArgs e)
    {
        // Tính toán vị trí mới của audioFileReader khi người dùng kéo thả thanh progress bar
        long newPosition = (long)(e.X / (float)progressBar1.Width * audioFileReader.Length);
        audioFileReader.Position = newPosition;

        // Cập nhật thời gian phát hiện hiện tại
        lblCurrentTime.Text = TimeSpan.FromSeconds(audioFileReader.CurrentTime.TotalSeconds).ToString(@"hh\:mm\:ss");
    }


    // Giải phóng tài nguyên
    private void DisposeAudio()
    {
        // Dừng và giải phóng tài nguyên WaveOut
        if (waveOutDevice != null)
        {
            waveOutDevice.Stop();
            waveOutDevice.Dispose();
            waveOutDevice = null;
        }

        // Giải phóng tài nguyên AudioFileReader
        if (audioFileReader != null)
        {
            audioFileReader.Dispose();
            audioFileReader = null;
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (isPlaying)
        {
            StopPlayback();
        }

        DisposeAudio(); // Giải phóng tài nguyên khi form đóng
        base.OnFormClosing(e);
    }
}
