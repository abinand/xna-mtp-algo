using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DGui;

namespace DGuiTest
{
    public class GuiTest : DrawableGameComponent
    {
        TestGame _game;

        DGuiManager _guiManager;
        DForm _form;
        DComboBox _combo1;
        DComboBox _combo2;
        DToggleButton _toggle;
        DButton _button;
        DCheckbox _checkBox;
        DTextBox _textBox;
        DTextBox _textBox2;
        DText _label;
        DListBox _listBox;
        DButton _listBoxAdd;
        DButton _listBoxClear;

        DGrid _grid;
        DGrid _grid2;
        DGrid _grid3;


        DText _gridText;
        DToggleButton _gridToggleButton;

        DMultiLineTextBox _multiLineText;


        public GuiTest(TestGame game)
            : base(game)
        {
            _game = game;
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            _guiManager = new DGuiManager(_game, _game.SpriteBatch);

            _form = new DForm(_guiManager, "GuiTest", null);
            _form.Size = new Vector2(520, 735);
            _form.Position = new Vector2(10, 10);
            _form.Initialize();
            _guiManager.AddControl(_form);


            // Setup sample controls in a layout
            DLayoutFlow layout = new DLayoutFlow(1, 12, DLayoutFlow.DLayoutFlowStyle.Vertically);
            layout.Position = new Vector2(10, 10);

            _combo1 = new DComboBox(_guiManager);
            layout.Add(_combo1);
            _combo1.Initialize();
            _combo1.AddItem("Test1", null);
            _combo1.AddItem("Test2", null);
            _form.AddPanel(_combo1);

            _combo2 = new DComboBox(_guiManager);
            layout.Add(_combo2);
            _combo2.Initialize();
            _combo2.AddItem("Test1", null);
            _combo2.AddItem("Test2", null);
            _form.AddPanel(_combo2);

            _button = new DButton(_guiManager);
            layout.Add(_button);
            _button.Text = "Button";
            _button.Initialize();
            _form.AddPanel(_button);
            _button.OnClick += new DButtonEventHandler(_button_OnClick);

            _toggle = new DToggleButton(_guiManager);
            layout.Add(_toggle);
            _toggle.Text = "Toggle";
            _toggle.Initialize();
            _form.AddPanel(_toggle);
            _toggle.OnToggle += new ToggleButtonEventHandler(_toggle_OnToggle);

            _label = new DText(_guiManager);
            _label.FontName = "Miramonte";
            layout.Add(_label);
            _label.Text = "Test Label";
            _label.HorizontalAlignment = DText.DHorizontalAlignment.Left;
            _label.Initialize();
            _form.AddPanel(_label);


            _checkBox = new DCheckbox(_guiManager);
            layout.Add(_checkBox);
            _checkBox.Text = "Checkbox";
            _checkBox.Initialize();
            _form.AddPanel(_checkBox);

            _textBox = new DTextBox(_guiManager);
            layout.Add(_textBox);
            _textBox.Initialize();
            _textBox.Text = "Text 1";
            _form.AddPanel(_textBox);

            _textBox2 = new DTextBox(_guiManager);
            layout.Add(_textBox2);
            _textBox2.Initialize();
            _textBox2.Text = "Text 2";
            _form.AddPanel(_textBox2);

            _listBox = new DListBox(_guiManager);
            layout.Add(_listBox);
            _listBox.Initialize();
            _listBox.AddListItem(new DListBoxItem(_guiManager, "Test 1"));
            _listBox.AddListItem(new DListBoxItem(_guiManager, "Test 2"));
            _form.AddPanel(_listBox);

            _listBoxAdd = new DButton(_guiManager);
            layout.Add(_listBoxAdd);
            _listBoxAdd.Text = "Add";
            _listBoxAdd.Initialize();
            _form.AddPanel(_listBoxAdd);
            _listBoxAdd.OnClick += new DButtonEventHandler(_listBoxAdd_OnClick);

            _listBoxClear = new DButton(_guiManager);
            layout.Add(_listBoxClear);
            _listBoxClear.Text = "Remove";
            _listBoxClear.Initialize();
            _form.AddPanel(_listBoxClear);
            _listBoxClear.OnClick += new DButtonEventHandler(_listBoxClear_OnClick);




            // Set up some grids
            _grid = new DGrid(_guiManager, 2, 2);
            _grid.Position = new Vector2(300, 10);
            _form.AddPanel(_grid);

            _grid2 = new DGrid(_guiManager, 2, 2, DGrid.DGridFillType.DButton);
            _grid2.Position = new Vector2(300, 120);
            _form.AddPanel(_grid2);

            _grid3 = new DGrid(_guiManager, 2, 2, DGrid.DGridFillType.DText);
            _grid3.Position = new Vector2(300, 230);
            _form.AddPanel(_grid3);

            _multiLineText = new DMultiLineTextBox(_guiManager);
            _multiLineText.Position = new Vector2(300, 340);
            _multiLineText.Size = new Vector2(200, 300);
            _form.AddPanel(_multiLineText);


            _gridText = new DText(_guiManager);
            _gridText.FontName = "Miramonte";
            _gridText.Text = "Test";
            _grid.AddGridPanel(0, 0, _gridText);


            _gridToggleButton = new DToggleButton(_guiManager);
            _gridToggleButton.Text = "Test";
            _grid.AddGridPanel(1, 1, _gridToggleButton);
        }

        void _listBoxAdd_OnClick(GameTime gameTime)
        {
            _listBox.AddListItem(new DListBoxItem(_guiManager, "NOM"));
        }

        void _listBoxClear_OnClick(GameTime gameTime)
        {
            if (_listBox.Items.Count > 0)
                _listBox.RemoveListItem(_listBox.Items[_listBox.Items.Count - 1]);
        }



        void _toggle_OnToggle(DButtonBase.DButtonState state)
        {
            _label.Text = "OnToggle (" + state.ToString() + "): " + DateTime.Now.ToLongTimeString();
        }

        void _button_OnClick(GameTime gameTime)
        {
            _label.Text = "OnClick: " + DateTime.Now.ToLongTimeString();
        }



        public override void Update(GameTime gameTime)
        {
            _guiManager.Update(gameTime);

            //base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            _guiManager.Draw(gameTime);

            //base.Draw(gameTime);
        }
    }
}
