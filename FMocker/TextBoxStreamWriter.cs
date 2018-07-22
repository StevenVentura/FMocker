using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//using System.Text;
using System.IO;
using System.Windows.Controls;
using System.Threading;
using System.Windows.Threading;
using System.Windows;

namespace FMocker
{

    class TextBoxStreamWriter : TextWriter
    {
        private TextBox txtBoxOutput = null;
        private Dispatcher Dispatcher;
        private StreamWriter StandardOut = null;
        ////////////////////////////////////////////////////////////////////
        // Description: we subclass TextWriter so we can make Console.Write be forwarded
        //              to both a WPF control and still write to the std console
        // Passed: the Dispatcher object from your wpf Window, & your wpf textbox control
        // Return: new TextBoxStreamWriter object
        /////////////////////////////////////////////////////////////////////
        public TextBoxStreamWriter(Dispatcher dispatcher, TextBox output)
        {
            txtBoxOutput = output;
            StandardOut = new StreamWriter(Console.OpenStandardOutput())
            {
                AutoFlush = true
            };
            this.Dispatcher = dispatcher;
            Console.SetOut(this);
        }
        ////////////////////////////////////////////////////////////////////
        // Description: overriding TextWriter parent method; writes character
        //              to both the TextBox and also the Console
        // Passed: -
        // Return: void
        /////////////////////////////////////////////////////////////////////
        public override void Write(char value)
        {
            base.Write(value);
            StandardOut.Write(value);
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.SystemIdle,
                (Action)(() =>
                {
                    txtBoxOutput.AppendText(value.ToString());
                    txtBoxOutput.ScrollToEnd();
                }));
        }
        ////////////////////////////////////////////////////////////////////
        // Description: cleanup; Console.WriteLine now only writes to std stream
        // Passed: none
        // Return: void
        /////////////////////////////////////////////////////////////////////
        public override void Close()
        {
            base.Close();
            Console.SetOut(StandardOut);
        }
        public override Encoding Encoding
        {
            get { return System.Text.Encoding.UTF8; }
        }
    }
}
