using GosUslugiBind.Models;
using GosUslugiBind.Services;
using GosUslugiBinder.Models;
using GosUslugiBinder.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace GosUslugiBind
{
    public partial class MainForm : Form
    {
        private ProfileManager _profileManager;
        private HotkeyManager _hotkeyManager;
        private ListBox _profileList;
        private ListView _bindsList;
        private TextBox _searchBox;
        private TextBox _keyBox;
        private TextBox _actionBox;
        private ComboBox _conditionBox;
        private Button _addBtn;
        private Label _statsLabel;

        public MainForm()
        {
            InitializeComponent();
            _profileManager = new ProfileManager();
            _hotkeyManager = new HotkeyManager(this.Handle);
            _hotkeyManager.OnHotkeyPressed += ExecuteAction;

            LoadProfiles();
            LoadBinds();
            UpdateStats();

            _profileManager.OnProfilesChanged += LoadProfiles;
            _profileManager.OnCurrentProfileChanged += () =>
            {
                LoadBinds();
                UpdateStats();
                RegisterHotkeys();
            };

            RegisterHotkeys();
        }

        private void InitializeComponent()
        {
            this.Text = "⚡ Биндер Pro";
            this.Size = new Size(850, 620);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.MinimumSize = new Size(700, 500);

            var leftPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 200,
                BackColor = Color.FromArgb(245, 245, 245),
                Padding = new Padding(12)
            };

            var profileLabel = new Label
            {
                Text = "📁 Профили",
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                Location = new Point(12, 12),
                AutoSize = true
            };
            leftPanel.Controls.Add(profileLabel);

            _profileList = new ListBox
            {
                Location = new Point(12, 40),
                Width = 176,
                Height = 350,
                Font = new Font("Segoe UI", 11),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            _profileList.SelectedIndexChanged += ProfileList_SelectedIndexChanged;
            leftPanel.Controls.Add(_profileList);

            var addProfileBtn = new Button
            {
                Text = "➕",
                Location = new Point(12, 400),
                Width = 30,
                Height = 30,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12),
                BackColor = Color.FromArgb(34, 197, 94),
                ForeColor = Color.White
            };
            addProfileBtn.Click += AddProfileBtn_Click;
            leftPanel.Controls.Add(addProfileBtn);

            var deleteProfileBtn = new Button
            {
                Text = "🗑",
                Location = new Point(48, 400),
                Width = 30,
                Height = 30,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12),
                BackColor = Color.FromArgb(239, 68, 68),
                ForeColor = Color.White
            };
            deleteProfileBtn.Click += DeleteProfileBtn_Click;
            leftPanel.Controls.Add(deleteProfileBtn);

            _statsLabel = new Label
            {
                Location = new Point(12, 440),
                Width = 176,
                Height = 80,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(75, 85, 99),
                Text = "⏱ 0 мин\n📌 0 использовано"
            };
            leftPanel.Controls.Add(_statsLabel);

            this.Controls.Add(leftPanel);

            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(16)
            };

            var titleLabel = new Label
            {
                Text = "⚡ Биндер",
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                Location = new Point(0, 0),
                AutoSize = true,
                ForeColor = Color.FromArgb(31, 41, 55)
            };
            mainPanel.Controls.Add(titleLabel);

            var badge = new Label
            {
                Text = "v3.0",
                Location = new Point(110, 6),
                AutoSize = true,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(107, 114, 128),
                BackColor = Color.FromArgb(243, 244, 246),
                Padding = new Padding(8, 2, 8, 2)
            };
            mainPanel.Controls.Add(badge);

            _searchBox = new TextBox
            {
                Location = new Point(0, 44),
                Width = 540,
                Height = 30,
                Font = new Font("Segoe UI", 12),
                BackColor = Color.FromArgb(248, 249, 250),
                BorderStyle = BorderStyle.FixedSingle,
                Text = "🔍 Поиск биндов..."
            };
            _searchBox.Enter += (s, e) => { if (_searchBox.Text == "🔍 Поиск биндов...") _searchBox.Text = ""; };
            _searchBox.Leave += (s, e) => { if (string.IsNullOrWhiteSpace(_searchBox.Text)) _searchBox.Text = "🔍 Поиск биндов..."; };
            _searchBox.TextChanged += SearchBox_TextChanged;
            mainPanel.Controls.Add(_searchBox);

            _keyBox = new TextBox
            {
                Location = new Point(0, 88),
                Width = 140,
                Height = 30,
                Font = new Font("Segoe UI", 12),
                BackColor = Color.FromArgb(248, 249, 250),
                BorderStyle = BorderStyle.FixedSingle,
                PlaceholderText = "Ctrl+Shift+X"
            };
            mainPanel.Controls.Add(_keyBox);

            _actionBox = new TextBox
            {
                Location = new Point(150, 88),
                Width = 160,
                Height = 30,
                Font = new Font("Segoe UI", 12),
                BackColor = Color.FromArgb(248, 249, 250),
                BorderStyle = BorderStyle.FixedSingle,
                PlaceholderText = "Действие..."
            };
            mainPanel.Controls.Add(_actionBox);

            _conditionBox = new ComboBox
            {
                Location = new Point(320, 88),
                Width = 100,
                Height = 30,
                Font = new Font("Segoe UI", 12),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(248, 249, 250)
            };
            _conditionBox.Items.AddRange(new[] { "Всегда", "Roblox", "Браузер", "Discord" });
            _conditionBox.SelectedIndex = 0;
            mainPanel.Controls.Add(_conditionBox);

            _addBtn = new Button
            {
                Text = "➕ Добавить",
                Location = new Point(430, 86),
                Width = 110,
                Height = 34,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(99, 102, 241),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold)
            };
            _addBtn.Click += AddBtn_Click;
            mainPanel.Controls.Add(_addBtn);

            _bindsList = new ListView
            {
                Location = new Point(0, 132),
                Width = 580,
                Height = 320,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Font = new Font("Segoe UI", 11),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                HideSelection = false
            };
            _bindsList.Columns.Add("Сочетание", 150);
            _bindsList.Columns.Add("Действие", 200);
            _bindsList.Columns.Add("Условие", 100);
            _bindsList.Columns.Add("Исп", 60);
            _bindsList.DoubleClick += BindsList_DoubleClick;
            mainPanel.Controls.Add(_bindsList);

            var controlPanel = new Panel
            {
                Location = new Point(0, 460),
                Width = 580,
                Height = 40
            };

            var deleteBtn = new Button
            {
                Text = "🗑 Удалить",
                Location = new Point(0, 4),
                Width = 80,
                Height = 30,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(239, 68, 68),
                ForeColor = Color.White
            };
            deleteBtn.Click += DeleteBtn_Click;
            controlPanel.Controls.Add(deleteBtn);

            var clearBtn = new Button
            {
                Text = "🧹 Очистить",
                Location = new Point(86, 4),
                Width = 80,
                Height = 30,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(245, 158, 11),
                ForeColor = Color.White
            };
            clearBtn.Click += ClearBtn_Click;
            controlPanel.Controls.Add(clearBtn);

            var saveBtn = new Button
            {
                Text = "💾 Сохранить",
                Location = new Point(172, 4),
                Width = 80,
                Height = 30,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(34, 197, 94),
                ForeColor = Color.White
            };
            saveBtn.Click += SaveBtn_Click;
            controlPanel.Controls.Add(saveBtn);

            var exportBtn = new Button
            {
                Text = "📤",
                Location = new Point(258, 4),
                Width = 30,
                Height = 30,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(243, 244, 246),
                ForeColor = Color.FromArgb(75, 85, 99)
            };
            exportBtn.Click += ExportBtn_Click;
            controlPanel.Controls.Add(exportBtn);

            var importBtn = new Button
            {
                Text = "📥",
                Location = new Point(292, 4),
                Width = 30,
                Height = 30,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(243, 244, 246),
                ForeColor = Color.FromArgb(75, 85, 99)
            };
            importBtn.Click += ImportBtn_Click;
            controlPanel.Controls.Add(importBtn);

            var exitBtn = new Button
            {
                Text = "🚪 Выход",
                Location = new Point(500, 4),
                Width = 80,
                Height = 30,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(107, 114, 128),
                ForeColor = Color.White
            };
            exitBtn.Click += (s, e) => this.Close();
            controlPanel.Controls.Add(exitBtn);

            mainPanel.Controls.Add(controlPanel);
            this.Controls.Add(mainPanel);
        }

        private void LoadProfiles()
        {
            _profileList.Items.Clear();
            foreach (var name in _profileManager.GetProfileNames())
            {
                _profileList.Items.Add(name);
            }

            var index = _profileList.Items.IndexOf(_profileManager.CurrentProfile);
            if (index >= 0) _profileList.SelectedIndex = index;
        }

        private void LoadBinds()
        {
            _bindsList.Items.Clear();
            var binds = _profileManager.GetCurrentBinds();
            var searchText = _searchBox.Text.ToLower();
            if (searchText == "🔍 поиск биндов...") searchText = "";

            foreach (var bind in binds)
            {
                if (!string.IsNullOrEmpty(searchText) &&
                    !bind.Key.ToLower().Contains(searchText) &&
                    !bind.Action.ToLower().Contains(searchText))
                {
                    continue;
                }

                var item = new ListViewItem(bind.Key);
                item.SubItems.Add(bind.Action);
                item.SubItems.Add(bind.Condition);
                item.SubItems.Add(bind.Uses.ToString());
                item.Tag = bind;
                _bindsList.Items.Add(item);
            }
        }

        private void UpdateStats()
        {
            var binds = _profileManager.GetCurrentBinds();
            var totalUses = binds.Sum(b => b.Uses);
            var minutes = (int)(DateTime.Now - System.Diagnostics.Process.GetCurrentProcess().StartTime).TotalMinutes;
            _statsLabel.Text = $"⏱ {minutes} мин\n📌 {totalUses} использовано";
        }

        private void RegisterHotkeys()
        {
            _hotkeyManager.ReloadHotkeys(_profileManager.GetCurrentBinds());
        }

        private void ExecuteAction(string action)
        {
            try
            {
                SendKeys.SendWait(action);
                var binds = _profileManager.GetCurrentBinds();
                var bind = binds.FirstOrDefault(b => b.Action == action);
                if (bind != null)
                {
                    _profileManager.IncrementUse(bind.Key);
                    LoadBinds();
                    UpdateStats();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ProfileList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_profileList.SelectedItem != null)
            {
                _profileManager.SwitchProfile(_profileList.SelectedItem.ToString());
                LoadBinds();
                UpdateStats();
                RegisterHotkeys();
            }
        }

        private void AddProfileBtn_Click(object sender, EventArgs e)
        {
            var name = InputBox.Show("Введите название профиля:", "Создать профиль");
            if (!string.IsNullOrWhiteSpace(name))
            {
                _profileManager.AddProfile(name);
                LoadProfiles();
                _profileManager.SwitchProfile(name);
                LoadBinds();
                UpdateStats();
                RegisterHotkeys();
            }
        }

        private void DeleteProfileBtn_Click(object sender, EventArgs e)
        {
            if (_profileList.SelectedItem != null)
            {
                var name = _profileList.SelectedItem.ToString();
                if (name == "default")
                {
                    MessageBox.Show("Нельзя удалить основной профиль!", "Предупреждение",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var result = MessageBox.Show($"Удалить профиль \"{name}\"?", "Подтверждение",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    _profileManager.DeleteProfile(name);
                    LoadProfiles();
                    LoadBinds();
                    UpdateStats();
                    RegisterHotkeys();
                }
            }
        }

        private void SearchBox_TextChanged(object sender, EventArgs e)
        {
            LoadBinds();
        }

        private void AddBtn_Click(object sender, EventArgs e)
        {
            var key = _keyBox.Text.Trim();
            var action = _actionBox.Text.Trim();
            var condition = _conditionBox.SelectedItem?.ToString()?.ToLower() ?? "all";

            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(action))
            {
                MessageBox.Show("Заполните оба поля!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var binds = _profileManager.GetCurrentBinds();
            var existing = binds.FirstOrDefault(b => b.Key == key);
            if (existing != null)
            {
                var result = MessageBox.Show($"Бинд \"{key}\" уже существует. Перезаписать?",
                    "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result != DialogResult.Yes) return;

                existing.Action = action;
                existing.Condition = condition;
            }
            else
            {
                binds.Add(new BinderItem
                {
                    Key = key,
                    Action = action,
                    Condition = condition
                });
            }

            _profileManager.SetCurrentBinds(binds);
            _keyBox.Text = "";
            _actionBox.Text = "";
            LoadBinds();
            UpdateStats();
            RegisterHotkeys();
        }

        private void DeleteBtn_Click(object sender, EventArgs e)
        {
            if (_bindsList.SelectedItems.Count == 0)
            {
                MessageBox.Show("Выберите бинд для удаления.", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedItem = _bindsList.SelectedItems[0];
            var bind = selectedItem.Tag as BinderItem;
            if (bind == null) return;

            var result = MessageBox.Show($"Удалить бинд \"{bind.Key}\"?", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                var binds = _profileManager.GetCurrentBinds();
                binds.RemoveAll(b => b.Key == bind.Key);
                _profileManager.SetCurrentBinds(binds);
                LoadBinds();
                UpdateStats();
                RegisterHotkeys();
            }
        }

        private void ClearBtn_Click(object sender, EventArgs e)
        {
            var binds = _profileManager.GetCurrentBinds();
            if (binds.Count == 0) return;

            var result = MessageBox.Show("Удалить все бинды в этом профиле?", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                _profileManager.SetCurrentBinds(new List<BinderItem>());
                LoadBinds();
                UpdateStats();
                RegisterHotkeys();
            }
        }

        private void SaveBtn_Click(object sender, EventArgs e)
        {
            _profileManager.SaveProfiles();
            MessageBox.Show("Сохранено!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ExportBtn_Click(object sender, EventArgs e)
        {
            using var saveDialog = new SaveFileDialog
            {
                Filter = "JSON файлы (*.json)|*.json",
                FileName = $"binder_{_profileManager.CurrentProfile}.json"
            };
            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                _profileManager.ExportProfile(saveDialog.FileName);
                MessageBox.Show("Профиль экспортирован!", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ImportBtn_Click(object sender, EventArgs e)
        {
            using var openDialog = new OpenFileDialog
            {
                Filter = "JSON файлы (*.json)|*.json"
            };
            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    _profileManager.ImportProfile(openDialog.FileName);
                    LoadProfiles();
                    LoadBinds();
                    UpdateStats();
                    RegisterHotkeys();
                    MessageBox.Show("Профиль импортирован!", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка импорта: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BindsList_DoubleClick(object sender, EventArgs e)
        {
            if (_bindsList.SelectedItems.Count > 0)
            {
                var selectedItem = _bindsList.SelectedItems[0];
                var bind = selectedItem.Tag as BinderItem;
                if (bind != null)
                {
                    _keyBox.Text = bind.Key;
                    _actionBox.Text = bind.Action;
                    _conditionBox.SelectedItem = bind.Condition;
                }
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _hotkeyManager.Dispose();
            base.OnFormClosed(e);
        }

        private const int WM_HOTKEY = 0x0312;

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_HOTKEY)
            {
                var id = m.WParam.ToInt32();
                _hotkeyManager.ProcessHotkey(id);
                return;
            }
            base.WndProc(ref m);
        }

        public static class InputBox
        {
            public static string Show(string prompt, string title)
            {
                var form = new Form
                {
                    Text = title,
                    Size = new Size(350, 150),
                    StartPosition = FormStartPosition.CenterParent,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    MaximizeBox = false,
                    MinimizeBox = false
                };

                var label = new Label
                {
                    Text = prompt,
                    Location = new Point(12, 16),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 11)
                };
                form.Controls.Add(label);

                var textBox = new TextBox
                {
                    Location = new Point(12, 46),
                    Width = 310,
                    Font = new Font("Segoe UI", 11)
                };
                form.Controls.Add(textBox);

                var okBtn = new Button
                {
                    Text = "OK",
                    Location = new Point(170, 80),
                    Width = 70,
                    Height = 30,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(99, 102, 241),
                    ForeColor = Color.White
                };
                okBtn.Click += (s, ev) => { form.DialogResult = DialogResult.OK; form.Close(); };
                form.Controls.Add(okBtn);

                var cancelBtn = new Button
                {
                    Text = "Отмена",
                    Location = new Point(250, 80),
                    Width = 70,
                    Height = 30,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(107, 114, 128),
                    ForeColor = Color.White
                };
                cancelBtn.Click += (s, ev) => { form.DialogResult = DialogResult.Cancel; form.Close(); };
                form.Controls.Add(cancelBtn);

                form.AcceptButton = okBtn;
                form.CancelButton = cancelBtn;

                return form.ShowDialog() == DialogResult.OK ? textBox.Text : "";
            }
        }
    }
}