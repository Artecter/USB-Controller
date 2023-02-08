using System.Runtime.InteropServices;

namespace USB_Controller
{
    public static class F
    {
        public const string whitelist_adress = @"whitelist.txt";
        public const string banlist_adress = @"banlist.txt";
        public const string passwords_adress = @"passwords.txt";
        public const string logs_adress = @"logs.txt";

        private static string parseSerialFromDeviceID(string deviceId)
        {
            string[] splitDeviceId = deviceId.Split('\\');
            string[] serialArray;
            string serial;
            int arrayLen = splitDeviceId.Length - 1;
            serialArray = splitDeviceId[arrayLen].Split('&');
            serial = serialArray[0];
            return serial;
        }

        private static string parseVenFromDeviceID(string deviceId)
        {
            string[] splitDeviceId = deviceId.Split('\\');
            string Ven;
            string[] splitVen = splitDeviceId[1].Split('&');
            Ven = splitVen[1].Replace("VEN_", "");
            Ven = Ven.Replace("_", " ");
            return Ven;
        }

        private static string parseProdFromDeviceID(string deviceId)
        {
            string[] splitDeviceId = deviceId.Split('\\');
            string Prod;
            string[] splitProd = splitDeviceId[1].Split('&');
            Prod = splitProd[2].Replace("PROD_", ""); ;
            Prod = Prod.Replace("_", " ");
            return Prod;
        }

        private static string parseRevFromDeviceID(string deviceId)
        {
            string[] splitDeviceId = deviceId.Split('\\');
            string Rev;
            string[] splitRev = splitDeviceId[1].Split('&');
            Rev = splitRev[3].Replace("REV_", ""); ;
            Rev = Rev.Replace("_", " ");
            return Rev;
        }

        private static List<string> get_list(string adress)
        {
            List<string> list = new List<string>();
            using (TextReader reader = File.OpenText(@adress))
            {
                while (true)
                {
                    string line = reader.ReadLine();
                    if (line == null) break;
                    else list.Add(line);
                }
            }
            return list;
        }

        public static void make_note(string note, string adress)
        {
            using (FileStream fs = new FileStream(adress, FileMode.Append, FileAccess.Write))
            using (StreamWriter sw = new StreamWriter(fs)) sw.WriteLine(note);
        }

        public static void clean_file(string adress) => File.WriteAllText(@adress, string.Empty);

        public static void audit_note(string note) => make_note(note, logs_adress);

        public static void read_file(string adress, ListBox listbox)
        {
            listbox.Items.Clear();
            List<string> list = new List<string>(get_list(adress));
            foreach (string line in list) listbox.Items.Add(line);
        }

        private static void delete_note(string note, string adress) => File.WriteAllLines(adress, File.ReadAllLines(adress).Where(v => v.Trim().IndexOf(note) == -1).ToArray());

        private static bool check_correct_password(string password) => get_list(passwords_adress).Contains(password);

        private static bool check_serial_correct(string serial) => get_list(whitelist_adress).Contains(serial);

        private static void block_drive(string diskName)
        {
            [DllImport("kernel32.dll", SetLastError = true)]
            static extern IntPtr CreateFile(
                string lpFileName,
                uint dwDesiredAccess,
                uint dwShareMode,
                IntPtr lpSecurityAttributes,
                uint dwCreationDisposition,
                uint dwFlagsAndAttributes,
                IntPtr hTemplateFile);
            IntPtr ptr;
            [DllImport("kernel32.dll", SetLastError = true)]
            static extern bool DeviceIoControl(
                IntPtr hDevice,
                uint dwIoControlCode,
                IntPtr lpInBuffer,
                uint nInBufferSize,
                [Out] IntPtr lpOutBuffer,
                uint nOutBufferSize,
                ref uint lpBytesReturned,
                IntPtr lpOverlapped);
            const uint FILE_SHARE_READ = 0x00000001;
            const uint FILE_SHARE_WRITE = 0x00000002;
            const uint OPEN_EXISTING = 3;
            const uint GENERIC_READ = 0x80000000;
            const uint GENERIC_WRITE = 0x40000000;
            const uint FSCTL_LOCK_VOLUME = 0x00090018;
            ptr = CreateFile(@"\\.\" + diskName, GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);
            uint byteReturned = 0;
            DeviceIoControl(ptr, FSCTL_LOCK_VOLUME, IntPtr.Zero, 0, IntPtr.Zero, 0, ref byteReturned, IntPtr.Zero);
        }

        public static void get_drive_info_list(ListBox listBox)
        {
            string diskName = string.Empty;
            listBox.Items.Clear();
            int count = 0;
            foreach (System.Management.ManagementObject drive in new System.Management.ManagementObjectSearcher("select * from Win32_DiskDrive where InterfaceType='USB'").Get())
            {
                count++;
                listBox.Items.Add("Устройство №" + count + ":");
                foreach (System.Management.ManagementObject partition in new System.Management.ManagementObjectSearcher("ASSOCIATORS OF {Win32_DiskDrive.DeviceID='" + drive["DeviceID"] + "'} WHERE AssocClass = Win32_DiskDriveToDiskPartition").Get())
                {
                    foreach (System.Management.ManagementObject disk in
                    new System.Management.ManagementObjectSearcher(
                    "ASSOCIATORS OF {Win32_DiskPartition.DeviceID='"
                    + partition["DeviceID"]
                    + "'} WHERE AssocClass = Win32_LogicalDiskToPartition").Get())
                    {
                        diskName = disk["Name"].ToString().Trim();
                        listBox.Items.Add("Буква накопителя=" + diskName);
                    }
                }
                listBox.Items.Add("Модель=" + drive["Model"]);
                listBox.Items.Add("Производитель=" + parseVenFromDeviceID(drive["PNPDeviceID"].ToString().Trim()));
                listBox.Items.Add("Продукт=" + parseProdFromDeviceID(drive["PNPDeviceID"].ToString().Trim()));
                listBox.Items.Add("Редакция=" + parseRevFromDeviceID(drive["PNPDeviceID"].ToString().Trim()));
                string serial = parseSerialFromDeviceID(drive["PNPDeviceID"].ToString().Trim());
                listBox.Items.Add("Серийный номер=" + serial);
                decimal dSize = Math.Round((Convert.ToDecimal(new System.Management.ManagementObject("Win32_LogicalDisk.DeviceID='" + diskName + "'")["Size"]) / 1073741824), 2);
                listBox.Items.Add("Полный объем=" + dSize + " gb");
                decimal dFree = Math.Round((Convert.ToDecimal(new System.Management.ManagementObject("Win32_LogicalDisk.DeviceID='" + diskName + "'")["FreeSpace"]) / 1073741824), 2);
                listBox.Items.Add("Свободный объем=" + dFree + " gb");
                decimal dUsed = dSize - dFree;
                listBox.Items.Add("Используемый объем=" + dUsed + " gb");
                if (check_serial_correct(serial))
                {
                    listBox.Items.Add("Разрешен!");
                    audit_note(DateTime.Now + " The device (" + serial + ") admitted");
                    delete_note(serial, banlist_adress);
                }
                else
                {
                    block_drive(diskName);
                    listBox.Items.Add("Запрещен!");
                    audit_note(DateTime.Now + " The device (" + serial + ") blocked");
                    make_note(serial, banlist_adress);
                }
                listBox.Items.Add("");
            }
        }

        public static void move_listbox_data(ListBox from_listbox, ListBox to_listbox)
        {
            for (int i = 0; i < from_listbox.SelectedItems.Count; i++)
            {
                to_listbox.Items.Add(from_listbox.SelectedItems[i]);
                from_listbox.Items.Remove(from_listbox.SelectedItems[i]);
            }
        }

        public static void update_file_and_listbox(string adress, ListBox listBox)
        {
            clean_file(adress);
            foreach (var item in listBox.Items) make_note(item.ToString(), adress);
            listBox.Items.Clear();
            read_file(adress, listBox);
        }

        public static void update(int delay)
        {
            Thread.Sleep(delay);
            Application.Restart();
            Environment.Exit(0);
        }

        public static bool check_admin_right() => check_correct_password(Microsoft.VisualBasic.Interaction.InputBox("Введите пароль администратора:", "Проверка прав администратора"));

        public static void hide_form(Form form)
        {
            form.WindowState = FormWindowState.Minimized;
            form.FormBorderStyle = FormBorderStyle.None;
            form.WindowState = FormWindowState.Minimized;
            form.ShowIcon = false;
            form.ShowInTaskbar = false;
        }

        public static void unhide_form(Form form, NotifyIcon notifyIcon)
        {
            notifyIcon.Visible = false;
            form.ShowInTaskbar = true;
            form.WindowState = FormWindowState.Normal;
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.ShowIcon = true;
            form.ShowInTaskbar = true;
        }
    }
}
