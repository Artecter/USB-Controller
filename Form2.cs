namespace USB_Controller
{
    public partial class Form2 : Form
    {
        public Form2() => InitializeComponent();

        private void Form2_Load(object sender, EventArgs e) 
        {
            F.read_file(F.logs_adress, listBox1);
            F.read_file(F.banlist_adress, listBox2);
            F.read_file(F.whitelist_adress, listBox3);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (Microsoft.VisualBasic.Interaction.InputBox("Введите да для подтверждения очистки журнала", "Очистка журнала событий") == "да")
            {
                listBox1.Items.Clear();
                F.clean_file(F.logs_adress);
            }
            else MessageBox.Show("Отмена действия");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (Microsoft.VisualBasic.Interaction.InputBox("Введите да для подтверждения очистики журнала заблокированных устройств", "Очистка журнала заблокированных устройств") == "да")
            {
                listBox2.Items.Clear();
                F.clean_file(F.banlist_adress);
            }
            else MessageBox.Show("Отмена действия");
        }

        private void button3_Click(object sender, EventArgs e) => F.move_listbox_data(listBox2, listBox3);

        private void button4_Click(object sender, EventArgs e)
        {
            F.update_file_and_listbox(F.banlist_adress, listBox2);
            F.update_file_and_listbox(F.whitelist_adress, listBox3);
            F.audit_note(DateTime.Now + " The whitelist updated");
            F.update(0);
        }

        private void button5_Click(object sender, EventArgs e) => F.move_listbox_data(listBox3, listBox2);

        private void button6_Click(object sender, EventArgs e)
        {
            string password = Microsoft.VisualBasic.Interaction.InputBox("Введите новый пароль для администратора", "Новый пароль для администратора");
            if (password != null)
            {
                F.clean_file(F.passwords_adress);
                F.make_note(password, F.passwords_adress);
            }
            else MessageBox.Show("Отмена действия");
        }
    }
}