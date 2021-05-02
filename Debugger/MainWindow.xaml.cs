using Microsoft.Win32;
using System.Collections.Generic;
using System.Windows;
using System.IO;
using PsCs.Disasm;
using PsCs.Hardware;

namespace Debugger
{
    public class Opcode
    {
        public bool Breakpoint { get; set; }
        public string Address { get; set; }
        public string NameOpcodes { get; set; }
        public string Commentary { get; set; }
    }
    public class Phone
    {
        public string Title { get; set; }
        public string Company { get; set; }
        public int Price { get; set; }
    }

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void load_button(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog();
            
            fileDialog.InitialDirectory = "C:\\Users\\mante\\go\\src\\github.com\\weqqr\\ps";
            fileDialog.Filter = "bin files (*.bin)|*.bin|All files (*.*)|*.*";
            fileDialog.FilterIndex = 1;
            fileDialog.RestoreDirectory = true;


            byte[] bios = {0};
            
            if (fileDialog.ShowDialog() == true)
            {
                int len = fileDialog.FileName.Length;
                bios = File.ReadAllBytes(fileDialog.FileName);
                nameFileString.Text = "Загружено: " + Path.GetFileName(fileDialog.FileName);
            }
            
            var bus = new Bus(bios);
            
            uint pc = 0x1FC00000;
            List<Opcode> opcodesList = new List<Opcode>();
                
            for (int i = 0; i < 10; i++)
            {
                var instruction = new Instruction(bus.LoadWord(pc));
                opcodesList.Add(new Opcode{Breakpoint = false, Address = pc.ToString("X8"), NameOpcodes = instruction.ToString(), Commentary = ""});
                pc += 4;
            }

            OpcodesGrid.ItemsSource = opcodesList;

        }
    }
   
    
   
}
