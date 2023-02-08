namespace USB_Controller
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            button3.Enabled = true;
            notifyIcon1.Visible = false;
            notifyIcon1.MouseDoubleClick += new MouseEventHandler(notifyIcon1_MouseDoubleClick);
            Resize += new EventHandler(Form1_Resize);
        }

        private void edit()
        {
            if (F.check_admin_right())
            {
                Form form2 = new Form2();
                form2.Show();
            }
            else MessageBox.Show("Неверный пароль!");
        }

        private void button1_Click(object sender, EventArgs e) => F.update(0);

        private void button2_Click(object sender, EventArgs e) => F.hide_form(this);

        private void button3_Click(object sender, EventArgs e) => edit();

        private void Form1_Load(object sender, EventArgs e) => F.get_drive_info_list(listBox1);

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true; F.hide_form(this);
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                ShowInTaskbar = false;
                notifyIcon1.Visible = true;
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e) => F.unhide_form(this, notifyIcon1);

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.WParam.ToInt32() == 0x8000)
            {
                F.update(3000);
                MessageBox.Show("Подсоединено новое устройство!");
                F.audit_note(DateTime.Now + " The device connected");
            }
            if (m.WParam.ToInt32() == 0x8004)
            {
                F.update(3000);
                MessageBox.Show("Устройство было удалено!");
                F.audit_note(DateTime.Now + " The device disconnected");
            }
        }
    }
}