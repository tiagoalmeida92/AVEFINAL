using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Linq;

namespace CSharpEditor
{
    public partial class CSharpEditorForm : Form
    {
        private readonly ListBox listBoxAutoComplete;
        private readonly CSharpEditorModel model;
        private readonly ToolStripStatusLabel toolStripStatusLabel = new ToolStripStatusLabel();
        private Point caretPos;
        //private static int _counter;
        private bool intellisenseMode;

        public CSharpEditorForm()
        {
            InitializeComponent();
            // Instantiate listBoxAutoComplete object
            listBoxAutoComplete = new ListBox();
            // Add the ListBox to the form. 
            Controls.Add(listBoxAutoComplete);
            // Add status bar
            statusStrip.Items.AddRange(new ToolStripItem[] {toolStripStatusLabel});


            //Redimensionar a janela
            StartPosition = FormStartPosition.Manual;
            Location = new Point(0, Location.Y);
            Height = Screen.PrimaryScreen.WorkingArea.Height;
            BringToFront();


            model = new CSharpEditorModel();
            model.SetView(this);


            //SECCAO Assembly references
            addAssemblyRefButton.Click += addAssemblyRefButton_Click;
            removeAssemblyRefButton.Click += removeAssemblyRefButton_Click;
            ;


            //SECCAO BUILD
            //Compile e RUN
            compileButton.Click += compileButton_Click;
            runButton.Click += runButton_Click;


            //SECCAO FILE
            // Eventos para os botoes NEW LOAD SAVE e X(Close)
            newFileButton.Click += newFileButton_Click;
            loadFileButton.Click += loadFileButton_Click;
            saveFileButton.Click += saveFileButton_Click;
            FormClosed += CSharpEditorForm_FormClosed;
        }

        private void addAssemblyRefButton_Click(object sender, EventArgs e)
        {
            string assembly;
            if (model.AddReference(out assembly))
            {
                assemblyRefsComboBox.Items.Add(assembly);
                assemblyRefsComboBox.Text = assembly;
            }
        }

        private void removeAssemblyRefButton_Click(object sender, EventArgs e)
        {
            string assemblyToRemove = assemblyRefsComboBox.Text;
            if (model.RemoveReference(assemblyToRemove))
            {
                assemblyRefsComboBox.Items.Remove(assemblyToRemove);
                assemblyRefsComboBox.Text = String.Empty;
            }
        }

        private void compileButton_Click(object sender, EventArgs e)
        {
            model.Compile(editorPane.Text);
        }

        private void runButton_Click(object sender, EventArgs e)
        {
            model.Run(editorPane.Text);
        }

        private void newFileButton_Click(object sender, EventArgs e)
        {
            assemblyRefsComboBox.Text = String.Empty;
            assemblyRefsComboBox.Items.Clear();
            model.New();
            editorPane.Text = String.Empty;
        }

        private void loadFileButton_Click(object sender, EventArgs e)
        {
            try
            {
                //TODO
                assemblyRefsComboBox.Text = String.Empty;
                assemblyRefsComboBox.Items.Clear();
                editorPane.Text = model.Load();
            }
            catch (LoadFileUserException)
            {
                //User cancelou o load no loadialog
            }
        }

        private void saveFileButton_Click(object sender, EventArgs e)
        {
            try
            {
                model.Save(editorPane.Text);
            }
            catch (SaveFileUserException)
            {
                //user cancelou o save no savedialog
            }
        }

        [DllImport("Kernel32.dll")]
        private static extern Boolean AllocConsole();

        private void CSharpEditorForm_Load(object sender, EventArgs e)
        {
            if (!AllocConsole())
                MessageBox.Show("Failed to alloc console");
        }

        private void editorPane_KeyDown(object sender, KeyEventArgs e)
        {
            Clear();
            StatusLine();
            //Excecção IdxOutOfBounds caso não exista texto e for premido '.'
            if (editorPane.Text == String.Empty)
                return;
            // Detecting the dot key
            if (e.KeyData == Keys.OemPeriod)
            {
                // Get current line
                int currentLineIndex = editorPane.GetLineFromCharIndex(editorPane.SelectionStart);
                string currentLine = editorPane.Lines[currentLineIndex];
                //Console.WriteLine("Current line numb.: {0}, {1}", currentLineIndex+1 , currentLine);
                IEnumerable<string> possibleAutoCompletes = model.Intellisense(editorPane.Text, currentLineIndex, currentLine);

                if (!listBoxAutoComplete.Visible)
                {
                    listBoxAutoComplete.Items.Clear();
                    // Populate the Auto Complete list box
                    //this.listBoxAutoComplete.Items.Add("Olá " + ++_counter);
                    if (possibleAutoCompletes != null)
                    {
                        listBoxAutoComplete.Items.AddRange(possibleAutoCompletes.ToArray());
                        intellisenseMode = true;

                        // Display the Auto Complete list box
                        DisplayAutoCompleteList();
                    }
                }
            }
            else if (intellisenseMode)
            {
                if (e.KeyCode == Keys.Up)
                {
                    listBoxAutoComplete.SelectedIndex = (listBoxAutoComplete.SelectedIndex - 1) == -2
                                                            ? listBoxAutoComplete.Items.Count - 1
                                                            : listBoxAutoComplete.SelectedIndex - 1;
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.Down)
                {
                    listBoxAutoComplete.SelectedIndex = (listBoxAutoComplete.SelectedIndex + 1) ==
                                                        listBoxAutoComplete.Items.Count
                                                            ? -1
                                                            : listBoxAutoComplete.SelectedIndex + 1;
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.Enter)
                {
                    e.Handled = true;
                    //nenhum autocomplete seleccionado
                    if (listBoxAutoComplete.SelectedIndex <= listBoxAutoComplete.Items.Count &&
                        listBoxAutoComplete.SelectedIndex >= 0)
                    {
                        var autoComplete = (string) listBoxAutoComplete.Items[listBoxAutoComplete.SelectedIndex];
                        autoComplete = autoComplete.Substring(autoComplete.IndexOf(' ') + 1);
                        if (autoComplete.IndexOf('(') != -1)
                        {
                            autoComplete = autoComplete.Substring(0, autoComplete.IndexOf('(') + 1);
                        }
                        int pos = editorPane.SelectionStart;
                        editorPane.Text = editorPane.Text.Insert(editorPane.SelectionStart, autoComplete);
                        editorPane.SelectionStart = pos + autoComplete.Length;
                        intellisenseMode = false;
                    }
                }
                else
                {
                    intellisenseMode = false;
                }
                if (intellisenseMode)
                    DisplayAutoCompleteList();
            }
        }


        // Display the Auto Complete list box
        private void DisplayAutoCompleteList()
        {
            // Find the position of the caret
            Point caretLoc = editorPane.GetPositionFromCharIndex(editorPane.SelectionStart);
            caretLoc.Y += (int) Math.Ceiling(editorPane.Font.GetHeight())*2 + 13;
            caretLoc.X += 20;
            listBoxAutoComplete.Location = caretLoc;
            listBoxAutoComplete.Height = 100;
            listBoxAutoComplete.Width = 140;
            listBoxAutoComplete.BringToFront();
            listBoxAutoComplete.Show();
        }

        private void StatusLine()
        {
            int ln = editorPane.GetLineFromCharIndex(editorPane.SelectionStart);
            int cn = (editorPane.SelectionStart) - (editorPane.GetFirstCharIndexFromLine(ln)) + 1;
            ln = ln + 1;
            caretPos.X = cn;
            caretPos.Y = ln;
            string lnColString = "Ln: " + ln.ToString() + " Col: " + cn.ToString();
            statusStrip.Items[0].Text = lnColString;
        }

        private void Clear()
        {
            listBoxAutoComplete.Hide();
        }

        private void editorPane_MouseClick(object sender, MouseEventArgs e)
        {
            StatusLine();
        }

        private void editorPane_MouseUp(object sender, MouseEventArgs e)
        {
            intellisenseMode = false;
            Clear();
            StatusLine();
        }

        private void editorPane_TextChanged(object sender, EventArgs e)
        {
            model.NotifyChanges();
            StatusLine();
        }

        // Botao  X de fechar a form
        private void CSharpEditorForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            //Nao sair da aplicacao sem gravar se o utilzador desejar
            model.CheckSave();
        }

        public void AskForSave()
        {
            DialogResult dialogResult = MessageBox.Show("Existem alterações! Gravar?", "Gravar", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
                model.Save(editorPane.Text);
        }


        public void SetErrorList(string newText)
        {
            errorsList.Text = newText;
        }

        //Mudar barra de estado ao lado da posicao do cursor

        public void SetState(string state)
        {
            toolStripStatusLabel.Text = toolStripStatusLabel.Text + state;
        }
    }
}