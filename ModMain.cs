using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// 1. Added the "GooseModdingAPI" project as a reference.
// 2. Compile this.
// 3. Create a folder with this DLL in the root, and *no GooseModdingAPI DLL*
using GooseShared;
using SamEngine;
using System.IO;
using System.Windows.Forms;
using System.Reflection;
using MoonSharp.Interpreter;
using System.Drawing;

namespace GooseDesktop
{
    class ModEntryPoint : IMod
    {
        void IMod.Init()
        {
            // Subscribe to whatever events we want
            InjectionPoints.PostRenderEvent += PostRender;
            Init();
        }
        public static GooseEntity goose;
        public void PostRender(GooseEntity goose1, Graphics g)
        {
            goose = goose1;
            if (ModRunning)
            {
                try
                {
                    UpdateMods(g);
                }
                catch (Exception e)
                {
                    DialogResult action = MessageBox.Show(null, "Uh oh! a BIG error occured!!!\n" + e.ToString(), "Quack Lua", MessageBoxButtons.AbortRetryIgnore);
                    if (action == DialogResult.Abort)
                    {
                        Application.Exit();
                    }
                    else if (action == DialogResult.Retry)
                    {
                        PostRender(goose1, g);
                    }
                    else if (action == DialogResult.Ignore)
                    {
                        ModRunning = false;
                    }
                }
            }
        }

        public static string ModContent = "";
        public static Script ModScript = new Script(CoreModules.Preset_HardSandbox);
        public static bool ModRunning = false;
        public void HandleRError(ScriptRuntimeException error)
        {
            ModRunning = false;
            MessageBox.Show("Honk! An exception has occured in the loaded mod! \n" + error.DecoratedMessage);
        }
        public void HandleSError(SyntaxErrorException error)
        {
            ModRunning = false;
            MessageBox.Show("Honk! A syntax errror has occured in the loaded mod! \n" + error.DecoratedMessage);
        }
        public DynValue StartMods()
        {
            try { DynValue ModLoadReturn = ModScript.DoString(ModContent); } catch (ScriptRuntimeException exc) { HandleRError(exc); return null; } catch (SyntaxErrorException exc) { HandleSError(exc); return null; }
            Table goosefuncs = new Table(ModScript);
            Table graphfuncs = new Table(ModScript);
            Table inputfuncs = new Table(ModScript);
            Table mousefuncs = new Table(ModScript);
            Table msgboxfuncs = new Table(ModScript);
            goosefuncs["GetGooseProp"] = Lua_GetGooseProp;
            goosefuncs["SetGooseProp"] = Lua_SetGooseProp;
            graphfuncs["DrawRect"] = Lua_DrawRect;
            graphfuncs["DrawText"] = Lua_DrawText;
            graphfuncs["MeasureText"] = Lua_MeasureText;
            mousefuncs["GetMousePos"] = Lua_GetMousePos;
            mousefuncs["GetMouseHeld"] = Lua_GetMouseHeld;
            msgboxfuncs["MessageBox"] = Lua_MessageBox;
            msgboxfuncs["MessageBoxAsk"] = Lua_MessageBoxAsk;
            msgboxfuncs["MessageBoxIcon"] = Lua_MessageBoxIcon;
            msgboxfuncs["MessageBoxIconAsk"] = Lua_MessageBoxIconAsk;
            msgboxfuncs["MessageBoxInput"] = Lua_MessageBoxInput;

            ModScript.Globals["Graphics"] = graphfuncs;
            inputfuncs["Mouse"] = mousefuncs;
            ModScript.Globals["Input"] = inputfuncs;
            ModScript.Globals["Interface"] = msgboxfuncs;

            try
            {
                DynValue ModStartReturn = ModScript.Call(ModScript.Globals.Get("Start"));
                return ModStartReturn;
            }
            catch (ScriptRuntimeException exc)
            {
                HandleRError(exc);
            }
            return null;
        }
        public static Graphics g;
        public DynValue UpdateMods(Graphics g1)
        {
            g = g1;
            //SetLuaValues(goosePosition);
            try
            {
                if (ModRunning)
                {
                    try { DynValue ModUpdReturn = ModScript.Call(ModScript.Globals.Get("Update")); return ModUpdReturn; } catch (ArgumentException) { ModRunning = false; return null; }
                    //GetAndSetLuaValues();
                }
                else
                {
                    return null;
                }
            }
            catch (ScriptRuntimeException exc) { HandleRError(exc); return null; }
        }
        Func<string, object> Lua_GetGooseProp = prop =>
        {
            try
            {
                return GetProp(prop);
            }
            catch (NullReferenceException)
            {
                throw new ScriptRuntimeException(new Exception("invalid goose property"));
            }
        };
        Func<string, DynValue, bool> Lua_SetGooseProp = (prop, toset) =>
        {
            SetProp(prop, toset);
            return true;
        };
        Func<DynValue, DynValue, string, bool> Lua_DrawRect = (pos1, pos2, color) =>
        {
            g.FillRectangle(new SolidBrush(Color.FromName(color)), new Rectangle((int)pos1.Table.Get(1).Number, (int)pos1.Table.Get(2).Number, (int)pos2.Table.Get(1).Number, (int)pos2.Table.Get(2).Number));
            return true;
        };
        Func<DynValue, string, string, float, bool> Lua_DrawText = (pos, content, color, size) =>
        {
            g.DrawString(content, new Font("Arial", size, FontStyle.Bold), new SolidBrush(Color.FromName(color)), (float)pos.Table.Get(1).Number, (float)pos.Table.Get(2).Number);
            return true;
        };
        Func<DynValue, string, string, float, DynValue> Lua_MeasureText = (pos, content, color, size) =>
        {
            SizeF stringsize = g.MeasureString(content, new Font("Arial", size, FontStyle.Bold), int.MaxValue);
            return DynValue.NewTable(new Table(ModScript, new DynValue[] { DynValue.NewNumber(stringsize.Width), DynValue.NewNumber(stringsize.Height) }));
        };
        Func<DynValue> Lua_GetMousePos = () =>
        {
            return DynValue.NewTable(new Table(ModScript, new DynValue[] { DynValue.NewNumber(Cursor.Position.X), DynValue.NewNumber(Cursor.Position.Y) }));
        };
        Func<bool> Lua_GetMouseHeld = () =>
        {
            return (Control.MouseButtons == MouseButtons.Left);
        };
        Func<string, string, bool> Lua_MessageBoxIcon = (content, icon) =>
        {
            try
            {
                MessageBox.Show(null, content, "Lua", MessageBoxButtons.OK, (MessageBoxIcon)Enum.Parse(typeof(MessageBoxIcon), icon));
            }
            catch (ArgumentException)
            {
                throw new ScriptRuntimeException(new Exception("invalid text box icon"));
            }
            return true;
        };
        Func<string, string, bool> Lua_MessageBoxIconAsk = (content, icon) =>
        {
            try
            {
                DialogResult dr = MessageBox.Show(null, content, "Lua", MessageBoxButtons.YesNo, (MessageBoxIcon)Enum.Parse(typeof(MessageBoxIcon), icon));
                if (dr == DialogResult.Yes)
                {
                    return true;
                }
                else if (dr == DialogResult.No)
                {
                    return false;
                }
            }
            catch (ArgumentException)
            {
                throw new ScriptRuntimeException(new Exception("invalid text box icon"));
            }
            return false;
        };
        Func<string, bool> Lua_MessageBox = (content) =>
        {
            if (dialogboxes < 3)
            {
                dialogboxes++;
                MessageBox.Show(null, content, "Lua", MessageBoxButtons.OK);
                dialogboxes--;
                return true;
            }
            else
            {
                return false;
            }
        };
        Func<string, bool> Lua_MessageBoxAsk = (content) =>
        {
            if (dialogboxes < 3)
            {
                dialogboxes++;
                DialogResult dr = MessageBox.Show(null, content, "Lua", MessageBoxButtons.YesNo);
                dialogboxes--;
                if (dr == DialogResult.Yes)
                {
                    return true;
                }
                else if (dr == DialogResult.No)
                {
                    return false;
                }
            }
            return false;
        };
        static int dialogboxes = 0;
        Func<string, string, string> Lua_MessageBoxInput = (content, defaultval) =>
        {
            var theval = defaultval;
            if (dialogboxes < 3)
            {
                dialogboxes++;
                DialogResult dr = InputBox("Lua", "A Lua mod has requested input:\n" + content, ref theval);
                dialogboxes--;
                return theval;
            }
            else
            {
                return "";
            }
        };

        //input box
        public static DialogResult InputBox(string title, string promptText, ref string value)
        {
            Form form = new Form();
            Label label = new Label();
            TextBox textBox = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            form.Text = title;
            label.Text = promptText;
            textBox.Text = value;

            buttonOk.Text = "OK";
            buttonCancel.Text = "Cancel";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(9, 20, 372, 13);
            textBox.SetBounds(12, 56, 372, 20);
            buttonOk.SetBounds(228, 100, 75, 23);
            buttonCancel.SetBounds(309, 100, 75, 23);

            label.AutoSize = true;
            textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(396, 140);
            form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
            form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            DialogResult dialogResult = form.ShowDialog();
            value = textBox.Text;
            return dialogResult;
        }
        //input box

        /*Func<string, DynValue, bool> Lua_SetGooseProp = (prop, toset) =>
        {
            return GooseConfig.RegisterConfig();
        };*/

        public static object GetProp(string prop)
        {
            var res = (dynamic)typeof(GooseEntity).GetField(prop).GetValue(goose);
            if (res.GetType() == typeof(Vector2))
            {
                res = new DynValue[] { DynValue.NewNumber(res.x), DynValue.NewNumber(res.y) };
            }
            return res;
        }

        public static object SetProp(string prop, DynValue toset)
        {
            object res = toset;
            if (toset.Type == DataType.Table)
            {
                res = new Vector2((float)toset.Table.Get(1).Number, (float)toset.Table.Get(2).Number);
            };
            if (toset.Type == DataType.Number)
            {
                res = (float)toset.Number;
            };
            if (toset.Type == DataType.String)
            {
                res = toset.String;
            };
            try
            {
                typeof(GooseEntity).GetField(prop).SetValue(goose, res);
            }
            catch (FieldAccessException)
            {
                throw new ScriptRuntimeException(new Exception("attempt to set constant goose property"));
            }
            return true;
        }
        public void Init()
        {
            if (File.Exists(@"Assets/Mods/mod.lua"))
            {
                ModContent = File.ReadAllText(@"Assets/Mods/mod.lua");
                StartMods();
                ModRunning = true;
            } else
            {
                Lua_MessageBoxIcon("There is no mod.lua in the root folder! This mod is useless without one!\nPut a lua mod in Assets/Mods! (make sure it's named \"mod.lua\")", "Error");
            }
        }
    }
}
